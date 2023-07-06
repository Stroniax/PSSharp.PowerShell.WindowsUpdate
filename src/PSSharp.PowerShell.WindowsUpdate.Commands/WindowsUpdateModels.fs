namespace PSSharp.PowerShell.WindowsUpdate.Commands
open System.Collections.Immutable
type HiddenAttribute = System.Management.Automation.HiddenAttribute

[<AbstractClass>]
[<Sealed>]
type internal ModelConversions =
    static member inline CollectionMap ( fn: ^b -> ^c ) ( c: ^a ) =
        let count = ( ^a : (member Count: int ) c )

        let builder = ImmutableList.CreateBuilder()
        for i in 0 .. count - 1 do
            let v = ( ^a : (member get_Item: int -> ^b ) (c, i) )
            let r = fn v
            builder.Add r

        builder.ToImmutable()

type ServerSelection =
    | Default = 0
    | ManagedServer = 1
    | WindowsUpdate = 2
    | Others = 3

type ModelConversions with
    static member ToModel(value: WUApiLib.ServerSelection) =
        match value with
        | WUApiLib.ServerSelection.ssDefault -> ServerSelection.Default
        | WUApiLib.ServerSelection.ssManagedServer -> ServerSelection.ManagedServer
        | WUApiLib.ServerSelection.ssOthers -> ServerSelection.Others
        | WUApiLib.ServerSelection.ssWindowsUpdate -> ServerSelection.WindowsUpdate
        | _ -> value |> int |> enum
    static member ToCom (value: ServerSelection) =
        match value with
        | ServerSelection.Default -> WUApiLib.ServerSelection.ssDefault
        | ServerSelection.ManagedServer -> WUApiLib.ServerSelection.ssManagedServer
        | ServerSelection.WindowsUpdate -> WUApiLib.ServerSelection.ssWindowsUpdate
        | ServerSelection.Others -> WUApiLib.ServerSelection.ssOthers
        | _ -> value |> int |> enum

type InstallationImpact =
    | Normal = 0
    | Minor = 1
    | RequiresExclusiveHandling = 2

type ModelConversions with
    static member ToModel(value: WUApiLib.InstallationImpact) =
        match value with
        | WUApiLib.InstallationImpact.iiNormal -> InstallationImpact.Normal
        | WUApiLib.InstallationImpact.iiMinor -> InstallationImpact.Minor
        | WUApiLib.InstallationImpact.iiRequiresExclusiveHandling -> InstallationImpact.RequiresExclusiveHandling
        | _ -> value |> int |> enum
    static member ToCom(value: InstallationImpact) =
        match value with
        | InstallationImpact.Normal -> WUApiLib.InstallationImpact.iiNormal
        | InstallationImpact.Minor -> WUApiLib.InstallationImpact.iiMinor
        | InstallationImpact.RequiresExclusiveHandling -> WUApiLib.InstallationImpact.iiRequiresExclusiveHandling
        | _ -> value |> int |> enum

type InstallationRebootBehavior =
    | NeverReboots = 0
    | AlwaysRequiresReboot = 1
    | CanRequestReboot = 2

type ModelConversions with
    static member ToModel(value: WUApiLib.InstallationRebootBehavior) =
        match value with
        | WUApiLib.InstallationRebootBehavior.irbNeverReboots -> InstallationRebootBehavior.NeverReboots
        | WUApiLib.InstallationRebootBehavior.irbAlwaysRequiresReboot -> InstallationRebootBehavior.AlwaysRequiresReboot
        | WUApiLib.InstallationRebootBehavior.irbCanRequestReboot -> InstallationRebootBehavior.CanRequestReboot
        | _ -> value |> int |> enum
    static member ToCom(value: InstallationRebootBehavior) =
        match value with
        | InstallationRebootBehavior.NeverReboots -> WUApiLib.InstallationRebootBehavior.irbNeverReboots
        | InstallationRebootBehavior.AlwaysRequiresReboot -> WUApiLib.InstallationRebootBehavior.irbAlwaysRequiresReboot
        | InstallationRebootBehavior.CanRequestReboot -> WUApiLib.InstallationRebootBehavior.irbCanRequestReboot
        | _ -> value |> int |> enum

type InstallationBehavior =
    {
    [<Hidden>]
    Source: WUApiLib.IInstallationBehavior
    CanRequestUserInput: bool
    Impact: InstallationImpact
    RebootBehavior: InstallationRebootBehavior
    RequiresNetworkConnectivity: bool
    }

