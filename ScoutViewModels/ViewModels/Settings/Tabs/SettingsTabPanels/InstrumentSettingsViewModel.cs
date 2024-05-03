using ApiProxies.Generic;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.UIConfiguration;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ScoutServices.Watchdog;
using ScoutServices.Interfaces;
using ScoutUtilities.Common;

namespace ScoutViewModels.ViewModels.Tabs.SettingsPanel
{
    public class InstrumentSettingsViewModel : BaseSettingsPanel
    {
        private IWatchdog _watchDog;
        private ILockManager _lockManager;
        private IOpcUaCfgManager _opcUaCfgManager;

        #region Constructor

        // constructor for unit testing
        public InstrumentSettingsViewModel(IWatchdog watchdog, ILockManager lockManager, IOpcUaCfgManager opcUaCfgManager, IDbSettingsService dbSettingsService,
            ISmtpSettingsService smtpSettingsService, IAutomationSettingsService automationSettingsService, IInstrumentStatusService instrumentStatusService) : base()
        {
            _watchDog = watchdog;
            _lockManager = lockManager;
            _opcUaCfgManager = opcUaCfgManager;
            _dbSettingsService = dbSettingsService;
            _smtpSettingsService = smtpSettingsService;
            _automationSettingsService = automationSettingsService;
            _instrumentStatusService = instrumentStatusService;
            IsSingleton = true;
            NetworkChange.NetworkAddressChanged += AddressChangedCallback;
            AutomationIpAddress = GetLocalIpAddress();
            InitializeVm(UISettings.SoftwareVersion);
        }

        ~InstrumentSettingsViewModel()
        {
            NetworkChange.NetworkAddressChanged -= AddressChangedCallback;
        }

        private void InitializeVm(string appVersion = null)
        {
            ListItemLabel = LanguageResourceHelper.Get("LID_ListOption_Instrument");
            HwdSettingModel = new HardwareSettingsModel();
            ApplicationVersion = appVersion ?? UISettings.SoftwareVersion;
            GetOpticalHardwareConfig();
            GetDbSettings();
            GetSmtpSettings();
            GetAutomationSettings();
        }

        #endregion

        #region Properties & Fields

        private string _ogDbIpAddress;
        private string _ogDbName;
        private uint _ogDbPort;
        private string _ogSmtpServer;
        private string _ogSmtpUsername;
        private string _ogSmtpPassword;
        private uint _ogSmtpPort;
        private bool _ogSmtpAuthEnabled;
        private bool _ogAutomationEnabled;
        private bool _ogACupEnabled;
        private uint _ogAutomationPort;

        private ISmtpSettingsService _smtpSettingsService;
        private IDbSettingsService _dbSettingsService;
        private IAutomationSettingsService _automationSettingsService;
        private readonly IInstrumentStatusService _instrumentStatusService;

        protected HardwareSettingsModel HwdSettingModel { get; set; }

        private OpticalHardwareConfig _currentOpticsSelection;
        private OpticalHardwareConfig _desiredOpticsSelection;

        public bool IsOpticsLegacyChecked
        {
            get { return GetProperty<bool>(); }
            set 
            {
                if (value == true)
                    SetDesiredOpticsSelection(OpticalHardwareConfig.OMICRON_BASLER);
                SetProperty(value); 
            }
        }

        public bool IsOpticsBaslerChecked
        {
            get { return GetProperty<bool>(); }
            set
            {
                if (value == true)
                    SetDesiredOpticsSelection(OpticalHardwareConfig.BECKMAN_BASLER);
                SetProperty(value);
            }
        }

        public bool IsOpticsAlliedChecked
        {
            get { return GetProperty<bool>(); }
            set
            {
                if(value == true)
                    SetDesiredOpticsSelection(OpticalHardwareConfig.BECKMAN_ALLIED);
                SetProperty(value);
            }
        }

        private void SetDesiredOpticsSelection(OpticalHardwareConfig type)
        {
            _desiredOpticsSelection = type;
            SaveOpticsConfigurationCommand.RaiseCanExecuteChanged();
        }

        // Use the code in BaseViewModel.
        //public bool IsAdminUser
        //{
        //    get { return GetProperty<bool>(); }
        //    set
        //    {
        //        SetProperty(value);
        //        NotifyPropertyChanged(nameof(IsAdminOrServiceUser));
        //    }
        //}

