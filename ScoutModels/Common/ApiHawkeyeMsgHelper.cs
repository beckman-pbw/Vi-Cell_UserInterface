using log4net;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;

namespace ScoutModels.Common
{
    public class ApiHawkeyeMsgHelper
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void ErrorCreateQualityControl(HawkeyeError result)
        {
            if (result.Equals(HawkeyeError.eAlreadyExists))
            {
                DisplayMessage(LanguageResourceHelper.Get("LID_ErrorContent_QualityControlAlreadyExists"));
            }
            if (result.Equals(HawkeyeError.eEntryInvalid))
            {
                DisplayMessage(LanguageResourceHelper.Get("LID_QCNameSameAsCellTypeName"));
            }
            else
            {
                ErrorCommon(result);
            }
        }

        public static void ErrorCreateSignature(HawkeyeError result, string signName)
        {
            if (result.Equals(HawkeyeError.eAlreadyExists))
            {
                DisplayMessage(LanguageResourceHelper.Get("LID_ErrorContent_SignatureAlreadyExists"));
            }
            else
            {
                ErrorCommon(result);
            }
        }

        public static void ErrorValidate(HawkeyeError result)
        {
            switch (result)
            {
                case HawkeyeError.eInvalidArgs:
                    DisplayMessage(LanguageResourceHelper.Get("LID_MSGBOX_InvalidArgs"));
                    break;
                case HawkeyeError.eValidationFailed:
                    DisplayMessage(LanguageResourceHelper.Get("LID_MSGBOX_ValidationFailed"));
                    break;
                default:
                    ErrorCommon(result);
                    break;
            }
        }

  
        public static void ErrorCreateUser(HawkeyeError result)
        {
            if (result.Equals(HawkeyeError.eAlreadyExists))
                DisplayMessage(LanguageResourceHelper.Get("LID_ERRMSGBOX_UserIdAlreadyExist"));
            else
                ErrorCommon(result);
        }

  
        public static void ErrorCreateCellType(HawkeyeError result)
        {
            if (result.Equals(HawkeyeError.eAlreadyExists))
                DisplayMessage(LanguageResourceHelper.Get("LID_ERRMSGBOX_ChooseADifferent_Name"));
            else
                ErrorCommon(result);
        }

  
        public static void ErrorResetPassword(HawkeyeError result)
        {
            if (result.Equals(HawkeyeError.eValidationFailed))
               DisplayMessage(LanguageResourceHelper.Get("LID_ERRMSGBOX_OldPwdSameNw"));
            else
                ErrorCommon(result);
        }

        public static void WrongPassword(HawkeyeError result)
        {
            if (result.Equals(HawkeyeError.eValidationFailed))
                DisplayMessage(LanguageResourceHelper.Get("LID_ERRMSGBOX_WrongPWD"));
            else
                ErrorCommon(result);
        }

  
        public static void ErrorMyPassword(HawkeyeError result)
        {
            if (result.Equals(HawkeyeError.eValidationFailed))
                DisplayMessage(LanguageResourceHelper.Get("LID_ERRMSGBOX_WrongOldPassword"));
            else
                ErrorCommon(result);
        }

   
        public static void ErrorLogin(HawkeyeError result)
        {
            if (result.Equals(HawkeyeError.eInvalidArgs))
                DisplayMessage(LanguageResourceHelper.Get("LID_ERRMSGBOX_InvalidUserId"));
            else
                ErrorCommon(result);
        }

  
        public static void ErrorInvalid(HawkeyeError result, string prefixMessage = null)
        {
            if (result.Equals(HawkeyeError.eInvalidArgs))
                DisplayMessage(LanguageResourceHelper.Get("LID_MSGBOX_InvalidArgs"));
            else
                ErrorCommon(result, prefixMessage);
        }

     
        public static void ErrorValidateMe(HawkeyeError result)
        {
            if (result.Equals(HawkeyeError.eValidationFailed))
                DisplayMessage(LanguageResourceHelper.Get("LID_ERRMSGBOX_InvalidUserId"));
            else
                ErrorCommon(result);
        }

