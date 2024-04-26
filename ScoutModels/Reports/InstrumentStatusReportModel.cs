using ScoutDomains.Reports.InstrumentStatus;

namespace ScoutModels.Reports
{
    public class InstrumentStatusReportModel
    {
        public InstrumentStatusReportDomain InstrumentStatusReportDomain { get; set; }

        public InstrumentStatusReportModel()
        {
            InstrumentStatusReportDomain = new InstrumentStatusReportDomain();
        }
    }
}