        public string SerialNumber
        {
            get { return HwdSettingModel.SerialNumber; }
            set
            {
                HwdSettingModel.SerialNumber = value;
                NotifyPropertyChanged(nameof(SerialNumber));
                ValidateSerialNumber();
            }
        }

        public string ConfirmSerialNumber
        {
            get { return HwdSettingModel.ConfirmSerialNumber; }
            set
            {
                HwdSettingModel.ConfirmSerialNumber = value;
                NotifyPropertyChanged(nameof(ConfirmSerialNumber));
                ValidateSerialNumber();
            }
        }

        public string Password
        {
            get { return HwdSettingModel.Password; }
            set
            {
                HwdSettingModel.Password = value;
                NotifyPropertyChanged(nameof(Password));
            }
        }

        private string _databaseChoosePassword;
        public string DatabaseChoosePassword
        {
            get { return _databaseChoosePassword; }
            set
            {
                _databaseChoosePassword = value;
                NotifyPropertyChanged(nameof(DatabaseChoosePassword));
                SetDbReadPasswordCommand.RaiseCanExecuteChanged();
            }
        }

        private string _databaseConfirmPassword;
        public string DatabaseConfirmPassword
        {
            get { return _databaseConfirmPassword; }
            set
            {
                _databaseConfirmPassword = value;
                NotifyPropertyChanged(nameof(DatabaseConfirmPassword));
                SetDbReadPasswordCommand.RaiseCanExecuteChanged();
            }
        }

        public HardwareSettingsDomain HardwareSetting
        {
            get { return HwdSettingModel.HardwareSettingsDomain; }
            set
            {
                HwdSettingModel.HardwareSettingsDomain = value;
                NotifyPropertyChanged(nameof(HardwareSetting));
            }
        }

