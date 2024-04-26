using System.Collections.Generic;
using ScoutDomains.Reports.Common;

namespace ScoutDomains.Reports.CellTypes
{
    public class CellTypesReportDomain
    {
        public List<ReportMandatoryHeaderDomain> ReportMandatoryHeaderDomainList { get; set; }
        public List<CellTypesReportTableDomain> CellTypesReportTableDataList { get; set; }
        public List<CellTypesReportTableDomain> CellTypesReportTableHeaderList { get; set; }
        public List<ReportTableColumnWidth> CellTypesReportTableColumnWidthList { get; set; }
        public List<AnalysisTypeReportDomain> AnalysisTypeList { get; set; }
        public List<CellTypeReportCommonInfoDomain> CellTypeReportCommonInfoList { get; set; }
    }
}