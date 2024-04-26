using System;
using System.Xml.Serialization;
using ScoutDomains.Common;
using ScoutUtilities.Enums;

namespace ScoutDomains
{
    [Serializable]
    [XmlRoot("SampleDetails")]
    [XmlType("ExportQueueCreationDomain")]
    public class ExportQueueCreationDomain
    {
        public string StageTypeAsString { get; set; }
        public string Positions { get; set; }
        public string SampleIDs { get; set; }
        public string CellTypes { get; set; }
        public string Dilution { get; set; }
        public SamplePostWash Wash { get; set; }
        public string Comment { get; set; }
        public string RowColWise { get; set; }
        public int NthImage { get; set; }
        public bool IsExportEachSampleActive { get; set; }
        public bool IsAppendResultExport { get; set; }
        public bool IsExportPdfResultExport { get; set; }
        public string ExportEachSample { get; set; }
        public string AppendResultExport { get; set; }
        public string ExportFileName { get; set; }
        public ExportQueueCreationQualityControlDomain QualityControlDetails { get; set; }
        public ExportQueueCreationCellTypeDomain CellTypeDetails { get; set; }
        public CtBpQcType SelectedQcCtBpQcType { get; set; }
        public ExportQueueCreationDomain()
        {
                
        }
    }
}