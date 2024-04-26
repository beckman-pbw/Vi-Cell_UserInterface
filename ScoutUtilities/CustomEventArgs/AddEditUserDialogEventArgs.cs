namespace ScoutUtilities.CustomEventArgs
{
    public class AddEditUserDialogEventArgs<T> : BaseDialogEventArgs
    {
        public bool AddNewUser { get; set; }
        public T User { get; set; }

        public AddEditUserDialogEventArgs()
        {
            SizeToContent = true;
            AddNewUser = true;
            User = default(T);
        }

        public AddEditUserDialogEventArgs(T user)
        {
            SizeToContent = true;
            AddNewUser = user == null;
            User = user;
        }
    }
}