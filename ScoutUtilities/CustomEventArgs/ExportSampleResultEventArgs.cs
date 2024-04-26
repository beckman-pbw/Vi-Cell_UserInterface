using ScoutUtilities.Structs;
using System.Collections.Generic;

namespace ScoutUtilities.CustomEventArgs
{
    public class ExportSampleResultEventArgs : BaseDialogEventArgs
    {
        public string ExportFilePath { get; set; }
        public List<uuidDLL> SamplesToExport { get; set; }

        public ExportSampleResultEventArgs(string exportPath, List<uuidDLL> samplesToExport)
        {
            ExportFilePath = exportPath;
            SamplesToExport = samplesToExport;

            DialogIconPath = "/Images/Word_exp.png";
            DialogIconIsSquare = true;
        }
    }
}