type ModelConversions with
    static member ToModel(value: WUApiLib.IInstallationBehavior) =
        {
            Source = value
            CanRequestUserInput = value.CanRequestUserInput
            Impact = value.Impact |> ModelConversions.ToModel
            RebootBehavior = value.RebootBehavior |> ModelConversions.ToModel
            RequiresNetworkConnectivity = value.RequiresNetworkConnectivity
        }
    static member ToCom(value: InstallationBehavior) =
        value.Source

type ImageInformation =
    {
    AltText: string
    Source: string
    Height: int
    Width: int
    }

type ModelConversions with
    static member ToModel (value: WUApiLib.IImageInformation) =
        {
            AltText = value.AltText
            Source = value.Source
            Height = value.Height
            Width = value.Width
        }
    static member ToCom (value: ImageInformation) =
        { new WUApiLib.IImageInformation with
            member _.AltText = value.AltText
            member _.Source = value.Source
            member _.Height = value.Height
            member _.Width = value.Width
        }


type DownloadPriority =
    | Low = 1
    | Normal = 2
    | High = 3
    | ExtraHigh = 4

type ModelConversions with
    static member ToModel (value: WUApiLib.DownloadPriority) =
        match value with
        | WUApiLib.DownloadPriority.dpLow -> DownloadPriority.Low
        | WUApiLib.DownloadPriority.dpNormal -> DownloadPriority.Normal
        | WUApiLib.DownloadPriority.dpHigh -> DownloadPriority.High
        | WUApiLib.DownloadPriority.dpExtraHigh -> DownloadPriority.ExtraHigh
        | _ -> value |> int |> enum
    static member ToCom (value: DownloadPriority) =
        value |> int |> enum

type UpdateIdentity =
    {
    UpdateId: string
    RevisionNumber: int
    }
type ModelConversions with
    static member ToModel (value: WUApiLib.IUpdateIdentity) =
        {
            UpdateId = value.UpdateID
            RevisionNumber = value.RevisionNumber
        }

type UpdateType =
    | Software = 1
    | Driver = 2

type ModelConversions with
    static member ToModel (value: WUApiLib.UpdateType) =
        match value with
        | WUApiLib.UpdateType.utSoftware -> UpdateType.Software
        | WUApiLib.UpdateType.utDriver -> UpdateType.Driver
        | _ -> value |> int |> enum

type DeploymentAction = 
    | None = 0
    | Installation = 1
    | Uninstallation = 2
    | Detection = 3
    | OptionalInstallation = 4

type ModelConversions with
    static member ToModel (value: WUApiLib.DeploymentAction) =
        match value with
        | WUApiLib.DeploymentAction.daNone -> DeploymentAction.None
        | WUApiLib.DeploymentAction.daInstallation -> DeploymentAction.Installation
        | WUApiLib.DeploymentAction.daUninstallation -> DeploymentAction.Uninstallation
        | WUApiLib.DeploymentAction.daDetection -> DeploymentAction.Detection
        | WUApiLib.DeploymentAction.daOptionalInstallation -> DeploymentAction.OptionalInstallation
        | _ -> value |> int |> enum
        
