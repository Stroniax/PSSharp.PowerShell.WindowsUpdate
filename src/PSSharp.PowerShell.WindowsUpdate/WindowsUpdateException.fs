namespace PSSharp.PowerShell.WindowsUpdate
open System

module WindowsUpdateApiException =
    type ErrorId =
    | WU_E_INVALID_CRITERIA = 0x80240032
    | WU_E_LEGACYSERVER = 0x8024002B

type WindowsUpdateApiException =
    inherit Exception

    val private Context: WUApiLib.UpdateExceptionContext

    new() = { inherit Exception(); Context = WUApiLib.UpdateExceptionContext.uecGeneral }
    new(message) = { inherit Exception(message); Context = WUApiLib.UpdateExceptionContext.uecGeneral }
    new(message, inner: Exception) = { inherit Exception(message, inner); Context = WUApiLib.UpdateExceptionContext.uecGeneral }
    new(message, context: WUApiLib.UpdateExceptionContext) = { inherit Exception(message); Context = context }
    new(message, inner: Exception, context: WUApiLib.UpdateExceptionContext) = { inherit Exception(message, inner); Context = context }
    new(info, context: System.Runtime.Serialization.StreamingContext) = { inherit Exception(info, context); Context = WUApiLib.UpdateExceptionContext.uecGeneral }