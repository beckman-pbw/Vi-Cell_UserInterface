using ApiProxies.Generic;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Events;
using System;
using System.Threading.Tasks;
using ScoutServices.Enums;
using ScoutServices.Interfaces;

namespace ScoutViewModels.ViewModels
{
    public class SignInViewModel : BaseViewModel
    {
        public SignInViewModel(bool usernameIsLocked, ILockManager lockManager)
        {
            UsernameIsLocked = usernameIsLocked;
            _lockManager = lockManager;
            _lockSubscription = _lockManager.SubscribeStateChanges().Subscribe(LockStatusChanged);
            IsSystemLocked = _lockManager.IsLocked();
            
#if DEBUG
            // Auto sign-in for debugging...
            // ie: factory_admin or bci_service
            var username = Environment.GetEnvironmentVariable("ScoutX_Username") ?? "bci_service" /*ApplicationConstants.FactoryAdminUserId*/;
            var password = Environment.GetEnvironmentVariable("ScoutX_Password") ?? "865910";
            Task.Delay(1000).ContinueWith((o) =>
            {
                DispatcherHelper.ApplicationExecute(() =>
                {
                    if (null != username && null != password)
                    {
                        Username = username;
                        Password = password;
                        //Login(); // uncomment to attempt auto-login
                    }
                });
            });
#endif
        }


        #region Properties
        
        private readonly ILockManager _lockManager;
        private readonly IDisposable _lockSubscription;

        public bool IsSystemLocked
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string Username
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Password // todo: should use SecureString for this - JDT
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool UsernameIsLocked
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        private RelayCommand _loginCommand;
        public RelayCommand LoginCommand
        {
            get
            {
                if (_loginCommand == null) _loginCommand = new RelayCommand(Login, CanLogin);
                return _loginCommand;
            }
        }

        private bool CanLogin()
        {
            return true;
        }

        private void Login()
        {
            try
            {
                if (!InputsAreValid(Username, Password)) return;
                LoginModel.Instance.Login(Username, Password, out _);
                Password = string.Empty;
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_LOGIN_USER_ERROR"));
            }
        }

        private void LockStatusChanged(LockResult res)
        {
            IsSystemLocked = _lockManager.IsLocked();
        }

        private bool InputsAreValid(string userName, string pwd)
        {
            return LoginValidation(userName, pwd) && Validation.TextBoxLength(userName, null);
        }

        private bool LoginValidation(string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_UserNameBlank"));

                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_UserNameBlank"));
                return false;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                return true;
            }

            DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_PasswordBlank"));

            Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_PasswordBlank"));
            return false;
        }

        protected override void DisposeUnmanaged()
        {
            _lockSubscription?.Dispose();
            base.DisposeUnmanaged();
        }

        #endregion
    }
}
