using ApiProxies.Generic;
using HawkeyeCoreAPI.Facade;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Home.QueueManagement;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using ScoutViewModels.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ScoutModels.Service;

namespace ScoutViewModels.ViewModels.Tabs.SettingsPanel
{
    public class RunOptionSettingsViewModel : BaseSettingsPanel
    {
        #region Constructor

        public override void OnUserChanged(UserDomain newUser)
        {
            base.OnUserChanged(newUser);
            var username = newUser?.UserID;
            var dataAccess = XMLDataAccess.Instance;
            RunOptionModel = new RunOptionSettingsModel(dataAccess, username);
            if ((newUser != null) &&
                (newUser.AssignedCellTypes != null))
            {
                AllCellTypesList = newUser.AssignedCellTypes.ToObservableCollection();//CellTypeFacade.Instance.GetCurrentUserCellTypesCopy().ToObservableCollection();
            }
            else
            {
                AllCellTypesList = new ObservableCollection<CellTypeDomain>();
            }
            SelectedCellType = AllCellTypesList.FirstOrDefault(x => x.CellTypeIndex == RunOptionModel.RunOptionsSettings.DefaultBPQC);
            SetDefaultRunSettings();
        }

        public RunOptionSettingsViewModel() : base()
        {
            _settingsService = new SettingsService();
            IsSingleton = true;
            ListItemLabel = LanguageResourceHelper.Get("LID_ListOption_RunOption");
            RunOptionModel = new RunOptionSettingsModel(XMLDataAccess.Instance, LoggedInUser.CurrentUserId);
            QmModel = new QueueManagementModel();
            SetDefaultRunSettings();

            ctqcSubscriber = MessageBus.Default.Subscribe<Notification>(OnCellTypesQualityControlsUpdated);
        }

        protected override void DisposeUnmanaged()
        {
            QmModel?.Dispose();
            MessageBus.Default.UnSubscribe(ref ctqcSubscriber);
            base.DisposeUnmanaged();
        }

        public override void UpdateListItemLabel()
        {
            ListItemLabel = LanguageResourceHelper.Get("LID_ListOption_RunOption");
        }

        #endregion

        #region Event Handlers

        private void OnCellTypesQualityControlsUpdated(Notification msg)
        {
            if (string.IsNullOrEmpty(msg?.Token))
                return;

            if (msg.Token.Equals(MessageToken.CellTypesUpdated))
            {
                uint selectedIndex = 0;
                AllCellTypesList = CellTypeFacade.Instance.GetAllCellTypes_BECall().ToObservableCollection();
                if (SelectedCellType != null)
                    selectedIndex = SelectedCellType.CellTypeIndex;
                else
                {
                    selectedIndex = (uint) CellTypeIndex.BciDefault;
                }
                
                SelectedCellType = AllCellTypesList.FirstOrDefault(x => x.CellTypeIndex == selectedIndex);
            }

            if (!string.IsNullOrEmpty(msg.Message) &&
                msg.Token == MessageToken.RunSampleSettingsChanged &&
                msg.Message == MessageToken.UserDefaultCellTypeChanged)
            {
                DispatcherHelper.ApplicationExecute(() =>
                {
                    RunOptionModel = _settingsService.GetRunOptionSettingsModel();
                    SelectedCellType = AllCellTypesList.FirstOrDefault(x => x.CellTypeIndex == RunOptionModel.DefaultBPQC);
                });
            }
        }

        #endregion

        #region Properties & Fields

        public QueueManagementModel QmModel { get; set; }
        public RunOptionSettingsModel RunOptionModel { get; set; }

        private Subscription<Notification> ctqcSubscriber;
        private List<SamplePostWash> _washList;
        public List<SamplePostWash> WashList
        {
            get
            {
                _washList = new List<SamplePostWash>(RunOptionModel.WashList);
                return _washList;
            }
            set
            {
                _washList = value;
                NotifyPropertyChanged(nameof(WashList));
            }
        }

