#Requires -RunAsAdministrator
# Requires -PSEdition Desktop
#Requires -Version 5.1

$Agent = New-Object -ComObject Microsoft.Update.AgentInfo
# GetInfo ('ProductVersionString' | 'ApiMajorVersion' | 'ApiMinorVersion')
$AutoUpdate = New-Object -ComObject Microsoft.Update.AutoUpdate
# DetectNow()
# EnableService()
# Pause()
# Resume()
# ShowSettingsDialog()
# Results {get;}
# - [string/DateTime] LastInstallationSuccessDate {get;}
# - [string/DateTime] LastSearchSuccessDate {get;}
# ServiceEnabled {get;}
# Settings {get;}
# - [bool] CheckPermission([AutomaticUpdatesUserType] userType, [AutomaticUpdatesPermissionType] permissionType)
# - [void] Refresh()
# - [void] Save()
# - [AutomaticUpdatesNotificationLevel] NotificationLevel {get; set;}
# - [bool] ReadOnly {get;}
# - [bool] Required {get;}
# - [AutomaticUpdatesScheduledInstallationDay] ScheduledInstallationDay {get; set;}
# - [int] ScheduledInstallationTime {get; set;}
# - [bool] IncludeRecommendedUpdates {get; set;}
# - [bool] NonAdministratorsElevated {get; set;}
# - [bool] FeatureUpdatesEnabled {get; set;}
$Downloader = New-Object -ComObject Microsoft.Update.Downloader
# [IDownloadJob] BeginDownload(IUnknown onProgressChanged, IUnknown onCompleted, Variant state)
# [IDownloadResult] Download()
# [IDownloadResult] EndDownload([IDownloadJob] value)
# [string] ClientApplicationId {get; set;}
# [bool] IsForced {get; set;}
# [DownloadPriority] Priority {get; set;}
# [IUpdateCollection] Updates {get; set;}
$InstallationAgent = New-Object -ComObject Microsoft.Update.InstallationAgent
# [void] RecordInstallationREsult([string] installationResultCookie, [int] HResult, [IStringCollection] extendedReportingData)
$Installer = New-Object -ComObject Microsoft.Update.Installer
# [IInstallationJob] BeginInstall ([IUnknown] onProgressChanged, [IUnknown] onCompleted, [Variant] state)
# [IInstallationJob] BeginUninstall ([IUnknown] onProgressChanged, [IUnknown] onCompleted, [Variant] state)
# [void] Commit ([uint] dwFlags)
# [IInstallationResult] EndInstall ([IInstallationJob] value)
# [IInstallationResult] EndUninstall ([IInstallationJob] value)
# [IInstallationResult] Install ()
# [IInstallationResult] RunWizard ([string] dialogTitle)
# [IInstallationResult] Uninstall ()
# [bool] AllowSourcePrompts {get; set;}
# [bool] AttemptCloseAppsIfNecessary {get; set;}
# [string] ClientApplicationId {get; set;}
# [bool] ForceQuiet {get; set;}
# [bool] IsBusy {get;}
# [bool] IsForced {get; set;}
# [IUnknown] parentWindow {get; set;}
# [bool] RebootRequiredBeforeInstallation {get;}
# [IUpdateColleciton] Updates {get; set;}
$Searcher = New-Object -ComObject Microsoft.Update.Searcher
# [ISearchJob] BeginSearch ([string] criteria, [IUnknown] onCompleted, [Variant] state)
# [ISearchResult] EndSearch ([ISearchJob] searchJob)
# [string] EscapeString ([string] unescaped)
# [int] GetTotalHistoryCount ()
# [IUpdateHistoryEntryCollection] QueryHistory ([int] startIndex, [int] count)
# [ISearchResult] Search ([string] criteria)
# [bool] CanAutomaticallyUpgradeService {get; set;}
# [string] ClientApplicationId {get; set;}
# [bool] IgnoreDownloadPriority {get; set;}
# [bool] IncludePotentiallySupersededUpdates {get; set;}
# [bool] Online {get; set;}
# [SearchScope] SearchScope {get; set;}
# [ServerSelection] ServerSelection {get; set;}
# [string] ServiceID {get; set;}
$ServiceManager = New-Object -ComObject Microsoft.Update.ServiceManager
# AddScanPackageService
# AddService
# AddService2
# QueryServiceRegistration
# RegisterServiceWithAU
# RemoveService
# SetOption
# UnregisterServiceWIthAU
# ClientApplicationId
# Services
$Session = New-Object -ComObject Microsoft.Update.Session
# [IUpdateDownloader] CreateupdateDownloader()
# [IUpdateInstaller] CreateUpdateInstaller()
# [IUpdateSearcher] CreateUpdateSearcher()
# [IUpdateSErviceManager2] CreateUpdateServiceManager()
# [IUpdateHistoryEntryCollection] QueryHistory([string] criteria, [int] startIndex, [int] count)
# [string] ClientApplicationId {get; set;}
# [bool] ReadOnly {get;}
# [uint] UserLocale {get; set;}
# [IWebProxy] WebProxy {get; set;}

# $StringColl = New-Object -ComObject Microsoft.Update.StringColl
# [int] Add ([string] value)
# [void] Clear ()
# [IStringCollection] Copy()
# [void] Insert([int] index, [string] value)
# [void] RemoveAt([int] index)
# Item
# [int] Count {get;}
# [bool] ReadOnly {get;}
# _NewEnum {get;}
$SystemInfo = New-Object -ComObject Microsoft.Update.SystemInfo
# [string] OemHardwareSupportLink {get;}
# [bool] RebootRequired {get;}

# $UpdateColl = New-Object -ComObject Microsoft.Update.UpdateColl
# [int] Add ([IUpdate] value)
# [void] Clear ()
# [IStringCollection] Copy()
# [void] Insert([int] index, [IUpdate] value)
# [void] RemoveAt([int] index)
# Item
# [int] Count {get;}
# [bool] ReadOnly {get;}
# _NewEnum {get;}

$WebProxy = New-Object -ComObject Microsoft.Update.WebProxy
# [void] PromptForCredentials ([IUnknown] parentWindow, [string] title)
# [void] SetPassword ([string] value)
# [string] Address {get; set;}
# [bool] AutoDetect {get; set;}
# [IStringCollection] BypassList {get; set;}
# [bool] BypassProxyOnLocal {get; set;}
# [bool] ReadOnly {get;}
# [string] UserName {get; set;}

function Get-WindowsUpdateAgentVersionInfo {
    process {
        $Version = $Agent.GetInfo('ProductVersionString')
        $ApiMajor = $Agent.GetInfo('ApiMajorVersion')
        $ApiMinor = $Agent.GetInfo('ApiMinorVersion')
        [PSCustomObject]@{
            PSTypeName      = 'PSSharp.PowerShell.WindowsUpdate.AgentVersionInfo'
            ProductVersion = [Version]$Version
            ApiVersion = [Version]::new($ApiMajor, $ApiMinor)
        }
    }
}