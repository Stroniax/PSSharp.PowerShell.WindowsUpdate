namespace PSSharp.PowerShell.WindowsUpdate.Commands
open System
open PSCommands
open WindowsUpdateSearcher

[<Cmdlet(VerbsCommon.Get, Nouns.WindowsUpdateHistory)>]
type GetWindowsUpdateHistoryCommand () =
    inherit FSCmdlet ()

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

    abstract member Searcher: WUApiLib.IUpdateSearcher option
    default _.Searcher = None

    override this.ProcessRecord () =
            let results = history {
                ServerSelection = this.BoundParameter (nameof this.ServerSelection)
                ServiceId = this.BoundParameter (nameof this.ServiceId)
                CanAutomaticallyUpgradeService = this.BoundParameter (nameof this.CanAutomaticallyUpgradeService)
                IncludePotentiallySupersededUpdates = this.BoundParameter (nameof this.IncludePotentiallySupersededUpdates)
                Online = this.BoundParameter (nameof this.Online)
                StartIndex = 0
                Count = System.Int32.MaxValue
                Searcher = this.Searcher
                WriteDebug = this.WriteDebug
                IgnoreDownloadPriority = None
                WriteWarning = this.WriteWarning
            }

            results |> Seq.map pso |> Seq.iter this.WriteObject
