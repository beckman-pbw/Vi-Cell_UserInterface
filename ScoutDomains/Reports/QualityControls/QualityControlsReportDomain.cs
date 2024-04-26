using System.Collections.Generic;
using ScoutDomains.Reports.Common;

namespace ScoutDomains.Reports.QualityControls
{
    public class QualityControlsReportDomain
    {
        public List<ReportMandatoryHeaderDomain> QualityControlReportMandatoryHeaderDomainList { get; set; }
        public List<QualityControlTableHeaderDomain> QualityControlReportTableHeaderList { get; set; }
        public List<ReportTableTemplate> QualityControlReportNameTableList { get; set; }
        public List<ReportTableTemplate> QualityControlReportCellTypeTableList { get; set; }
        public List<ReportTableTemplate> QualityControlReportCurrentLotTableList { get; set; }
        public List<ReportTableTemplate> QualityControlReportLastCheckTableList { get; set; }
        public List<QualityControlsReportGaphXyAxis> QualityControlsReportGaphXyAxisList { get; set; }
        public List<QualityControlsReportGraphTitles> QualityControlsReportGraphTitlesList { get; set; }
    }
}