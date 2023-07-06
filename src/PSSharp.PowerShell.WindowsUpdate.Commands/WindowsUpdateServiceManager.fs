namespace PSSharp.PowerShell.WindowsUpdate.Commands

module WindowsUpdateServiceManager =
    open WUApiLib
    open System.Collections.Immutable

    type IUpdateServiceManagerParameters =
        abstract ClientApplicationId: string option
        abstract Options: ImmutableDictionary<string, obj>

    let private _defaultOptions = { new IUpdateServiceManagerParameters with
            member _.ClientApplicationId = None
            member _.Options = ImmutableDictionary<string, obj>.Empty
        }

    let private _isDefaultServiceManager (options: IUpdateServiceManagerParameters) =
        options.ClientApplicationId = None && options.Options.IsEmpty

    /// Creates a new UpdateServiceManager
    let CreateUpdateServiceManager (parameters: #IUpdateServiceManagerParameters) =
        let x = new UpdateServiceManagerClass()
        x.ClientApplicationID <- parameters.ClientApplicationId |> Option.defaultValue "PSSharp.PowerShell.WindowsUpdate"
        for opt in parameters.Options do
            x.SetOption(opt.Key, opt.Value)
        x :> IUpdateServiceManager
    
    let private CreateDefault () =
        CreateUpdateServiceManager _defaultOptions

    /// Singleton update service manager
    let mutable private _updateServiceManager = Loading.initial

    /// Gets a shared singleton instance of the UpdateServiceManager, or
    /// a new instance with the specified parameters, depending on whether
    /// the parameters represent the default parameters.
    let GetUpdateServiceManager (parameters: #IUpdateServiceManagerParameters) =
        if (_isDefaultServiceManager parameters) then
            Loading.get CreateDefault &_updateServiceManager
        else CreateUpdateServiceManager parameters

    type UpdateServicesParameters =
        {
        ClientApplicationId: string option
        Options: ImmutableDictionary<string, obj>
        }
        interface IUpdateServiceManagerParameters with
            member this.ClientApplicationId = this.ClientApplicationId
            member this.Options = this.Options

    let GetUpdateServices (parameters: UpdateServicesParameters) =
        let sm = GetUpdateServiceManager parameters
        sm.Services |> ModelConversions.ToModel

    type AddServiceParameters =
        {
        ClientApplicationId: string option
        Options: ImmutableDictionary<string, obj>
        ServiceId: string
        AuthorizationCabPath: string
        }
        interface IUpdateServiceManagerParameters with
            member this.ClientApplicationId = this.ClientApplicationId
            member this.Options = this.Options

    let AddService parameters =
        let sm = GetUpdateServiceManager parameters
        sm.AddService (parameters.ServiceId, parameters.AuthorizationCabPath)
        |> ModelConversions.ToModel

    type RegisterAutomaticUpdateServiceParameters =
        {
        ClientApplicationId: string option
        Options: ImmutableDictionary<string, obj>
        ServiceId: string
        }
        interface IUpdateServiceManagerParameters with
            member this.ClientApplicationId = this.ClientApplicationId
            member this.Options = this.Options

    let RegisterServiceWithAU parameters =
        let sm = GetUpdateServiceManager parameters
        sm.RegisterServiceWithAU parameters.ServiceId
        

    type UnregisterAutomaticUpdateServiceParameters =
        {
        ClientApplicationId: string option
        Options: ImmutableDictionary<string, obj>
        ServiceId: string
        }
        interface IUpdateServiceManagerParameters with
            member this.ClientApplicationId = this.ClientApplicationId
            member this.Options = this.Options

    let UnregisterServiceWithAU parameters =
        let sm = GetUpdateServiceManager parameters
        sm.UnregisterServiceWithAU parameters.ServiceId