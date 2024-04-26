using ScoutDomains;

namespace ScoutModels.Reports
{
    public class RunResultsReportModel
    {
        public RunResultsReportDomain RunResultsReportDomainInstance { get; set; }

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RunResultsReportModel"/> class.
        /// </summary>
        public RunResultsReportModel()
        {
            RunResultsReportDomainInstance = new RunResultsReportDomain();
        }

        #endregion
    }
}