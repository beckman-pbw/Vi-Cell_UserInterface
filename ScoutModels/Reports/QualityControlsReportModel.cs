using ScoutDomains.Reports.QualityControls;

namespace ScoutModels.Reports
{
    public class QualityControlsReportModel
    {
        public QualityControlsReportDomain QualityControlsReportDomainInstance { get; set; }

        public QualityControlsReportModel()
        {
            QualityControlsReportDomainInstance = new QualityControlsReportDomain();
        }
    }
}
