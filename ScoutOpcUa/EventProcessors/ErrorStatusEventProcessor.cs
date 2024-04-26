using System;
using System.Collections.Generic;
using AutoMapper;
using Google.Protobuf.Collections;
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
    public class ErrorStatusEventProcessor : EventProcessor<ErrorStatusEvent>
    {
        protected IInstrumentStatusService _instrumentStatusService;

        public ErrorStatusEventProcessor(ILogger logger, IMapper mapper, IInstrumentStatusService instrumentStatusService) : base(logger, mapper)
        {
            _instrumentStatusService = instrumentStatusService;
        }

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<ErrorStatusEvent> responseStream)
        {
            _subscription?.Dispose();
            _logger.Debug("Subscription started for Error Status changed");
            _responseStream = responseStream;

            // This is done to help when debugging both the UI and OpcUa - this will initialize the variable
			// when it is subscribed to.
            _subscription = _instrumentStatusService.SubscribeErrorStatusCallback().Subscribe(SetErrorStatus);
            base.Subscribe(context, responseStream);
            _logger.Debug("Subscription ended for Error Status changed");
        }

        private void SetErrorStatus (ScoutUtilities.Structs.ErrorStatusType errorStatus)
        {
	        var status = new ErrorStatusType();
	        try
	        {
		        status = _mapper.Map<ErrorStatusType>(errorStatus);
	        }
	        catch (Exception e)
	        {
		        _logger.Error(e, $"Failed to map the event args. Passing the event without a SampleConfig");
	        }

			var message = new ErrorStatusEvent
	        {
		        Status = status
			};

			QueueMessage(message);
        }
    }
}