        public static void ValidationError(HawkeyeError result)
        {
            if (result.Equals(HawkeyeError.eValidationFailed))
                DisplayMessage(LanguageResourceHelper.Get("LID_MSGBOX_ValidationFailed"));
            else
                ErrorCommon(result);
        }

        public static void ErrorCommon(HawkeyeError result, string prefixMessage = null, bool showDialogPrompt = true)
        {
            if (result == HawkeyeError.eSuccess) return;

            // For clarity the order of these case statements must match the order
            // in the HawkeyeError enumeration.

            var message = GetHawkeyeErrorMessage(result);

            if (!string.IsNullOrEmpty(prefixMessage))
            {
                message = $"{prefixMessage}: {message}";
            }
            
            DisplayMessage(message, showDialogPrompt);
        }

        public static string GetHawkeyeErrorMessage(HawkeyeError result)
        {
            string message;
            switch (result)
            {
                case HawkeyeError.eInvalidArgs:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_InvalidArgs");
                    break;
                case HawkeyeError.eNotPermittedByUser:
                    message = LanguageResourceHelper.Get("LID_ERRMSGBOX_eNotPermittedByUser");
                    break;
                case HawkeyeError.eNotPermittedAtThisTime:
                    message = LanguageResourceHelper.Get("LID_ERRMSGBOX_NotPermittedAtThisTime");
                    break;
                case HawkeyeError.eAlreadyExists:
                    message = LanguageResourceHelper.Get("LID_ERRMSGBOX_AlreadyExists");
                    break;
                case HawkeyeError.eIdle:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_Idel");
                    break;
                case HawkeyeError.eBusy:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_Busy");
                    break;
                case HawkeyeError.eTimedout:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_TimedOut");
                    break;
                case HawkeyeError.eHardwareFault:
                    message = LanguageResourceHelper.Get("LID_ERRMSGBOX_HardwareFault");
                    break;
                default:
                case HawkeyeError.eSoftwareFault:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_SoftwareFault");
                    break;
                case HawkeyeError.eValidationFailed:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_ValidationFailed");
                    break;
                case HawkeyeError.eEntryNotFound:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_EntryNotFound");
                    break;
                case HawkeyeError.eNotSupported:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_NotSupported");
                    break;
                case HawkeyeError.eNoneFound:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_NoneFound");
                    break;
                case HawkeyeError.eEntryInvalid:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_EntryInvalid");
                    break;
                case HawkeyeError.eStorageFault:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_StorageFault");
                    break;
                case HawkeyeError.eStageNotRegistered:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_HardwareNotIntitalized");
                    break;
                case HawkeyeError.ePlateNotFound:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_PlateNotFound");
                    break;
                case HawkeyeError.eCarouselNotFound:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_CarouselNotFound");
                    break;
                case HawkeyeError.eLowDiskSpace:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_LowDiskSpace");
                    break;
                case HawkeyeError.eReagentError:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_ReagentError");
                    break;
                case HawkeyeError.eSpentTubeTray:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_SpentTubeTray");
                    break;
                case HawkeyeError.eDatabaseError:
                    message = LanguageResourceHelper.Get("LID_MSGBOX_DatabaseError");
                    break;
            }

            return message;
        }

        public static void CustomDialogMessage(string msg, bool showPrompt)
        {
            if (showPrompt)
                DialogEventBus.DialogBoxOk(null, msg);
        }

         public static void PublishHubMessage(string msg, MessageType messageType = MessageType.Warning)
        {
            if (messageType != MessageType.Normal)
                Log.Warn(msg);
            MessageBus.Default.Publish(new SystemMessageDomain
            {
                IsMessageActive = true,
                Message = msg,
                MessageType = messageType,
            });
        }

        private static void DisplayMessage(string message, bool showPrompt = true)
        {
            if (showPrompt)
                DialogEventBus.DialogBoxOk(null, message);
            PublishHubMessage(message);
        }
    }
}
