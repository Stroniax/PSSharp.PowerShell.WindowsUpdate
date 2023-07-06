namespace PSSharp.PowerShell.WindowsUpdate.Commands

module WindowsUpdateErrors =
    open System.Management.Automation
    open WUApiLib

    type WUA_AU_ERROR_CODES =
        | WU_E_AUCLIENT_UNEXPECTED = 0x80243FFF
        | WU_E_AU_NOSERVICE = 0x8024A000
        | WU_E_AU_NONLEGACYSERVER = 0x8024A002
        | WU_E_AU_LEGACYCLIENTDISABLED = 0x8024A003
        | WU_E_AU_PAUSED = 0x8024A004
        | WU_E_AU_NO_REGISTERED_SERVICE = 0x8024A005
        | WU_E_AU_UNEXPECTED = 0x8024AFFF

    type WUA_UI_ERROR_CODES =
        | WU_E_INSTALLATION_RESULTS_UNKNOWN_VERSION = 0x80243001
        | WU_E_INSTALLATION_RESULTS_INVALID_DATA = 0x80243002
        | WU_E_INSTALLATION_RESULTS_NOT_FOUND = 0x80243003
        | WU_E_TRAYICON_FAILURE = 0x80243004
        | WU_E_NON_UI_MODE = 0x80243FFD
        | WU_E_WUCLTUI_UNSUPPORTED_VERSION = 0x80243FFE
        | WU_E_AUCLIENT_UNEXPECTED = 0x80243FFF
        | WU_E_SERVICEPROP_NOTAVAIL = 0x8024043D

    type WUA_INVENTORY_ERROR_CODES =
        | WU_E_INVENTORY_PARSEFAILED = 0x80249001
        | WU_E_INVENTORY_GET_INVENTORY_TYPE_FAILED = 0x80249002
        | WU_E_INVENTORY_RESULT_UPLOAD_FAILED = 0x80249003
        | WU_E_INVENTORY_UNEXPECTED = 0x80249004
        | WU_E_INVENTORY_WMI_ERROR = 0x80249005

    //type WUA_EXPRESSION_EVALUATOR_ERRORS =
    // ... https://learn.microsoft.com/en-us/windows/deployment/update/windows-update-error-reference

    let fromUpdateExn (api: WUApiLib.IUpdateException) =
        let exn = new System.Runtime.InteropServices.COMException(api.Message, api.HResult)
        exn.Data.Item (nameof api.Context) <- api.Context
        exn
