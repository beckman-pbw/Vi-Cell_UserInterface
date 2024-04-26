using System;
using AutoMapper;
using Grpc.Core;
using GrpcService;
using Ninject.Extensions.Logging;
using ScoutServices.Enums;
using ScoutServices.Interfaces;

// ReSharper disable InconsistentNaming

namespace GrpcServer.EventProcessors
{
    /// <summary>
    /// A LockResultProcessor exists for each gRPC/OPC client and is associated with its GrpcClient instance.
    /// </summary>
    public class LockResultProcessor : EventProcessor<LockStateChangedEvent>
    {
        protected ILockManager _lockManager;

        public LockResultProcessor(ILogger logger, IMapper mapper, ILockManager lockManager) : base(logger, mapper)
        {
            _lockManager = lockManager;
        }

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<LockStateChangedEvent> responseStream)
        {
            _subscription?.Dispose();
			// Keep for debugging: _logger.Debug("LockResultProcessor:: Subscribe: enter");
			_responseStream = responseStream;
            SetLockStatus(_lockManager.IsLocked() ? LockResult.Locked : LockResult.Unlocked);
            _subscription = _lockManager.SubscribeStateChanges().Subscribe(SetLockStatus);

            // Loops in Subscribe
            base.Subscribe(context, responseStream);
			// Keep for debugging: _logger.Debug("LockResultProcessor:: Subscribe: exit");
		}

		protected void SetLockStatus(LockResult res)
        {
            // Keep for debugging: _logger.Debug("LockResultProcessor::SetLockStatus: enter");
            var message = new LockStateChangedEvent
            {
                LockState = _mapper.Map<LockStateEnum>(res)
            };

            QueueMessage(message);
        }
    }
}