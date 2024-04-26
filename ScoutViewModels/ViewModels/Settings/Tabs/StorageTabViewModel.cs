using ApiProxies.Generic;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Structs;
using ScoutUtilities.UIConfiguration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ScoutModels.Interfaces;

namespace ScoutViewModels.ViewModels.Tabs
{
    public class StorageTabViewModel : BaseViewModel
    {
        #region Constructor

        public StorageTabViewModel(IInstrumentStatusService instrumentStatusService, IApplicationStateService applicationStateService, IFileSystemService fileSystemService)
        {
            _instrumentStatusService = instrumentStatusService;
            _applicationStateService = applicationStateService;
            _fileSystemService = fileSystemService;
            Initialize();
        }

        public void InitUserList()
        {
            UserList = new ObservableCollection<UserDomain>(UserModel.GetUsersForSelectionBoxes());
            try
            {
                var defaultUser = UserList.FirstOrDefault(u => u.UserID.Equals(LanguageResourceHelper.Get("LID_Label_All")));
                var currentUserSession = LoggedInUser.CurrentUser.Session;
                var savedDefaultUser = currentUserSession.GetVariable(SessionKey.StorageTab_SelectedUser, defaultUser);
                if (savedDefaultUser != null)
                {
                    SelectedUser = UserList.FirstOrDefault(u => u.UserID.Equals(savedDefaultUser.UserID));
                } else if(defaultUser != null)
                {
                    SelectedUser = UserList.FirstOrDefault(u => u.UserID.Equals(defaultUser.UserID));
                }
            } 
            catch(Exception e)
            {
                Log.Error($"Failed to Initialize User List", e);
            }
        }

        private void Initialize()
        {
            DiskSpaceModel = new DiskSpaceModel();
            SampleRecords = new ObservableCollection<SampleRecordDomain>();

            InitUserList();

            IsStorageLoaded = true;

            _systemMessageSubscriber = MessageBus.Default.Subscribe<string>(OnSystemMessageCallback);
            _setUserSubscriber = MessageBus.Default.Subscribe<object>(OnSetUserCallback);

            FilterSamplesAndCalculateDiskSpace();
        }

        protected override void DisposeUnmanaged()
        {
            MessageBus.Default.UnSubscribe(ref _systemMessageSubscriber);
            MessageBus.Default.UnSubscribe(ref _setUserSubscriber);
            base.DisposeUnmanaged();
        }

        #endregion

        #region Event Handlers

        public void OnSystemMessageCallback(string msg)
        {
            PostToMessageHub(msg);
        }

        private void OnSetUserCallback<T>(T type)
        {
            if (type is string)
            {
                var value = type as object;
                var message = (string)value;
                DialogEventBus.DialogBoxOk(this, message);
            }
        }

        #endregion

        #region Properties & Fields

        private readonly IInstrumentStatusService _instrumentStatusService;
        private readonly IFileSystemService _fileSystemService;
        private Subscription<string> _systemMessageSubscriber;
        private Subscription<object> _setUserSubscriber;

        public bool IsExportEnable => IsAdminOrServiceUser && _instrumentStatusService.SystemStatus == SystemStatus.Idle;

        public bool IsImportEnable => IsAdminOrServiceUser &&
                                      _instrumentStatusService.SystemStatus == SystemStatus.Idle &&
                                      !SystemHasData;


        public DiskSpaceModel DiskSpaceModel { get; set; }
        public string Message { get; set; }

        public string DiskFreeSpace
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool SystemHasData
        {
            get { return StorageModel.SystemHasData(); }
            set { SetProperty(value);}
        }

        public bool IsStorageLoaded
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        //Enable/disable sample delete button AND sample export button
        public bool IsDeleteEnable
        {
            get { return SampleRecords.Any() && GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsAllSelected
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                foreach (SampleRecordDomain sr in SampleRecords)
                {
                    if (LoggedInUser.CurrentUserRoleId == UserPermissionLevel.eService)
                    {
                        sr.IsSelected = sr.UserId == "bci_service" ? value : false;
                    }
                    else
                    {
                        sr.IsSelected = value;
                    }
                }
                IsDeleteEnable = value;
                NotifyPropertyChanged(nameof(SampleRecords));
            }
        }

