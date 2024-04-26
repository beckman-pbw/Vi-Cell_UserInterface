using ApiProxies.Misc;
using AutoMapper;
using Google.Protobuf.Collections;
using Grpc.Core;
using GrpcService;
using Ninject.Extensions.Logging;
using ScoutServices.Interfaces;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;

namespace GrpcServer.EventProcessors
{
    public class SampleProcessingWorkListCompleteEventProcessor : EventProcessor<WorkListCompleteEvent>
    {
        protected ISampleProcessingService SampleProcessingService;

        public SampleProcessingWorkListCompleteEventProcessor(ILogger logger, IMapper mapper,
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
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<WorkListCompleteEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for WorkList Complete (Sample Processing)");
            _responseStream = responseStream;
            _subscription = SampleProcessingService.SubscribeToWorkListCompleteCallback().Subscribe(SetWorkListCompleteUpdate);
            base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for WorkList Complete (Sample Processing)");
        }

        private void SetWorkListCompleteUpdate(ApiEventArgs<List<uuidDLL>> obj)
        {
            var message = new WorkListCompleteEvent();
            try
            {
                var uuidList = new RepeatedField<string>();
                foreach (var uuid in obj.Arg1)
                    uuidList.Add(_mapper.Map<string>(uuid));
                message = new WorkListCompleteEvent
                {
                    SampleDataUuidList = { uuidList }
                };
            }
            catch(Exception e)
            {
                _logger.Error(e, $"Failed to map the event args. Passing the event without a uuid string");
            }

            QueueMessage(message);
        }
    }
}