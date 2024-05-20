using ScoutDomains;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System.Collections.Generic;

namespace ScoutModels.Reports
{
    public class ReportsModel : BaseNotifyPropertyChanged
    {
        public ReportsModel()
        {
        }

        /// <summary>
        /// The report name list
        /// </summary>
        private IList<ReportsDomain> _reportNameList;

        public IList<ReportsDomain> ReportNameList
        {
            get
            {
                if (_reportNameList == null)
                {
                    _reportNameList = new List<ReportsDomain>(LoadResultReportsList());
                }

                return _reportNameList;
            }
            set { _reportNameList = value; }
        }


        private IList<ReportsDomain> _logsNameList;

        public IList<ReportsDomain> LogsNameList
        {
            get
            {
                if (_logsNameList == null)
                {
                    _logsNameList = new List<ReportsDomain>(LoadReportLogsNameList());
                }

                return _logsNameList;
            }
            set
            {
                _logsNameList = value;
                NotifyPropertyChanged("LogsNameList");
            }
        }

        private List<ReportsDomain> LoadResultReportsList()
        {
            var reportNameList = new List<ReportsDomain>
            {
                new ReportsDomain
                {
                    ReportName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GridLabel_CompletedRunSummary"),
                    ReportId = 1
                },
                new ReportsDomain
                {
                    ReportName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GridLabel_RunResult"),
                    ReportId = 2
                },
                new ReportsDomain
                {
                    ReportName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_QualityControl"),
                    ReportId = 4
                },
                new ReportsDomain
                {
                    ReportName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_UsersLabel_CellType"),
                    ReportId = 5
                },
                new ReportsDomain
                {
                    ReportName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_POPUPHeader_InstrumentStatus"),
                    ReportId = 6
                }
            };
            if ((LoggedInUser.CurrentUserRoleId == ScoutUtilities.Enums.UserPermissionLevel.eAdministrator) ||
                (LoggedInUser.CurrentUserRoleId == ScoutUtilities.Enums.UserPermissionLevel.eService))
            {
                var rep = new ReportsDomain
                {
                    ReportName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GridLabel_ScheduledDataExports"),
                    ReportId = 7
                };
                reportNameList.Add(rep);
            }

            return reportNameList;
        }

        private List<ReportsDomain> LoadReportLogsNameList()
        {
            var logsNameList = new List<ReportsDomain>();
            ReportsDomain temp;

            if (HardwareManager.HardwareSettingsModel.InstrumentType != InstrumentType.ViCELL_GO_Instrument)
            {
                temp = new ReportsDomain
                {
                    ReportLogName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_LogsList_AuditLog"),
                    ReportId = 1
                };
                logsNameList.Add(temp);
            }

            temp = new ReportsDomain
            {
                ReportLogName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_LogsList_SampleActivity"),
                ReportId = 2
            };
            logsNameList.Add(temp);

            temp = new ReportsDomain
            {
                ReportLogName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_LogsList_SystemError"),
                ReportId = 3
            };
            logsNameList.Add(temp);

            temp = new ReportsDomain
            {
                ReportLogName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_LogsList_CalibraionLog"),
                ReportId = 4
            };
            logsNameList.Add(temp);

            if ((LoggedInUser.CurrentUserRoleId == ScoutUtilities.Enums.UserPermissionLevel.eAdministrator) ||
                (LoggedInUser.CurrentUserRoleId == ScoutUtilities.Enums.UserPermissionLevel.eService))
            {
                var rep = new ReportsDomain
                {
                    ReportLogName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_LogsList_ScheduledLogExports"),
                    ReportId = 5
                };
                logsNameList.Add(rep);
            }

            return logsNameList;
        }
    }
}