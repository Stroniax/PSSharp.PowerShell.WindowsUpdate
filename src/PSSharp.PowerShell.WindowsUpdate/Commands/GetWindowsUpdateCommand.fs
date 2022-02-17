namespace PSSharp.PowerShell.WindowsUpdate.Commands
open PSSharp.PowerShell.WindowsUpdate
open System
open System.Management.Automation
open Models
open SwitchParameter
open PSObject

type GetWindowsUpdateCommand () =
    inherit WindowsUpdateCmdlet()

    member val Title = Array.empty with get, set
    member val KBArticleId = Array.empty with get, set
    member val UpdateId = Array.empty with get, set
    member val CategoryId = Array.empty with get, set
    member val Type = Array.empty with get, set
    member val IncludeInstalled = switch with get, set
    member val IncludeHidden = switch with get, set
    member val IncludePresent = switch with get, set
    member val Server = ServerSelection.Default with get, set
    member val AsJob = switch with get, set