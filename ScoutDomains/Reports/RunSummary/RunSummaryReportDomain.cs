using System.Collections.Generic;
using ScoutDomains.Reports.Common;

namespace ScoutDomains.Reports.RunSummary
{
    public class RunSummaryReportDomain
    {
        public List<ReportMandatoryHeaderDomain> ReportMandatoryHeaderDomainList { get; set; }
        public List<RunSummaryReportTableDomain> RunSummaryReportTableHeaderList { get; set; }
        public List<RunSummaryReportTableDomain> RunSummaryReportTableDataList { get; set; }
        public List<RunSummaryReportTableVisibility> RunSummaryReportTableVisibilityList { get; set; }
        public List<RunSummaryTableSize> RunSummaryTableSizeList { get; set; }
    }
}