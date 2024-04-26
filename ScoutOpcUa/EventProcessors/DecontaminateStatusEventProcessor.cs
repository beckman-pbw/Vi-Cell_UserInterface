using AutoMapper;
using Grpc.Core;
using GrpcService;
using Ninject.Extensions.Logging;
using ScoutServices.Interfaces;
using ScoutUtilities.Enums;
using System;

// ReSharper disable InconsistentNaming

namespace GrpcServer.EventProcessors
{
    public class DecontaminateStatusEventProcessor : EventProcessor<DecontaminateStatusEvent>
    {
	    protected IMaintenanceService _maintenanceService;

		public DecontaminateStatusEventProcessor(ILogger logger, IMapper mapper, IMaintenanceService maintenanceService) : base(logger, mapper)
		{
			_maintenanceService = maintenanceService;
		}

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<DecontaminateStatusEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for Decontaminate Status");
            _responseStream = responseStream;

            // This is done to help when debugging both the UI and OpcUa - this will initialize the variable when it is subscribed to.
			_subscription = _maintenanceService.SubscribeDecontaminateStatus().Subscribe(SetDecontaminateStatus);
			base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for Decontaminate Status");
        }

        private void SetDecontaminateStatus(eDecontaminateFlowCellState status)
        {
	        var message = new DecontaminateStatusEvent()
	        {
		        Status = _mapper.Map<DecontaminateStatusEnum>(status) 
	        };

	        QueueMessage(message);
        }
	}
}