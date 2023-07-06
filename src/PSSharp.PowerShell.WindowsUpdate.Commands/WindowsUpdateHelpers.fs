namespace PSSharp.PowerShell.WindowsUpdate.Commands

module internal WindowsUpdateHelpers =
    open System

    type AsyncCancellationWrapper (token: System.Threading.CancellationToken) =
        let mutable registration = ValueNone
        member _.Register (arg: 'a) (callback: 'a -> unit) =
            registration <- token.Register(Action<_>(unbox >> callback), arg) |> ValueSome

        interface IDisposable with
            member _.Dispose () =
                match registration with
                | ValueSome v -> v.Dispose ()
                | ValueNone -> ()

    let inline Register (job) (wrapper: AsyncCancellationWrapper) =
        let abort = fun () -> ( ^a : (member RequestAbort: unit -> unit) job )
        wrapper.Register () abort

