namespace ScoutDomains.Reports.RunSummary
{
    public class RunSummaryReportTableDomain
    {
        public string SampleId { get; set; }
        public string AnalysisDate { get; set; }
        public string AnalysisTimeStamp { get; set; }
        public string Concentration { get; set; }
        public string ViableConcentration { get; set; }
        public string Viability { get; set; }
        public string ColumnFive { get; set; }
        public string ColumnSix { get; set; }
        public string ColumnSeven { get; set; }
        public string Signature { get; set; }
        public string ViabilityUnit { get; set; }
        public string ConcentrationUnit { get; set; }
        public string ViableConcentrationUnit { get; set; }       
        public string UnitFive { get; set; }
        public string UnitSix { get; set; }
        public string UnitSeven { get; set; }
        public int ColumnFiveAlignment { get; set; }
        public int ColumnSixAlignment { get; set; }
        public int ColumnSevenAlignment { get; set; }
    }
}