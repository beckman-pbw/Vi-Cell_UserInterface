using Ninject.Extensions.Logging;
using ScoutDomains.Reports.ScheduledExports;
using ScoutLanguageResources;
using ScoutModels.Interfaces;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Events;
using System;
using System.IO;
using System.Windows;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class ScheduledExportAuditLogsAddEditDialogViewModel : BaseDialogViewModel
    {
        public ScheduledExportAuditLogsAddEditDialogViewModel(
            ScheduledExportAuditLogsDialogEventArgs<AuditLogScheduledExportDomain> args, 
            Window parentWindow, IScheduledExportsService scheduledExportsService,
            ISmtpSettingsService smtpSettings, ILogger logger, IFileSystemService fileSystemService,
            IDialogCaller dialogCaller, IUserService userService) 
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

        public AuditLogScheduledExportDomain ScheduledExport
        {
            get { return GetProperty<AuditLogScheduledExportDomain>(); }
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
                initialPath = FileSystem.GetDefaultExportPath(ScoutModels.LoggedInUser.CurrentUserId);

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

                if (!ScheduledExport.IsValid())
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MsgBox_IncludeLogsSelectionIsNotValid"));
                    return false;
                }

                // We don't need to validate ScheduledExport.DataFilterCriteria

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