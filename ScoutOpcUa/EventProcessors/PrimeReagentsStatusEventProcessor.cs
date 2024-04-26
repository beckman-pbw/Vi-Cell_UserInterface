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
    public class PrimeReagentsStatusEventProcessor : EventProcessor<PrimeReagentsStatusEvent>
    {
	    protected IMaintenanceService _maintenanceService;

		public PrimeReagentsStatusEventProcessor(ILogger logger, IMapper mapper, IMaintenanceService maintenanceService) : base(logger, mapper)
		{
			_maintenanceService = maintenanceService;
		}

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<PrimeReagentsStatusEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for Prime Reagents Status");
            _responseStream = responseStream;

            // This is done to help when debugging both the UI and OpcUa - this will initialize the variable when it is subscribed to.
			_subscription = _maintenanceService.SubscribePrimeReagentsStatus().Subscribe(SetPrimeReagentsStatus);
			base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for  Prime Reagents Status");
        }

        private void SetPrimeReagentsStatus(ePrimeReagentLinesState status)
        {
	        var message = new PrimeReagentsStatusEvent()
	        {
		        Status = _mapper.Map<PrimeReagentsStatusEnum>(status)
	        };

	        QueueMessage(message);
        }
	}
}