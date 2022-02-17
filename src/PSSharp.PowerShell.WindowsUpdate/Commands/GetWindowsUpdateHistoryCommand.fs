namespace PSSharp.PowerShell.WindowsUpdate.Commands
open PSSharp.PowerShell.WindowsUpdate
open System
open System.Management.Automation
open Models
open SwitchParameter
open PSObject
open WUCore
open Completers
open WUApiLib

[<Cmdlet(VerbsCommon.Get, Nouns.WindowsUpdateHistory)>]
type GetWindowsUpdateHistoryCommand () =
    inherit WindowsUpdateCmdlet()

    let mutable _titleWildcards = None
    let mutable _updateIds = None

    [<Parameter>]
    [<SupportsWildcards>]
    [<ArgumentCompleter(typeof<UpdateHistoryTitleCompleter>)>]
    member val Title = Array.empty with get, set
    [<Parameter>]
    [<SupportsWildcards>]
    [<ArgumentCompleter(typeof<UpdateHistoryUpdateIdCompleter>)>]
    member val UpdateId = Array.empty with get, set
    [<Parameter>]
    [<ArgumentCompleter(typeof<NoCompleter>)>]
    member val MinimumDate = DateTime.MinValue with get, set
    [<Parameter>]
    [<ArgumentCompleter(typeof<NoCompleter>)>]
    member val MaximumDate = DateTime.MaxValue with get, set
    [<Parameter>]
    member val FromServer = Models.ServerSelection.Default with get, set
    [<Parameter>]
    member val ListUninstallations = switch with get, set

    member private this.GetWildcards(persist: WildcardPattern list option byref, source: string[]) =
        match persist with
        | None ->
            let wildcards = [
                for item in source do
                    WildcardPattern.Get(item, WildcardOptions.IgnoreCase)
            ]
            persist <- Some wildcards
            wildcards
        | Some wildcards -> wildcards
    member private this.GetTitleWildcards() =
        this.GetWildcards(&_titleWildcards, this.Title)
    member private this.GetUpdateIdWildcards() =
        this.GetWildcards(&_updateIds, this.UpdateId)

    member this.ShouldWrite(entry: IUpdateHistoryEntry) =
        let ids = this.GetUpdateIdWildcards()
        let titles = this.GetTitleWildcards()
        // compare wildcards last as that'll be the slowest process
        (this.FromServer = Models.ServerSelection.Default || entry.ServerSelection = (this.FromServer |> int |> enum))
        && (this.MinimumDate < entry.Date)
        && (this.MaximumDate > entry.Date)
        && (this.ListUninstallations.IsPresent || entry.Operation = tagUpdateOperation.uoInstallation)
        && (ids.IsEmpty || WildcardPattern.matchAny ids entry.UpdateIdentity.UpdateID)
        && (titles.IsEmpty || WildcardPattern.matchAny titles entry.Title)

    member this.ProcessNextBatch(searcher: IUpdateSearcher, batchSize, batchIter, totalCount) =
        let page = searcher.QueryHistory(batchSize * batchIter, batchSize)
        for entry in page do
            if this.ShouldWrite entry then
                let pso = pso entry
                pso.TypeNames.Insert(0, "PSSharp.PowerShell.WindowsUpdate.WindowsUpdateHistory")
                this.WriteObject(pso)
            else this.WriteDebug "Filtered"
        if (not this.Stopping) && totalCount > batchSize * batchIter then
            this.ProcessNextBatch(searcher, batchSize, batchIter + 1, totalCount)

    override this.ProcessRecord () =
        let searcher = WUCore.session.CreateUpdateSearcher()

        let totalCount = searcher.GetTotalHistoryCount()
        this.WriteDebug $"Found {totalCount} update history"
        this.ProcessNextBatch(searcher, 25, 0, totalCount)