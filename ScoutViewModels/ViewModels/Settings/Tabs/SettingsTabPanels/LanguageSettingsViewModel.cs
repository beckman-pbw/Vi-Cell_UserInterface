using ApiProxies.Generic;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Events;
using ScoutUtilities.Structs;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace ScoutViewModels.ViewModels.Tabs.SettingsPanel
{
    public class LanguageSettingsViewModel : BaseSettingsPanel
    {
        #region Constructor

        public LanguageSettingsViewModel() : base()
        {
            IsSingleton = true;
            ListItemLabel = LanguageResourceHelper.Get("LID_FrameLabel_Language");
            GeneralSettingsModel = new GeneralSettingsModel();
            SetDefaultSettings();
        }

        public override void UpdateListItemLabel()
        {
            ListItemLabel = LanguageResourceHelper.Get("LID_FrameLabel_Language");
        }

        #endregion

        #region Properties & Fields

        public LanguageType CurrentLanguage => GeneralSettingsModel.GetUserLanguage(LoggedInUser.CurrentUserId);

        public GeneralSettingsModel GeneralSettingsModel { get; set; }

        public bool IsDayLightOptionSupported
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public TimeZoneInfo SelectedTimeZone
        {
            get { return GetProperty<TimeZoneInfo>(); }
            set
            {
                SetProperty(value);
                OnSelectedTimeZone(value);
            }
        }

        public DateTime CurrentDate
        {
            get { return GeneralSettingsModel.CurrentDate; }
            set
            {
                GeneralSettingsModel.CurrentDate = value;
                NotifyPropertyChanged(nameof(CurrentDate));
            }
        }

        public GeneralSettingsDomain GeneralSettings
        {
            get { return GeneralSettingsModel.GeneralSettings; }
            set
            {
                GeneralSettingsModel.GeneralSettings = value;
                NotifyPropertyChanged(nameof(GeneralSettings));
            }
        }

        public ObservableCollection<Language> LanguageList
        {
            get { return GetProperty<ObservableCollection<Language>>(); }
            set { SetProperty(value); }
        }

        public string SystemTimeHours
        {
            get { return GeneralSettingsModel.SystemTimeHours; }
            set { GeneralSettingsModel.SystemTimeHours = value; }
        }

        public string SystemTimeSeconds
        {
            get { return GeneralSettingsModel.SystemTimeHours; }
            set { GeneralSettingsModel.SystemTimeSeconds = value; }
        }

        public string SystemTimeMinutes
        {
            get { return GeneralSettingsModel.SystemTimeMinutes; }
            set { GeneralSettingsModel.SystemTimeMinutes = value; }
        }

        #endregion

        #region Commands

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetLocalTime(ref SystemTime st);

        private ICommand _languageCheck;
        public ICommand LanguageCheck => _languageCheck ?? (_languageCheck = new RelayCommand(OnSelectLanguage, null));
        
        private void OnSelectLanguage(object param)
        {
            try
            {
                if (param is Language language)
                {
                    GeneralSettings.SelectedLanguage = "";
                    GeneralSettings.SelectedTimeZone = "";
                    GeneralSettings.IsDayLightSavings = false;
                    GeneralSettings.LanguageID = language.LanguageID;
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_LanguageSelectionFailed"));
            }
        }

        protected override bool CanPerformSave()
        {
            return GeneralSettings.LanguageID != CurrentLanguage;
        }

        protected override void PerformSave()
        {
            try
            {
                if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_ChangeLanguageSettings")) != true)
                {
                    return;
                }

                if (GeneralSettings != null)
                {
                    GeneralSettingsModel.SaveUserLanguage(LoggedInUser.CurrentUserId);
                    MessageBus.Default.Publish(this);
                    SaveCommand.RaiseCanExecuteChanged();
                }

                PostToMessageHub(LanguageResourceHelper.Get("LID_MSGBOX_LanguageSettingsSuceesfully"));
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SaveFailed"));
            }
        }

        #endregion

        public sealed override void SetDefaultSettings()
        {
            try
            {
                GeneralSettingsModel.UpdateLanguageList(LoggedInUser.CurrentUserId);
                GeneralSettings = GeneralSettingsModel.GeneralSettings;
                LanguageList = new ObservableCollection<Language>(GeneralSettingsModel.LanguageList);

                SelectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_FetchFailed"));
            }
        }

        private void OnSelectedTimeZone(TimeZoneInfo selectedTimeZone)
        {
            try
            {
                IsDayLightOptionSupported = selectedTimeZone.SupportsDaylightSavingTime;
                var dateTimeZone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SelectedTimeZone);
                SystemTimeHours = Misc.ConvertToString(dateTimeZone.Hour);
                SystemTimeMinutes = Misc.ConvertToString(dateTimeZone.Minute);
                SystemTimeSeconds = Misc.ConvertToString(dateTimeZone.Second);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_TimeZoneSelectionFailed"));
            }
        }
    }
}