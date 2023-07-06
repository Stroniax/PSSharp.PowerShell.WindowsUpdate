namespace PSSharp.PowerShell.WindowsUpdate.Commands
open System
open PSCommands
open WindowsUpdateSearcher

[<Cmdlet(VerbsCommon.Get, Nouns.WindowsUpdate)>]
type GetWindowsUpdateCommand () =
    inherit FSAsyncCmdlet ()

    [<Parameter>]
    member val ServerSelection = ServerSelection.ssDefault with get, set

    [<Parameter>]
    member val ServiceId = null: string with get, set

    [<Parameter>]
    member val CanAutomaticallyUpgradeService = false with get, set

    [<Parameter>]
    member val IncludePotentiallySupersededUpdates = false with get, set

    [<Parameter>]
    member val Online = false with get, set

    [<Parameter>]
    member val IgnoreDownloadPriority = false with get, set

    [<Parameter>]
    member val Criteria = null: string with get, set

    abstract member Searcher: WUApiLib.IUpdateSearcher option
    default _.Searcher = None

    override this.ProcessRecordAsync output =
        async {

            let! results = search {
                ServerSelection = this.BoundParameter (nameof this.ServerSelection)
                ServiceId = this.BoundParameter (nameof this.ServiceId)
                CanAutomaticallyUpgradeService = this.BoundParameter (nameof this.CanAutomaticallyUpgradeService)
                IncludePotentiallySupersededUpdates = this.BoundParameter (nameof this.IncludePotentiallySupersededUpdates)
                Online = this.BoundParameter (nameof this.Online)
                IgnoreDownloadPriority = this.BoundParameter (nameof this.IgnoreDownloadPriority)
                Criteria = this.Criteria
                Searcher = this.Searcher
                WriteDebug = output.WriteDebug
                WriteWarning = output.WriteWarning
            }

            let inline errorrecord exn =
                new ErrorRecord(
                    exn,
                    ErrorIds.UpdateSearchWarning,
                    ErrorCategory.NotSpecified,
                    null
                    )

            results.Warnings |> Seq.map errorrecord |> Seq.iter output.WriteError

            results.Updates |> Seq.map pso |> Seq.iter output.WriteObject
        }