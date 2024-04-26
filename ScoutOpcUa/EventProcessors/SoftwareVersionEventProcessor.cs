using System;
using AutoMapper;
using Grpc.Core;
using GrpcService;
using Ninject.Extensions.Logging;
using ScoutDomains;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutUtilities.Enums;
using ScoutUtilities.UIConfiguration;

// ReSharper disable InconsistentNaming

namespace GrpcServer.EventProcessors
{
    public class SoftwareVersionEventProcessor : EventProcessor<SoftwareVersionChangedEvent>
    {
        protected IInstrumentStatusService _instrumentStatusService;

        public SoftwareVersionEventProcessor(ILogger logger, IMapper mapper, IInstrumentStatusService instrumentStatusService) : base(logger, mapper)
        {
            _instrumentStatusService = instrumentStatusService;
        }

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<SoftwareVersionChangedEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for Software Version changed");
            _responseStream = responseStream;

            // This is done to help when debugging both the UI and OpcUa - this will initialize the variable
			// when it is subscribed to.
			SetSoftwareVersion(UISettings.SoftwareVersion);

            _subscription = _instrumentStatusService.SubscribeSoftwareVersionCallback().Subscribe(SetSoftwareVersion);
            base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for Software Version changed");
        }

        private void SetSoftwareVersion(string version)
        {
            var message = new SoftwareVersionChangedEvent()
            {
                Version = version
            };

            QueueMessage(message);
        }
    }
}