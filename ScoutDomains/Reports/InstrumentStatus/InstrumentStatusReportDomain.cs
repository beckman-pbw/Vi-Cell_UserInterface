using System.Collections.Generic;
using ScoutDomains.Reports.Common;
using ScoutDomains.Reports.RunResult;

namespace ScoutDomains.Reports.InstrumentStatus
{
    public class InstrumentStatusReportDomain
    {
        public List<ReportMandatoryHeaderDomain> InstrumentStatusReportMandatoryHeaderDomainList { get; set; }
        public List<InstrumentStatusReportTableHeaderDomain> InstrumentStatusReportTableHeaderDomainList { get; set; }
        public List<InstrumentStatusTableVisibility> InstrumentStatusReportTableVisibilityList { get; set; }
        public List<ReportTableTemplate> InstrumentStatusReportAboutFirstTableList { get; set; }
        public List<ReportTableTemplate> InstrumentStatusReportAboutSecondTableList { get; set; }        
        public List<InstrumentStatusReportTableColumn> InstrumentStatusReportUsersTableList { get; set; }
        public List<InstrumentStatusReportTableColumn> InstrumentStatusReportCellTypesTableList { get; set; }
        public List<ReportTableTemplate> InstrumentStatusReportAppTypesTableList { get; set; }
        public List<InstrumentStatusReportCalibrationDomainTable> InstrumentStatusReportConcDataDomainList { get; set; }
        public List<InstrumentStatusReportCalibrationDomainTable> InstrumentStatusReportACupConcDataDomainList { get; set; }
        public List<ReportTableTemplate> InstrumentStatusReportConcSlopeTableList { get; set; }
        public List<ReportTableTemplate> InstrumentStatusReportACupConcSlopeTableList { get; set; }
        public List<ReportTableTemplate> InstrumentStatusReportStorageList { get; set; }
        public List<ReportTableTemplate> InstrumentStatusReportLowerLevelList { get; set; }
        public List<ReportTableTemplate> InstrumentStatusReportSensorStatusStateList { get; set; }
        public List<ReportTableTemplate> InstrumentStatusReportMotorStatusStateList { get; set; }
        public List<ReportTableTemplate> InstrumentStatusReportStatusStateList { get; set; }
        public List<ReportTableTemplate> InstrumentStatusReportVoltageList { get; set; }
        public List<ReportTableTemplate> InstrumentStatusReportTemperatureList { get; set; }
        public List<RunResultReagentParameterDomain> InstrumentStatusReagentParameterList { get; set; }
        public List<InstrumentStatusReportErrorDomain> InstrumentStatusReportSystemErrorList { get; set; }
        public List<InstrumentStatusReportSensorStatusDomain> InstrumentStatusReportSensorStatusList { get; set; }
        public List<ReportTableTemplate> InstrumentStatusReportRemainingTubesList { get; set; }
    }
}
