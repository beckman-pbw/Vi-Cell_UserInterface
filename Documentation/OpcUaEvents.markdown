# Implementing OPC/UA Event Support in ScoutX {#opcua_events}

ScoutX OPC/UA is implemented in a separate OpcUaServer process. Its design is a
thin layer with the OPC/UA serverside stubs calling a corresponding gRPC client
to the ScoutX process. The ScoutX process hosts a gRPC server, and its server
side stubs make calls into the ScoutServices layer.

## Design and Naming Considerations {#automapper}
There are multiple layers of POCO (Plain Old C# Objects) in the ScoutX system.

* ScoutX Domain classes provided by the new Scout Services layer
* gRPC message objects defined in the gRPC .proto file. This generates C# POCO classes used by both the server and the client.
* OPC/UA object types defined in the OPC/UA ModelDesign.xml file. This also generates C# POCO classes.

In order to reduce boiler plate code, we are using [AutoMapper](https://automapper.org/). This popular library has extensive
[documentation](https://docs.automapper.org/en/stable/Getting-started.html). ScoutX 1.4 has added integration with Ninject,
so the code to copy the values out of one layer's POCOs into the next, can be performed with a single line of code.
```csharp
OrderDto dto = mapper.Map<OrderDto>(order);
```
The caveat is that AutoMapper can do this automatically if the property names of both POCOs are the same. If they are not,
then additional instructions have to be given to AutoMapper to handle those exceptions.

## Implementing a new event
The example below shows how the [GrpcServer.LockResultProcessor](@ extends the [EventProcessor](@ref GrpcServer.EventProcessor).
An event source is needed, and going forward, we will use [Microsoft's Reactive Extensions](https://docs.microsoft.com/en-us/archive/msdn-magazine/2016/june/reactive-framework-scale-asynchronous-client-server-links-with-reactive).
The base generic class EventProcessor provides the message processing with the generic type being the gRPC message type. It
encapsulates the threading, ensuring that only one message is written to the queue at a time.

The derived class, in this case LockResultProcessor, is responsible for registering with the Reactive Subject to receive events.
This is done in the Subscribe() method. The SetLockStatus() could be named anything and calls the base class's QueueMessage()
method.

Note the Scout Services ILockManager is also being injected into the LockResultProcessor, which provides the Reactive Subject.

```csharp
    public class LockResultProcessor : EventProcessor<LockStateChangedEvent>
    {
        protected ILockManager _lockManager;

        public LockResultProcessor(ILogger logger, IMapper mapper, ILockManager lockManager) : base(logger, mapper)
        {
            _lockManager = lockManager;
        }

        public override void Subscribe(ServerCallContext context, IServerStreamWriter<LockStateChangedEvent> responseStream)
        {
            if (null != _subscription)
            {
                return;
            }

            _responseStream = responseStream;
            _subscription = _lockManager.SubscribeStateChanges().Subscribe(SetLockStatus);
            base.Subscribe(context, responseStream);
        }

        private void SetLockStatus(LockResult res)
        {
            var message = new LockStateChangedEvent
            {
                LockState = _mapper.Map<LockStateEnum>(res)
            };

            QueueMessage(message);
        }
    }
```
Note the use of the IMapper interface. With a single call we can map a hierarchy of objects. In this case, we are only mapping
a single enum. To setup up the mapping, we add an entry, or possible multiple entries, in the case of more complex structure in
the [OpcUaGrpcModule](@ref GrpcServer.OpcUaGrpcModule) class in the CreateConfiguration() method. [Consistent naming](#automapper)
can reduce the need for designing custom mapping extensions. Here, a single CreateMap entry is needed to map the enumeration.
```csharp
       private MapperConfiguration CreateConfiguration()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                // Add all profiles for each gRPC event
                cfg.CreateMap<LockStateEnum, LockResult>();
            });

            return mapperConfig;
        }
```

The LockResultProcessor is then injected into the [GrpcClient](@ref GrpcServer.GrpcClient) class. The implication is that each
client maintains its own set of event processors. When the GrpcClient is disposed of, it cleans up both its subscriptions to
the Reactive Subject, any gRPC or other resources, and exits its message processing thread.

```csharp
   public class GrpcClient : IDisposable
    {
        private string _id;
        private string _username;
        private string _password;
        private readonly LockResultProcessor _lockResultProcessor;

        public GrpcClient(string clientId, string username, string password, LockResultProcessor lockResultProcessor)
        {
            _id = clientId;
            _username = username;
            _password = password;
            _lockResultProcessor = lockResultProcessor;
        }

        public void Dispose()
        {
            _lockResultProcessor.Dispose();
        }

        public void SubscribeLockResult(ServerCallContext context, IServerStreamWriter<LockStateChangedEvent> responseStream)
        {
            _lockResultProcessor.Subscribe(context, responseStream);
        }
    }
```

The SubscribeLockResult is called from the corresponding ScoutOpcUaGrpcService class, after it looks up the GrpcClient.

Note: In the OPCUA repo, in the OpcEventManager.cs ensure that in the RegisterForScoutEvents() method, that you have properly registered the event via AddRegisteredEvent():
```csharp
AddRegisteredEvent(_registeredEventFactory.CreateLockStateRegisteredEvent(ServiceUser,
                    _nodeManager.FindNode(ViCellBlu.ObjectTypes.LockStateChangedEvent)),
                nameof(LockStateChangedEvent));
```

## Troubleshooting Tips
- If and when you encounter connectivity issues or bugs related to gRPC, you can set these environment variables either in
the ScoutUI Debug Properties, or in your unit tests Setup() method.
```csharp
Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");
Environment.SetEnvironmentVariable("GRPC_TRACE", "channel,client_channel_call,connectivity_state,handshaker,server_channel,transport_security");
```
The above GRPC_TRACE settings are helpful, but if you need more, this [gRPC wiki page](https://github.com/grpc/grpc/blob/master/doc/environment_variables.md) has them all.
- If you encounter an issue where the OPC UA Client is not receiving expected OPC UA Events, the problem may be that the gRPC Server (ScoutUI) is not up and running first, before the HawkeyeOpcUa (OPC Server/gRPC Client).
  - If you are debugging and want to use your HawkeyeOpcUa code, you will need to stop the Watchdog service in ScoutUI.sln. Remove the ViCellOpcServer.exe from ScoutServices\Watchdog\WatchdogConfiguration.json to make it look like this:
```json
{
  "PollingIntervalMS": "5000",
  "servers": [ "\\Instrument\\OPCUaServer\\ViCellOpcUaServer.exe" ]
}
```
