using System;
using AutoMapper;
using Grpc.Core;
using GrpcService;
using Ninject.Extensions.Logging;
using ScoutDomains;
using ScoutModels.Interfaces;
using ScoutUtilities.Enums;

// ReSharper disable InconsistentNaming

namespace GrpcServer.EventProcessors
{
    /// <summary>
    /// A ViCellStatusResultProcessor exists for each gRPC/OPC client and is associated with its GrpcClient instance.
    /// </summary>
    public class ViCellStatusResultProcessor : EventProcessor<ViCellStatusChangedEvent>
    {
        protected IInstrumentStatusService _instrumentStatusService;
        private SystemStatus _previouSystemStatus;

        public ViCellStatusResultProcessor(ILogger logger, IMapper mapper, IInstrumentStatusService instrumentStatusService) : base(logger, mapper)
        {
            _instrumentStatusService = instrumentStatusService;
            _previouSystemStatus = SystemStatus.Faulted;
        }

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<ViCellStatusChangedEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for ViCellStatus");
            _responseStream = responseStream;
            // This is done to help when debugging both the UI and OpcUa - this will initialize the variable
            // when it is subscribed to
            SetViCellStatus(_instrumentStatusService.SystemStatusDom);
            _subscription = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe(SetViCellStatus);
            base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for ViCellStatus");
        }

        private void SetViCellStatus(SystemStatusDomain systemStatusDomain)
        {
            if (systemStatusDomain.SystemStatus != _previouSystemStatus)
            {
                _previouSystemStatus = systemStatusDomain.SystemStatus;

                var systemStatus = systemStatusDomain.SystemStatus;
                if (systemStatus != SystemStatus.Stopped && (systemStatusDomain.NightlyCleanStatus == eNightlyCleanStatus.ncsInProgress ||
                                                             systemStatusDomain.NightlyCleanStatus == eNightlyCleanStatus.ncsAutomationInProgress))
                {
                    systemStatus = SystemStatus.NightlyClean;
                }

                var message = new ViCellStatusChangedEvent
                {
                    ViCellStatus = systemStatus == SystemStatus.Stopped
                        ? _mapper.Map<ViCellStatusEnum>(SystemStatus.Idle)
                        : _mapper.Map<ViCellStatusEnum>(systemStatus)
                };

                QueueMessage(message);
            }
        }
    }
}