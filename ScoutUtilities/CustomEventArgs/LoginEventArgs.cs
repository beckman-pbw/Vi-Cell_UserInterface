using ScoutUtilities.Enums;
using ScoutUtilities.CustomEventArgs;

namespace ScoutUtilities.CustomEventArgs
{
    public enum LoginState
    {
        AdminUnlock,
        LockScreen,
        PasswordReset,
        ValidateServiceUser,
        ValidateCurrentUserOnly,
        SecuritySettingsUpdate,
        SecurityChangeToLocal,
        SecurityChangeToNone,
        SecurityChangeToAD,
        AutomationLock,
    }

    public enum LoginResult
    {
        None,
        Cancelled,
        UserHasBeenDisabled,

        // for the lock dialog
        CurrentUserLoginSuccess,
        CurrentUserLoginFailed,
        SwapUserLoginSuccess,
        SwapUserLoginFailed,
        AdminUnlockSuccess,
        AdminUnlockFailed,
        AdminValidSuccess,
        AdminValidFailed,

        // for the SignIn page
        NormalLoginSuccess,
        NormalLoginFailed,

    }

    public class LoginEventArgs : BaseDialogEventArgs
    {

        public string DisplayedUsername { get; set; } // the username to auto-populate in the textbox
        public string CurrentLoggedInUsername { get; set; }
        public LoginState LoginState { get; set; }
        public LoginResult LoginResult { get; set; }
        public bool ReturnOnFirstFailure { get; set; }
        public bool ReturnPassword { get; set; }
        public string Password { get; set; } // this is required to make certain API calls
        public AdCfgValidation ActiveDirValArgs { get; set; }
        public string Message { get; set; }

        public LoginEventArgs(string usernameToDisplay, string currentUsername, LoginState loginState, DialogLocation dialogLocation = DialogLocation.TopCenterScreen, 
            bool returnOnFirstFailure = false, bool returnPassword = false, AdCfgValidation valArgs = null, string message = null)
            : base(true, dialogLocation)
        {
            DisplayedUsername = usernameToDisplay;
            CurrentLoggedInUsername = currentUsername;
            LoginState = loginState;
            DialogResult = null;
            LoginResult = LoginResult.None;
            DialogLocation = dialogLocation;
            ReturnOnFirstFailure = returnOnFirstFailure;
            ReturnPassword = returnPassword;
            Password = string.Empty;
            if (valArgs != null)
            {
                ActiveDirValArgs = valArgs;
            }
            Message = message;
        }
    }
}
