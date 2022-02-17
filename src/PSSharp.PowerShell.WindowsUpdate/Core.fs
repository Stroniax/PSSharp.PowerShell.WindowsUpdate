namespace PSSharp.PowerShell.WindowsUpdate

module internal WUCore =
    open System
    open WUApiLib
    open System.Management.Automation

    [<CompiledName("Session")>]
    let session = new UpdateSessionClass():> UpdateSession
    do session.ClientApplicationID <- "PSSharp.PowerShell.WindowsUpdate"

    module Nouns =
        /// [Noun]-WindowsUpdate
        /// Get-WindowsUpdate
        /// Request-WindowsUpdate
        /// Install-WindowsUpdate
        /// Uninstall-WindowsUpdate
        [<Literal>]
        let WindowsUpdate = "WindowsUpdate"
        /// [Noun]-WindowsUpdateHistory
        /// Get-WindowsUpdateHistory
        [<Literal>]
        let WindowsUpdateHistory = "WindowsUpdateHistory"
        /// [Noun]-WsusServer
        /// Get-WsusServer
        /// Register-WsusServer
        /// Unregister-WsusServer
        [<Literal>]
        let WsusServer = "WsusServer"

module SwitchParameter =
    open System.Management.Automation
    
    type switch = SwitchParameter
    let switch = switch()
module PSObject =
    open System.Management.Automation

    let psnull = Internal.AutomationNull.Value
    type pso = PSObject
    let pso obj = 
        match obj with
        | null -> psnull
        | _ -> PSObject.AsPSObject(obj)
    let psbase obj =
        match box obj with
        | :? PSObject as pso -> pso.BaseObject
        | _ -> obj

module WildcardPattern =
    open System.Management.Automation

    type wc = WildcardPattern
    let wc pattern = wc.Get(pattern, WildcardOptions.IgnoreCase)

    let matchAny (wildcards: WildcardPattern list) comparand =
        let rec matchOrNext (rem : WildcardPattern list) =
            if rem.IsEmpty then false
            elif rem.Head.IsMatch comparand then true
            else matchOrNext rem.Tail
        matchOrNext wildcards