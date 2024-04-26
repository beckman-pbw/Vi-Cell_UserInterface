using AutoMapper;
using Grpc.Core;
using GrpcServer.EventProcessors;
using GrpcService;
using Ninject.Extensions.Logging;
using ScoutOpcUaTests;
using ScoutServices.Enums;
using ScoutServices.Interfaces;
using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading;
// ReSharper disable InconsistentNaming

namespace GrpcServer
{
    /// <summary>
    /// A LockResultProcessor exists for each gRPC/OPC client and is associated with its GrpcClient instance.
    /// </summary>
    public class TestLockResultProcessor : LockResultProcessor, ITestRegisterMutex
    {
        private readonly GrpcBaseTestClass _testClass;
        public AutoResetEvent RegisterMutex { get; } = new AutoResetEvent(false);

        public TestLockResultProcessor(ILogger logger, IMapper mapper, ILockManager lockManager, GrpcBaseTestClass testClass) : base(logger, mapper, lockManager)
        {
            _testClass = testClass;
         }

        /// <summary>
        /// Duplicate subscriptions will be ignored.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseStream"></param>
        public override void Subscribe(ServerCallContext context, IServerStreamWriter<LockStateChangedEvent> responseStream)
        {
            _subscription?.Dispose();
            Debug.WriteLine("TestLockResultProcessor.Subscribe(): Entered - Subscription started for LockManager state.");

            _responseStream = responseStream;
            _subscription = _lockManager.SubscribeStateChanges().Subscribe(SetLockStatus);
            RegisterMutex.Set();
            Debug.WriteLine("TestLockResultProcessor.Subscribe(): Called RegisterWait.Set().");

            if (_lockManager.SubscribeStateChanges() is Subject<LockResult> lockStateSubject)
            {
                Debug.WriteLine($"LockResultProcessor.Subscribe(): Just subscribed to lock state, HasObservers={lockStateSubject.HasObservers}");
            }

            // Loops in base.base.Subscribe (skip LockResultProcessor implementation)
            StreamBaseHelper.CallBaseBaseSubscribe<LockStateChangedEvent>(this, context, responseStream);
            Debug.WriteLine("LockResultProcessor.Subscribe(): Exit");
        }
    }
}