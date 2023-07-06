namespace PSSharp.PowerShell.WindowsUpdate.Commands
open WUApiLib
open PSCommands
open WindowsUpdateServiceManager

[<Cmdlet(VerbsLifecycle.Register, Nouns.WindowsAutomaticUpdateService)>]
type RegisterWindowsAutomaticUpdateServiceCommand () =
    inherit Cmdlet ()

    [<Parameter>]
    member val ServiceId = System.String.Empty with get, set

    [<Parameter>]
    member val PassThru = switch with get, set

    override this.ProcessRecord () =
        let manager = GetUpdateServiceManager()

        manager.RegisterServiceWithAU(this.ServiceId)

        if this.PassThru.IsPresent then
            let o = manager.Services.Item (manager.Services.Count - 1)
            let pso = PSObject<IUpdateService>(o)
            this.WriteObject(pso)

