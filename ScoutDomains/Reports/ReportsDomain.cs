using ScoutUtilities.Interfaces;

namespace ScoutDomains
{
    public class ReportsDomain : IListItem
    {
        public string ReportName { get; set; }

        public int ReportId { get; set; }

        public string ReportLogName { get; set; }

        public string ListItemLabel => string.IsNullOrEmpty(ReportName) ? ReportLogName : ReportName;
    }
}

