using ScoutDomains.Reports.ScheduledExports;
using ScoutLanguageResources;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using ScoutViewModels.ViewModels.Interfaces;
using ScoutModels;
using ScoutUtilities.Enums;

namespace ScoutViewModels.ViewModels.Reports.ReportViewModels
{
    public class ScheduledExportsViewModel : ReportWindowViewModel, IScheduledExportsViewModel
    {
        public ScheduledExportsViewModel(IScheduledExportsService scheduledExportsService)
        {
            _scheduledExportsService = scheduledExportsService;
            ReportTitle = LanguageResourceHelper.Get("LID_GridLabel_ScheduledDataExports");
            ReportWindowTitle = LanguageResourceHelper.Get("LID_GridLabel_ScheduledDataExports");
            ExportFilenameColumnHeader = LanguageResourceHelper.Get("LID_Label_ScheduledExportFileName");
            RefreshList();
        }

        #region Properties & Fields

        private readonly IScheduledExportsService _scheduledExportsService;

        public string ExportFilenameColumnHeader
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<SampleResultsScheduledExportDomain> ScheduledExports
        {
            get { return GetProperty<ObservableCollection<SampleResultsScheduledExportDomain>>(); }
            set { SetProperty(value); }
        }

        public SampleResultsScheduledExportDomain SelectedScheduledExport
        {
            get { return GetProperty<SampleResultsScheduledExportDomain>(); }
            set
            {
                SetProperty(value);
                EditScheduledExport.RaiseCanExecuteChanged();
                DeleteScheduledExport.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Methods

        private void RefreshList()
        {
            var selectedUuid = SelectedScheduledExport?.Uuid ?? new uuidDLL(Guid.Empty);
            ScheduledExports = _scheduledExportsService.GetSampleResultsScheduledExports().ToObservableCollection();
            if (!selectedUuid.IsNullOrEmpty())
            {
                SelectedScheduledExport = ScheduledExports
                    .FirstOrDefault(s => s.Uuid.Equals(selectedUuid));
            }
        }
        
        #endregion

        #region Commands

        #region AddScheduledExport

        private RelayCommand _addScheduledExport;
        public RelayCommand AddScheduledExport => _addScheduledExport ?? (_addScheduledExport = new RelayCommand(PerformAddScheduledExport, CanPerformAddScheduledExport));

        private bool CanPerformAddScheduledExport()
        {
            if (LoggedInUser.CurrentUser != null)
                return LoggedInUser.CurrentUser.RoleID != UserPermissionLevel.eService;
            return false;
        }

        private void PerformAddScheduledExport()
        {
            var args = new ScheduledExportDialogEventArgs<SampleResultsScheduledExportDomain>(
                new SampleResultsScheduledExportDomain(), false);
            if (DialogEventBus<SampleResultsScheduledExportDomain>.ScheduledExportDialog(this, args) == true)
            {
                RefreshList();
            }
        }

        #endregion

        #region EditScheduledExport

        private RelayCommand _editScheduledExport;
        public RelayCommand EditScheduledExport => _editScheduledExport ?? (_editScheduledExport = new RelayCommand(PerformEditScheduledExport, CanPerformEditScheduledExport));

        private bool CanPerformEditScheduledExport()
        {
            if (LoggedInUser.CurrentUser != null)
                return LoggedInUser.CurrentUser.RoleID != UserPermissionLevel.eService &&
                    SelectedScheduledExport != null;
            return false;
        }

        private void PerformEditScheduledExport()
        {
            var clone = (SampleResultsScheduledExportDomain) SelectedScheduledExport.Clone(); // we don't want to send the OG
            var args = new ScheduledExportDialogEventArgs<SampleResultsScheduledExportDomain>(clone, true);
            if (DialogEventBus<SampleResultsScheduledExportDomain>.ScheduledExportDialog(this, args) == true)
            {
                RefreshList();
            }
        }

        #endregion

        #region DeleteScheduledExport

        private RelayCommand _deleteScheduledExport;
        public RelayCommand DeleteScheduledExport => _deleteScheduledExport ?? (_deleteScheduledExport = new RelayCommand(PerformDeleteScheduledExport, CanPerformDeleteScheduledExport));

        private bool CanPerformDeleteScheduledExport()
        {
            if (LoggedInUser.CurrentUser != null)
                return LoggedInUser.CurrentUser.RoleID != UserPermissionLevel.eService &&
                SelectedScheduledExport != null;
            return false;
        }

        private void PerformDeleteScheduledExport()
        {
            var msg = string.Format(
                LanguageResourceHelper.Get("LID_Label_AreYouSureYouWantToDelete"),
                SelectedScheduledExport.Name);
            
            if (DialogEventBus.DialogBoxYesNo(this, msg) != true)
            {
                return;
            }

            if (_scheduledExportsService.DeleteScheduledExport(SelectedScheduledExport.Uuid))
            {
                RefreshList();
            }
            else
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ScheduleExport_DeleteFailed"));
            }
        }

        #endregion

        #endregion
    }
}
