namespace PSSharp.PowerShell.WindowsUpdate.Commands

module WindowsUpdateDownloader =
    open System
    open WUApiLib
    open System.Collections.Immutable

    let createDownloader () = new UpdateDownloaderClass () :> IUpdateDownloader

    type DownloadPriority = WUApiLib.DownloadPriority

    type private IDownloadParameters =
        abstract IsForced: bool option
        abstract Updates: UpdateCollection option
        abstract Priority: DownloadPriority option
        abstract Service: IUpdateDownloader option
        abstract WriteWarning: string -> unit
        abstract WriteProgress: obj -> unit

    type DownloadParameters =
        {
        IsForced: bool option
        Updates: UpdateCollection option
        Priority: DownloadPriority option
        Service: IUpdateDownloader option
        WriteWarning: string -> unit
        WriteProgress: obj -> unit
        }
        interface IDownloadParameters with
            member this.IsForced = this.IsForced
            member this.Updates = this.Updates
            member this.Priority = this.Priority
            member this.Service = this.Service
            member this.WriteWarning message = this.WriteWarning message
            member this.WriteProgress progress = this.WriteProgress progress

    let private _getDownloader (parameters: #IDownloadParameters) =
        let service = parameters.Service |> Option.defaultWith createDownloader
        service.ClientApplicationID <- "PSSharp.PowerShell.WindowsUpdate"
        parameters.IsForced |> Option.iter service.set_IsForced
        parameters.Updates |> Option.iter service.set_Updates
        parameters.Priority |> Option.iter service.set_Priority
        service

    type UpdateDownloadResult = {
        HResult: int
        ResultCode: OperationResultCode
    }

    let download (parameters: DownloadParameters) =
        async {
            let downloader = _getDownloader parameters

            let! ct = Async.CancellationToken
            use creg = new WindowsUpdateHelpers.AsyncCancellationWrapper(ct)

            let work: Async<IDownloadJob> = Async.FromContinuations <| fun (scont, econt, tcont) ->
                let callback = {
                    new IDownloadCompletedCallback with
                        member _.Invoke(job, args) =
                            let scont = job.AsyncState :?> (_ -> unit)
                            scont job
                    }
                let job = downloader.BeginDownload(parameters.WriteProgress, callback, scont)
                creg |> WindowsUpdateHelpers.Register job

            let! job = work

            let result = downloader.EndDownload(job)
            let il = ImmutableList.CreateBuilder()

            try
                for i in 0 .. job.Updates.Count - 1 do
                    let update = job.Updates.Item i |> ModelConversions.ToModel
                    let result = result.GetUpdateResult i |> ModelConversions.ToModel
                    il.Add {|
                        Update = update
                        Result = result
                    |}
                return {|
                    Updates = il.ToImmutable()
                    HResult = result.HResult
                    ResultCode = result.ResultCode
                |}
            finally
                // Can't be called in BeginDownload's OnCompleted callback which is effectively
                // the entire async continuation. I'm not really sure how else to do clean up the job
                Async.Start <| async { job.CleanUp() }
        }