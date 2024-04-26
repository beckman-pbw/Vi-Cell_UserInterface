using ScoutUtilities.Enums;

namespace ScoutUtilities.CustomEventArgs
{
    public class EditSecuritySettingsDialogEventArgs : BaseDialogEventArgs
    {
        public SecurityType SelectedSecurityType { get; set; }
        public int InactivityTimeoutMinutes { get; set; }
        public int PasswordChangeDays { get; set; }

        public EditSecuritySettingsDialogEventArgs(SecurityType securityType, int inactivityTimeout, int passwordChangeDays)
        {
            SizeToContent = true;
            SelectedSecurityType = securityType;
            InactivityTimeoutMinutes = inactivityTimeout;
            PasswordChangeDays = passwordChangeDays;
        }
    }
}