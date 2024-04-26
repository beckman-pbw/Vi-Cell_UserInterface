using GrpcService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScoutDomains;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ScoutServices.Interfaces
{
    public interface ISampleResultsManager
    {
        IObservable<ExportStatusEvent> SubscribeExportStatus();
        IObservable<SampleProgressEventArgs> SubscribeDeleteSampleResultsProgress();
        IObservable<SamplesDeletedEventArgs> SubscribeSamplesDeleted();
        bool CheckUserPrivilegedData(string loggedInUser, string requestedUsername, out string errMsg);
        List<SampleResult> GetSampleResults(string username, string password, RequestGetSampleResults request);

        string StartExport(string username, string password, RequestStartExport request);

        RetrieveBulkBlockStatusEnum GetBulkDataBlock(uint blockIdx, string dataId, out byte[] data);

        bool DeleteSampleResults(string username, string password, List<uuidDLL> sampleResultsToDelete, bool retainResultsAndFirstImage);

        HawkeyeError GetSampleSet(uuidDLL sampleSetUuidDll, bool getSampleResults,
            out SampleSetDomain sampleSetDomain);

        bool GetResultsValidation(RequestGetSampleResults request);

        string StartLogDataExport (RequestStartLogDataExport request);
    }
}
