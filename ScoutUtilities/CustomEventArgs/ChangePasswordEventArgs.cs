namespace ScoutUtilities.CustomEventArgs
{
    public class ChangePasswordEventArgs : BaseDialogEventArgs
    {
        public string Username { get; set; }
        public bool UserMustChangePassword { get; set; }

        public ChangePasswordEventArgs(string username, string userIconPath = null, bool userMustChangePassword = false)
        {
            Username = username;
            DialogIconPath = userIconPath;
            UserMustChangePassword = userMustChangePassword;

            SizeToContent = true;
        }
    }
}
