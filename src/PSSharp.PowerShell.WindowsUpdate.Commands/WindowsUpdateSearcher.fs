namespace PSSharp.PowerShell.WindowsUpdate.Commands

module WindowsUpdateSearcher =
    open System
    open WUApiLib
    open System.Collections.Immutable

    let createSearcher () = new UpdateSearcherClass () :> IUpdateSearcher

    type ServerSelection = WUApiLib.ServerSelection

    type private ISearcherParameters =
        abstract ServerSelection: ServerSelection option
        abstract ServiceId: string option
        abstract CanAutomaticallyUpgradeService: bool option
        abstract IncludePotentiallySupersededUpdates: bool option
        abstract Online: bool option
        abstract Searcher: IUpdateSearcher option
        /// v2 only
        abstract IgnoreDownloadPriority: bool option
        abstract WriteWarning: string -> unit
        
    let private _getConfiguredSearcher (parameters: #ISearcherParameters) =
        let searcher = parameters.Searcher |> Option.defaultWith createSearcher
        parameters.ServerSelection |> Option.iter searcher.set_ServerSelection
        parameters.ServiceId |> Option.iter searcher.set_ServiceID
        parameters.Online |> Option.iter searcher.set_Online
        parameters.IncludePotentiallySupersededUpdates |> Option.iter searcher.set_IncludePotentiallySupersededUpdates
        parameters.CanAutomaticallyUpgradeService |> Option.iter searcher.set_CanAutomaticallyUpgradeService
        searcher.ClientApplicationID <- "PSSharp.PowerShell.WindowsUpdate"

        let notsupported membername _ =
            membername
            |> sprintf "%s is not supported by the current version of the Windows Update API. The setting will be ignored."
            |> parameters.WriteWarning
            
        match searcher with
        | :? IUpdateSearcher2 as s2 -> parameters.IgnoreDownloadPriority |> Option.iter s2.set_IgnoreDownloadPriority
        | _ -> parameters.IgnoreDownloadPriority |> Option.iter (notsupported "IgnoreDownloadPriority")


        searcher

    type UpdateSearchParameters =
        {
        ServerSelection: ServerSelection option
        ServiceId: string option
        CanAutomaticallyUpgradeService: bool option
        IncludePotentiallySupersededUpdates: bool option
        Online: bool option
        Criteria: string
        Searcher: IUpdateSearcher option
        WriteDebug: string -> unit
        WriteWarning: string -> unit
        IgnoreDownloadPriority: bool option
        }
        interface ISearcherParameters with
            member this.ServerSelection = this.ServerSelection
            member this.ServiceId = this.ServiceId
            member this.CanAutomaticallyUpgradeService = this.CanAutomaticallyUpgradeService
            member this.IncludePotentiallySupersededUpdates = this.IncludePotentiallySupersededUpdates
            member this.Online = this.Online
            member this.Searcher = this.Searcher
            member this.IgnoreDownloadPriority = this.IgnoreDownloadPriority
            member this.WriteWarning message = this.WriteWarning message

    let search parameters =
        async {
            let searcher = _getConfiguredSearcher parameters

            sprintf "Created searcher %A from parameters %A" searcher parameters
            |> parameters.WriteDebug

            let! ct = Async.CancellationToken

            use abortWrapper = new WindowsUpdateHelpers.AsyncCancellationWrapper(ct)

            let asyncfn = Async.FromContinuations (fun (scont, econt, tcont) ->
                let searchCallback = {
                    new ISearchCompletedCallback with
                        member _.Invoke (job, args) =
                            scont job
                }
                let job = searcher.BeginSearch(parameters.Criteria, searchCallback, null)
                abortWrapper.Register () job.RequestAbort
            )
            let! job = asyncfn

            let result = searcher.EndSearch(job)

            // schedule the remaining continuation on the thread pool
            // Never call IDownloadJob::CleanUp, IInstallationJob::CleanUp, or ISearchJob::CleanUp
            // on a job object in its callback function.
            // https://learn.microsoft.com/en-us/windows/win32/wua_sdk/guidelines-for-asynchronous-wua-operations
            do! Async.SwitchToThreadPool ()

            job.CleanUp()

            return result |> ModelConversions.ToModel
        }


    type QueryHistoryParameters =
        {
        ServerSelection: ServerSelection option
        ServiceId: string option
        CanAutomaticallyUpgradeService: bool option
        IncludePotentiallySupersededUpdates: bool option
        Online: bool option
        IgnoreDownloadPriority: bool option
        Searcher: IUpdateSearcher option
        StartIndex: int
        Count: int
        WriteDebug: string -> unit
        WriteWarning: string -> unit
        }
        interface ISearcherParameters with
            member this.ServerSelection = this.ServerSelection
            member this.ServiceId = this.ServiceId
            member this.CanAutomaticallyUpgradeService = this.CanAutomaticallyUpgradeService
            member this.IncludePotentiallySupersededUpdates = this.IncludePotentiallySupersededUpdates
            member this.Online = this.Online
            member this.Searcher = this.Searcher
            member this.IgnoreDownloadPriority = this.IgnoreDownloadPriority
            member this.WriteWarning message = this.WriteWarning message

    let history parameters =
        let searcher = _getConfiguredSearcher parameters
        
        let min =
            searcher.GetTotalHistoryCount()
            |> (-) parameters.StartIndex
            |> min parameters.Count

        searcher.QueryHistory(parameters.StartIndex, min)
        |> ModelConversions.ToModel
