using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BAFW;

using ScoutDomains;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ExportManager
{

    // **********************************************************************
    public class EvExportOfflineReq : BPublicEvent
    {
        public EvExportOfflineReq(string username, string password, 
            List<uuidDLL> sampleIds, 
            string outPath, string zipfile, 
            eExportImages exportImages, UInt16 exportNthImage,
            bool isAutomaitonExport,
            export_data_progress_callback_pcnt progressCB, 
            export_data_completion_callback completeCB,
            export_data_status_callback statusCB) 
        : base((uint)PubEvIds.ExportMgr_StartOffline)
        {
            Username = username;
            Password = password;
            OutPath = outPath;
            Zipfile = zipfile;
            ExportImages = exportImages;
            ExportNthImage = exportNthImage;
            IsAutomationExport = isAutomaitonExport; 
            ProgressCB = progressCB;
            CompleteCB = completeCB;
            StatusCB = statusCB;

            foreach (var id in sampleIds)
            {
                if (id.IsNullOrEmpty())
                {
                    EvAppLogReq.Publish("EvExportOfflineReq", EvAppLogReq.LogLevel.Warning, "Removed bad ID", 0);
                }
                else
                {
                    SampleIds.Add(id);
                }
            }

            if (SampleIds.Count == 0)
            {
                EvAppLogReq.Publish("EvExportOfflineReq", EvAppLogReq.LogLevel.Error, "No valid ids", 0);
            }
        }

        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public List<uuidDLL> SampleIds { get; set; } = new List<uuidDLL>();
        public string OutPath { get; set; } = "";
        public string Zipfile { get; set; } = "";
        public eExportImages ExportImages { get; set; } = eExportImages.eAll;
        public UInt16 ExportNthImage { get; set; }
        public bool IsAutomationExport { get; set; }
        public export_data_progress_callback_pcnt ProgressCB { get; set; } = null;
        public export_data_completion_callback CompleteCB { get; set; } = null;
        public export_data_status_callback StatusCB { get; set; } = null;        

        public static void Publish(string username, string password, 
	        List<uuidDLL> summaryResultIds, 
	        string outPath, string zipfile,
	        eExportImages exportImages, UInt16 exportNthImage, 
	        bool isAutomationExport,
	        export_data_progress_callback_pcnt progressCB, 
	        export_data_completion_callback completeCB,
	        export_data_status_callback statusCB)
        {
            BAppFW.Publish(new EvExportOfflineReq(username, password, 
	            summaryResultIds, outPath, zipfile, 
                exportImages, exportNthImage, isAutomationExport,
	            progressCB, completeCB, statusCB));
        }
    }

	// **********************************************************************
	public class EvExportCsvReq : BPublicEvent
    {
        public EvExportCsvReq(
            string username, string password, 
            List<SampleRecordDomain> samples,
            string summaryOutPath,
            string detailsOutPath,
            string summaryCsvFileBase,
            string zipFileBase,
            bool exportSummary,
            bool appendSummary,
            bool exportDetails,
            bool exportImages,
            UInt32 saveEveryNthImage = 1,
            export_data_status_callback statusCB = null) :
            base((uint)PubEvIds.ExportMgr_StartCsv)
        {
            Username = username;
            Password = password;

            SummaryOutPath = summaryOutPath;
            DetailsOutPath = detailsOutPath;
            SummaryCsvFileBase = summaryCsvFileBase;
            ZipFileBase = zipFileBase;

            ExportSummary = exportSummary;
            AppendSummary = appendSummary;
            ExportDetails = exportDetails;
            ExportImages = exportImages;
            SaveEveryNthImage = saveEveryNthImage;

            StatusCB = statusCB;

            foreach (var s in samples)
            {
                if (s.UUID.IsNullOrEmpty())
                {
                    EvAppLogReq.Publish("EvExportCsvReq", EvAppLogReq.LogLevel.Warning, "Removed bad ID", 0);
                }
                else
                {
                    Samples.Add(s);
                }
            }
            if (Samples.Count == 0)
            {
                EvAppLogReq.Publish("EvExportCsvReq", EvAppLogReq.LogLevel.Error, "No valid ids", 0);
            }
        }

        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public List<SampleRecordDomain> Samples { get; set; } = new List<SampleRecordDomain>();

        public string SummaryOutPath { get; set; } = "";
        public string DetailsOutPath { get; set; } = "";

        // filename without path or extention 
        public string SummaryCsvFileBase { get; set; } = "";

        // filename without path or extention 
        public string ZipFileBase { get; set; } = "";

        public bool ExportSummary { get; set; } = false;
        public bool AppendSummary { get; set; } = false;
        public bool ExportDetails { get; set; } = false;
        public bool ExportImages { get; set; } = false;
        public UInt32 SaveEveryNthImage { get; set; } = 1;


        public export_data_status_callback StatusCB { get; set; } = null;

        public static void Publish(
            string username, string password, List<SampleRecordDomain> samples,
            string summaryOutPath,
            string detailsOutPath,
            string summaryCsvFileBase,
            string zipFileBase,
            bool exportSummary,
            bool appendSummary,
            bool exportDetails,
            bool exportImages,
            UInt32 saveEveryNthImage = 1,
            export_data_status_callback statusCB = null)
        {
            BAppFW.Publish(new EvExportCsvReq(username, password, samples,
                summaryOutPath, detailsOutPath, summaryCsvFileBase, zipFileBase, exportSummary, appendSummary, exportDetails, exportImages, saveEveryNthImage, statusCB));
        }

        public static void PublishSummaryOnly(
            string username, string password, List<SampleRecordDomain> samples,
            string summaryOutPath,
            string summaryCsvFileBase, bool appendSummary)
        {
            BAppFW.Publish(new EvExportCsvReq(username, password, samples,
                summaryOutPath, "", summaryCsvFileBase, "", true, appendSummary, false, false));
        }
    }

    // **********************************************************************
    public class EvExportLogsReq : BPublicEvent
    {
        public EvExportLogsReq(string username, string password,
            DateTime startTime, DateTime endTime, string outFile, bool includeAudit, bool includeError) :
            base((uint)PubEvIds.ExportMgr_StartLogs)
        {
            Username = username;
            Password = password;
            StartTime = startTime;
            EndTime = endTime;
            OutFile = outFile;
            IncludeAudit = includeAudit;
            IncludeError = includeError;
        }

        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; } = DateTime.Now;
        public string OutFile { get; set; } = "";
        public bool IncludeAudit { get; set; } = false;
        public bool IncludeError { get; set; } = false;

        public static void Publish(string username, string password,
            DateTime startTime, DateTime endTime, string outPath, bool includeAudit, bool includeError)
        {
            BAppFW.Publish(new EvExportLogsReq(username, password, startTime, endTime, outPath, includeAudit, includeError));
        }
	}

	// **********************************************************************
	public class EvExportLogDataCsvReq : BPublicEvent
    {
	    public EvExportLogDataCsvReq (
		    DateTime startTime, DateTime endTime, 
		    string exportFilepath,
		    string localExportFilepath,
			string bulkDataId,
			export_data_status_callback statusCB = null)
		    : base((uint)PubEvIds.ExportMgr_StartLogDataExport)
	    {
		    StartTime = startTime;
		    EndTime = endTime;
		    ExportFilepath = exportFilepath;
		    LocalExportFilepath = localExportFilepath;
		    BulkDataId = bulkDataId;
			StatusCB = statusCB;
	    }

	    public DateTime StartTime { get; set; } = DateTime.Now;
	    public DateTime EndTime { get; set; } = DateTime.Now;
	    public string ExportFilepath { get; set; } = "";
	    public string LocalExportFilepath { get; set; } = "";
	    public string BulkDataId { get; set; } = "";
	    public export_data_status_callback StatusCB { get; set; } = null;
	    public export_data_completion_callback CompleteCB { get; set; } = null;

		public static void Publish(
		    DateTime startTime, DateTime endTime, 
			string exportFilepath, 
			string LocalExportFilepath,
			string bulkDataId,
			export_data_status_callback statusCB)
		{
			BAppFW.Publish(new EvExportLogDataCsvReq(
				startTime, endTime,
				exportFilepath,
				LocalExportFilepath,
				bulkDataId,
				statusCB));
        }
    }

    // **********************************************************************
    public class EvSystemStatusInd : BPublicEvent
    {
        public EvSystemStatusInd(SystemStatusData status) :
            base((uint)PubEvIds.SystemStatus)
        {
            Status = status;
        }

        public SystemStatusData Status { get; set; } = new SystemStatusData();

        public static void Publish(SystemStatusData status)
        {
            BAppFW.Publish(new EvSystemStatusInd(status));
        }

    }
    
}