        public bool IsSetSerialNumberEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string ApplicationVersion
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsErrorMsgEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string SavedSerialNumber
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string DbIpAddress
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                SetDbConfigCommand.RaiseCanExecuteChanged();
            }
        }

        public string DbName
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                SetDbConfigCommand.RaiseCanExecuteChanged();
            }
        }

        public uint DbPort
        {
            get { return GetProperty<uint>(); }
            set
            {
                SetProperty(value);
                SetDbConfigCommand.RaiseCanExecuteChanged();
            }
        }

        public string SmtpServer
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                SetSmtpConfigCommand.RaiseCanExecuteChanged();
            }
        }

        public string SmtpUsername
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                SetSmtpConfigCommand.RaiseCanExecuteChanged();
            }
        }

        public string SmtpPassword
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                SetSmtpConfigCommand.RaiseCanExecuteChanged();
            }
        }

        public uint SmtpPort
        {
            get { return GetProperty<uint>(); }
            set
            {
                SetProperty(value);
                SetSmtpConfigCommand.RaiseCanExecuteChanged();
            }
        }

        public bool SmtpAuthEnabled
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                SetSmtpConfigCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsAutomationConfigAvailable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool CanToggleAutomation => (IsAutomationOn && IsAdminUser) || IsServiceUser;

        public bool CanToggleACupEnable => (IsAutomationOn && IsServiceUser) || (_ogACupEnabled && IsAdminUser);

        public bool IsAutomationOn
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(CanToggleAutomation));
                NotifyPropertyChanged(nameof(CanToggleACupEnable));
                NotifyPropertyChanged(nameof(IsACupEnabled));
                SetAutomationConfigCommand.RaiseCanExecuteChanged();
                CancelAutomationConfigCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsACupEnabled
        {
            get { return IsAutomationOn && GetProperty<bool>();}
			set
			{
				SetProperty(value);
				NotifyPropertyChanged(nameof(CanToggleACupEnable));
				SetAutomationConfigCommand.RaiseCanExecuteChanged();
				CancelAutomationConfigCommand.RaiseCanExecuteChanged();
			}
		}

        private static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return string.Empty;
        }

        private void AddressChangedCallback(object sender, EventArgs e)
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                AutomationIpAddress = GetLocalIpAddress();
            });
        }

        public string AutomationIpAddress
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public uint AutomationPort
        {
            get { return GetProperty<uint>(); }
            set
            {
                SetProperty(value);
                SetAutomationConfigCommand.RaiseCanExecuteChanged();
                CancelAutomationConfigCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Commands

        #region Set Optics Configuration

        private RelayCommand _setOpticsConfigurationCommand;
        public RelayCommand SaveOpticsConfigurationCommand => _setOpticsConfigurationCommand ?? (_setOpticsConfigurationCommand = new RelayCommand(SetOpticsConfiguration, CanSetOpticsConfiguration));

        private bool CanSetOpticsConfiguration()
        {
            return IsServiceUser && (_currentOpticsSelection != _desiredOpticsSelection);
        }

        private void SetOpticsConfiguration()
        {
            if (_dbSettingsService.SetOpticsConfiguration(_desiredOpticsSelection))
            {
                var msg = $"Optics Configuration successfully saved. Restart the application to take effect.";
                PostToMessageHub(msg, MessageType.Normal);
                DialogEventBus.DialogBoxOk(this, msg);

                _currentOpticsSelection = _desiredOpticsSelection;
                SaveOpticsConfigurationCommand.RaiseCanExecuteChanged();
            }
            else
            {
                var msg = "Failed to save Optics Configuration.";
                PostToMessageHub(msg, MessageType.Warning);
                DialogEventBus.DialogBoxOk(this, msg);
            }
        }

        #endregion

        #region Set Db Read Password

        private RelayCommand _setDbReadPasswordCommand;
        public RelayCommand SetDbReadPasswordCommand => _setDbReadPasswordCommand ?? (_setDbReadPasswordCommand = new RelayCommand(SetDbReadPassword, CanSetDbReadPassword));

        private bool CanSetDbReadPassword()
        {
            if (!IsAdminUser ||
                string.IsNullOrWhiteSpace(DatabaseChoosePassword) ||
                string.IsNullOrWhiteSpace(DatabaseConfirmPassword))
            {
                return false;
            }

            return true;
        }

        private void SetDbReadPassword()
        {
            if (!DatabaseChoosePassword.Equals(DatabaseConfirmPassword))
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_MSGBOX_PasswordMismatch"));
                Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_PasswordMismatch"));
                return;
            }

            if (!Validation.IsStrongPassword(DatabaseChoosePassword))
            {
                return;
            }

            if (_dbSettingsService.SetDbReadPassword(DatabaseChoosePassword))
            {
                PostToMessageHub(LanguageResourceHelper.Get("LID_Label_DatabaseReadPasswordChangeSuccess"), MessageType.Normal);
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Label_DatabaseReadPasswordChangeSuccess"));
                DatabaseChoosePassword = string.Empty;
                DatabaseConfirmPassword = string.Empty;
                SetDbReadPasswordCommand.RaiseCanExecuteChanged();
            }
            else
            {
                PostToMessageHub(LanguageResourceHelper.Get("LID_Label_DatabaseReadPasswordChangeFailure"), MessageType.Warning);
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Label_DatabaseReadPasswordChangeFailure"));
            }
        }
        #endregion

        #region Set Db Config Command

        private RelayCommand _setDbConfigCommand;
        public RelayCommand SetDbConfigCommand => _setDbConfigCommand ?? (_setDbConfigCommand = new RelayCommand(SetDbConfig, CanSetDbConfig));

        private bool CanSetDbConfig()
        {
            return IsServiceUser && (_ogDbIpAddress != DbIpAddress || _ogDbName != DbName || _ogDbPort != DbPort);
        }

        private void SetDbConfig()
        {
            if (_dbSettingsService.SetDbConfig(DbPort, DbIpAddress, DbName))
            {
                PostToMessageHub(LanguageResourceHelper.Get("LID_Label_DatabaseConfigurationChanged"), MessageType.Normal);
                _ogDbName = DbName;
                _ogDbIpAddress = DbIpAddress;
                _ogDbPort = DbPort;
                SetDbConfigCommand.RaiseCanExecuteChanged();
            }
            else
            {
                PostToMessageHub(LanguageResourceHelper.Get("LID_Error_FailedToSetDatabaseConfiguration"), MessageType.Warning);
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Error_FailedToSetDatabaseConfiguration"));
            }
        }

        #endregion

        #region Set SMTP Config Command

        private RelayCommand _setSmtpConfigCommand;
        public RelayCommand SetSmtpConfigCommand => _setSmtpConfigCommand ?? (_setSmtpConfigCommand = new RelayCommand(SetSmtpConfig, CanSetSmtpConfig));

        private bool CanSetSmtpConfig()
        {
            return (_ogSmtpServer != SmtpServer ||
                   _ogSmtpUsername != SmtpUsername ||
                   _ogSmtpPassword != SmtpPassword ||
                   _ogSmtpPort != SmtpPort ||
                   _ogSmtpAuthEnabled != SmtpAuthEnabled) &&
                   !string.IsNullOrEmpty(SmtpServer) &&
                   !string.IsNullOrEmpty(SmtpUsername) &&
                   !string.IsNullOrEmpty(SmtpPassword);
        }

        private void SetSmtpConfig()
        {
            if (_smtpSettingsService.SetSmtpConfig(SmtpPort, SmtpServer, SmtpUsername, SmtpPassword, SmtpAuthEnabled))
            {
                PostToMessageHub(LanguageResourceHelper.Get("LID_Label_SMTPConfigurationChanged"), MessageType.Normal);
                _ogSmtpServer = SmtpServer;
                _ogSmtpUsername = SmtpUsername;
                _ogSmtpPassword = SmtpPassword;
                _ogSmtpPort = SmtpPort;
                _ogSmtpAuthEnabled = SmtpAuthEnabled;
                SetSmtpConfigCommand.RaiseCanExecuteChanged();
            }
            else
            {
                PostToMessageHub(LanguageResourceHelper.Get("LID_Error_FailedToSetSMTPConfiguration"), MessageType.Warning);
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Error_FailedToSetSMTPConfiguration"));
            }
        }

        #endregion

        #region Set Serial Number Command

        private RelayCommand _setSerialNumberCommand;
        public RelayCommand SetSerialNumberCommand => _setSerialNumberCommand ?? (_setSerialNumberCommand = new RelayCommand(OnSetSerialNumber));

        private void OnSetSerialNumber()
        {
            try
            {
                if (!ValidateMe(Password))
                    return;
                var validateStatus = HwdSettingModel.ValidateMe(Password);
                if (validateStatus.Equals(HawkeyeError.eSuccess))
                {
                    if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_SetSerialConfirmation")) != true)
                    {
                        RefreshSetSerialNumber();
                        return;
                    }

                    var msg = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_SerialNumberVerification"), SerialNumber);
                    if (DialogEventBus.DialogBoxOk(this, msg) != true)
                    {
                        RefreshSetSerialNumber();
                        return;
                    }

                    SetSerialNumber();
                }
                else
                {
                    ApiHawkeyeMsgHelper.WrongPassword(validateStatus);
                    Password = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SerialNumFailed"));
            }
        }

        #endregion

        #region Set Automation Config Command

        private RelayCommand _setAutomationConfigCommand;
        public RelayCommand SetAutomationConfigCommand =>
            _setAutomationConfigCommand ?? (_setAutomationConfigCommand = new RelayCommand(PerformSetAutomationConfig, CanPerformSetAutomationConfig));

        private bool CanPerformSetAutomationConfig()
        {
            if ((IsAutomationOn == _ogAutomationEnabled) &&
                ((AutomationPort == _ogAutomationPort) || !IsAutomationOn) &&
                IsACupEnabled == _ogACupEnabled)
                return false; // Don't allow save if the settings haven't changed

            return _automationSettingsService.CanSaveAutomationConfig(IsAutomationOn, IsACupEnabled, AutomationPort, IsAdminUser, IsServiceUser);
        }

        private void PerformSetAutomationConfig()
        {
            // Check for automation-lock. If locked, can't change automation settings
            if (_lockManager.IsLocked())
            {
                DialogEventBus.DialogBoxOk(this,
                    LanguageResourceHelper.Get("LID_MSGBOX_AutomationLocked"));
                IsAutomationOn = _ogAutomationEnabled;
				IsACupEnabled = _ogACupEnabled;
				AutomationPort = _ogAutomationPort;
                return;
            }
            // Make sure the user really wants to disable automation
            if (!IsAutomationOn && DialogEventBus.DialogBoxYesNo(this, 
                    LanguageResourceHelper.Get("LID_MSGBOX_DisableAutomationWarning"), null, DialogLocation.UpAndRightOfCenter) != true)
            {
                IsAutomationOn = true;
                return;
            }
            //Make sure the user really wants to disable A-Cup
            if ((IsAutomationOn == _ogAutomationEnabled) && (IsACupEnabled != _ogACupEnabled) && !IsACupEnabled && DialogEventBus.DialogBoxYesNo(this,
                    LanguageResourceHelper.Get("LID_MSGBOX_DisableACupWarning"), null, DialogLocation.UpAndRightOfCenter) != true)
            {
                IsACupEnabled = true;
                return;
            }

            // If port number changed, warn user that current connections will be lost
            // and server restarted
            if ((IsAutomationOn == _ogAutomationEnabled) && (AutomationPort != _ogAutomationPort) && 
                    DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_ChangeAutomationPort")) != true)
            {
                IsAutomationOn = _ogAutomationEnabled;
                IsACupEnabled = _ogACupEnabled;
                AutomationPort = _ogAutomationPort;
                return;
            }
            else
            {
                // Stop OpcUa Server. Will re-start below after port number is changed
                if (IsAutomationOn)
                    _watchDog.ClearAllWatches();
            }
            
            // Save automation settings to database and xml config file             
            if (SaveAutomationSettings() == HawkeyeError.eSuccess)
            {
                if (IsAutomationOn)
                    _watchDog.AddAllWatches();
                else
                    _watchDog.ClearAllWatches();

                PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_AutomationSettingsChanged"));
                GetAutomationSettings();
            }
            else
            {
                IsAutomationOn = _ogAutomationEnabled;
                IsACupEnabled = _ogACupEnabled;
                AutomationPort = _ogAutomationPort;

                PostToMessageHub(LanguageResourceHelper.Get("LID_Error_FailedToSetAutomationConfiguration"), MessageType.Warning);
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Error_FailedToSetAutomationConfiguration"));
            }
        }

        #endregion

        #region Cancel Save Automation Config Command

        private RelayCommand _cancelAutomationConfigCommand;
        public RelayCommand CancelAutomationConfigCommand => _cancelAutomationConfigCommand ??
                                                             (_cancelAutomationConfigCommand = new RelayCommand(PerformCancelAutomationConfig, CanPerformCancelAutomationConfig));

        private bool CanPerformCancelAutomationConfig()
        {
            return IsAutomationOn != _ogAutomationEnabled || 
                   AutomationPort != _ogAutomationPort ||
                   IsACupEnabled != _ogACupEnabled;
        }

        private void PerformCancelAutomationConfig()
        {
            IsAutomationOn = _ogAutomationEnabled;
            AutomationPort = _ogAutomationPort;
            IsACupEnabled = _ogACupEnabled;
        }

        #endregion

        #endregion

        #region Private Methods

        private void GetOpticalHardwareConfig()
        {
            var hardwareType = _dbSettingsService.GetOpticalHardwareConfig();
            if (hardwareType == OpticalHardwareConfig.UNKNOWN)
            {
                _dbSettingsService.SetOpticsConfiguration(OpticalHardwareConfig.OMICRON_BASLER);
                hardwareType = OpticalHardwareConfig.OMICRON_BASLER;
            }

            switch (hardwareType)
            {
                case OpticalHardwareConfig.UNKNOWN:
                    IsOpticsLegacyChecked = true;
                    break;
                case OpticalHardwareConfig.OMICRON_BASLER:
                    IsOpticsLegacyChecked = true;
                    break;
                case OpticalHardwareConfig.BECKMAN_BASLER:
                    IsOpticsBaslerChecked = true;
                    break;
                default:
                    IsOpticsLegacyChecked = true;
                    break;
            }

            _currentOpticsSelection = hardwareType;
            _desiredOpticsSelection = hardwareType;
        }

        private void GetDbSettings()
        {
            var dbConfig = _dbSettingsService.GetDbConfig();
            _ogDbName = DbName = dbConfig.Name;
            _ogDbIpAddress = DbIpAddress = dbConfig.IpAddress;
            _ogDbPort = DbPort = dbConfig.Port;
        }

        private void GetSmtpSettings()
        {
            var smtpConfig = _smtpSettingsService.GetSmtpConfig();
            _ogSmtpServer = SmtpServer = smtpConfig.Server;
            _ogSmtpUsername = SmtpUsername = smtpConfig.Username;
            _ogSmtpPassword = SmtpPassword = smtpConfig.Password;
            _ogSmtpPort = SmtpPort = smtpConfig.Port;
            _ogSmtpAuthEnabled = SmtpAuthEnabled = Misc.ByteToBool(smtpConfig.AuthEnabled);
        }

        private void GetAutomationSettings()
        {
            try
            {
                var autoConfig = _automationSettingsService.GetAutomationConfig();
                _ogAutomationEnabled = IsAutomationOn = Misc.ByteToBool(autoConfig.AutomationIsEnabled);
				_ogACupEnabled = IsACupEnabled = Misc.ByteToBool(autoConfig.ACupIsEnabled);
				_ogAutomationPort = AutomationPort = _opcUaCfgManager.GetOrSetOpcUaPort();
                SetAutomationConfigCommand.RaiseCanExecuteChanged();
                CancelAutomationConfigCommand.RaiseCanExecuteChanged();
            }
            catch (Exception e)
            {
                Log.Error($"Failed to get automation settings.", e);
            }
        }

        private HawkeyeError SaveAutomationSettings()
        {
            // Save automation settings to database and if successful save OpcUa port to server xml config.
            var result = _automationSettingsService.SaveAutomationConfig(IsAutomationOn, IsACupEnabled, AutomationPort);
            if (result == HawkeyeError.eSuccess)
            {
                try
                {
                    _opcUaCfgManager.GetOrSetOpcUaPort(AutomationPort);
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to save automation settings.", e);
                    result = HawkeyeError.eEntryNotFound;
                }
            }

            return result;
        }
        
        private bool ValidateMe(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrEmpty(password))
            {
                var passwordString = "\"" + LanguageResourceHelper.Get("LID_Label_Password") + "\"";
                var msg = string.Format(LanguageResourceHelper.Get("LID_ERRMSGBOX_BlankField_With_Double_Quotes"), passwordString);

                DialogEventBus.DialogBoxOk(this, msg);
                Log.Debug(LanguageResourceHelper.Get("LID_Label_EnterPassword"));
                return false;
            }

            return true;
        }

        private void RefreshSetSerialNumber()
        {
            ConfirmSerialNumber = Password = string.Empty;
            HwdSettingModel.GetVersionInformation();
            NotifyPropertyChanged(nameof(HardwareSetting));
            NotifyPropertyChanged(nameof(SerialNumber));
        }

        private void ValidateSerialNumber()
        {
            try
            {
                var isSerialNumberBlank = string.IsNullOrEmpty(SerialNumber) || string.IsNullOrWhiteSpace(SerialNumber);
                var isConfirmSerialNumberBlank = string.IsNullOrEmpty(ConfirmSerialNumber) ||
                                                 string.IsNullOrWhiteSpace(ConfirmSerialNumber);

                var serialNumber = string.Empty;
                var getSerialStatus = HwdSettingModel.GetSystemSerialNumber(ref serialNumber);
                if (getSerialStatus.Equals(HawkeyeError.eSuccess))
                {
                    SavedSerialNumber = serialNumber;
                }
                // Enable set serial number button if: neither field is blank and both fields match && SerialNumber != SavedSerialNumber
                IsSetSerialNumberEnable = !(isSerialNumberBlank || isConfirmSerialNumberBlank) &&
                                          string.Equals(ConfirmSerialNumber, SerialNumber) &&
                                          !string.Equals(SavedSerialNumber, SerialNumber);

                // Display error if: neither field is blank and fields do NOT match
                IsErrorMsgEnable = !(isSerialNumberBlank || isConfirmSerialNumberBlank) &&
                                   !string.Equals(ConfirmSerialNumber, SerialNumber);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ValidationFailed"));
            }

        }

        private void SetSerialNumber()
        {
            var serialStatus = HwdSettingModel.SetSystemSerialNumber(SerialNumber, Password);
            if (serialStatus.Equals(HawkeyeError.eSuccess))
            {
                PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_SerialNumberChanged"));
                RefreshSetSerialNumber();
                _instrumentStatusService.PublishViCellIdentifierCallback(SerialNumber);
            }
            else
            {
                ApiHawkeyeMsgHelper.ErrorCommon(serialStatus);
            }
        }

        #endregion

        #region Override Methods

        public override void UpdateListItemLabel()
        {
            ListItemLabel = LanguageResourceHelper.Get("LID_ListOption_Instrument");
        }

        public override void OnUserChanged(UserDomain newUser)
        {
            GetDbSettings();
            GetSmtpSettings();
            GetAutomationSettings();
            NotifyPropertyChanged(nameof(CanToggleAutomation));
            NotifyPropertyChanged(nameof(CanToggleACupEnable));
            base.OnUserChanged(newUser);
        }

        protected override void PerformSave()
        {
            try
            {
                HwdSettingModel.SaveInstrument(LoggedInUser.CurrentUserId);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SaveInstrumentFailed"));
            }
        }

        public override void SetDefaultSettings()
        {
            HwdSettingModel.GetInstrumentSettings(LoggedInUser.CurrentUserId);
            HardwareSetting = HwdSettingModel.HardwareSettingsDomain;
            Password = ConfirmSerialNumber = string.Empty;
            ValidateSerialNumber();
        }

        #endregion
    }
}