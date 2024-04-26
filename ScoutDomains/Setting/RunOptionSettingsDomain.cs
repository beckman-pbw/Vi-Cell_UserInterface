using ScoutUtilities.Enums;

namespace ScoutDomains
{
    public class RunOptionSettingsDomain
    {
        public string DefaultSampleId { get; set; }
        public string DefaultDilution { get; set; }
        public SamplePostWash DefaultWash { get; set; }
        public uint DefaultBPQC { get; set; }
        public string NumberOfImages { get; set; }
        public bool IsExportSampleResultActive { get; set; }
        public bool IsAppendSampleResultExportActive { get; set; }
        public string ExportSampleResultPath { get; set; }
        public string AppendSampleResultPath { get; set; }
        public bool IsAutoExportPDFSelected { get; set; }        
        public string DefaultFileName { get; set; }

        public RunOptionSettingsDomain()
        {
            DefaultSampleId = "Sample";
            
            // CellHealth uses a dilution that is fixed in the A-Cup Sample workflow script.
            DefaultDilution = "4";
            
            DefaultWash = 0;
            DefaultBPQC = (uint)CellTypeIndex.BciDefault;
            NumberOfImages = "1";
            IsExportSampleResultActive = false;
            IsAppendSampleResultExportActive = false;
            ExportSampleResultPath = @"..\Export";
            AppendSampleResultPath = @"..\Export";
            IsAutoExportPDFSelected = false;
            DefaultFileName = "Summary";
        }
    }
}