        public SamplePostWash SelectedWash
        {
            get { return IsFastModeEnabled ? GetProperty<SamplePostWash>() : SamplePostWash.NormalWash; }
            set
            {
                SetProperty(value);
                RunOptionModel.DefaultWash = value;
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<CellTypeDomain> AllCellTypesList
        {
            get { return GetProperty<ObservableCollection<CellTypeDomain>>(); }
            set { SetProperty(value); }
        }

        public CellTypeDomain SelectedCellType
        {
            get { return GetProperty<CellTypeDomain>(); }
            set
            {
                SetProperty(value);
                if (value != null)
                {
                    RunOptionModel.DefaultBPQC = value.CellTypeIndex;
                    // Run on main UI thread
                    DispatcherHelper.ApplicationExecute(() => SaveCommand.RaiseCanExecuteChanged());
                }
            }
        }

        private uint _dilution;
        public uint Dilution
        {
            get { return _dilution; }
            set
            {
                if ((value >= ApplicationConstants.MinimumDilutionFactor) &&
                    (value >= ApplicationConstants.MinimumDilutionFactor))
                {
                    if (_dilution != value)
                    {
                        _dilution = value;
                        RunOptionModel.DefaultDilution = value.ToString();
                        NotifyPropertyChanged(nameof(Dilution));
                        SaveCommand.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        public RunOptionSettingsDomain SavedSettings
        {
            get { return RunOptionModel.SavedSettings; }
            set
            {
                RunOptionModel.SavedSettings = value;
                NotifyPropertyChanged(nameof(SavedSettings));
            }
        }

        public bool IsExportSampleResultActive
        {
            get { return RunOptionModel.RunOptionsSettings.IsExportSampleResultActive; }
            set
            {
                RunOptionModel.RunOptionsSettings.IsExportSampleResultActive = value;
                if (value == false)
                {
                    IsAutoExportPDFSelected = false;
                }
                NotifyPropertyChanged(nameof(IsExportSampleResultActive));
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsAppendSampleResultExportActive
        {
            get { return RunOptionModel.RunOptionsSettings.IsAppendSampleResultExportActive; }
            set
            {
                RunOptionModel.RunOptionsSettings.IsAppendSampleResultExportActive = value;
                NotifyPropertyChanged(nameof(IsAppendSampleResultExportActive));
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsAutoExportPDFSelected
        {
            get { return RunOptionModel.RunOptionsSettings.IsAutoExportPDFSelected; }
            set
            {
                RunOptionModel.RunOptionsSettings.IsAutoExportPDFSelected = value;
                NotifyPropertyChanged(nameof(IsAutoExportPDFSelected));
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public string AppendSampleResultPath
        {
            get { return Path.GetFullPath(RunOptionModel.RunOptionsSettings.AppendSampleResultPath); }
            set
            {
                RunOptionModel.RunOptionsSettings.AppendSampleResultPath = value;
                NotifyPropertyChanged(nameof(AppendSampleResultPath));
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public string DefaultPathName
        {
            get { return RunOptionModel.RunOptionsSettings.DefaultFileName; }
            set
            {
                RunOptionModel.RunOptionsSettings.DefaultFileName = value;
                NotifyPropertyChanged(nameof(DefaultPathName));
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public string NumberOfImages
        {
            get { return RunOptionModel.RunOptionsSettings.NumberOfImages; }
            set
            {
                RunOptionModel.NumberOfImages = value;
                NotifyPropertyChanged(nameof(NumberOfImages));
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public string DefaultSampleId
        {
            get { return RunOptionModel.RunOptionsSettings.DefaultSampleId; }
            set
            {
                RunOptionModel.DefaultSampleId = value;
                NotifyPropertyChanged(nameof(DefaultSampleId));
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public string ExportSampleResultPath
        {
            get { return Path.GetFullPath(RunOptionModel.RunOptionsSettings.ExportSampleResultPath); }
            set
            {
                RunOptionModel.RunOptionsSettings.ExportSampleResultPath = value;
                NotifyPropertyChanged(nameof(ExportSampleResultPath));
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsTopDisable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsLastDisable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool AllSelected
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsFastModeEnabled
        {
            get 
            {
                if (IsAdminOrServiceUser) return true;
                if (LoggedInUser.IsConsoleUserLoggedIn())
                {
                    return LoggedInUser.CurrentUser.IsFastModeEnabled;
                }
                return false;
            }
            set { SetProperty(value); }
        }

        public bool IsChanged
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public uint SelectedDisplayDigits
        {
            get { return GetProperty<uint>(); }
            set
            {
                if (GetProperty<uint>() == value) return; 
                SetProperty(value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public List<uint> DisplayDigitsList
        {
            get
            {
                var digits = GetProperty<List<uint>>();
                if (digits != null) return digits;
                SetProperty(new List<uint>(GetDisplayDigitsList()));
                return GetProperty<List<uint>>();
            }
            set { SetProperty(value); }
        }


        private List<uint> GetDisplayDigitsList()
        {
            var digitsList = new List<uint>();
            for (uint i = 2; i <= 4; i++)
            { 
                digitsList.Add(i);
            }

            return digitsList;
        }

        private ObservableCollection<GenericParametersDomain> _genericParameters;
        public ObservableCollection<GenericParametersDomain> GenericParameters
        {
            get
            {
                return _genericParameters ?? (_genericParameters =
                           new ObservableCollection<GenericParametersDomain>(RunOptionModel.GenericParameters));
            }
            set
            {
                _genericParameters = value;
                NotifyPropertyChanged(nameof(GenericParameters));
            }
        }

        private GenericParametersDomain _selectedGenericParameter;
        public GenericParametersDomain SelectedGenericParameter
        {
            get { return _selectedGenericParameter; }
            set
            {
                _selectedGenericParameter = value;
                NotifyPropertyChanged(nameof(SelectedGenericParameter));
                if (value != null)
                    OnGenericChanged();
            }
        }

        #endregion
        
        #region Commands

        private ICommand _allGenericCheckCommand;
        public ICommand AllGenericCheckCommand => _allGenericCheckCommand ?? (_allGenericCheckCommand = new RelayCommand(AllCheckStatus, null));

        private ICommand _genericCheckStatus;
        public ICommand GenericCheckStatus => _genericCheckStatus ?? (_genericCheckStatus = new RelayCommand(CheckStatus, null));

        private ICommand _traversalCommand;
        public ICommand TraversalCommand => _traversalCommand ?? (_traversalCommand = new RelayCommand(OnTraversalChange, null));

        private ICommand _openExportFolder;
        private readonly SettingsService _settingsService;
        public ICommand OpenExportFolder => _openExportFolder ?? (_openExportFolder = new RelayCommand(OnExportFolderOpen, null));

        #endregion

        #region Public Methods

        public override void SetDefaultSettings()
        {
            SetDefaultRunSettings();
            SetDefaultRunResult();
        }

        public void SetDefaultRunResult()
        {
            RunOptionModel.GetGenericParameters(LoggedInUser.CurrentUserId);
            RunOptionModel.GetSavedParameters(LoggedInUser.CurrentUserId);
            GenericParameters = new ObservableCollection<GenericParametersDomain>(RunOptionModel.GenericParameters);
            if (GenericParameters == null || GenericParameters.Count == 0)
                return;
            SelectedGenericParameter = GenericParameters.First();
            AllSelected = AllShowRunResultSettings(GenericParameters);
        }

        public bool AllShowRunResultSettings(ObservableCollection<GenericParametersDomain> genericParameters)
        {
            bool resultStatus = false;
            int AllCount = genericParameters.Count;
            int ActiveCount = genericParameters.Count(x => x.IsVisible == true);
            CheckIfParamsChanged();
            if (AllCount.Equals(ActiveCount))
                resultStatus = true;
            return resultStatus;
        }

        protected override bool CanPerformSave()
        {
            var savedAppendSampleResultPath = Path.GetFullPath(SavedSettings.AppendSampleResultPath);
            var savedExportSampleResultPath = Path.GetFullPath(SavedSettings.ExportSampleResultPath);

            if ((IsFastModeEnabled) && (SavedSettings.DefaultWash != SelectedWash))
                return true;

            var retVal = SavedSettings.IsAutoExportPDFSelected != IsAutoExportPDFSelected ||
                    SavedSettings.IsAppendSampleResultExportActive != IsAppendSampleResultExportActive ||
                    SavedSettings.IsExportSampleResultActive != IsExportSampleResultActive ||
                    !savedAppendSampleResultPath.Equals(AppendSampleResultPath) ||
                    !savedExportSampleResultPath.Equals(ExportSampleResultPath) ||
                    !SavedSettings.DefaultFileName.Equals(DefaultPathName) ||
                    !SavedSettings.DefaultDilution.Equals(Dilution.ToString()) ||
                    SavedSettings.DefaultBPQC != RunOptionModel.DefaultBPQC ||
                    !SavedSettings.DefaultSampleId.Equals(DefaultSampleId) ||
                    !SavedSettings.NumberOfImages.Equals(NumberOfImages) ||
                    RunOptionModel.SavedDisplayDigits != SelectedDisplayDigits ||
                    IsChanged;
            return retVal;
        }

        protected override void PerformSave()
        {
            try
            {
                if (SettingSampleId(RunOptionModel.DefaultSampleId) &&
                    ScoutModels.Common.Validation.SettingNoOfImage(RunOptionModel.NumberOfImages) &&
                    ScoutModels.Common.Validation.SettingFileName(DefaultPathName))
                {
                    if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_RunOptionSetting")) != true)
                    {
                        return;
                    }
                    // @todo - need to add the save of user record here to use DB / backend to save all info / remove XML_DB
                    if (RunOptionModel.SaveRunOptionSettings(LoggedInUser.CurrentUserId) &&
                        RunOptionSettingsModel.SetDisplayDigits(LoggedInUser.CurrentUserId,SelectedDisplayDigits) == HawkeyeError.eSuccess)
                    {
                        if (RunOptionModel.SaveGenericParameters(GenericParameters.ToList(), LoggedInUser.CurrentUserId))
                        {
                            PostToMessageHub(LanguageResourceHelper.Get("LID_MSGBOX_RunOptionSuccessful"));
                            MessageBus.Default.Publish(new Notification(MessageToken.RunSampleSettingsChanged));
                        }
                        RunOptionModel.GetSavedSettingsDomain(LoggedInUser.CurrentUserId);
                        RunOptionModel.GetSavedParameters(LoggedInUser.CurrentUserId);
                        RunOptionModel.SavedDisplayDigits = SelectedDisplayDigits;
                        RunOptionSettingsModel.SetConcTrailingPoint(SelectedDisplayDigits);
                        IsChanged = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_RunOptionSaveFailed"));
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Setup all the default run settings for a user. This is called in the constructor,
        /// when a user changes, and when setting all the default settings, both for the user and run settings (generic parameters, etc).
        /// </summary>
        private void SetDefaultRunSettings()
        {
            RunOptionModel = _settingsService.GetRunOptionSettingsModel();

            if ((LoggedInUser.CurrentUser != null) &&
                (LoggedInUser.CurrentUser.AssignedCellTypes != null))
            {
                AllCellTypesList = LoggedInUser.CurrentUser.AssignedCellTypes.ToObservableCollection();
            }
            else
            {
                AllCellTypesList = new ObservableCollection<CellTypeDomain>();
            }

            //
            // @todo - add GetUserRecord call to get the values and then populate them
            //
            SelectedCellType = AllCellTypesList.FirstOrDefault(x => x.CellTypeIndex == RunOptionModel.DefaultBPQC) ??
                               AllCellTypesList.FirstOrDefault(x => x.CellTypeIndex == (ulong) CellTypeIndex.BciDefault);
            SelectedWash = RunOptionModel.DefaultWash;
            SelectedDisplayDigits = RunOptionModel.SavedDisplayDigits;
            _dilution = 1;
            uint.TryParse(RunOptionModel.DefaultDilution, out _dilution);
        }

        private void CheckIfParamsChanged()
        {
            var current = GenericParameters.ToList();
            var saved = RunOptionModel.SavedParameters.ToList();
            for (var i = 0; i < 8; i++)
            {
                if ((current[i].IsVisible != saved[i].IsVisible) ||
                    current[i].ParameterID != saved[i].ParameterID)
                {
                    IsChanged = true;
                    break;
                }
                IsChanged = false;
            }
        }

        private void OnGenericChanged()
        {
            try
            {
                if (GenericParameters != null)
                {
                    var firstGenericParameter = GenericParameters.FirstOrDefault();
                    var lastGenericParameter = GenericParameters.LastOrDefault();

                    if (firstGenericParameter != null && firstGenericParameter.ParameterName.Equals(SelectedGenericParameter.ParameterName))
                    {
                        EnableDisableRunSample("Top");
                    }
                    else if (lastGenericParameter != null && lastGenericParameter.ParameterName.Equals(SelectedGenericParameter.ParameterName))
                    {
                        EnableDisableRunSample("End");
                    }
                    else
                    {
                        EnableDisableRunSample("Mid");
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_GenericParameterFailed"));
            }
        }

        private void EnableDisableRunSample(string enableType)
        {
            switch (enableType)
            {
                case "Top":
                    IsTopDisable = false;
                    IsLastDisable = true;
                    break;
                case "End":
                    IsTopDisable = true;
                    IsLastDisable = false;
                    break;
                case "Mid":
                    IsTopDisable = true;
                    IsLastDisable = true;
                    break;
                default:
                    break;
            }
        }

        private bool SettingSampleId(string value)
        {
            if (!string.IsNullOrEmpty(value)) return true;

            DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_SampleIDBlank"));
            Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_SampleIDBlank"));
            return false;
        }

        private void AllCheckStatus()
        {
            try
            {
                foreach (var x in GenericParameters)
                {
                    if (x.IsDefault)
                        x.IsVisible = AllSelected;
                }

                CheckIfParamsChanged();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_CheckStatusFailed"));
            }
        }

        private void OnTraversalChange(object param)
        {
            try
            {
                if (param == null)
                    return;

                switch (param.ToString())
                {
                    case "Up":
                        var upResult = GenericParameters.Where(x => x.Order < SelectedGenericParameter.Order)
                            .OrderBy(x => x.Order).Last();
                        ChangeTraversal(upResult);
                        CheckIfParamsChanged();
                        break;
                    case "Down":
                        var downResult = GenericParameters.Where(x => x.Order > SelectedGenericParameter.Order)
                            .OrderBy(x => x.Order).First();
                        ChangeTraversal(downResult);
                        CheckIfParamsChanged();
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_TraversalChangeFailed"));
            }

        }

        private void ChangeTraversal(GenericParametersDomain result)
        {
            var genericParametersDomain = new GenericParametersDomain
            {
                ParameterName = SelectedGenericParameter.ParameterName,
                IsVisible = SelectedGenericParameter.IsVisible,
                IsDefault = SelectedGenericParameter.IsDefault,
                ParameterID = SelectedGenericParameter.ParameterID
            };

            GenericParameters.Where(x => x.Order == SelectedGenericParameter.Order).OrderBy(x => x.Order).ToList()
                .ForEach(x =>
                {
                    x.ParameterName = result.ParameterName;
                    x.IsVisible = result.IsVisible;
                    x.IsDefault = result.IsDefault;
                    x.ParameterID = result.ParameterID;
                });
            result.ParameterName = genericParametersDomain.ParameterName;
            result.IsVisible = genericParametersDomain.IsVisible;
            result.IsDefault = genericParametersDomain.IsDefault;
            result.ParameterID = genericParametersDomain.ParameterID;
            SelectedGenericParameter = result;
        }


        private void CheckStatus(object objStatus)
        {
            try
            {
                if (objStatus != null)
                {
                    AllSelected = AllShowRunResultSettings(GenericParameters);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ShowSettingFailed"));
            }
        }

        private void OnExportFolderOpen(object exportType)
        {

            switch (exportType as string)
            {
                case "ExportPathForEachSample":
                    ExportSampleResultPath = GetFolderPath(ExportSampleResultPath);
                    break;
                case "AppendSampleExport":
                    AppendSampleResultPath = GetFolderPath(AppendSampleResultPath);
                    break;
            }
        }

        private string GetFolderPath(string path)
        {
            string outpath = ImportExportSamplesViewModel.ExportAdvanceSetting(path);
            if(FileSystem.IsPathValid(outpath) && FileSystem.IsFolderValidForExport(outpath) && Directory.Exists(path))            
                return outpath;

            var invalidPath = LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_PathError");
            var msg = $"{invalidPath}";
            if (FileSystem.IsPathValid(outpath))
            {
                string drive = Path.GetPathRoot(outpath);
                if (drive.ToUpper().StartsWith("C:"))
                    msg += "\n" + LanguageResourceHelper.Get("LID_MSGBOX_ExportPathError");

            }
            DialogEventBus.DialogBoxOk(this, msg);
            if (FileSystem.IsFolderValidForExport(path) && Directory.Exists(path))
                return Path.GetFullPath(path);

            return FileSystem.GetDefaultExportPath(LoggedInUser.CurrentUserId);
        }

        #endregion
    }
}