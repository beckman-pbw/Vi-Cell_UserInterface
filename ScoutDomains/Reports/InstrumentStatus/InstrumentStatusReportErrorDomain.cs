namespace ScoutDomains.Reports.InstrumentStatus
{
    public class InstrumentStatusReportErrorDomain
    {
        public string SeverityKey { get; set; }
        public string SeverityDisplayValue { get; set; }
        public string System { get; set; }
        public string Subsystem { get; set; }
        public string Instance { get; set; }
        public string Description { get; set; }
    }
}
