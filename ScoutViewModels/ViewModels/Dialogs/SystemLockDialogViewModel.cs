using ScoutLanguageResources;
using ScoutServices.Enums;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using System.Windows;
using ScoutModels;

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

        private readonly ILockManager _lockManager;
        private readonly UserPermissionLevel _currentUserRole;

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
