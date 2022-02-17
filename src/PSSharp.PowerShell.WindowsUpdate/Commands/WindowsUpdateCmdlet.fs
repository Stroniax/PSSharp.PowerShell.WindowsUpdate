namespace PSSharp.PowerShell.WindowsUpdate
open System
open System.Management.Automation
open WUApiLib

type WindowsUpdateCmdlet internal () =
    inherit PSCmdlet()
