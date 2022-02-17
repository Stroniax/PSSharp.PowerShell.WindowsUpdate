namespace PSSharp.PowerShell.WindowsUpdate.Commands
open System
open System.Management.Automation
open PSSharp.PowerShell.WindowsUpdate

[<Alias("Download-WindowsUpdate", "dlwu", "rqwu")>]
type RequestWindowsUpdateCommand () =
    inherit WindowsUpdateCmdlet ()