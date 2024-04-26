using ApiProxies.Generic;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutModels.Reports;
using ScoutUtilities;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoutViewModels.ViewModels.Reports
{
    public class LogPanelViewModel : BaseViewModel
    {
        public LogPanelViewModel(int reportId)
        {
            ReportId = reportId;
            LogsModel = new LogsModel();
            switch (reportId)
            {
                case 1:
                    ReportLogName = LanguageResourceHelper.Get("LID_LogsList_AuditLog");
                    LoadAuditLog();
                    break;
                case 2:
                    ReportLogName = LanguageResourceHelper.Get("LID_LogsList_SampleActivity");
                    LoadSampleActivityLog();
                    break;
                case 3:
                    ReportLogName = LanguageResourceHelper.Get("LID_LogsList_SystemError");
                    LoadErrorLog();
                    break;
                case 4:
                    ReportLogName = LanguageResourceHelper.Get("LID_LogsList_CalibraionLog");
                    LoadCalibrationLog();
                    break;
            }
        }

        #region Properties & Fields

        public LogsModel LogsModel;

        public int ReportId
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public string ReportLogName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsExportEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public List<ErrorLogDomain> ErrorLogDomainList
        {
            get { return GetProperty<List<ErrorLogDomain>>(); }
            set { SetProperty(value); }
        }

        public List<AuditLogDomain> AuditLogDomainList
        {
            get { return GetProperty<List<AuditLogDomain>>(); }
            set { SetProperty(value); }
        }

        public List<CalibrationActivityLogDomain> CalibrationErrorLogList
        {
            get { return GetProperty<List<CalibrationActivityLogDomain>>(); }
            set { SetProperty(value); }
        }

        public List<SampleActivityDomain> SampleActivityLogList
        {
            get { return GetProperty<List<SampleActivityDomain>>(); }
            set { SetProperty(value); }
        }

        public bool IsAuditLogLoaded
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsSystemLogLoaded
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsSampleActivityLogLoaded
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsCalibrationLogLoaded
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        private RelayCommand _exportCommand;
        public RelayCommand ExportCommand => _exportCommand ?? (_exportCommand = new RelayCommand(ExportData));

        private void ExportData()
        {
            string fileName;
            switch (ReportId)
            {
                case 1:
                    fileName = "AuditLog_" + Misc.ConvertToFileNameFormat(DateTime.Now);
                    ExportModel.PromptAndExportReportLogs(AuditLogDomainList, ReportId, fileName);
                    break;
                case 2:
                    fileName = "SampleActivityLog_" + Misc.ConvertToFileNameFormat(DateTime.Now);
                    ExportModel.PromptAndExportReportLogs(SampleActivityLogList, ReportId, fileName);
                    break;
                case 3:
                    fileName = "SystemErrorLog_" + Misc.ConvertToFileNameFormat(DateTime.Now);
                    ExportModel.PromptAndExportReportLogs(ErrorLogDomainList, ReportId, fileName);
                    break;
                case 4:
                    fileName = "ConcentrationSlopeHistoryLog_" + Misc.ConvertToFileNameFormat(DateTime.Now);
                    ExportModel.PromptAndExportReportLogs(CalibrationErrorLogList, ReportId, fileName);
                    break;
            }
        }

        #endregion

        #region Public Methods

        public void LoadCalibrationLog()
        {
            IsCalibrationLogLoaded = false;
            Task.Run(() =>
            {
                try
                {
                    LogsModel.RetrieveCalibrationActivityLog(calibration_type.cal_All);
                    CalibrationErrorLogList = new List<CalibrationActivityLogDomain>(LogsModel.CalibrationErrorLogList);
                    IsExportEnable = false;

                    if (CalibrationErrorLogList.Count > 0)
                    {
                        IsCalibrationLogLoaded = true;
                        IsExportEnable = true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to retrieve and update the calibration logs", ex);
                }
            });
        }

        public void LoadErrorLog()
        {
            IsSystemLogLoaded = false;
            Task.Run(() =>
            {
                try
                {
                    LogsModel.RetrieveErrorLog();
                    ErrorLogDomainList = new List<ErrorLogDomain>(LogsModel.ErrorLogDomainList);
                    IsExportEnable = false;

                    if (ErrorLogDomainList.Count > 0)
                    {
                        IsSystemLogLoaded = true;
                        IsExportEnable = true;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_LOGS_RETRIEVE_INSTR_LOG"));
                }
            });
        }

        public void LoadSampleActivityLog()
        {
            IsSampleActivityLogLoaded = false;
            Task.Run(() =>
            {
                try
                {
                    LogsModel.RetrieveSampleActivityLog();
                    SampleActivityLogList = new List<SampleActivityDomain>(LogsModel.SampleActivityLogList);
                    IsExportEnable = false;

                    if (SampleActivityLogList.Count > 0)
                    {
                        IsSampleActivityLogLoaded = true;
                        IsExportEnable = true;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_LOGS_SAMPLE_ACTIVITY_LOG"));
                }
            });
        }

        public void LoadAuditLog()
        {
            IsAuditLogLoaded = false;
            Task.Run(() =>
            {
                try
                {
                    LogsModel.RetrieveAuditLog();
                    AuditLogDomainList = new List<AuditLogDomain>(LogsModel.AuditLogDomainList);
                    IsExportEnable = false;

                    if (AuditLogDomainList.Count > 0)
                    {
                        IsAuditLogLoaded = true;
                        IsExportEnable = true;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_LOGS_RETRIEVE_AUDIT_LOG"));
                }
            });
        }

        #endregion
    }
}