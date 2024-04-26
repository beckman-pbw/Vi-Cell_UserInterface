using System;
using System.Collections.Concurrent;
using Ninject.Extensions.Logging;

namespace GrpcServer
{
    /// <summary>
    /// The client manager maintains a dictionary of GrpcClient(s). When a client registers for new events, the client Id
    /// is used to look up the client, so that the event can be added to it. When a client logs out, it is removed from the
    /// dictionary, all of its resources are freed, and the events it has registered for are removed from their
    /// corresponding Reactive subjects.
    /// </summary>
    public class GrpcClientManager
    {
        // The string key is actually a Guid
        private ConcurrentDictionary<string, GrpcClient> _clientLookup = new ConcurrentDictionary<string, GrpcClient>();
        private IOpcUaGrpcFactory _opcUaGrpcFactory;
        private ILogger _logger;


        public GrpcClientManager(IOpcUaGrpcFactory opcUaGrpcFactory, ILogger logger)
        {
            _opcUaGrpcFactory = opcUaGrpcFactory;
            _logger = logger;
        }

        /// <summary>
        /// Will not add another client if the same clientId already exists.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public GrpcClient AddClient(string clientId, string username, string password)
        {
            var client = Lookup(clientId);
            if (client == null)
            {
                client = _opcUaGrpcFactory.CreateGrpcClient(clientId, username, password);
                if (client != null)
                {
                    _clientLookup.TryAdd(clientId, client);
                }
            }
            return client;
        }

        public GrpcClient Lookup(string clientId)
        {
            return _clientLookup.TryGetValue(clientId, out var client) ? client : null;
        }

    }
}