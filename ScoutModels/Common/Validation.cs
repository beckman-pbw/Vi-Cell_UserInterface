using System;
using log4net;
using ScoutLanguageResources;
using ScoutUtilities.Common;
using ScoutUtilities.Events;
using System.Linq;
using System.Text.RegularExpressions;
using HawkeyeCoreAPI;
using HawkeyeCoreAPI.Interfaces;
using ScoutModels.Settings;
using ScoutUtilities;

namespace ScoutModels.Common
{
    public class Validation
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool IsStrongPassword(string newPassword)
        {
            // languages (Chinese, Japanese) don't have concept of upper and lower case letters, so don't check for it
            // "\p{Lo}: a letter or ideograph that does not have lowercase and uppercase variants."
            bool noUpperLowerCase = Regex.IsMatch(newPassword, @"[\p{Lo}]");

            bool longEnough = newPassword.Length >= ApplicationConstants.MinimumPasswordLength;
            bool hasUpperCase = noUpperLowerCase || newPassword.Any(char.IsUpper);
            bool hasLowerCase = noUpperLowerCase || newPassword.Any(char.IsLower);
            bool hasDigit = newPassword.Any(char.IsDigit);
            bool hasSpecialChar = Regex.IsMatch(newPassword, @"[^\p{L}\p{N}]");

            bool isValid = longEnough && hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
            if (!isValid)
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_PasswordRange"));
                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_PasswordRange"));
            }
            return isValid;
        }

        public static bool ValidateChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_OldPasswordBlank"));

                Log.Info(LanguageResourceHelper.Get("LID_ERRMSGBOX_OldPasswordBlank"));
                return false;
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_NewPasswordBlank"));

                Log.Info(LanguageResourceHelper.Get("LID_ERRMSGBOX_NewPasswordBlank"));
                return false;
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_ConfirmPasswordBlank"));

                Log.Info(LanguageResourceHelper.Get("LID_ERRMSGBOX_ConfirmPasswordBlank"));
                return false;
            }

            if (!newPassword.Equals(confirmPassword))
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_MSGBOX_PasswordMismatch"));
                Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_PasswordMismatch"));
                return false;
            }

            return IsStrongPassword(newPassword);
        }

        public static bool StrongPasswordInvalidate(string newPassword)
        {
            // languages (Chinese, Japanese) don't have concept of upper and lower case letters, so don't check for it
            // "\p{Lo}: a letter or ideograph that does not have lowercase and uppercase variants."
            var noUpperLowerCase = Regex.IsMatch(newPassword, @"[\p{Lo}]");
            var containsSpecialChar = Regex.IsMatch(newPassword, @"[^\p{L}\p{N}]");
            var hasUpperCase = noUpperLowerCase || newPassword.Any(char.IsUpper);
            var hasLowerCase = noUpperLowerCase || newPassword.Any(char.IsLower);
            
            var isValid = (newPassword.Length >= ApplicationConstants.MinimumPasswordLength && hasUpperCase && hasLowerCase && newPassword.Any(char.IsDigit) && containsSpecialChar);
            
            if (!isValid)
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_PasswordRange"));
                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_PasswordRange"));
            }

            return isValid;
        }

        public static bool UserPasswordValidation(string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_PasswordBlank"));

                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_PasswordBlank"));
                return false;
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_ConfirmPasswordBlank"));

                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_ConfirmPasswordBlank"));
                return false;
            }

            if (password.Equals(confirmPassword))
            {
                return true;
            }

            DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_MSGBOX_PasswordMismatch"));

            Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_PasswordMismatch"));
            return false;
        }

        public static bool TextBoxLength(string userId, string displayName)
        {
            if (userId.Length > 100)
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_UserIdRange"));

                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_UserIdRange"));
                return false;
            }

            if (displayName != null && displayName.Length > 15)
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_DisplayNameRange"));

                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_DisplayNameRange"));
                return false;
            }

            return true;
        }

        public static bool EmailValidation(string emailAddress)
        {
            var smtpModel = new SmtpSettingsModel();
            if (!smtpModel.IsValidEmail(emailAddress))
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_InvalidEmailAddress"));

                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_InvalidEmailAddress"));
                return false;
            }

            return true;
        }


        public static bool SettingFileName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_FilenameBlank"));
                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_FilenameBlank"));
                return false;
            }            
            if (!FileSystem.IsFileNameValid(value))
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_MSGBOX_EntryInvalid"));
                Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_EntryInvalid"));
                return false;
            }
            return true;
        }

        public static bool SettingNoOfImage(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_NoOfImagesBlank"));
                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_NoOfImagesBlank"));
                return false;
            }

            if (Convert.ToInt32(value) >= ApplicationConstants.MinimumNumberOfNthImages &&
                Convert.ToInt32(value) <= ApplicationConstants.MaximumNumberOfNthImages)
            {
                return true;
            }

            string message = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_NoOfImagesLimit"),
                Misc.ConvertToString(ApplicationConstants.MinimumNumberOfNthImages),
                Misc.ConvertToString(ApplicationConstants.MaximumNumberOfNthImages));

            DialogEventBus.DialogBoxOk(null, message);
            Log.Debug(message);
            return false;
        }

        public static bool OnDateValidate(DateTime fromDate, DateTime toDate)
        {
            if (fromDate.Date > toDate.Date)
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_FromDate"));

                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_FromDate"));
                return false;
            }

            return true;
        }

    }
}