        [SessionVariable(SessionKey.StorageTab_ToDate)]
        public DateTime ToDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        [SessionVariable(SessionKey.StorageTab_FromDate)]
        public DateTime FromDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<UserDomain> UserList
        {
            get { return GetProperty<ObservableCollection<UserDomain>>(); }
            set { SetProperty(value); }
        }

        [SessionVariable(SessionKey.StorageTab_SelectedUser)]
        public UserDomain SelectedUser
        {
            get { return GetProperty<UserDomain>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<SampleRecordDomain> SampleRecords
        {
            get { return GetProperty<ObservableCollection<SampleRecordDomain>>(); }
            set { SetProperty(value); }
        }

        #region Model-Based Property Wrappers

        public double PercentDiskSpaceFree
        {
            get { return DiskSpaceModel.PercentDiskSpaceFree; }
            set
            {
                if (!DiskSpaceModel.PercentDiskSpaceFree.Equals(value))
                {
                    DiskSpaceModel.PercentDiskSpaceFree = value;
                    NotifyPropertyChanged(nameof(PercentDiskSpaceFree));
                }
            }
        }

        public double PercentDiskSpaceData
        {
            get { return DiskSpaceModel.PercentDiskSpaceData; }
            set
            {
                if (!DiskSpaceModel.PercentDiskSpaceData.Equals(value))
                {
                    DiskSpaceModel.PercentDiskSpaceData = value;
                    NotifyPropertyChanged(nameof(PercentDiskSpaceData));
                }
            }
        }

        public double PercentDiskSpaceOther
        {
            get { return DiskSpaceModel.PercentDiskSpaceOther; }
            set
            {
                if (!DiskSpaceModel.PercentDiskSpaceOther.Equals(value))
                {
                    DiskSpaceModel.PercentDiskSpaceOther = value;
                    NotifyPropertyChanged(nameof(PercentDiskSpaceOther));
                }
            }
        }

        public double PercentDiskSpaceExport
        {
            get { return DiskSpaceModel.PercentDiskSpaceExport; }
            set
            {
                if (!DiskSpaceModel.PercentDiskSpaceExport.Equals(value))
                {
                    DiskSpaceModel.PercentDiskSpaceExport = value;
                    NotifyPropertyChanged(nameof(PercentDiskSpaceExport));
                }
            }
        }

        #endregion

        #endregion

        #region Commands

        #region OnPageLoaded Command

        private RelayCommand _loadedCommand;
        public RelayCommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnPageLoaded));

        private void OnPageLoaded()
        {
            FromDate = DateTime.Now.AddDays(ApplicationConstants.DefaultFilterFromDaysToSubtract);
            ToDate = DateTime.Now;
            FilterSamplesAndCalculateDiskSpace();
        }

        #endregion

        #region Delete Command

        private RelayCommand _deleteCommand;
        public RelayCommand DeleteCommand => _deleteCommand ?? (_deleteCommand = new RelayCommand(DeleteRecords));

        private void DeleteRecords()
        {
            try
            {
                if (GetUserSelectedSampleCount() > 0)
                {
                    var selectedSamples = GetSelectedSamples().Select(y => y.UUID).Distinct().ToList();
                    
                    DialogEventBus.DeleteSampleResultsDialog(this, new DeleteSampleResultsEventArgs(selectedSamples));

                    FilterSamplesAndCalculateDiskSpace();
                    MessageBus.Default.Publish(new Notification(MessageToken.RecordsDeleted));
                }
                else
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_SelectSampleResultToDelete"));
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_DELETE_RECORDS"));
            }
        }

        #endregion

        #region Export Sample

        private RelayCommand _exportSampleCommand;
        public RelayCommand ExportSampleCommand => _exportSampleCommand ?? (_exportSampleCommand = new RelayCommand(ExportSamples));

