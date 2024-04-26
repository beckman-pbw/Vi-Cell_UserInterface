using System;
using System.Collections.ObjectModel;
using System.Linq;
using ScoutDomains.Reports.ScheduledExports;
using ScoutLanguageResources;
using ScoutModels;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using ScoutViewModels.ViewModels.Interfaces;

namespace ScoutViewModels.ViewModels.Reports
{
    public class AuditLogScheduledExportsViewModel : BaseViewModel, IScheduledExportsViewModel
    {
        public AuditLogScheduledExportsViewModel(IScheduledExportsService scheduledExportsService)
        {
            _scheduledExportsService = scheduledExportsService;
            ReportTitle = LanguageResourceHelper.Get("LID_LogsList_ScheduledLogExports");
            ExportFilenameColumnHeader = LanguageResourceHelper.Get("LID_Label_AuditLogScheduledExportFileName");
            RefreshList();
        }

        #region Properties & Fields

        private readonly IScheduledExportsService _scheduledExportsService;

        public string ReportTitle
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ExportFilenameColumnHeader
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<AuditLogScheduledExportDomain> ScheduledExports
        {
            get { return GetProperty<ObservableCollection<AuditLogScheduledExportDomain>>(); }
            set { SetProperty(value); }
        }

        public AuditLogScheduledExportDomain SelectedScheduledExport
        {
            get { return GetProperty<AuditLogScheduledExportDomain>(); }
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
            ScheduledExports = _scheduledExportsService.GetAuditLogScheduledExports().ToObservableCollection();
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
            var args = new ScheduledExportAuditLogsDialogEventArgs<AuditLogScheduledExportDomain>(
                new AuditLogScheduledExportDomain(), false);
            if (DialogEventBus<AuditLogScheduledExportDomain>.ScheduledExportAuditLogsDialog(this, args) == true)
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
            var clone = (AuditLogScheduledExportDomain)SelectedScheduledExport.Clone(); // we don't want to send the OG
            var args = new ScheduledExportAuditLogsDialogEventArgs<AuditLogScheduledExportDomain>(clone, true);
            if (DialogEventBus<AuditLogScheduledExportDomain>.ScheduledExportAuditLogsDialog(this, args) == true)
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