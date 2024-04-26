using ScoutDomains;
using ScoutDomains.Reports.ScheduledExports;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Interfaces;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using Org.BouncyCastle.Math.Field;
using ScoutDataAccessLayer.DAL;
using ScoutDomains.Analysis;
using ScoutModels.Settings;
using ScoutUtilities.UIConfiguration;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class ScheduledExportAddEditViewModel : BaseDialogViewModel
    {
        public ScheduledExportAddEditViewModel(
            ScheduledExportDialogEventArgs<SampleResultsScheduledExportDomain> args, Window parentWindow, 
            IScheduledExportsService scheduledExportsService, ICellTypeManager cellTypeManager, 
            IUserService userService, ISmtpSettingsService smtpSettings, ILogger logger,
            IFileSystemService fileSystemService, IDialogCaller dialogCaller)
            : base(args, parentWindow)
        {
            _scheduledExportsService = scheduledExportsService;
            _smtpSettings = smtpSettings;
            _fileSystemService = fileSystemService;
            _logger = logger;
            _dialogCaller = dialogCaller;
            _userService = userService;

            InEditMode = args.InEditMode;
            ScheduledExport = args.ScheduledExportDomain;
            if (!args.InEditMode)
            {
                ScheduledExport.IsEnabled = true;
            }
            // Populate the CellTypesAndQualityControls
            ScheduledExport.DataFilterCriteria.CellTypesAndQualityControls = LoggedInUser.GetCtQcs().ToObservableCollection();                
            var selectedCtQcName = ScheduledExport.DataFilterCriteria.SelectedCellTypeOrQualityControlGroup?.Name;
            if (!string.IsNullOrEmpty(selectedCtQcName))
            {
                ScheduledExport.DataFilterCriteria.SelectedCellTypeOrQualityControlGroup =
                    ScheduledExport.DataFilterCriteria.CellTypesAndQualityControls.GetCellTypeQualityControlByName(ScheduledExport.DataFilterCriteria
                        .SelectedCellTypeOrQualityControlGroup?.Name);
            }

            // Populate the Usernames
            var users = _userService.GetUserList().Select(u => u.UserID).ToList();
            users.Insert(0, LanguageResourceHelper.Get("LID_Label_All"));
            if (LoggedInUser.CurrentUserRoleId == UserPermissionLevel.eService)
                users.Add(ApplicationConstants.ServiceUser);
            ScheduledExport.DataFilterCriteria.Usernames = users.ToObservableCollection();
            if (string.IsNullOrEmpty(ScheduledExport.DataFilterCriteria.SelectedUsername))
            {
                ScheduledExport.DataFilterCriteria.SelectedUsername = ScheduledExport.DataFilterCriteria.Usernames.FirstOrDefault();
            }
        }

        #region Properties & Fields

        private readonly IScheduledExportsService _scheduledExportsService;
        private readonly ISmtpSettingsService _smtpSettings;
        private readonly IFileSystemService _fileSystemService;
        private readonly ILogger _logger;
        private readonly IDialogCaller _dialogCaller;
        private readonly IUserService _userService;

        public bool InEditMode
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public SampleResultsScheduledExportDomain ScheduledExport
        {
            get { return GetProperty<SampleResultsScheduledExportDomain>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        private RelayCommand _selectExportDirectoryCommand;
        public RelayCommand SelectExportDirectoryCommand => _selectExportDirectoryCommand ?? (_selectExportDirectoryCommand = new RelayCommand(PerformSelectDirectory));

        private void PerformSelectDirectory()
        {
            string initialPath = ScheduledExport.DestinationFolder;
            if (string.IsNullOrEmpty(initialPath) || !FileSystem.IsFolderValidForExport(initialPath))
                initialPath = FileSystem.GetDefaultExportPath(LoggedInUser.CurrentUserId);
            if (!Directory.Exists(initialPath))
                initialPath = FileSystem.GetDefaultExportPath("");

            var args = new FolderSelectDialogEventArgs(initialPath);
            if (_dialogCaller.OpenFolderSelectDialog(this, args) == true)
            {
                if (!FileSystem.IsFolderValidForExport(args.SelectedFolderPath))
                {
                    var invalidPath = LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_PathError");
                    var msg = $"{invalidPath}";
                    if (FileSystem.IsPathValid(args.SelectedFolderPath))
                    {
                        string drive = Path.GetPathRoot(args.SelectedFolderPath);
                        if (drive.ToUpper().StartsWith("C:"))
                            msg += "\n" + LanguageResourceHelper.Get("LID_MSGBOX_ExportPathError");

                    }
                    DialogEventBus.DialogBoxOk(this, msg);
                    return;
                }
                ScheduledExport.DestinationFolder = args.SelectedFolderPath;
            }
        }

        #endregion

        public bool FieldsAreValid()
        {
            try
            {
                if (!ScheduledExport.IsEnabled)
                    return true;

                if (!string.IsNullOrEmpty(ScheduledExport.NotificationEmail) && 
                    !_smtpSettings.IsValidEmail(ScheduledExport.NotificationEmail))
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_InvalidEmailAddress"));
                    return false;
                }

                if (!FileSystem.IsFolderValidForExport(ScheduledExport.DestinationFolder))
                {
                    var invalidPath = LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_PathError");
                    var msg = $"{invalidPath}";
                    if (FileSystem.IsPathValid(ScheduledExport.DestinationFolder))
                    {
                        string drive = Path.GetPathRoot(ScheduledExport.DestinationFolder);
                        if (drive.ToUpper().StartsWith("C:"))                        
                            msg += "\n" + LanguageResourceHelper.Get("LID_MSGBOX_ExportPathError");                        
                    }
                    DialogEventBus.DialogBoxOk(this, msg);
                    return false;
                }

                if (!_fileSystemService.DirectoryExists(ScheduledExport.DestinationFolder))
                {
                    // directory doesn't exist. create it?
                    if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_Question_DirectoryNotExistCreateIt")) == true)
                    {
                        if (!_fileSystemService.CreateDirectory(ScheduledExport.DestinationFolder))
                        {
                            // failed to create directory
                            DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MsgBox_FailedToCreateDirectory"));
                            return false;
                        }
                    }
                    else
                    {
                        // leave the dialog open with any work the user has done but not currently Valid
                        return false; // user said NO to "create dir?"
                    }
                }

                if (string.IsNullOrEmpty(ScheduledExport.FilenameTemplate) ||
                    !FileSystem.IsFileNameValid(ScheduledExport.FilenameTemplate))
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MsgBox_FilenameIsNotValid"));
                    return false;
                }

                if (!ScheduledExport.RecurrenceRule.RecurrenceRulesAreValid())
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MsgBox_RecurrenceRulesAreNotValid"));
                    return false;
                }

                if (!ScheduledExport.DataFilterCriteria.DataFilterIsValid())
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MsgBox_DataFilterValuesAreNotValid"));
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.Warn(e, $"Exception while validating scheduled export fields");
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MsgBox_ErrorWhileCreatingScheduledExport"));
                return false;
            }
        }

        protected override void OnAccept()
        {
            if (!FieldsAreValid())
            {
                // informative dialog boxes are shown in the FieldsAreValid method
                return;
            }

            if (InEditMode)
            {
                if (_scheduledExportsService.EditScheduledExport(ScheduledExport) == false)
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ScheduleExport_EditFailed"));
                    return;
                }
            }
            else
            {
                if (_scheduledExportsService.AddScheduledExport(ScheduledExport) == false)
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ScheduleExport_AddFailed"));
                    return;
                }
            }

            BAFW.BAppFW.Publish(new BAFW.BPublicEvent((uint)ExportManager.PubEvIds.ScheduledExportChanged));
            base.OnAccept();
        }
    }
}