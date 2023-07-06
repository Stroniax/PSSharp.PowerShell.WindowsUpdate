namespace PSSharp.PowerShell.WindowsUpdate.Commands
open System.Management.Automation
open FSharp.Reflection


module Loading =
    open System.Threading
    open System.Threading.Tasks

    type Loading<'T> =
        private
        | NotStarted
        | Loading of Task<'T>
        | Complete of 'T

    let initial = NotStarted

    let private complete (loading: Loading<'T> byref) (value) =
        Interlocked.Exchange(&loading, Complete value)

    let private reset (loading: Loading<'T> byref) =
        Interlocked.Exchange(&loading, NotStarted)

    let private tryStartAsync (loading: Loading<'T> byref) =
        let tcs = TaskCompletionSource<'T>()
        let current = Loading tcs.Task
        let previous = Interlocked.CompareExchange(&loading, current, NotStarted)
        match previous with
        | NotStarted -> Choice1Of2 tcs
        | Complete c -> Choice2Of2 <| Task.FromResult(c)
        | Loading t -> Choice2Of2 t

    let private tryStart (loading: Loading<'T> byref) =
        match tryStartAsync &loading with
        | Choice1Of2 tcs -> Choice1Of2 tcs
        | Choice2Of2 task -> Choice2Of2 <| task.GetAwaiter().GetResult()

    let get (factory: unit -> 'T) (loading: Loading<'T> byref) =
        match tryStart &loading with
        | Choice1Of2 tcs ->
            try
                let result = factory ()
                tcs.TrySetResult result |> ignore
                complete &loading result |> ignore
                result
            with e ->
                tcs.TrySetException e |> ignore
                reset &loading |> ignore
                reraise ()
        | Choice2Of2 f -> f

    /// When called the first time, runs on the current thread. Only concurrent
    /// calls to getAsync will return an async Task.
    let getAsync (factory: unit -> 'T) (loading: Loading<'T> byref) =
        match tryStartAsync &loading with
        | Choice1Of2 tcs ->
            try
                let result = factory ()
                complete &loading result |> ignore
                tcs.TrySetResult result |> ignore
            with e ->
                reset &loading |> ignore
                tcs.TrySetException e |> ignore
            tcs.Task
        | Choice2Of2 task -> task

type FSharpUnionCompleter<'TUnion> () =
    let mutable cases = None

    let GenerateCases () =
        FSharpType.GetUnionCases(typeof<'TUnion>)
        |> Array.filter (fun c -> c.GetFields().Length = 0)
        |> Array.map (fun c -> c.Name)

    let GetCases() =
        match cases with
        | Some v -> v
        | None ->
            let loaded = GenerateCases()
            cases <- Some loaded
            loaded
    
    interface IArgumentCompleter with
        member _.CompleteArgument(commandName, parameterName, wordToComplete, commandAst, fakeBoundParameters) =
            seq {
                for i in GetCases() do
                    if WildcardPattern.Get(wordToComplete, WildcardOptions.IgnoreCase).IsMatch(i) then
                        yield CompletionResult(
                            i,
                            i,
                            CompletionResultType.ParameterValue,
                            i
                        )
            }

type FSUnionConverter () =
    inherit System.Management.Automation.PSTypeConverter()
    let unionCasePredicate obj (case: FSharp.Reflection.UnionCaseInfo) =
        case.Name = unbox obj && case.GetFields().Length = 0
    override _.CanConvertTo(obj: obj, t: System.Type) =
        obj :? string && FSharp.Reflection.FSharpType.IsUnion(t)
        && FSharp.Reflection.FSharpType.GetUnionCases(t) |> Array.exists (unionCasePredicate obj)
    override _.CanConvertFrom(obj: obj, t: System.Type) =
        FSharp.Reflection.FSharpType.IsUnion(obj.GetType())
        && t = typeof<string>
    override _.ConvertTo(obj: obj, t: System.Type, formatProvider: System.IFormatProvider, ignoreCase: bool) =
        let case =
            FSharp.Reflection.FSharpType.GetUnionCases(t)
            |> Array.find (fun c -> c.Name = unbox obj)
        FSharp.Reflection.FSharpValue.MakeUnion(case, Array.empty)
    override _.ConvertFrom(obj: obj, t: System.Type, formatProvider: System.IFormatProvider, ignoreCase: bool) =
        let (case, _) = FSharp.Reflection.FSharpValue.GetUnionFields(obj, obj.GetType())
        box case.Name