        private void ExportSamples(object param)
        {
            if (GetUserSelectedSampleCount() > 0)
            {
                var selectedSampleRecordList = GetSelectedSamples().GroupBy(i => i.UUID).Select(group => group.First()).Where(x => x.IsSelected);
                var sampleUuids = (from sampleRecord in selectedSampleRecordList from resultSummaryDomain in sampleRecord.ResultSummaryList select resultSummaryDomain.UUID).ToList();

                string outpath = LoggedInUser.CurrentExportPath;
                if (!FileSystem.IsFolderValidForExport(outpath) || !Directory.Exists(outpath))
                {
                    outpath = FileSystem.GetDefaultExportPath(LoggedInUser.CurrentUserId);
                    if(!FileSystem.EnsureDirectoryExists(outpath))                    
                        outpath = FileSystem.GetDefaultExportPath("");
                    LoggedInUser.CurrentExportPath = outpath;
                }

                var args = new ExportSampleResultEventArgs(outpath, sampleUuids);
                var result = DialogEventBus.ExportSampleResultsDialog(this, args);

                if (result == true)
                {
                    IsAllSelected = false; // deselect everything
                    CalculateDiskSpace();
                }
            }
            else
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_SelectSampleResultToExport"));
            }
        }

        #endregion

        #region Clean Command

        private RelayCommand _cleanCommand;
        public RelayCommand CleanCommand => _cleanCommand ?? (_cleanCommand = new RelayCommand(Clean));