[<Struct>]
type MaybeSupported<'T> =
    | Unsupported of ComInterfaceRequired: string
    | Supported of Value: 'T
    override this.ToString() =
        match this with
        | Unsupported i -> sprintf "Requires %s implementation" i
        | Supported v -> string v
    member this.OptionalGetValue() =
        match this with
        | Supported v -> Some v
        | Unsupported _ -> None
    member this.TryGetValue(value: outref<_>) =
        match this with
        | Supported v -> value <- v; true
        | Unsupported _ -> false
    member this.GetValueOrDefault(arg: 'TArg, defaultThunk: System.Func<'TArg, 'T>) =
        match this with
        | Supported v -> v
        | Unsupported _ -> defaultThunk.Invoke arg
    member this.GetValueOrDefault(defaultThunk: System.Func<'T>) =
        this.GetValueOrDefault(defaultThunk, fun fn -> fn.Invoke ())
    member this.GetValueOrDefault(defaultValue: 'T) =
        this.GetValueOrDefault(defaultValue, id)
    member this.GetValueOrDefault() =
        this.GetValueOrDefault(Unchecked.defaultof<'T>)
    static member op_Explicit s =
        match s with
        | Supported value -> value
        | Unsupported requirement ->
            requirement
            |> sprintf "Cannot convert to the required type because the source does not implement %s."
            |> System.InvalidCastException
            |> raise
    static member op_Implicit s = Supported s

module MaybeSupported =
    let TryGet (fn: 'TDerived -> 'TReturn) (v: 'TSource): MaybeSupported<'TReturn> =
        match box v with
        | :? 'TDerived as d -> fn d |> Supported
        | _ -> Unsupported typeof<'TDerived>.Name

    [<CompiledName("Map")>]
    let map fn s =
        match s with
        | Supported v -> Supported <| fn v
        | Unsupported m -> Unsupported m

    [<CompiledName("Bind")>]
    let bind fn s =
        match s with
        | Supported v -> fn v
        | Unsupported m -> Unsupported m

type UpdateDownloadContent =
    {
    DownloadUrl: string
    IsDeltaCompressedContent: MaybeSupported<bool>
    }


type ModelConversions with
    static member ToModel (value: WUApiLib.IUpdateDownloadContent) =
        let isDeltaContentCompressed (value: WUApiLib.IUpdateDownloadContent2) =
            value.IsDeltaCompressedContent
        {
            DownloadUrl = value.DownloadUrl
            IsDeltaCompressedContent = value |> MaybeSupported.TryGet isDeltaContentCompressed
        }
    static member ToModel (value: WUApiLib.IUpdateDownloadContentCollection) =
        value |> ModelConversions.CollectionMap ModelConversions.ToModel

type Category =
    {
    Updates: ImmutableList<Update>
    CategoryId: string
    Children: ImmutableList<Category>
    Description: string
    Image: ImageInformation
    Name: string
    Order: int
    Parent: Category
    Type: string
    }
and Update =
    {
    [<Hidden>] Source: WUApiLib.IUpdate
    AutoSelectOnWebSites: bool
    BundledUpdates: ImmutableList<Update>
    CanRequireSource: bool
    Categories: ImmutableList<Category>
    Deadline: obj
    DeltaCompressedContentAvailable: bool
    DeltaCompressedContentPreferred: bool
    DeploymentAction: DeploymentAction
    Description: string
    DownloadContents: ImmutableList<UpdateDownloadContent>
    DownloadPriority: DownloadPriority
    EulaAccepted: bool
    EulaText: string
    HandlerID: string
    Identity: UpdateIdentity
    Image: ImageInformation
    InstallationBehavior: InstallationBehavior
    IsBeta: bool
    IsDownloaded: bool
    IsHidden: bool
    IsInstalled: bool
    IsMandatory: bool
    IsUninstallable: bool
    KBArticleIDs: ImmutableList<string>
    Languages: ImmutableList<string>
    LastDeploymentChangeTime: System.DateTime
    MaxDownloadSize: decimal
    MinDownloadSize: decimal
    MoreInfoUrls: ImmutableList<string>
    MsrcSeverity: string
    RecommendedCpuSpeed: int
    RecommendedHardDiskSpace: int
    RecommendedMemory: int
    ReleaseNotes: string
    SecurityBulletinIDs: ImmutableList<string>
    SupersededUpdateIDs: ImmutableList<string>
    SupportUrl: string
    Title: string
    Type: UpdateType
    UninstallationBehavior: InstallationBehavior
    UninstallationNotes: string
    UninstallationSteps: ImmutableList<string>
    }


type ModelConversions with
    static member ToModel(value: WUApiLib.ICategory) =
        {
            CategoryId = value.CategoryID
            Children = value.Children
                |> ModelConversions.CollectionMap ModelConversions.ToModel
            Updates = value.Updates
                |> ModelConversions.CollectionMap ModelConversions.ToModel
            Description = value.Description
            Image = value.Image |> ModelConversions.ToModel
            Name = value.Name
            Order = value.Order
            Parent = value.Parent |> ModelConversions.ToModel
            Type = value.Type
        }
    static member ToModel (value: WUApiLib.ICategoryCollection) =
        value |> ModelConversions.CollectionMap ModelConversions.ToModel
    static member ToModel(value: WUApiLib.IUpdate) =
        {
            Source = value
            AutoSelectOnWebSites = value.AutoSelectOnWebSites
            BundledUpdates = value.BundledUpdates |> ModelConversions.ToModel
            CanRequireSource = value.CanRequireSource
            Categories = value.Categories |> ModelConversions.ToModel
            Deadline = value.Deadline
            DeltaCompressedContentAvailable = value.DeltaCompressedContentAvailable
            DeltaCompressedContentPreferred = value.DeltaCompressedContentPreferred
            DeploymentAction = value.DeploymentAction |> ModelConversions.ToModel
            Description = value.Description
            DownloadContents = value.DownloadContents |> ModelConversions.ToModel
            DownloadPriority = value.DownloadPriority |> ModelConversions.ToModel
            EulaAccepted = value.EulaAccepted
            EulaText = value.EulaText
            HandlerID = value.HandlerID
            Identity = value.Identity |> ModelConversions.ToModel
            Image = value.Image |> ModelConversions.ToModel
            InstallationBehavior = value.InstallationBehavior |> ModelConversions.ToModel
            IsBeta = value.IsBeta
            IsDownloaded = value.IsDownloaded
            IsHidden = value.IsHidden
            IsInstalled = value.IsInstalled
            IsMandatory = value.IsMandatory
            IsUninstallable = value.IsUninstallable
            KBArticleIDs = value.KBArticleIDs |> ModelConversions.ToModel
            Languages = value.Languages |> ModelConversions.ToModel
            LastDeploymentChangeTime = value.LastDeploymentChangeTime
            MaxDownloadSize = value.MaxDownloadSize
            MinDownloadSize = value.MinDownloadSize
            MoreInfoUrls = value.MoreInfoUrls |> ModelConversions.ToModel
            MsrcSeverity = value.MsrcSeverity
            RecommendedCpuSpeed = value.RecommendedCpuSpeed
            RecommendedHardDiskSpace = value.RecommendedHardDiskSpace
            RecommendedMemory = value.RecommendedMemory
            ReleaseNotes = value.ReleaseNotes
            SecurityBulletinIDs = value.SecurityBulletinIDs |> ModelConversions.ToModel
            SupersededUpdateIDs = value.SupersededUpdateIDs |> ModelConversions.ToModel
            SupportUrl = value.SupportUrl
            Title = value.Title
            Type = value.Type |> ModelConversions.ToModel
            UninstallationBehavior = value.UninstallationBehavior |> ModelConversions.ToModel
            UninstallationNotes = value.UninstallationNotes
            UninstallationSteps = value.UninstallationSteps |> ModelConversions.ToModel
        }
    static member ToModel (value: WUApiLib.IUpdateCollection) =
        value |> ModelConversions.CollectionMap ModelConversions.ToModel
    static member ToModel (value: WUApiLib.IStringCollection) =
        value |> ModelConversions.CollectionMap id

type OperationResultCode =
    | NotStarted = 0
    | InProgress = 1
    | Succeeded = 2
    | SucceededWithErrors = 3
    | Failed = 4
    | Aborted = 5

type ModelConversions with
    static member ToModel(value: WUApiLib.OperationResultCode) =
        match value with
        | WUApiLib.OperationResultCode.orcNotStarted -> OperationResultCode.NotStarted
        | WUApiLib.OperationResultCode.orcInProgress -> OperationResultCode.InProgress
        | WUApiLib.OperationResultCode.orcSucceeded -> OperationResultCode.Succeeded
        | WUApiLib.OperationResultCode.orcSucceededWithErrors -> OperationResultCode.SucceededWithErrors
        | WUApiLib.OperationResultCode.orcFailed -> OperationResultCode.Failed
        | WUApiLib.OperationResultCode.orcAborted -> OperationResultCode.Aborted
        | _ -> value |> int |> enum
    static member ToCom(value: OperationResultCode) =
        match value with
        | OperationResultCode.NotStarted -> WUApiLib.OperationResultCode.orcNotStarted
        | OperationResultCode.InProgress -> WUApiLib.OperationResultCode.orcInProgress
        | OperationResultCode.Succeeded -> WUApiLib.OperationResultCode.orcSucceeded
        | OperationResultCode.SucceededWithErrors -> WUApiLib.OperationResultCode.orcSucceededWithErrors
        | OperationResultCode.Failed -> WUApiLib.OperationResultCode.orcFailed
        | OperationResultCode.Aborted -> WUApiLib.OperationResultCode.orcAborted
        | _ -> value |> int |> enum

type SearchResult =
    {
    ResultCode: OperationResultCode
    Updates: ImmutableList<Update>
    RootCategories: ImmutableList<Category>
    Warnings: ImmutableList<System.Runtime.InteropServices.COMException>
    }

type ModelConversions with
    static member ToModel(value: WUApiLib.IUpdateException) =
        let exn = new System.Runtime.InteropServices.COMException(value.Message, value.HResult)
        exn.Data.Add(nameof value.Context, value.Context)
        exn
    static member ToModel(value: WUApiLib.ISearchResult) =
        {
        ResultCode = value.ResultCode |> ModelConversions.ToModel
        Updates = value.Updates |> ModelConversions.CollectionMap ModelConversions.ToModel
        Warnings = value.Warnings |> ModelConversions.CollectionMap ModelConversions.ToModel
        RootCategories = value.RootCategories |> ModelConversions.CollectionMap ModelConversions.ToModel
        }


type UpdateDownloadResult =
    {
    HResult: int
    ResultCode: OperationResultCode
    }

type ModelConversions with

    static member ToModel (value: WUApiLib.IUpdateDownloadResult) =
        {
            HResult = value.HResult
            ResultCode = value.ResultCode |> ModelConversions.ToModel
        }

[<Struct>]
type UpdateOperation =
    | Installation
    | Uninstallation

    static member Parse str =
        match str with
        | (nameof Installation) -> Installation
        | (nameof Uninstallation) -> Uninstallation
        | null -> raise <| new System.ArgumentNullException(nameof str)
        | _ -> raise <| new System.ArgumentOutOfRangeException(nameof str, str, null)

    static member TryParse (str, v: _ outref) =
        match str with
        | (nameof Installation) -> v <- Installation; true
        | (nameof Uninstallation) -> v <- Uninstallation; true
        | _ -> false

type ModelConversions with
    static member ToModel op =
        match op with
        | WUApiLib.tagUpdateOperation.uoInstallation -> Installation
        | WUApiLib.tagUpdateOperation.uoUninstallation -> Uninstallation
        | _ -> raise <| new System.ArgumentOutOfRangeException(nameof op, op, null)

type UpdateHistory =
    {
    ClientApplicationId: string
    Date: System.DateTime
    Description: string
    HResult: int
    Operation: UpdateOperation
    ResultCode: OperationResultCode
    ServerSelection: ServerSelection
    ServiceId: string
    SupportUrl: string
    Title: string
    UninstallationNotes: string
    UninstallationSteps: ImmutableList<string>
    UnmappedResultCode: int
    UpdateIdentity: UpdateIdentity
    Categories: MaybeSupported<ImmutableList<Category>>
    }



type ModelConversions with
    static member ToModel (value: WUApiLib.IUpdateHistoryEntry) =
        let categories (value: WUApiLib.IUpdateHistoryEntry2) =
            value.Categories
        {
        ClientApplicationId = value.ClientApplicationID
        Date = value.Date
        Description = value.Description
        HResult = value.HResult
        Operation = value.Operation |> ModelConversions.ToModel
        ResultCode = value.ResultCode |> ModelConversions.ToModel
        ServerSelection = value.ServerSelection |> ModelConversions.ToModel
        ServiceId = value.ServiceID
        SupportUrl = value.SupportUrl
        Title = value.Title
        UninstallationNotes = value.UninstallationNotes
        UninstallationSteps = value.UninstallationSteps |> ModelConversions.CollectionMap id
        UnmappedResultCode = value.UnmappedResultCode
        UpdateIdentity = value.UpdateIdentity |> ModelConversions.ToModel
        Categories = MaybeSupported.TryGet categories value |> MaybeSupported.map ModelConversions.ToModel
        }

    static member ToModel (value: WUApiLib.IUpdateHistoryEntryCollection) =
        value |> ModelConversions.CollectionMap ModelConversions.ToModel

type UpdateService =
    {
    CanRegisterWithAU: bool
    ContentValidationCert: obj
    ExpirationDate: System.DateTime
    IsManaged: bool
    IsRegisteredWithAU: bool
    IsScanPackageService: bool
    IssueDate: System.DateTime
    Name: string
    OffersWindowsUpdates: bool
    RedirectUrls: ImmutableList<string>
    ServiceId: string
    ServiceUrl: string
    SetupPrefix: string
    IsDefaultAuService: MaybeSupported<bool>
    }

type ModelConversions with
    static member ToModel (value: WUApiLib.IUpdateService) =
        let isDefaultAuService (value: WUApiLib.IUpdateService2) =
            value.IsDefaultAUService
        {
            CanRegisterWithAU = value.CanRegisterWithAU
            ContentValidationCert = value.ContentValidationCert
            ExpirationDate = value.ExpirationDate
            IsManaged = value.IsManaged
            IsRegisteredWithAU = value.IsRegisteredWithAU
            IsScanPackageService = value.IsScanPackageService
            IssueDate = value.IssueDate
            Name = value.Name
            OffersWindowsUpdates = value.OffersWindowsUpdates
            RedirectUrls = value.RedirectUrls |> ModelConversions.ToModel
            ServiceId = value.ServiceID
            ServiceUrl = value.ServiceUrl
            SetupPrefix = value.SetupPrefix
            IsDefaultAuService = MaybeSupported.TryGet isDefaultAuService value
        }

    static member ToModel (value: WUApiLib.IUpdateServiceCollection) =
        value |> ModelConversions.CollectionMap ModelConversions.ToModel