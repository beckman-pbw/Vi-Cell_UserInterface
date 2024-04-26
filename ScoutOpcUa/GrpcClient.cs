using Grpc.Core;
using GrpcServer.EventProcessors;
using GrpcService;
using System;
using ScoutModels;
using ScoutUtilities.Common;

namespace GrpcServer
{
    /// <summary>
    /// This client object maintains the collection of resources and registered events requested by the client
    /// on the gRPC server running within ScoutX.
    /// 
    /// When the client "logs out", all resources are freed and Reactive subjects are notified that the client is
    /// no longer "listening". The clients are managed by the GrpcClient Manager.
    /// </summary>
    public class GrpcClient : Disposable
    {
        private string _id;

        private string _username;

        private string _password;

        private readonly LockResultProcessor _lockResultProcessor;
        private readonly SampleProcessingSampleStatusEventProcessor _sampleProcessingSampleStatusEventProcessor;
        private readonly SampleProcessingSampleCompleteEventProcessor _sampleProcessingSampleCompleteProcessor;
        private readonly SampleProcessingWorkListCompleteEventProcessor _sampleProcessingWorkListCompleteEventProcessor;
        private readonly ViCellStatusResultProcessor _viCellStatusResultProcessor;
        private readonly ViCellIdentifierResultProcessor _viCellIdResultProcessor;
        private readonly ReagentUseRemainingResultProcessor _reagentUseRemainingResultProcessor;
        private readonly WasteTubeCapacityResultProcessor _wasteTubeCapacityResultProcessor;
        private readonly DeleteSampleResultsEventProcessor _deleteSampleResultsEventProcessor;
        private readonly ExportStatusEventProcessor  _exportStatusEventProcessor;

        private readonly PrimeReagentsStatusEventProcessor _primeReagentsStatusEventProcessor;
        private readonly PurgeReagentsStatusEventProcessor _purgeReagentsStatusEventProcessor;
        private readonly CleanFluidicsStatusEventProcessor _cleanFluidicsStatusEventProcessor;
        private readonly DecontaminateStatusEventProcessor _decontaminateStatusEventProcessor;
        private readonly SoftwareVersionEventProcessor _softwareVersionEventProcessor;
        private readonly FirmwareVersionEventProcessor _firmwareVersionEventProcessor;
        private readonly ErrorStatusEventProcessor _errorStatusEventProcessor;