        private void Clean()
        {
            try
            {
                if (StorageModel.CleanExportDetails(UISettings.ExportPath))
                {
                    CalculateDiskSpace();
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONCLEAN"));
            }
        }

        #endregion

        #region Export Config

        private RelayCommand _exportConfigurationCommand;
        public RelayCommand ExportConfigurationCommand => _exportConfigurationCommand ?? (_exportConfigurationCommand = new RelayCommand(ExportSystemConfiguration));

        private void ExportSystemConfiguration()
        {
            try
            {

                var username = ScoutModels.LoggedInUser.CurrentUserId;
                var myPath = LoggedInUser.CurrentExportPath;
                if (!FileSystem.IsFolderValidForExport(myPath) || !Directory.Exists(myPath))
                {
                    myPath = FileSystem.GetDefaultExportPath(username);
                    LoggedInUser.CurrentExportPath = myPath;
                }
                
                var filePath = ExportModel.GetExportFullFilePath(myPath, Misc.ConvertToFileNameFormat(DateTime.Now), ".cfg");
                var saveDialogArgs = new SaveFileDialogEventArgs(filePath, myPath,
                    "(*.cfg)|*.cfg", "cfg");

                if (DialogEventBus.OpenSaveFileDialog(this, saveDialogArgs) != true)
                    return;

                if (!FileSystem.IsFolderValidForExport(saveDialogArgs.FullFilePathSelected))
                {
                    var invalidPath = LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_PathError");
                    var msg = $"{invalidPath}";
                    if (FileSystem.IsPathValid(saveDialogArgs.FullFilePathSelected))
                    {
                        string drive = Path.GetPathRoot(saveDialogArgs.FullFilePathSelected);
                        if (drive.ToUpper().StartsWith("C:"))
                            msg += "\n" + LanguageResourceHelper.Get("LID_MSGBOX_ExportPathError");
                    }
                    DialogEventBus.DialogBoxOk(this, msg);
                    return;
                }

                FileSystem.EnsureDirectoryExists(saveDialogArgs.FullFilePathSelected);

                var result = StorageModel.ExportInstrumentConfiguration(username, "", saveDialogArgs.FullFilePathSelected);
                if (result.Equals(HawkeyeError.eSuccess))
                {
                    PostToMessageHub(LanguageResourceHelper.Get("LID_Icon_ExportInstrumentStatusSuccessful"));
                }
                else
                {
                    ApiHawkeyeMsgHelper.ErrorCommon(result);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONEXPORT"));
            }
        }

        #endregion

        #region Import Config

        private RelayCommand _importConfigurationCommand;
        public RelayCommand ImportConfigurationCommand => _importConfigurationCommand ?? (_importConfigurationCommand = new RelayCommand(DoImportConfig));

        private void DoImportConfig()
        {
            if (DialogEventBus.DialogBoxOkCancel(this, LanguageResourceHelper.Get("LID_MSGBOX_ImportConfigAlertContent")) == true)
            {
                try
                {
                    var username = ScoutModels.LoggedInUser.CurrentUserId;
                    var myPath = LoggedInUser.CurrentExportPath;
                    if (!FileSystem.IsFolderValidForExport(myPath) || !Directory.Exists(myPath))
                    {
                        myPath = FileSystem.GetDefaultExportPath(username);
                        LoggedInUser.CurrentExportPath = myPath;
                    }

                    var args = new SelectFileDialogEventArgs(myPath, "(*.cfg)|*.cfg", "cfg");
                    if (DialogEventBus.OpenSelectFileDialog(this, args) != true)
                        return;
                   
                    var result = StorageModel.ImportInstrumentConfiguration(username, "", args.FullFilePathSelected);
                    if (result.Equals(HawkeyeError.eSuccess))
                    {
                        PostToMessageHub(LanguageResourceHelper.Get("LID_FrameLabel_ImportSuccessful"));
                        // ToDo: Can add a resource string to display on the exiting dialog that the system is restarting after an Import Config (replacing the 2nd parameter below).
                        _applicationStateService.PublishStateChange(ApplicationStateEnum.Shutdown, LanguageResourceHelper.Get("LID_MSGBOX_ExitProgressMsg"), true);
                        // MainWindowViewModel.Instance.OnShutDown(true);
                    }
                    else
                    {
                        ApiHawkeyeMsgHelper.ValidationError(result);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_ERRMSGBOX_ImportingConfigration"));
                }
            }
        }

        #endregion

        #region Get Sample Record List

        private RelayCommand _getSampleRecordsListCommand;
        public RelayCommand GetSampleRecordsListCommand => _getSampleRecordsListCommand ?? (_getSampleRecordsListCommand = new RelayCommand(GetSampleRecordsList));

        private void GetSampleRecordsList()
        {
            try
            {
                FilterSamplesAndCalculateDiskSpace();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_SAMPLE_LIST"));
            }
        }

        #endregion

        #region Sample Selection Command

        private RelayCommand<uuidDLL> _sampleSelectionCommand;
        private readonly IApplicationStateService _applicationStateService;
        public RelayCommand<uuidDLL> SampleSelectionCommand => _sampleSelectionCommand ?? (_sampleSelectionCommand = new RelayCommand<uuidDLL>(PerformSampleSelection));

        //If a sample is reanalyzed, only the original sample can be selected/unselected. 
        //When the original sample is selected/unselected, the reanalyzed sample(s) automatically are as well.
        private void PerformSampleSelection(uuidDLL uuidParam)
        {
            var selectedSamples = SampleRecords.Where(x => x.ResultSummaryList.Any(y => y.UUID.Equals(uuidParam))).ToList();
            if (selectedSamples.Any())
            {
                selectedSamples.ForEach(x => x.IsSelected = GetSampleRecordIsSelected(selectedSamples, uuidParam));
            }
            
            bool GetSampleRecordIsSelected(List<SampleRecordDomain> samplesToCheck, uuidDLL uuid)
            {
                return samplesToCheck.FirstOrDefault(y =>
                {
                    var first = y.ResultSummaryList.FirstOrDefault();
                    return first != null && first.UUID.Equals(uuid);
                })?.IsSelected == true;
            }

            var numSelected = GetUserSelectedSampleCount();
            if (numSelected > 0)
                IsDeleteEnable = true;
            else
            {
                IsDeleteEnable = IsAllSelected = false;
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private int GetUserSelectedSampleCount()
        {
            var result = GetSelectedSamples().Select(y => y.UUID).Distinct().ToList().Count;
            return result;
        }

        private IEnumerable<SampleRecordDomain> GetSelectedSamples()
        {
            var result = SampleRecords.Where(x => x.IsSelected);
            return result;
        }

        private void SetLoadingIndicator(bool status)
        {
            MessageBus.Default.Publish(new Notification<bool>(status, MessageToken.AdornerVisible));
            if (LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eAdministrator))
            {
                IsDeleteEnable = SampleRecords.Any();
            }
        }

        private async void FilterSamplesAndCalculateDiskSpace()
        {
            await Task.Run(FilterSampleRecords);
            await Task.Run(CalculateDiskSpace);
        }

        private async void FilterSampleRecords()
        {
            if (!Validation.OnDateValidate(FromDate, ToDate))
                return;
            IsStorageLoaded = true;
            await Task.Run(() =>
            {
                var startDate = DateTimeConversionHelper.DateTimeToUnixSecondRounded(DateTimeConversionHelper.DateTimeToStartOfDay(FromDate));
                var endDate = DateTimeConversionHelper.DateTimeToEndOfDayUnixSecondRounded(DateTimeConversionHelper.DateTimeToEndOfDay(ToDate));

                var samples = StorageModel.GetSampleRecords(SelectedUser, startDate, endDate);

                if (samples.Count <= UISettings.MaximumSearchCountForOpenSample)
                {
                    SampleRecords = new ObservableCollection<SampleRecordDomain>(samples);
                    foreach (var sample in SampleRecords)
                    {
                        if (LoggedInUser.CurrentUserRoleId == UserPermissionLevel.eService)
                        {
                            sample.IsSettingsCheckboxEnabled = sample.UserId == "bci_service";
                        }
                        else if (LoggedInUser.CurrentUserRoleId == UserPermissionLevel.eAdministrator)
                        {
                            sample.IsSettingsCheckboxEnabled = true;
                        }
                    }
                }
                else
                {
                    var refineSearchMessage = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_RefineSearch"), UISettings.MaximumSearchCountForOpenSample);
                    DialogEventBus.DialogBoxOk(this, refineSearchMessage);
                }
            });
            IsAllSelected = IsStorageLoaded = false;
        }

        private async void CalculateDiskSpace()
        {
            try
            {
                await Task.Run(DiskSpaceModel.CalculateDiskSpace);

                DiskFreeSpace = string.Format(LanguageResourceHelper.Get("LID_Label_Space"),
                    ScoutUtilities.Misc.ConvertBytesToSize(DiskSpaceModel.TotalFreeSpace),
                    ScoutUtilities.Misc.ConvertBytesToSize(DiskSpaceModel.TotalDiskSpace));

                DispatcherHelper.ApplicationExecute(() =>
                {
                    NotifyPropertyChanged(nameof(DiskSpaceModel));
                    NotifyPropertyChanged(nameof(PercentDiskSpaceFree));
                    NotifyPropertyChanged(nameof(PercentDiskSpaceData));
                    NotifyPropertyChanged(nameof(PercentDiskSpaceOther));
                    NotifyPropertyChanged(nameof(PercentDiskSpaceExport));
                });
            }
            catch (UnauthorizedAccessException unauthorizedAccessException)
            {
                ExceptionHelper.HandleExceptions(unauthorizedAccessException, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_DISKSPACE_PERMISSION_ERROR"));
            }
            catch (DirectoryNotFoundException directoryNotFoundException)
            {
                ExceptionHelper.HandleExceptions(directoryNotFoundException, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_EXPORT_BROWSE_FILE"));
            }
            catch (IOException ioException)
            {
                ExceptionHelper.HandleExceptions(ioException, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_FILE_ERROR"));
            }
            catch (Exception exception)
            {
                ExceptionHelper.HandleExceptions(exception, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_UNABLETOCALCULATEDISKSPACE"));
            }
        }

        public override void OnUserChanged(UserDomain newUser)
        {
            IsAllSelected = false;
            NotifyPropertyChanged(nameof(IsExportEnable));
            base.OnUserChanged(newUser);
        }

        #endregion

        #region Protected Methods

        protected override void DisposeManaged()
        {
            DiskSpaceModel = null;
        }

        #endregion
    }
}
