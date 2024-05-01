using HawkeyeCoreAPI.Facade;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutViewModels.Common;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;

namespace ScoutViewModels.ViewModels
{
    public class BaseViewModel : BaseDisposableNotifyPropertyChanged
    {
        /* Any class inheriting from BaseViewModel should have its Dispose() method called
         * because BaseViewModel subscribes to MessageBus events, which should be unsubscribed to
         * when no longer needed.
        */
        public BaseViewModel(UserDomain user = null)
        {
            IsServiceUser = LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eService);
            IsAdminUser = LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eAdministrator);
            IsAdvancedUser = LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eElevated);
            _userChangedSubscriber = MessageBus.Default.Subscribe<Notification<UserDomain>>(OnUserChangedEvent);
            _securityTypeSubscriber = MessageBus.Default.Subscribe<Notification<SecurityType>>(OnSecurityTypeChangedEvent);
        }

        protected override void DisposeUnmanaged()
        {
            MessageBus.Default.UnSubscribe(ref _userChangedSubscriber);
            MessageBus.Default.UnSubscribe(ref _securityTypeSubscriber);
            base.DisposeUnmanaged();
        }

        private void OnUserChangedEvent(Notification<UserDomain> obj)
        {
            if (obj.Token.Equals(NotificationClasses.NewCurrentUser))
            {
                IsServiceUser = LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eService);
                IsAdminUser = LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eAdministrator);
                IsAdvancedUser = LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eElevated);
                OnUserChanged(obj.Target);
            }
        }

        private void OnSecurityTypeChangedEvent(Notification<SecurityType> msg)
        {
            if (string.IsNullOrEmpty(msg?.Token) || !msg.Token.Equals(MessageToken.SecuritySettingsChanged))
                return;

            NotifyPropertyChanged(nameof(IsSecurityTurnedOn));
            NotifyPropertyChanged(nameof(SecurityIsLocal));
        }

        public virtual void OnUserChanged(UserDomain newUser)
        {
            foreach (var commandProp in GetType().GetProperties().Where(p => p.PropertyType.Equals(typeof(RelayCommand))))
            {
                var command = (RelayCommand) commandProp.GetValue(this);
                command?.RaiseCanExecuteChanged();
            }
        }

        #region Properties

        public bool IsSecurityTurnedOn => SystemStatusFacade.Instance.GetSecurity() != SecurityType.NoSecurity;
        public bool SecurityIsLocal => SystemStatusFacade.Instance.GetSecurity() == SecurityType.LocalSecurity;

        public bool IsSingleton { get; set; } = false;

        private Subscription<Notification<UserDomain>> _userChangedSubscriber;
        private Subscription<Notification<SecurityType>> _securityTypeSubscriber;

        public bool IsServiceUser
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(IsAdminOrServiceUser));
            }
        }

        public bool IsAdminUser
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(IsAdminOrServiceUser));
            }
        }

        public bool IsAutomationUser
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(IsAdminOrServiceUser));
            }
        }

        public bool IsAdvancedUser
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsAdminOrServiceUser => IsAdminUser || IsServiceUser || IsAutomationUser;

        public XmlLanguage CurrentLanguageXml // Used by the DatePicker
        {
            get
            {
                var xmlLang = XmlLanguage.GetLanguage(ApplicationLanguage.GetLanguage());
                return xmlLang;
            }
        }

        #endregion

        #region Commands

        private RelayCommand _idleStateCommand;
        public RelayCommand IdleStateCommand => _idleStateCommand ?? (_idleStateCommand = new RelayCommand(ValidateIdleState));

        public void ValidateIdleState(object parameter)
        {
            IdleState.ValidateIdleState(parameter);
        }

        #endregion

        #region Methods

        protected void PostToMessageHub(string statusBarMessage, MessageType messageType = MessageType.Normal)
        {
            MessageBus.Default.Publish(new SystemMessageDomain
            {
                IsMessageActive = true,
                Message = statusBarMessage,
                MessageType = messageType
            });
        }

        protected virtual void CloseWindow(object param)
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                var window = param as Window;
                window?.Close();
            });
        }

        public virtual void DisplayErrorDialogByUser(HawkeyeError result)
        {
            ApiHawkeyeMsgHelper.ErrorCreateUser(result);
        }

        public virtual void DisplayErrorDialogByApi(HawkeyeError result, string prefixMessage = null)
        {
            ApiHawkeyeMsgHelper.ErrorCommon(result, prefixMessage);
        }

        public virtual void DisplayLiveImageErrorByAPi(HawkeyeError result)
        {
            if (result.Equals(HawkeyeError.eValidationFailed))
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_ValidationFailed"));
                Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_ValidationFailed"));
            }

            ApiHawkeyeMsgHelper.ErrorCommon(result);
        }

        #endregion
    }
}