using System;

namespace ScoutDomains.Common
{
    [Serializable]
    public class ExportQueueCreationCellTypeDomain
    {
        public uint CellTypeIndex { get; set; }
        public string CellTypeName { get; set; }
        public double? MinimumDiameter { get; set; }
        public double? MaximumDiameter { get; set; }
        public int? Images { get; set; }
        public float? CellSharpness { get; set; }
        public string DeclusterDegree { get; set; }
        public int AspirationCycles { get; set; }
        public double? MinimumCircularity { get; set; }
        public int MixingCycle { get; set; }
        public string ViableSpotArea { get; set; }
        public string ViableSpotBrightness { get; set; }
       
        public ExportQueueCreationCellTypeDomain GetEmptyData()
        {
            CellTypeName = string.Empty;
            DeclusterDegree = string.Empty;
            ViableSpotArea = string.Empty;
            ViableSpotBrightness = string.Empty;
            return this;
        }
    }
}
