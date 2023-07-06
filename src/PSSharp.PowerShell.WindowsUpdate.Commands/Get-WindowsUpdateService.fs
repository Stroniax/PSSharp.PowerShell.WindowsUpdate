namespace PSSharp.PowerShell.WindowsUpdate.Commands
open WUApiLib
open System.Management.Automation
open PowerShellApi
open WindowsUpdateServiceManager

[<Cmdlet(VerbsCommon.Get, "WindowsUpdateService")>]
type GetWindowsUpdateServiceCommand () =
    inherit FSAsyncCmdlet ()

    override _.ProcessRecordAsync output =
        // This function is not truly async but will run in a
        // ThreadPool thread when called via Async.RunSynchronously
        async {
            let manager = GetUpdateServiceManager()

            for i in 0..manager.Services.Count do
                let o = manager.Services.Item i
                let pso = PSObject<WUApiLib.IUpdateService>(o)
                output.WriteObject pso
        }

