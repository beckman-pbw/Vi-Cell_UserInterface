namespace ScoutDomains.Reports.Common
{
    public class ReportMandatoryHeaderDomain
    {
        public string ReportTitle { get; set; }
        public string ReportSubTitle { get; set; }
        public string CommentHeader { get; set; }
        public string CommentData { get; set; }
        public string DeviceSerialHeader { get; set; }
        public string DeviceSerialData { get; set; }
        public string CurrentDateTime { get; set; }
    }
}