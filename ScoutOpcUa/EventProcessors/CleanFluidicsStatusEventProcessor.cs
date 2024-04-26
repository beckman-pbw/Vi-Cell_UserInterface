using System;
using AutoMapper;
using Grpc.Core;
using GrpcService;
using Ninject.Extensions.Logging;
using ScoutServices.Interfaces;
using ScoutUtilities.Enums;

// ReSharper disable InconsistentNaming

namespace GrpcServer.EventProcessors
{
    public class CleanFluidicsStatusEventProcessor : EventProcessor<CleanFluidicsStatusEvent>
    {
	    protected IMaintenanceService _maintenanceService;

		public CleanFluidicsStatusEventProcessor(ILogger logger, IMapper mapper, IMaintenanceService maintenanceService) : base(logger, mapper)
		{
			_maintenanceService = maintenanceService;
		}

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<CleanFluidicsStatusEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for CleanFluidics Status");
            _responseStream = responseStream;
			
            // This is done to help when debugging both the UI and OpcUa - this will initialize the variable when it is subscribed to.
			_subscription = _maintenanceService.SubscribeCleanFluidicsStatus().Subscribe(SetCleanFluidicsStatus);
			base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for Clean Fluidics Status");
        }

        private void SetCleanFluidicsStatus (eFlushFlowCellState status)
        {
	        var message = new CleanFluidicsStatusEvent
			{
				Status = _mapper.Map<CleanFluidicsStatusEnum>(status)
	        };

			QueueMessage(message);
        }
    }
}