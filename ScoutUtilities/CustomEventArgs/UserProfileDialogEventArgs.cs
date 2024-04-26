namespace ScoutUtilities.CustomEventArgs
{
    public class UserProfileDialogEventArgs : BaseDialogEventArgs
    {
        public string Username { get; set; }

        public UserProfileDialogEventArgs(string username)
        {
            Username = username;
            SizeToContent = true;
        }
    }
}
