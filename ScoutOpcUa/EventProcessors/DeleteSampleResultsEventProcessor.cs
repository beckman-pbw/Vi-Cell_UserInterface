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
    public class DeleteSampleResultsEventProcessor : EventProcessor<DeleteSampleResultsProgressEvent>
    {
        protected ISampleResultsManager _sampleResultsManager;

        public DeleteSampleResultsEventProcessor(ILogger logger, IMapper mapper, 
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
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<DeleteSampleResultsProgressEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for DeleteSampleResultsProgress");
            _responseStream = responseStream;
            _subscription = _sampleResultsManager
                            .SubscribeDeleteSampleResultsProgress()
                            .Subscribe(DeleteSampleProgressUpdated);
            base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for DeleteSampleResults");
        }

        private void DeleteSampleProgressUpdated(SampleProgressEventArgs args)
        {
            var message = new DeleteSampleResultsProgressEvent
            {
                DeleteSampleResultsArgs = _mapper.Map<DeleteSampleResultsArgs>(args)
            };

            QueueMessage(message);
        }
    }
}
