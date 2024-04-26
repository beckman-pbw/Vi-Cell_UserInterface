using System;

namespace ScoutDomains.Common
{
    [Serializable]
    public class ExportQueueCreationQualityControlDomain
    {
        public string QcName { get; set; }
        public string CellTypeName { get; set; }
        public uint CellTypeIndex { get; set; }
        public int AssayParameter { get; set; }
        public string LotInformation { get; set; }
        public double? AssayValue { get; set; }
        public int? AcceptanceLimit { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Comments { get; set; }
        public ExportQueueCreationQualityControlDomain GetEmptyData()
        {
            QcName = string.Empty;
            CellTypeName = string.Empty;
            Comments = string.Empty;
            LotInformation = string.Empty;
            return this;
        }
    }
}
