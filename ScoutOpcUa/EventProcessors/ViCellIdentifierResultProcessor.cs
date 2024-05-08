using System;
using AutoMapper;
using Grpc.Core;
using GrpcService;
using Ninject.Extensions.Logging;
using ScoutDomains;
using ScoutModels;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutUtilities.Enums;
using ScoutUtilities.UIConfiguration;

// ReSharper disable InconsistentNaming

namespace GrpcServer.EventProcessors
{
	/// <summary>
	/// A ViCellIdentifierResultProcessor exists for each gRPC/OPC client and is associated with its GrpcClient instance.
	/// </summary>
	public class ViCellIdentifierResultProcessor : EventProcessor<ViCellIdentifierChangedEvent>
    {
        protected IInstrumentStatusService _instrumentStatusService;

        public ViCellIdentifierResultProcessor(ILogger logger, IMapper mapper, IInstrumentStatusService instrumentStatusService) : base(logger, mapper)
        {
            _instrumentStatusService = instrumentStatusService;
        }

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<ViCellIdentifierChangedEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for ViCellId changed");
            _responseStream = responseStream;

            // This is done to help when debugging both the UI and OpcUa - this will initialize the variable
            // when it is subscribed to
			SetViCellIdentifier(HardwareManager.HardwareSettingsModel.HardwareSettingsDomain.SerialNumber);

            _subscription = _instrumentStatusService.SubscribeViCellIdentifierCallback().Subscribe(SetViCellIdentifier);
            base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for ViCellId changed");
        }

        private void SetViCellIdentifier(string serialNumber)
        {
            var message = new ViCellIdentifierChangedEvent()
            {
                ViCellIdentifier = serialNumber
            };

            QueueMessage(message);
        }
    }
}