namespace ScoutDomains.Reports.CellTypes
{
    public class CellTypesReportTableDomain
    {
        public string CellType { get; set; }
        public string MinDia { get; set; }
        public string MaxDia { get; set; }
        public string Images { get; set; }
        public string CellSharp { get; set; }
        public string MinCircularity { get; set; }
        public string AppTypes { get; set; }
        public string DeclusterDegree { get; set; }
        public string SpotBrightness { get; set; }
        public string SpotArea { get; set; }
        public string Comments { get; set; }
        public string AspirationCycles { get; set; }
        public string MixingCycles { get; set; }
        public string CalculationAdjustmentFactor { get; set; }
      
    }

    public class AnalysisTypeReportDomain
    {
        public string CellType { get; set; }
        public string AnalysisType { get; set; }
        public string ViableSportBrightness { get; set; }
        public string ViableSportArea { get; set; }
        public string MixingCycles { get; set; }
    }
}