namespace PSSharp.PowerShell.WindowsUpdate.Commands
    module PSCommands =
        type Cmdlet = System.Management.Automation.Cmdlet
        type PSCmdlet = System.Management.Automation.PSCmdlet
        type FSCmdlet = PowerShellApi.FSCmdlet
        type FSAsyncCmdlet = PowerShellApi.FSAsyncCmdlet
        type FSAsyncJob = PowerShellApi.FSAsyncJob
        type CmdletAttribute = System.Management.Automation.CmdletAttribute
        type ParameterAttribute = System.Management.Automation.ParameterAttribute
        type ErrorRecord = System.Management.Automation.ErrorRecord
        type Switch = System.Management.Automation.SwitchParameter
        type ErrorCategory = System.Management.Automation.ErrorCategory

        type PSObject = System.Management.Automation.PSObject
        type PSObject<'T> = PowerShellApi.PSObject<'T>
        type pso = PSObject
        type 'a pso = PSObject<'a>
        let pso a = PSObject<_>(a)
        /// Get base object of PSObject
        let psbase (pso: pso) = pso.BaseObject
        /// Get base object of typed PSObject
        let psbaset (pso: 'a pso) = pso.BaseObject
        /// Use PowerShell type conversion to try to convert an object to another type
        let pscast o =
            match System.Management.Automation.LanguagePrimitives.TryConvertTo o with
            | true, v -> ValueSome v
            | _ -> ValueNone
        /// Non-present switch parameter
        let switch = Switch(false)

        type VerbsCommon = System.Management.Automation.VerbsCommon
        type VerbsLifecycle = System.Management.Automation.VerbsLifecycle

        module Nouns =
            [<Literal>]
            let WindowsUpdateService = "WindowsUpdateService"
            [<Literal>]
            let WindowsAutomaticUpdateService = "WindowsAutomaticUpdateService"
            [<Literal>]
            let WindowsUpdate = "WindowsUpdate"
            [<Literal>]
            let WindowsUpdateHistory = "WindowsUpdateHistory"

        module ErrorIds =
            /// ErrorId generated for unspecified UpdateSearcher warnings
            [<Literal>]
            let UpdateSearchWarning = "UpdateSearchWarning"
