namespace PSSharp.PowerShell.WindowsUpdate

/// Models for WindowsUpdate com objects and/or enums.
module Models =
    open System
    /// Maps to WUApiLib.DeploymentAction
    type DeploymentAction =
    | None = 0
    | Installation = 1
    | Uninstallation = 2
    | Detection = 3
    | OptionalInstallation = 4

    /// Maps to WUApiLib.ServerSelection
    type ServerSelection =
    /// <summary>
    /// Used only by <see cref="WUApiLib.IUpdateSearcher"/>. Indicates that the search call should search the
    /// default server.
    /// <para>
    /// The default server used by the Windwos Update Agent (WUA) is the same as <see cref="ManagedServer"/>
    /// if the comptuer is set up to have a managed server. If the computer is not been set up to have a managed
    /// server, WUA uses the first update service for which the <see cref="WUApiLib.IUpdateService.IsRegisteredWithAU"/>
    /// ise <see langword="true"/> and the <see cref="WUApiLib.IUpdateService.IsManaged"/> is <see langword="false"/>.
    /// </para>
    /// </summary>
    | Default = 0
    /// Indicates the managed server, in an environment that uses Windows Server Update Services or a similar corporate
    /// update server to manage the computer.
    | ManagedServer = 1
    /// Indicates the Windows Update service.
    | WindowsUpdate = 2
    /// Indicates some update service other than those listed previously
    | Others = 3

    /// Maps to WSApiLib.UpdateType
    type UpdateType =
    | Software = 1
    | Driver = 2

    type Update =
        {
            UpdateId: Guid
            RevisionNumber: int
            AutoDownload: WUApiLib.AutoDownloadMode
            AutoSelection: WUApiLib.AutoSelectionMode
            AutoSelectOnWebSites: bool
            BrowseOnly: bool
            BundledUpdates: Update list
            CanRequireSource: bool
            Categories: UpdateCategory list
            CveIds: string list
            Deadline: obj
            DeltaCompressedContentAvailable: bool
            DeltaCompressedContentPreferred: bool
            DeploymentAction: DeploymentAction
            Description: string
            DownloadContents: WUApiLib.IUpdateDownloadContentCollection
            DownloadPriority: WUApiLib.DownloadPriority
        }
        static member OfComObject(com: WUApiLib.IUpdate) =
            com.
            ()
    and UpdateCategory =
        {
            CategoryId: Guid
            Children : UpdateCategory list
            Description: string
            Name: string
            Order: int
            Parent: UpdateCategory
            Type: string
            Updates: Update list
        }
        static member OfComObject(com: WUApiLib.ICategory) =
            {
                CategoryId = com.CategoryID |> Guid.Parse
                Children = com.Children |> Seq.cast |> Seq.map UpdateCategory.OfComObject |> Seq.toList
                Description = com.Description
                Name = com.Name
                Order = com.Order
                Parent = com.Parent |> UpdateCategory.OfComObject
                Type = com.Type
                Updates = com.Updates |> Seq.cast |> Seq.map Update.OfComObject |> Seq.toList
            }
    type UpdateHistory =
        {
        Operation: WUApiLib.tagUpdateOperation
        ResultCode: WUApiLib.OperationResultCode
        HResult: int
        Date: DateTime
        UpdateId: Guid
        RevisionNumber: int
        Title: string
        Description: string
        UnmappedResultCode: int
        ClientApplicationId: string
        ServerSelection: ServerSelection
        ServiceId: Guid
        UninstallationSteps: string list
        UninstallationNotes: string
        SupportUrl: string
        Categories: UpdateCategory list
        }
        static member OfComObject(com: WUApiLib.IUpdateHistoryEntry) =
            {
                Operation = com.Operation
                ResultCode = com.ResultCode
                HResult = com.HResult
                Date = com.Date
                UpdateId = Guid.Parse com.UpdateIdentity.UpdateID
                RevisionNumber = com.UpdateIdentity.RevisionNumber
                Title = com.Title
                Description = com.Description
                UnmappedResultCode = com.UnmappedResultCode
                ClientApplicationId = com.ClientApplicationID
                ServerSelection = com.ServerSelection |> int |> enum
                ServiceId = com.ServiceID |> Guid.Parse
                UninstallationSteps = com.UninstallationSteps |> Seq.cast |> Seq.toList
                UninstallationNotes = com.UninstallationNotes
                SupportUrl = com.SupportUrl
                Categories = List.empty
            }