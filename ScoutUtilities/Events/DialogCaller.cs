using ScoutUtilities.CustomEventArgs;

namespace ScoutUtilities.Events
{
    public class DialogCaller : IDialogCaller
    {
        public bool? DialogBoxOkCancel(object caller, string message, string title = null)
        {
            var result = DialogEventBus.DialogBoxOkCancel(caller, message, title);
            return result;
        }

        public bool? DialogBoxOk(object caller, string message, string title = null)
        {
            var result = DialogEventBus.DialogBoxOk(caller, message, title);
            return result;
        }

        public bool? OpenFolderSelectDialog(object caller, FolderSelectDialogEventArgs args)
        {
            var result = DialogEventBus.OpenFolderSelectDialog(caller, args);
            return result;
        }
    }
}