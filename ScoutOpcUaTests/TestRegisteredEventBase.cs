using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcClient;
using GrpcClient.Interfaces;
using GrpcService;

namespace ScoutOpcUaTests
{
    public abstract class TestRegisteredEventBase<T> : RegisteredEvent<T>
    {
        private readonly AutoResetEvent _msgWait = new AutoResetEvent(false);
        private readonly List<T> _messageHistory = new List<T>();

        public TestRegisteredEventBase(Ninject.Extensions.Logging.ILogger logger, IGrpcClient client) : base(logger, client)
        {
        }

        /// <summary>
        /// In the caller, save the value. Don't call twice, unless you send additional messages.
        /// </summary>
        public List<T> MessageHistory
        {
            get
            {
                Debug.WriteLine($"{GetType().Name}.MessageHistory: About to wait");
                _msgWait.WaitOne();
                Debug.WriteLine($"{GetType().Name}.MessageHistory: Signaled and continuing");
                return _messageHistory;
            }
        }

        protected override void OnMessage(T msg)
        {
            Debug.WriteLine($"{GetType().Name}.OnMessage(): Entered");
            _messageHistory.Add(msg);
            _msgWait.Set();
        }

        public abstract void RegisterForTest();

        public override void Register()
        {
            Debug.WriteLine($"{GetType().Name}.Register(): Entered");
            RegisterForTest();
            var task = StreamingCall.ResponseHeadersAsync;
            Client.AddRegisteredEvent(this);
            var registerResponseReceived = new Action<Task<Metadata>>((task1) =>
            {
                Debug.WriteLine($"{GetType().Name}.Register(): Response Header Received.");
            });
            task.ContinueWith(registerResponseReceived);
            Debug.WriteLine($"{GetType().Name}.Register(): Exiting");
        }
    }
}