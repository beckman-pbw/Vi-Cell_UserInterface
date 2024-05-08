using ApiProxies.Generic;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Settings;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.UIConfiguration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ScoutModels.Interfaces;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class InstrumentStatusDialogViewModel : BaseDialogViewModel
    {
        public InstrumentStatusDialogViewModel(InstrumentStatusEventArgs args, System.Windows.Window parentWindow, 
            ILockManager lockManager, IInstrumentStatusService instrumentStatusService, IAutomationSettingsService automationSettingsService) : base(args, parentWindow)
        {
            _lockManager = lockManager;
            ShowDialogTitleBar = true;
            DialogTitle = LanguageResourceHelper.Get("LID_POPUPHeader_InstrumentStatus");
            _instrumentStatusService = instrumentStatusService;
            _diskSpaceModel = new DiskSpaceModel();

            var autoConfig = automationSettingsService.GetAutomationConfig();
            IsACupEnabled = Misc.ByteToBool(autoConfig.ACupIsEnabled);

            SerialNumber = HardwareManager.HardwareSettingsModel.HardwareSettingsDomain.SerialNumber;
            ApplicationVersion = UISettings.SoftwareVersion;
            FirmwareVersion = HardwareManager.HardwareSettingsModel.HardwareSettingsDomain.FirmwareVersion;

            SetUserLevelAccess();
            GetDiskSpace();

            _updateInstrumentUiSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe(UpdateInstrumentUi);
        }

        protected override void DisposeUnmanaged()
        {
            _updateInstrumentUiSubscriber?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties & Fields

        private DiskSpaceModel _diskSpaceModel;
        private readonly ILockManager _lockManager;
        private readonly IInstrumentStatusService _instrumentStatusService;

        public bool IsACupEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public int RemainingTubeTrayCapacity
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public string SerialNumber
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
        
        public string FirmwareVersion
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ApplicationVersion
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string DiskFreeSpace
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public uint SystemTotalSampleCount
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public string StagePosition
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsCleanWasteTubeEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsSharedClearEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsStorageLoaded
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public DateTime LastCalibrationDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public DateTime LastACupCalibrationDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public bool EnableCalibrationControl
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsBlurEffectEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public double PercentDiskSpaceOther
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double PercentDiskSpaceData
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double PercentDiskSpaceExport
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double PercentDiskSpaceFree
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        private RelayCommand _dustCollectionCommand;
        private IDisposable _updateInstrumentUiSubscriber;
        public RelayCommand DustCollectionCommand => _dustCollectionCommand ?? (_dustCollectionCommand = new RelayCommand(PerformDustCollection));

        private void PerformDustCollection()
        {
            DialogEventBus.DustReferenceDialog(this, new DustReferenceEventArgs());
        }

        private RelayCommand _setFocusCommand;
        public RelayCommand SetFocusCommand => _setFocusCommand ?? (_setFocusCommand = new RelayCommand(PerformSetFocus));

        private void PerformSetFocus()
        {
            DialogEventBus.SetFocusDialog(this, new SetFocusEventArgs());
        }

        private RelayCommand _cleanTubesCommand;
        public RelayCommand CleanTubesCommand => _cleanTubesCommand ?? (_cleanTubesCommand = new RelayCommand(PerformClean));

        private void PerformClean()
        {
            DialogEventBus.EmptySampleTubesDialog(this, new EmptySampleTubesEventArgs());
        }

        private RelayCommand _cleanExportsCommand;
        public RelayCommand CleanExportsCommand => _cleanExportsCommand ?? (_cleanExportsCommand = new RelayCommand(PerformCleanExports));

        private void PerformCleanExports()
        {
            if (Directory.Exists(UISettings.ExportPath))
            {
                var files = Directory.GetFiles(UISettings.ExportPath);
                var folders = new DirectoryInfo(UISettings.ExportPath);
                if (files.Any() || folders.GetDirectories().Any())
                {
                    Array.ForEach(Directory.GetFiles(UISettings.ExportPath), File.Delete);

                    foreach (var dir in folders.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    PostToMessageHub(LanguageResourceHelper.Get("LID_MSGBOX_FileDeleted"));
                }
                else
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_NoFile"));
                }
            }
            else
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_NoDir"));
            }

            GetDiskSpace();
        }

        #endregion

        #region Event Handlers

        private void UpdateInstrumentUi(SystemStatusDomain systemStatusDomain)
        {
            if (!_instrumentStatusService.IsRunning())
            {
                SetUserLevelAccess();
            }
            LastCalibrationDate = DateTimeConversionHelper.FromSecondUnixToDateTime(systemStatusDomain.LastCalibratedDateConcentration);
            LastACupCalibrationDate = DateTimeConversionHelper.FromSecondUnixToDateTime(systemStatusDomain.LastCalibratedDateACupConcentration);
            RemainingTubeTrayCapacity = (int)systemStatusDomain.SampleTubeDisposalRemainingCapacity;
            SystemTotalSampleCount = systemStatusDomain.SystemTotalSampleCount;
            StagePosition = systemStatusDomain.StagePositionString;
        }

        #endregion

        #region Private Methods

        private void SetUserLevelAccess()
        {
            if (_instrumentStatusService.SystemStatus == SystemStatus.Pausing ||
                _instrumentStatusService.SystemStatus == SystemStatus.ProcessingSample ||
                _instrumentStatusService.SystemStatus == SystemStatus.Stopping)
            {
                IsCleanWasteTubeEnable = false;
            }
            else
            {
                IsCleanWasteTubeEnable = true;
            }

            if (LoggedInUser.CurrentUser?.RoleName == null ||
                _instrumentStatusService.SystemStatus != SystemStatus.Idle &&
                _instrumentStatusService.SystemStatus != SystemStatus.Paused)
            {
                EnableCalibrationControl = IsSharedClearEnable = false;
                return;
            }

            switch (LoggedInUser.CurrentUser.RoleID)
            {
                case UserPermissionLevel.eNormal:
                    EnableCalibrationControl = false;
                    IsSharedClearEnable = false;
                    IsBlurEffectEnable = false;
                    break;
                case UserPermissionLevel.eAdministrator:
                    IsSharedClearEnable = !_lockManager.IsLocked();
                    EnableCalibrationControl = !(_instrumentStatusService.IsPaused() || _lockManager.IsLocked());
                    IsBlurEffectEnable = false;
                    break;
                case UserPermissionLevel.eElevated:
                case UserPermissionLevel.eService:
                    EnableCalibrationControl = !_lockManager.IsLocked();
                    IsSharedClearEnable = false;
                    IsBlurEffectEnable = false;
                    EnableCalibrationControl = !(_instrumentStatusService.IsPaused() || _lockManager.IsLocked());
                    break;
            }
        }

        private async void GetDiskSpace()
        {
            try
            {
                // Waits until disk Space calculation task is complete.
                await Task.Run(CalculateDiskSpace);
                DiskFreeSpace = string.Format(LanguageResourceHelper.Get("LID_Label_Space"),
                    ScoutUtilities.Misc.ConvertBytesToSize(_diskSpaceModel.TotalFreeSpace),
                    ScoutUtilities.Misc.ConvertBytesToSize(_diskSpaceModel.TotalDiskSpace));
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_DISK_SPACE_CALCULATION"));
            }
            finally
            {
                // Once the task is completed, progress indication become hidden state
                IsStorageLoaded = true;
            }
        }

        private void CalculateDiskSpace()
        {
            try
            {
                _diskSpaceModel.CalculateDiskSpace();
                PercentDiskSpaceExport = _diskSpaceModel.PercentDiskSpaceExport;
                PercentDiskSpaceData = _diskSpaceModel.PercentDiskSpaceData;
                PercentDiskSpaceFree = _diskSpaceModel.PercentDiskSpaceFree;
                PercentDiskSpaceOther = _diskSpaceModel.PercentDiskSpaceOther;
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

        #endregion
    }
}