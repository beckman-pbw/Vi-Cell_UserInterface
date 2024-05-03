using ScoutLanguageResources;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using System.Windows;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class SystemLockDialogViewModel : BaseDialogViewModel
    {
        public SystemLockDialogViewModel(BaseDialogEventArgs args, Window parentWindow) : base(args, parentWindow)
        {
            SizeToContent = true;
            ShowDialogTitleBar = true;
            DialogTitle = LanguageResourceHelper.Get("LID_POPUPHeader_SystemLock");
        }

        #region Properties and Fields

        #endregion

        #region Commands

        private RelayCommand _systemUnlockCommand;
        public RelayCommand SystemUnlockCommand => _systemUnlockCommand ?? (_systemUnlockCommand = new RelayCommand(RequestSystemUnlock, CanRequestSystemUnlock));

        private bool CanRequestSystemUnlock()
        {
            return true;
        }

        private void RequestSystemUnlock()
        {
            Close(true);
        }

        protected override void OnCancel()
        {
            Close(false);
        }

        #endregion
    }
}
