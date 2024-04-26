using System;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ScoutUtilities.CustomEventArgs
{
    public class SampleProgressEventArgs : EventArgs
    {
        public bool OperationIsComplete { get; set; }
        public int PercentComplete { get; set; }
        public HawkeyeError? Error { get; set; }
        public string ProgressMessage { get; set; }
        
        public SampleProgressEventArgs(int percentComplete, bool operationIsComplete, HawkeyeError error, string progressMessage = null)
        {
            PercentComplete = percentComplete;
            OperationIsComplete = operationIsComplete;
            Error = error;
            ProgressMessage = progressMessage;
        }

        public SampleProgressEventArgs(HawkeyeError error)
        {
            Error = error;
        }
    }

    public class SamplesDeletedEventArgs
    {
        public int PercentComplete { get; set; }
        public uuidDLL SampleUuid { get; set; }

        public SamplesDeletedEventArgs(int percentComplete, uuidDLL sampleUuid)
        {
            PercentComplete = percentComplete;
            SampleUuid = sampleUuid;
        }
    }


    public class SampleExportProgressEventArgs : SampleProgressEventArgs
    {
        public string ExportFolderLocation { get; set; }

        public SampleExportProgressEventArgs(int percentComplete, bool operationIsComplete, HawkeyeError error, string progressMessage = null, string exportFolderLocation = null)
            : base(percentComplete, operationIsComplete, error, progressMessage)
        {
            ExportFolderLocation = exportFolderLocation;
        }

        public SampleExportProgressEventArgs(HawkeyeError error, string exportFolderLocation = null) : base(error)
        {
            ExportFolderLocation = exportFolderLocation;
        }
    }
}