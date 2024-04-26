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
    public class SampleProcessingSampleCompleteEventProcessor : EventProcessor<SampleCompleteEvent>
    {
        protected ISampleProcessingService SampleProcessingService;

        public SampleProcessingSampleCompleteEventProcessor(ILogger logger, IMapper mapper,
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
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<SampleCompleteEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for Sample Complete (Sample Processing)");
            _responseStream = responseStream;
            _subscription = SampleProcessingService.SubscribeToSampleCompleteCallback().Subscribe(OnSampleCompleteCallback);
            base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for Sample Complete (Sample Processing)");
        }


        private void OnSampleCompleteCallback(ApiEventArgs<SampleEswDomain, ScoutDomains.SampleRecordDomain> args)
        {
            var sampleResult = new SampleResult();
            try
            {
                sampleResult = _mapper.Map<SampleResult>(args.Arg1);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to map the event args. Passing the event without a SampleConfig");
            }

            var message = new SampleCompleteEvent
            {
                SampleResultData = sampleResult
            };

            QueueMessage(message);
        }

    }

}
