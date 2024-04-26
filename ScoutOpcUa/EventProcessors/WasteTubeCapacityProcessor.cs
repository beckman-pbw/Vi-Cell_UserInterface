using System;
using AutoMapper;
using Grpc.Core;
using GrpcService;
using Ninject.Extensions.Logging;
using ScoutDomains;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutUtilities.Enums;

// ReSharper disable InconsistentNaming

namespace GrpcServer.EventProcessors
{
    public class WasteTubeCapacityResultProcessor : EventProcessor<WasteTubeCapacityChangedEvent>
    {
        protected IInstrumentStatusService _instrumentStatusService;

        public WasteTubeCapacityResultProcessor(ILogger logger, IMapper mapper, IInstrumentStatusService instrumentStatusService) : base(logger, mapper)
        {
            _instrumentStatusService = instrumentStatusService;
        }

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<WasteTubeCapacityChangedEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for Waste Tube Capacity");
            _responseStream = responseStream;
            // This is done to help when debugging both the UI and OpcUa - this will initialize the variable
            // when it is subscribed to
            SetWastTubeCapacity((int)_instrumentStatusService.SystemStatusDom.SampleTubeDisposalRemainingCapacity);
            _subscription = _instrumentStatusService.SubscribeWasteTubeCapacityCallback().Subscribe(SetWastTubeCapacity);
            base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for Waste Tube Capacity");
        }

        private void SetWastTubeCapacity(int wastTubeCapacity)
        {
            var message = new WasteTubeCapacityChangedEvent()
            {
               WasteTubeRemainingCapacity = wastTubeCapacity
            };

            QueueMessage(message);
        }
    }
}