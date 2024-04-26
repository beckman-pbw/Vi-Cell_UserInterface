using ApiProxies.Misc;
using AutoMapper;
using Grpc.Core;
using GrpcService;
using Ninject.Extensions.Logging;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutServices.Interfaces;
using System;

namespace GrpcServer.EventProcessors
{
    public class SampleProcessingSampleStatusEventProcessor : EventProcessor<SampleStatusChangedEvent>
    {
        protected ISampleProcessingService SampleProcessingService;

        public SampleProcessingSampleStatusEventProcessor(ILogger logger, IMapper mapper, 
            ISampleProcessingService sampleProcessingService) 
            : base(logger, mapper)
        {
            SampleProcessingService = sampleProcessingService;
        }

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<SampleStatusChangedEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for Sample Status Changed (Sample Processing)");
            _responseStream = responseStream;
            _subscription = SampleProcessingService.SubscribeToSampleStatusCallback().Subscribe(SetSampleStatusUpdate);
            base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for Sample Status Changed (Sample Processing)");
        }

        private void SetSampleStatusUpdate(ApiEventArgs<SampleEswDomain> obj)
        {
            var sample = new SampleStatusData();
            try
            {
                sample = _mapper.Map<SampleStatusData>(obj.Arg1);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to map the event args. Passing the event without a SampleConfig");
            }
            
            var message = new SampleStatusChangedEvent
            {
                SampleStatusData = sample
            };

            QueueMessage(message);
        }
    }
}