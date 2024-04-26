using ScoutDomains.Reports.RunSummary;

namespace ScoutModels.Reports
{
    public class RunSummaryReportModel
    {
        public RunSummaryReportDomain RunSummaryReportDomainInstance { get; set; }

        public RunSummaryReportModel()
        {
            RunSummaryReportDomainInstance = new RunSummaryReportDomain();
        }
    }
}