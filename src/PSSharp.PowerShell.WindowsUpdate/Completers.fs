namespace PSSharp.PowerShell.WindowsUpdate
module Completers =
    open System.Management.Automation
    open WUApiLib

    type QuotationType =
    | None = 0
    | SingleQuote = 1
    | DoubleQuote = 2
    type RequoteWildcard =
        {
        Wildcard: WildcardPattern
        QuotationType: QuotationType
        }
        member this.IsMatch str = this.Wildcard.IsMatch str
        member this.Requote (str: string) =
            match this.QuotationType with
            | QuotationType.None when str.Contains(' ') -> $"'{str}'"
            | QuotationType.SingleQuote -> $"'{str}'"
            | QuotationType.DoubleQuote -> $"\"{str}\""
            | _ -> str

    let getWildcardFromWordToComplete (wordToComplete: string) =
        let quoteChar =
            if wordToComplete.StartsWith ''' then
                QuotationType.SingleQuote
            elif wordToComplete.StartsWith '"' then
                QuotationType.DoubleQuote
            else
                QuotationType.None
        let trimmedWord =
            match quoteChar with
            | QuotationType.None -> wordToComplete
            | QuotationType.SingleQuote ->
                if wordToComplete.EndsWith ''' then wordToComplete.Substring(1, wordToComplete.Length - 2)
                else wordToComplete.Substring 1
            | QuotationType.DoubleQuote ->
                if wordToComplete.EndsWith '"' then wordToComplete.Substring(1, wordToComplete.Length - 2)
                else wordToComplete.Substring 1
            | _ -> wordToComplete
        {
            Wildcard = WildcardPattern.wc $"{trimmedWord}*"
            QuotationType = quoteChar
        }

        
    type NoCompleter() =
        interface IArgumentCompleter with
            member _.CompleteArgument(_, _, _, _, _) = Array.empty
    type UpdateHistoryUpdateIdCompleter() =
        interface IArgumentCompleter with
            member _.CompleteArgument(_, _, wordToComplete, _, _) =
                let wc = getWildcardFromWordToComplete wordToComplete
                let searcher = WUCore.session.CreateUpdateSearcher()
                searcher.QueryHistory(0, searcher.GetTotalHistoryCount())
                |> Seq.cast
                |> Seq.filter (fun (h: IUpdateHistoryEntry) -> wc.IsMatch h.UpdateIdentity.UpdateID)
                |> Seq.map (fun h ->
                    new CompletionResult(
                        wc.Requote(h.UpdateIdentity.UpdateID),
                        h.UpdateIdentity.UpdateID,
                        CompletionResultType.ParameterValue,
                        $"{h.UpdateIdentity.UpdateID} '{h.Title}'"
                    ))
    type UpdateHistoryTitleCompleter() =
        interface IArgumentCompleter with
            member _.CompleteArgument(_, _, wordToComplete, _, _) =
                let wc = getWildcardFromWordToComplete wordToComplete
                let searcher = WUCore.session.CreateUpdateSearcher()
                searcher.QueryHistory(0, searcher.GetTotalHistoryCount())
                |> Seq.cast
                |> Seq.filter (fun (h: IUpdateHistoryEntry) -> wc.IsMatch h.UpdateIdentity.UpdateID)
                |> Seq.map (fun h ->
                    new CompletionResult(
                        wc.Requote(h.Title),
                        h.Title,
                        CompletionResultType.ParameterValue,
                        $"{h.UpdateIdentity.UpdateID} '{h.Title}'"
                    ))