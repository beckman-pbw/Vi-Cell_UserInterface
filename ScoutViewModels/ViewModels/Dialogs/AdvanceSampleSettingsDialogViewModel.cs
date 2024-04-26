using ScoutLanguageResources;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Events;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class AdvanceSampleSettingsDialogViewModel : BaseDialogViewModel
    {
        public AdvanceSampleSettingsDialogViewModel(AdvanceSampleSettingsDialogEventArgs<AdvancedSampleSettingsViewModel> args, 
            Window parentWindow, IFileSystemService fileSystemService) : base(args, parentWindow)
        {
            _fileSystemService = fileSystemService;

            DialogTitle = LanguageResourceHelper.Get("LID_TabItem_AdvanceOption");
            DialogIconDrawBrush = (DrawingBrush) Application.Current.Resources["SettingsCog"];

            AdvancedSampleSettingsVm = args.AdvancedSampleSettingsViewModel;
        }

        #region Properties & Fields

        private readonly IFileSystemService _fileSystemService;

        public AdvancedSampleSettingsViewModel AdvancedSampleSettingsVm
        {
            get { return GetProperty<AdvancedSampleSettingsViewModel>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        private RelayCommand _selectExportDirectoryCommand;
        public RelayCommand SelectExportDirectoryCommand => _selectExportDirectoryCommand ?? (_selectExportDirectoryCommand = new RelayCommand(PerformSelectExportDir, CanPerformSelectExportDir));

        private bool CanPerformSelectExportDir()
        {
            return true;
        }

        private void PerformSelectExportDir()
        {
            var myPath = AdvancedSampleSettingsVm.ExportSampleDirectory;
            if (!FileSystem.IsFolderValidForExport(myPath) || !Directory.Exists(myPath))
            {
                if (FileSystem.IsFolderValidForExport(ScoutModels.LoggedInUser.CurrentExportPath) &&
                    Directory.Exists(ScoutModels.LoggedInUser.CurrentExportPath))
                    myPath = ScoutModels.LoggedInUser.CurrentExportPath;
                else
                    myPath = FileSystem.GetDefaultExportPath(ScoutModels.LoggedInUser.CurrentUserId);
            }

            var args = new FolderSelectDialogEventArgs(myPath);
            if (DialogEventBus.OpenFolderSelectDialog(this, args) != true)
                return;

            if (ValidateExportPath(args.SelectedFolderPath))
            {
                AdvancedSampleSettingsVm.ExportSampleDirectory = args.SelectedFolderPath;
            }
            else if (!FileSystem.IsFolderValidForExport(AdvancedSampleSettingsVm.ExportSampleDirectory))
            {
                if (FileSystem.IsFolderValidForExport(ScoutModels.LoggedInUser.CurrentExportPath))
                    AdvancedSampleSettingsVm.ExportSampleDirectory = ScoutModels.LoggedInUser.CurrentExportPath;
                else
                    AdvancedSampleSettingsVm.ExportSampleDirectory = FileSystem.GetDefaultExportPath("");
            }
        }

        private bool ValidateExportPath(string path)
        {                        
            if (ScoutUtilities.Common.FileSystem.IsFolderValidForExport(path))
                return true;

            var invalidPath = LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_PathError");
            var msg = $"{invalidPath}";
            if (FileSystem.IsPathValid(path))
            {
                var drive = Path.GetPathRoot(path);
                if (drive.ToUpper().StartsWith("C:"))
                    msg += "\n" + LanguageResourceHelper.Get("LID_MSGBOX_ExportPathError");
            }
            DialogEventBus.DialogBoxOk(this, msg);
            return false;
        }

        private RelayCommand _selectAppendDirectoryCommand;
        public RelayCommand SelectAppendDirectoryCommand => _selectAppendDirectoryCommand ?? (_selectAppendDirectoryCommand = new RelayCommand(PerformSelectAppendDir, CanPerformSelectAppendDir));

        private bool CanPerformSelectAppendDir()
        {
            return true;
        }

        private void PerformSelectAppendDir()
        {
            var args = new FolderSelectDialogEventArgs(AdvancedSampleSettingsVm.AppendExportDirectory);
            if (DialogEventBus.OpenFolderSelectDialog(this, args) != true)
                return;

            if (ValidateExportPath(args.SelectedFolderPath))
            {
                AdvancedSampleSettingsVm.AppendExportDirectory = args.SelectedFolderPath;
            }
            else if (!FileSystem.IsFolderValidForExport(AdvancedSampleSettingsVm.AppendExportDirectory))
            {
                AdvancedSampleSettingsVm.AppendExportDirectory = FileSystem.GetDefaultExportPath("");
            }
        }

        #endregion
    }
}