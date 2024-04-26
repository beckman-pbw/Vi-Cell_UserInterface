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
    public class ReagentUseRemainingResultProcessor : EventProcessor<ReagentUsesRemainingChangedEvent>
    {
        protected IInstrumentStatusService _instrumentStatusService;

        public ReagentUseRemainingResultProcessor(ILogger logger, IMapper mapper, IInstrumentStatusService instrumentStatusService) : base(logger, mapper)
        {
            _instrumentStatusService = instrumentStatusService;
        }

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<ReagentUsesRemainingChangedEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for Reagent Use Remaining");
            _responseStream = responseStream;
            // This is done to help when debugging both the UI and OpcUa - this will initialize the variable
            // when it is subscribed to
            SetReagentUseRemaining((int)_instrumentStatusService.SystemStatusDom.RemainingReagentPackUses);
            _subscription = _instrumentStatusService.SubscribeReagentUseRemainingCallback().Subscribe(SetReagentUseRemaining);
            base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for Reagent Use Remaining");
        }

        private void SetReagentUseRemaining(int reagentUseRemaining)
        {
            var message = new ReagentUsesRemainingChangedEvent()
            {
                ReagentUsesRemaining = reagentUseRemaining
            };

            QueueMessage(message);
        }
    }
}