using System.Collections.Generic;
using ScoutDomains.Reports.Common;
using ScoutDomains.Reports.RunResult;

namespace ScoutDomains
{
    /// <summary>
    /// Class RunResultsReportDomain.
    /// </summary>
    public class RunResultsReportDomain
    {
        public List<ReportTableTemplate> RunResultAboutReportParameterDomainList { get; set; }
        public List<ReportTableTemplate> RunResultAnalysisParameterReportDomainList { get; set; }
        public List<ReportThreeColumnDomain> RunResultSignatureColumnList { get; set; }
        public List<RunResultTableHeaderDomain> RunResultTableHeaderDomainList { get; set; }
        public List<ReportMandatoryHeaderDomain> ReportMandatoryHeaderDomainList { get; set; }
        public List<ReportTableTemplate> SampleDetailsList { get; set; }
        public List<ReportTableTemplate> ImageDetailsList { get; set; }
        public List<RunResultTableVisibility> RunResultTableVisbilityList { get; set; }

        public List<RunResultsReportGraphTitlesDomain> RunResultsReportGraphTitlesDomainList { get; set; }
        public List<RunResultsReportGraphAxesDataDomain> RunResultsReportGraphAxesDataList { get; set; }
        public List<RunResultsReportGraphDataListDomain> RunResultsReportXyAxisValueList { get; set; }
        public List<RunResultGraphVisibility> RunResultGraphVisibilityList { get; set; }

        public List<RunResultReagentParameterDomain> RunResultReagentParameterList { get; set; }

        public List<RunResultDiscardedImageDomain> RunResultDiscardedImageList { get; set; }
    }
}

