using ScoutDomains.Reports.CellTypes;

namespace ScoutModels.Reports
{
    public class CellTypesReportModel
    {
        /// <summary>
        /// Gets or sets the cell types report domain instance.
        /// </summary>
        /// <value>The cell types report domain instance.</value>
        public CellTypesReportDomain CellTypesReportDomainInstance { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CellTypesReportModel"/> class.
        /// </summary>
        public CellTypesReportModel()
        {
            CellTypesReportDomainInstance = new CellTypesReportDomain();
        }
    }
}