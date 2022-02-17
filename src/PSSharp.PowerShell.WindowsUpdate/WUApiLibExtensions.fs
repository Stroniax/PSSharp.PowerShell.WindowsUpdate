namespace PSSharp.PowerShell.WindowsUpdate


[<AutoOpen>]
module WUApiLibExtensions =
    open System
    open System.Management.Automation
    open System.Threading
    open WUApiLib

    type WUApiLib.IUpdate with
        member this.UpdateId =
            this.Identity.UpdateID
        member this.RevisionNumber =
            this.Identity.RevisionNumber

    type WUApiLib.IUpdateSearcher with
        member this.AsyncSearch(criteria: string) =
            async {
                let! ct = Async.CancellationToken
                let mutable abortRequested = 0
                return! Async.FromContinuations(fun (succeedWith, failWith, cancelWith) ->
                    let onCompleted = { new ISearchCompletedCallback with
                        member _.Invoke(searchJob: ISearchJob, args: ISearchCompletedCallbackArgs) =
                            let searcher : IUpdateSearcher = unbox searchJob.AsyncState
                            if Interlocked.Exchange(&abortRequested, 0) = 0 then
                                let searchResult = searcher.EndSearch(searchJob)
                                succeedWith searchResult
                            else
                                cancelWith <| new OperationCanceledException()
                            searchJob.CleanUp()
                    }
                    let job = this.BeginSearch(criteria, onCompleted, this)
                    ct.Register(fun () ->
                        Interlocked.Exchange(&abortRequested, 1) |> ignore
                        job.RequestAbort()
                        ) |> ignore
                )
            }
