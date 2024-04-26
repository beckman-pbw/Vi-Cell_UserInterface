using AutoMapper;
using Grpc.Core;
using GrpcService;
using Ninject.Extensions.Logging;
using ScoutServices.Interfaces;
using ScoutUtilities.CustomEventArgs;
using System;
using ScoutUtilities.Enums;

namespace GrpcServer.EventProcessors
{
    public class ExportStatusEventProcessor : EventProcessor<ExportStatusEvent>
    {
        protected ISampleResultsManager _sampleResultsManager;

        public ExportStatusEventProcessor(ILogger logger, IMapper mapper,
            ISampleResultsManager sampleResultsManager)
            : base(logger, mapper)
        {
            _sampleResultsManager = sampleResultsManager;
        }

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<ExportStatusEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for ExportStatusEvent");
            _responseStream = responseStream;
            _subscription = _sampleResultsManager
                            .SubscribeExportStatus()
                            .Subscribe(ExportProgressUpdated);
            base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for ExportStatusEvent");
        }

        private void ExportProgressUpdated(ExportStatusEvent args)
        {
	        _logger.Debug("ExportStatus: " + args.StatusInfo.Status);
            QueueMessage(args);
        }
    }

}
