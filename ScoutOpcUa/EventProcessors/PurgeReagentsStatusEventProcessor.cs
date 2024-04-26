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
    public class PurgeReagentsStatusEventProcessor : EventProcessor<PurgeReagentsStatusEvent>
    {
	    protected IMaintenanceService _maintenanceService;

		public PurgeReagentsStatusEventProcessor(ILogger logger, IMapper mapper, IMaintenanceService maintenanceService) : base(logger, mapper)
		{
			_maintenanceService = maintenanceService;
		}

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<PurgeReagentsStatusEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for Purge Reagents Status");
            _responseStream = responseStream;

            // This is done to help when debugging both the UI and OpcUa - this will initialize the variable when it is subscribed to.
			_subscription = _maintenanceService.SubscribePurgeReagentsStatus().Subscribe(SetPurgeReagentsStatus);
			base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for Purge Reagents Status");
        }

        private void SetPurgeReagentsStatus(ePurgeReagentLinesState status)
        {
	        var message = new PurgeReagentsStatusEvent()
	        {
		        Status = _mapper.Map<PurgeReagentsStatusEnum>(status)
	        };

	        QueueMessage(message);
        }
	}
}