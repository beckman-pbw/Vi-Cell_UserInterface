using ApiProxies.Generic;
using ScoutLanguageResources;
using ScoutModels;
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
using System.Threading.Tasks;
using System.IO;


namespace ScoutViewModels.ViewModels.Dialogs
{
    public class ExportSampleResultViewModel : BaseDialogViewModel
    {
        #region Constructor

        public ExportSampleResultViewModel(ExportSampleResultEventArgs args, System.Windows.Window parentWindow) : base(args, parentWindow)
        {
            ShowDialogTitleBar = true;
            DialogTitle = LanguageResourceHelper.Get("LID_Label_ExportSampleResults");
            _samplesToExport = args.SamplesToExport;

            if (FileSystem.IsFolderValidForExport(args.ExportFilePath) && Directory.Exists(args.ExportFilePath))
                ExportSampleResultLocation = args.ExportFilePath;
            else if (FileSystem.IsFolderValidForExport(LoggedInUser.CurrentExportPath) && Directory.Exists(LoggedInUser.CurrentExportPath))
                ExportSampleResultLocation = LoggedInUser.CurrentExportPath;
            else
                ExportSampleResultLocation = FileSystem.GetDefaultExportPath(ScoutModels.LoggedInUser.CurrentUserId);

            IsExportSampleProcessActive = false;
            NthImage = 1;
            ExportMessage = string.Empty;
            ExportPercentage = string.Empty;
        }

        #endregion

        #region Properties & Fields

        private List<uuidDLL> _samplesToExport;

        public bool IsExportSampleProcessActive
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                SelectExportLocationCommand.RaiseCanExecuteChanged();
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public int NthImage
        {
            get { return GetProperty<int>(); }
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public string ExportSampleResultLocation
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public string ExportMessage
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ExportPercentage
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Event Handlers

        private void OnExportSamplesModelExportProgress(object sender, SampleExportProgressEventArgs e)
        {
            DispatcherHelper.ApplicationExecute(() => { HandleProgressUpdates(e); });
        }

        private void HandleProgressUpdates(SampleExportProgressEventArgs e)
        {
            if (e == null || e.Error == null)
            {
                return;
            }

            if (e.Error != HawkeyeError.eSuccess)
            {
                ApiHawkeyeMsgHelper.ErrorCommon(e.Error.Value);
            }
            else
            {
                if (e.OperationIsComplete)
                {
                    IsExportSampleProcessActive = false;

                    ExportPercentage = $"100%";
                    ExportMessage = LanguageResourceHelper.Get("LID_MSGBOX_ExportSucces");

                    MessageBus.Default.Publish(LanguageResourceHelper.Get("LID_MSGBOX_ExportSucces"));

                    if (!string.IsNullOrEmpty(e.ExportFolderLocation))
                    {
                        FileSystem.OpenFolderLocationInExplorer(e.ExportFolderLocation);
                    }

                    Close(true);
                }
                else
                {
                    IsExportSampleProcessActive = true;
                    ExportPercentage = $"{e.PercentComplete}%";
                    ExportMessage = LanguageResourceHelper.Get("LID_Label_ExportInProgress");
                }
            }
        }

        #endregion

        #region Commands

        #region Select Export Location

        private RelayCommand _selectExportLocationCommand;
        public RelayCommand SelectExportLocationCommand => _selectExportLocationCommand ?? (_selectExportLocationCommand = new RelayCommand(SelectExportLocation, CanSelectExportLocation));

        private bool CanSelectExportLocation()
        {
            return !IsExportSampleProcessActive;
        }

        private void SelectExportLocation()
        {
            var args = new FolderSelectDialogEventArgs(ExportSampleResultLocation);
            var result = DialogEventBus.OpenFolderSelectDialog(this, args);

            if (result == true)
            {
                if (FileSystem.IsFolderValidForExport(args.SelectedFolderPath))
                {
                    ExportSampleResultLocation = args.SelectedFolderPath;
                }
                else
                {
                    var invalidPath = LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_PathError");
                    var msg = $"{invalidPath}";
                    if (FileSystem.IsPathValid(ExportSampleResultLocation))
                    {
                        string drive = Path.GetPathRoot(ExportSampleResultLocation);
                        if (drive.ToUpper().StartsWith("C:"))
                            msg += "\n" + LanguageResourceHelper.Get("LID_MSGBOX_ExportPathError");

                    }
                    DialogEventBus.DialogBoxOk(this, msg);
                    return;
                }
            }
            else if (result == null && args.ThrownException != null)
            {
                ExceptionHelper.HandleExceptions(args.ThrownException, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS"));
            }
        }

        #endregion

        public override bool Close(bool? result)
        {
            if (!IsExportSampleProcessActive)
                return base.Close(result);

            ExportMessage = LanguageResourceHelper.Get("LID_Label_ExportCancelInprogress");
            ExportPercentage = string.Empty;

            ExportManager.AoExportMgr.PublishCancelOffline();
            result = true;
            return base.Close(result);
        }

        public override bool CanAccept()
        {
            return NthImage > 0 && !string.IsNullOrEmpty(ExportSampleResultLocation) && !IsExportSampleProcessActive;
        }

        protected override void OnAccept()
        {
            if (!FileSystem.IsFolderValidForExport(ExportSampleResultLocation))
            {
                var invalidPath = LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_PathError");
                var msg = $"{invalidPath}";
                if (FileSystem.IsPathValid(ExportSampleResultLocation))
                {
                    string drive = Path.GetPathRoot(ExportSampleResultLocation);
                    if (drive.ToUpper().StartsWith("C:"))
                    {
                        msg += "\n" + LanguageResourceHelper.Get("LID_MSGBOX_ExportPathError");
                    }
                }
                DialogEventBus.DialogBoxOk(this, msg);
                ExportSampleResultLocation = FileSystem.GetDefaultExportPath(LoggedInUser.CurrentUserId);
                return;
            }

            IsExportSampleProcessActive = true;
            LoggedInUser.CurrentExportPath = ExportSampleResultLocation;

            ExportManager.EvExportOfflineReq.Publish(
                LoggedInUser.CurrentUserId, "",
                _samplesToExport, ExportSampleResultLocation, "",
                eExportImages.eExportNthImage, (ushort)NthImage, false,
                OnExportDataProgress,
                OnExportDataCompletion, 
                null);
        }

        #endregion


        private void OnExportDataProgress(HawkeyeError status, uuidDLL sampleId, int percent)
        {
            if (status == HawkeyeError.eSuccess)
            {
                var operationComplete = (percent >= 100);
                var msg = operationComplete
                    ? LanguageResourceHelper.Get("LID_Label_ExportCompressingInProgress")
                    : null;
                DispatcherHelper.ApplicationExecute(() => { HandleProgressUpdates(new SampleExportProgressEventArgs(percent, operationComplete, status, "", "")); });
            }
            else
            {                
                DispatcherHelper.ApplicationExecute(() => { HandleProgressUpdates(new SampleExportProgressEventArgs(status)); });
            }
        }

        private void OnExportDataCompletion(HawkeyeError status, string exportFile)
        {            
            if (status == HawkeyeError.eSuccess)
            {                
                DispatcherHelper.ApplicationExecute(() => { HandleProgressUpdates(new SampleExportProgressEventArgs(100, true, status, "", exportFile)); });
            }
            else
            {
                DispatcherHelper.ApplicationExecute(() => { HandleProgressUpdates(new SampleExportProgressEventArgs(status)); });
            }
        }

    }

}