		/// <summary>
		/// The OPC/UA server makes gRPC calls to the ScoutX process. For each OPC/UA client,
		/// a corresponding unique Id is created
		/// </summary>
		/// <param name="clientId">A unique id for the client, which is a stringified Guid.</param>
		/// <param name="username">A ScoutX username, which could be local ScoutX user, or Active Directory user.</param>
		/// <param name="password">The credentials for the user.</param>
		/// <param name="lockResultProcessor">EventHandler for lock state</param>
		/// <param name="sampleProcessingSampleStatusEventProcessor"></param>
		/// <param name="sampleProcessingSampleCompleteEventProcessor"></param>
		/// <param name="sampleProcessingWorkListCompleteEventProcessor"></param>
		/// <param name="viCellStatusResultProcessor"></param>
		/// <param name="viCellIdResultProcessor"></param>
		/// <param name="reagentUseRemainingResultProcessor"></param>
		/// <param name="wasteTubeCapacityResultProcessor"></param>
		/// <param name="deleteSampleResultsEventProcessor"></param>
		/// <param name="exportStatusEventProcessor"></param>
		/// <param name="primeReagentsStatusEventProcessor"></param>
		/// <param name="purgeReagentsStatusEventProcessor"></param>
		/// <param name="cleanFluidicsStatusEventProcessor"></param>
		/// <param name="decontaminateStatusEventProcessor"></param>
		/// <param name="softwareVersionEventProcessor"></param>
		/// <param name="firmwareVersionEventProcessor"></param>
		/// <param name="errorStatusEventProcessor"></param>
		public GrpcClient(string clientId, string username, string password, 
			LockResultProcessor lockResultProcessor,
            SampleProcessingSampleStatusEventProcessor sampleProcessingSampleStatusEventProcessor,
            SampleProcessingSampleCompleteEventProcessor sampleProcessingSampleCompleteEventProcessor,
            SampleProcessingWorkListCompleteEventProcessor sampleProcessingWorkListCompleteEventProcessor,
            ViCellStatusResultProcessor viCellStatusResultProcessor, 
            ViCellIdentifierResultProcessor viCellIdResultProcessor,
            ReagentUseRemainingResultProcessor reagentUseRemainingResultProcessor,
            WasteTubeCapacityResultProcessor wasteTubeCapacityResultProcessor,
            DeleteSampleResultsEventProcessor deleteSampleResultsEventProcessor,
            ExportStatusEventProcessor exportStatusEventProcessor,
			PrimeReagentsStatusEventProcessor primeReagentsStatusEventProcessor,
			PurgeReagentsStatusEventProcessor purgeReagentsStatusEventProcessor,
            CleanFluidicsStatusEventProcessor cleanFluidicsStatusEventProcessor,
            DecontaminateStatusEventProcessor decontaminateStatusEventProcessor,
            SoftwareVersionEventProcessor softwareVersionEventProcessor,
            FirmwareVersionEventProcessor firmwareVersionEventProcessor,
            ErrorStatusEventProcessor errorStatusEventProcessor)
		{

			_id = clientId;
            _username = username;
            _password = password;
            _lockResultProcessor = lockResultProcessor;
            _sampleProcessingSampleStatusEventProcessor = sampleProcessingSampleStatusEventProcessor;
            _sampleProcessingSampleCompleteProcessor = sampleProcessingSampleCompleteEventProcessor;
            _sampleProcessingWorkListCompleteEventProcessor = sampleProcessingWorkListCompleteEventProcessor;
            _viCellStatusResultProcessor = viCellStatusResultProcessor;
            _viCellIdResultProcessor = viCellIdResultProcessor;
            _reagentUseRemainingResultProcessor = reagentUseRemainingResultProcessor;
            _wasteTubeCapacityResultProcessor = wasteTubeCapacityResultProcessor;
            _deleteSampleResultsEventProcessor = deleteSampleResultsEventProcessor;
            _exportStatusEventProcessor = exportStatusEventProcessor;
			_primeReagentsStatusEventProcessor = primeReagentsStatusEventProcessor;
			_purgeReagentsStatusEventProcessor = purgeReagentsStatusEventProcessor;
            _cleanFluidicsStatusEventProcessor = cleanFluidicsStatusEventProcessor;
            _decontaminateStatusEventProcessor = decontaminateStatusEventProcessor;
            _softwareVersionEventProcessor = softwareVersionEventProcessor;
            _firmwareVersionEventProcessor = firmwareVersionEventProcessor;
            _errorStatusEventProcessor = errorStatusEventProcessor;
		}

		protected override void DisposeManaged()
        {
            _lockResultProcessor.Dispose();
            _sampleProcessingSampleStatusEventProcessor.Dispose();
            _sampleProcessingSampleCompleteProcessor.Dispose();
            _sampleProcessingWorkListCompleteEventProcessor.Dispose();
            _viCellIdResultProcessor.Dispose();
            _viCellStatusResultProcessor.Dispose();
            _reagentUseRemainingResultProcessor.Dispose();
            _wasteTubeCapacityResultProcessor.Dispose();
            _deleteSampleResultsEventProcessor.Dispose();
            _exportStatusEventProcessor.Dispose();
            _primeReagentsStatusEventProcessor.Dispose();
            _purgeReagentsStatusEventProcessor.Dispose();
            _cleanFluidicsStatusEventProcessor.Dispose();
            _decontaminateStatusEventProcessor.Dispose();
            _softwareVersionEventProcessor.Dispose();
            _firmwareVersionEventProcessor.Dispose();
            _errorStatusEventProcessor.Dispose();

	        base.DisposeManaged();
        }

        public void SubscribeLockResult(ServerCallContext context, 
            IServerStreamWriter<LockStateChangedEvent> responseStream)
        {
            _lockResultProcessor.Subscribe(context, responseStream);
        }

