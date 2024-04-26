using log4net;
using ScoutUtilities.CustomEventArgs;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using ScoutUtilities.Enums;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace ScoutUtilities.Events
{
    public static class DialogEventBus<T>
    {
        public static Func<Dispatcher> GetUiDispatcher => DialogEventBus.GetUiDispatcher;

        public static EventHandler<CreateSampleSetEventArgs<T>> CreateSampleSetDialogRequested;
        public static EventHandler<SampleResultDialogEventArgs<T>> SampleResultsDialogRequested;
        public static EventHandler<AdvanceSampleSettingsDialogEventArgs<T>> AdvanceSampleSettingsDialogRequested;
        public static EventHandler<AddEditUserDialogEventArgs<T>> AddEditUserDialogRequested;
        public static EventHandler<SampleSetSettingsDialogEventArgs<T>> SampleSetSettingsDialogRequested;
        public static EventHandler<ScheduledExportDialogEventArgs<T>> ScheduledExportDialogRequested;
        public static EventHandler<ScheduledExportAuditLogsDialogEventArgs<T>> ScheduledExportAuditLogsDialogRequested;

        public static bool? CreateSampleSetDialog(object caller, CreateSampleSetEventArgs<T> args)
        {
            CreateSampleSetDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? SampleResultsDialog(object caller, SampleResultDialogEventArgs<T> args)
        {
            SampleResultsDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? AdvanceSampleSettingsDialog(object caller, AdvanceSampleSettingsDialogEventArgs<T> args)
        {
            AdvanceSampleSettingsDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? AddEditUserDialog(object caller, AddEditUserDialogEventArgs<T> args)
        {
            AddEditUserDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? SampleSetSettingsDialog(object caller, SampleSetSettingsDialogEventArgs<T> args)
        {
            SampleSetSettingsDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? ScheduledExportDialog(object caller, ScheduledExportDialogEventArgs<T> args)
        {
            ScheduledExportDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? ScheduledExportAuditLogsDialog(object caller, ScheduledExportAuditLogsDialogEventArgs<T> args)
        {
            ScheduledExportAuditLogsDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }
    }

    public static class DialogEventBus
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Func<Dispatcher> GetUiDispatcher { get; set; }

        public static EventHandler<LoginEventArgs> LoginRequested;
        public static EventHandler<DialogBoxEventArgs> DialogBoxRequested;
        public static EventHandler<MessageHubEventArgs> MessageHubRequested;
        public static EventHandler<UserProfileDialogEventArgs> UserProfileRequested;
        public static EventHandler<ChangePasswordEventArgs> ChangePasswordRequested;
        public static EventHandler<LargeLoadingScreenEventArgs> LargeLoadingScreenRequested;
        public static EventHandler<AddSignatureEventArgs> AddSignatureRequested;
        public static EventHandler<SaveCellTypeEventArgs> SaveCellTypeDialogRequested;
        public static EventHandler<SelectCellTypeEventArgs> SelectCellTypeDialogRequested;
        public static EventHandler<ExpandedImageGraphEventArgs> ExpandedImageGraphRequested;
        public static EventHandler<DeleteSampleResultsEventArgs> DeleteSampleResultsRequested;
        public static EventHandler<ExportSampleResultEventArgs> ExportSampleResultsRequested;
        public static EventHandler<EmptySampleTubesEventArgs> EmptySampleTubesRequested;
        public static EventHandler<InstrumentStatusEventArgs> InstrumentStatusRequested;
        public static EventHandler<ReagentStatusEventArgs> ReagentStatusRequested;
        public static EventHandler<DustReferenceEventArgs> DustReferenceRequested;
        public static EventHandler<SetFocusEventArgs> SetFocusRequested;
        public static EventHandler<DecontaminateEventArgs> DecontaminateRequested;
        public static EventHandler<ReplaceReagentPackEventArgs> ReplaceReagentPackRequested;
        public static EventHandler<AddCellTypeEventArgs> AddCellTypeRequested;
        public static EventHandler<ExitUiDialogEventArgs> ExitUiDialogRequested;
        public static EventHandler<OpenSampleEventArgs> OpenSampleDialogRequested;
        public static EventHandler<InactivityEventArgs> InactivityDialogRequested;
        public static EventHandler<SequentialNamingEventArgs> SequentialNamingDialogRequested;
        public static EventHandler<FilterSampleSetsEventArgs> FilterSampleSetsDialogRequested;
        public static EventHandler<InfoDialogEventArgs> InfoDialogRequested;
        public static EventHandler<ActiveDirectoryConfigDialogEventArgs> ActiveDirectoryConfigDialogRequested;
        public static EventHandler<EditSecuritySettingsDialogEventArgs> EditSecuritySettingsDialogRequested;
        public static EventHandler<BaseDialogEventArgs> SystemLockDialogRequested;
        public static EventHandler<BaseDialogEventArgs> RemoveACupConcentrationDialogRequested;

        public static LoginResult? Login(object caller, LoginEventArgs args)
        {
            LoginRequested?.Invoke(caller, args);
            return args.LoginResult;
        }

        public static bool? DialogBox(object caller, DialogBoxEventArgs args)
        {
            DialogBoxRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? DialogBoxOk(object caller, string message, string title = null)
        {
            var args = new DialogBoxEventArgs(message, title);
            DialogBoxRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? DialogBoxOkCancel(object caller, string message, string title = null)
        {
            var args = new DialogBoxEventArgs(message, title, DialogButtons.OkCancel);
            DialogBoxRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? DialogBoxYesNo(object caller, string message, string title = null, DialogLocation dialogLocation = DialogLocation.CenterApp)
        {
            var args = new DialogBoxEventArgs(message, title, DialogButtons.YesNo, null, null, MessageBoxImage.None,
                dialogLocation, true, null, null, false, null, null);
            DialogBoxRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? DialogBoxRemoveACupConcentration(object caller, DialogBoxEventArgs args)
        {
            DialogBoxRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static void ShowMessageHub(object caller, MessageHubEventArgs args)
        {
            MessageHubRequested?.Invoke(caller, args);
        }

        public static bool? UserProfileDialog(object caller, UserProfileDialogEventArgs args)
        {
            UserProfileRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? ChangePasswordDialog(object caller, ChangePasswordEventArgs args)
        {
            ChangePasswordRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? LargeLoadingScreen(object caller, LargeLoadingScreenEventArgs args)
        {
            LargeLoadingScreenRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? AddSignature(object caller, AddSignatureEventArgs args)
        {
            AddSignatureRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? OpenSelectFileDialog(object caller, SelectFileDialogEventArgs args)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    DefaultExt = args.DefaultExt,
                    Filter = args.Filter,
                    InitialDirectory = args.InitialDirectory
                };

                if (openFileDialog.ShowDialog() != true) return false;

                args.FullFilePathSelected = openFileDialog.FileName;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to open/use SaveFileDialog", ex);
                return false;
            }
        }

        public static bool? OpenSaveFileDialog(object caller, SaveFileDialogEventArgs args)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    DefaultExt = args.DefaultExt,
                    Filter = args.Filter,
                    FileName = args.DefaultFileName,
                    InitialDirectory = args.InitialDirectory,
                    OverwritePrompt = args.OverwritePrompt
                };

                if (saveFileDialog.ShowDialog() != true) return false;

                args.FullFilePathSelected = saveFileDialog.FileName;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to open/use SaveFileDialog", ex);
                return false;
            }
        }

        public static bool? OpenFolderSelectDialog(object caller, FolderSelectDialogEventArgs args)
        {
            try
            {
                var folderBrowserDialog = new FolderBrowserDialog
                {
                    SelectedPath = args.SelectedFolderPath,
                    ShowNewFolderButton = args.ShowNewFolderButton,
                    Description = args.DialogDescription
                };

                if(!System.IO.Path.IsPathRooted(folderBrowserDialog.SelectedPath))
                {
                    var drives = System.IO.DriveInfo.GetDrives();
                    foreach (var d in drives)
                    {
                        var path = System.IO.Path.Combine(d.Name, @"Instrument\Export");
                        if (System.IO.Directory.Exists(path))
                        {
                            folderBrowserDialog.SelectedPath = path;
                            break;
                        }
                    }
                }

                var result = folderBrowserDialog.ShowDialog();
                if (result != DialogResult.OK) return false;

                args.SelectedFolderPath = folderBrowserDialog.SelectedPath;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to open/use FolderSelectDialog", ex);
                args.ThrownException = ex;
                return null;
            }
        }

        public static bool? SaveCellTypeDialog(object caller, SaveCellTypeEventArgs args)
        {
            SaveCellTypeDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? SelectCellTypeDialog(object caller, SelectCellTypeEventArgs args)
        {
            SelectCellTypeDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? ExpandedImageGraph(object caller, ExpandedImageGraphEventArgs args)
        {
            ExpandedImageGraphRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? DeleteSampleResultsDialog(object caller, DeleteSampleResultsEventArgs args)
        {
            DeleteSampleResultsRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? ExportSampleResultsDialog(object caller, ExportSampleResultEventArgs args)
        {
            ExportSampleResultsRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? EmptySampleTubesDialog(object caller, EmptySampleTubesEventArgs args)
        {
            EmptySampleTubesRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? InstrumentStatusDialog(object caller, InstrumentStatusEventArgs args)
        {
            InstrumentStatusRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? ReagentStatusDialog(object caller, ReagentStatusEventArgs args)
        {
            ReagentStatusRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? DustReferenceDialog(object caller, DustReferenceEventArgs args)
        {
            DustReferenceRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? SetFocusDialog(object caller, SetFocusEventArgs args)
        {
            SetFocusRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? DecontaminateDialog(object caller, DecontaminateEventArgs args)
        {
            DecontaminateRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? ReplaceReagentPackDialog(object caller, ReplaceReagentPackEventArgs args)
        {
            ReplaceReagentPackRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? AddCellTypeDialog(object caller, AddCellTypeEventArgs args)
        {
            AddCellTypeRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? ExitUiDialog(object caller, ExitUiDialogEventArgs args)
        {
            ExitUiDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? OpenSampleDialog(object caller, OpenSampleEventArgs args)
        {
            OpenSampleDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? InactivityDialog(object caller, InactivityEventArgs args)
        {
            InactivityDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? SequentialNamingDialog(object caller, SequentialNamingEventArgs args)
        {
            SequentialNamingDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? FilterSampleSetsDialog(object caller, FilterSampleSetsEventArgs args)
        {
            FilterSampleSetsDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? InfoDialog(object caller, InfoDialogEventArgs args)
        {
            InfoDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? ActiveDirectoryConfigDialog(object caller, ActiveDirectoryConfigDialogEventArgs args)
        {
            ActiveDirectoryConfigDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? EditSecuritySettingsDialog(object caller, EditSecuritySettingsDialogEventArgs args)
        {
            EditSecuritySettingsDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }

        public static bool? SystemLockDialog(object caller, BaseDialogEventArgs args)
        {
            SystemLockDialogRequested?.Invoke(caller, args);
            return args.DialogResult;
        }
    }
}
