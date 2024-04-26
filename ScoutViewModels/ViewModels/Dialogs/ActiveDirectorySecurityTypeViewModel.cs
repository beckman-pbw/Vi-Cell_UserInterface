using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class ActiveDirectorySecurityTypeViewModel : SecurityTypeViewModel
    {
        private readonly EditSecuritySettingsDialogViewModel _parent;
        private RelayCommand _activeDirConfigCommand;

        public ActiveDirectorySecurityTypeViewModel(SecurityType securityType, EditSecuritySettingsDialogViewModel parent, UserDomain user = null) : base(securityType, user)
        {
            _parent = parent;
        }

        public override bool IsChecked
        {
            get => GetProperty<bool>();
            set
            {
                SetProperty(value);
                ShowButton = value;
                ActiveDirConfigCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand ActiveDirConfigCommand => _activeDirConfigCommand ?? (_activeDirConfigCommand = new RelayCommand(OpenActiveDirDialog, CanOpenAdConfig));

        public bool CanOpenAdConfig()
        {
            return _parent.IsInactivityTimeoutValid();
        }

        internal void OpenActiveDirDialog()
        {
            var args = new ActiveDirectoryConfigDialogEventArgs(_parent.ActiveDirectoryDomain);
            if (DialogEventBus.ActiveDirectoryConfigDialog(this, args) == true)
            {
                if (args.ActiveDirectoryDomain is ActiveDirectoryDomain adDomain && adDomain.IsPopulated())
                {
                    _parent.ActiveDirectoryDomain = adDomain;
                    _parent.ActiveDirectoryUser = args.Username;
                    _parent.ActiveDirectoryPassword = args.Password;
                }
                //_parent.Close(true); // user clicked OK/Save to Active Dir settings, so consider everything is good to go
            }
        }
    }
}