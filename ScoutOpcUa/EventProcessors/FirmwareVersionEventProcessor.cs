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
    public class FirmwareVersionEventProcessor : EventProcessor<FirmwareVersionChangedEvent>
    {
        protected IInstrumentStatusService _instrumentStatusService;

        public FirmwareVersionEventProcessor(ILogger logger, IMapper mapper, IInstrumentStatusService instrumentStatusService) : base(logger, mapper)
        {
            _instrumentStatusService = instrumentStatusService;
        }

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<FirmwareVersionChangedEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for Firmware Version changed");
            _responseStream = responseStream;

            // This is done to help when debugging both the UI and OpcUa - this will initialize the variable
			// when it is subscribed to.
			var hardwareInfo = new HardwareSettingsModel();
			var versionInfo = hardwareInfo.GetVersionInformation();
			SetFirmwareVersion(versionInfo.FirmwareVersion);

            _subscription = _instrumentStatusService.SubscribeFirmwareVersionCallback().Subscribe(SetFirmwareVersion);
            base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for Software Version changed");
        }

        private void SetFirmwareVersion(string version)
        {
            var message = new FirmwareVersionChangedEvent()
            {
                Version = version
            };

            QueueMessage(message);
        }
    }
}