        public void SubscribeSampleStatusChanged(ServerCallContext context, 
            IServerStreamWriter<SampleStatusChangedEvent> responseStream)
        {
            _sampleProcessingSampleStatusEventProcessor.Subscribe(context, responseStream);
        }

        public void SubscribeSampleComplete(ServerCallContext context,
            IServerStreamWriter<SampleCompleteEvent> responseStream)
        {
            _sampleProcessingSampleCompleteProcessor.Subscribe(context, responseStream);
        }

        public void SubscribeWorkListCompleted(ServerCallContext context, 
            IServerStreamWriter<WorkListCompleteEvent> responseStream)
        {
            _sampleProcessingWorkListCompleteEventProcessor.Subscribe(context, responseStream);
        }

        public void SubscribeSystemStatusChanged(ServerCallContext context,
            IServerStreamWriter<ViCellStatusChangedEvent> responseStream)
        {
            _viCellStatusResultProcessor.Subscribe(context, responseStream);
        }

        public void SubscribeViCellIdChanged(ServerCallContext context,
            IServerStreamWriter<ViCellIdentifierChangedEvent> responseStream)
        {
            _viCellIdResultProcessor.Subscribe(context, responseStream);
        }

        public void SubscribeReagentUseRemainingChanged(ServerCallContext context,
            IServerStreamWriter<ReagentUsesRemainingChangedEvent> responseStream)
        {
            _reagentUseRemainingResultProcessor.Subscribe(context, responseStream);
        }

        public void SubscribeWasteTubeCapacityChanged(ServerCallContext context,
            IServerStreamWriter<WasteTubeCapacityChangedEvent> responseStream)
        {
            _wasteTubeCapacityResultProcessor.Subscribe(context, responseStream);
        }

        public void SubscribeDeleteSampleResultsProgress(ServerCallContext context, 
            IServerStreamWriter<DeleteSampleResultsProgressEvent> responseStream)
        {
            _deleteSampleResultsEventProcessor.Subscribe(context, responseStream);
        }

        public void SubscribeExportStatus(ServerCallContext context,
            IServerStreamWriter<ExportStatusEvent> responseStream)
        {
            _exportStatusEventProcessor.Subscribe(context, responseStream);
        }

        public void SubscribePrimeReagentsStatusChanged(ServerCallContext context,
	        IServerStreamWriter<PrimeReagentsStatusEvent> responseStream)
        {
	        _primeReagentsStatusEventProcessor.Subscribe(context, responseStream);
        }

        public void SubscribePurgeReagentsStatusChanged(ServerCallContext context,
	        IServerStreamWriter<PurgeReagentsStatusEvent> responseStream)
        {
	        _purgeReagentsStatusEventProcessor.Subscribe(context, responseStream);
        }

        public void SubscribeCleanFluidicsStatusChanged(ServerCallContext context,
	        IServerStreamWriter<CleanFluidicsStatusEvent> responseStream)
        {
	        _cleanFluidicsStatusEventProcessor.Subscribe(context, responseStream);
        }

        public void SubscribeDecontaminateStatusChanged(ServerCallContext context,
	        IServerStreamWriter<DecontaminateStatusEvent> responseStream)
        {
	        _decontaminateStatusEventProcessor.Subscribe(context, responseStream);
        }

        public void SubscribeSoftwareVersionChanged(ServerCallContext context,
	        IServerStreamWriter<SoftwareVersionChangedEvent> responseStream)
        {
	        _softwareVersionEventProcessor.Subscribe(context, responseStream);
        }

        public void SubscribeFirmwareVersionChanged(ServerCallContext context,
	        IServerStreamWriter<FirmwareVersionChangedEvent> responseStream)
        {
	        _firmwareVersionEventProcessor.Subscribe(context, responseStream);
        }

        public void SubscribeErrorStatusChanged(ServerCallContext context,
	        IServerStreamWriter<ErrorStatusEvent> responseStream)
        {
			_errorStatusEventProcessor.Subscribe(context, responseStream);
        }
	}
}
