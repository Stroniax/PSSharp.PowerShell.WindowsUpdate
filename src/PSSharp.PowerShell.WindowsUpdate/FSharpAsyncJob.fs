namespace PSSharp.PowerShell.WindowsUpdate
open System.Management.Automation
open System
open System.Threading

type PSStreamData =
| Output of PSObject
| Debug of DebugRecord
| Verbose of VerboseRecord
| Information of InformationRecord
| Progress of ProgressRecord
| Warning of WarningRecord
| Error of ErrorRecord

type FSharpAsyncJobOperation = (PSStreamData -> unit) -> Async<unit>

type FSharpAsyncJob (jobOp: FSharpAsyncJobOperation) =
    inherit Job()

    let cts = new CancellationTokenSource()

    member private this.NotifyFailed e =
        let er = new ErrorRecord(
            e,
            "TerminalJobException",
            ErrorCategory.NotSpecified,
            null
            )
        this.Error.Add(er)
        this.SetJobState(JobState.Failed)
    member private this.NotifyStopped() =
        this.SetJobState(JobState.Stopped)
    member private this.NotifyCompleted() =
        this.SetJobState(JobState.Completed)

    override this.StopJob () =
        this.SetJobState(JobState.Stopping)
        cts.Cancel()

    override _.Location = Environment.MachineName

    override this.HasMoreData =
        this.Debug.Count > 0
        || this.Verbose.Count > 0
        || this.Information.Count > 0
        || this.Warning.Count > 0
        || this.Error.Count > 0
        || this.Progress.Count > 0
        || this.Output.Count > 0

    override _.StatusMessage = ""

    member this.StartJob () =
        if this.JobStateInfo.State <> JobState.NotStarted then
            invalidOp "The job cannot be started because it was already begun."
        this.SetJobState(JobState.Running)

        let jobOpHandler data =
            match data with
            | Debug d -> this.Debug.Add d
            | Verbose v -> this.Verbose.Add v
            | Information i -> this.Information.Add i
            | Warning w -> this.Warning.Add w
            | Error e -> this.Error.Add e
            | Progress p -> this.Progress.Add p
            | Output o -> this.Output.Add o
        Async.StartImmediate(
            Async.TryCancelled(
                async {
                    try
                        do! jobOp jobOpHandler
                        this.NotifyCompleted()
                    with ex -> this.NotifyFailed ex
                },
                (fun e -> this.NotifyStopped())
            ),
            cts.Token
            )

    override _.Dispose(disposing) =
        cts.Dispose()
        base.Dispose(disposing)