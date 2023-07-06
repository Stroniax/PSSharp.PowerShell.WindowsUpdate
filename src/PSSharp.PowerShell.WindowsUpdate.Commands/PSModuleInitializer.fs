namespace PSSharp.PowerShell.WindowsUpdate.Commands

module PSModuleInitializer =

    open System.Management.Automation

    type WindowsInitializer () =
        interface IModuleAssemblyInitializer with
            member _.OnImport () =
                ()