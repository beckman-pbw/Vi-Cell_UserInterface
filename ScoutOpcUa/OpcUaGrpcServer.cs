using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcServer.GrpcInterceptor;
using GrpcService;

namespace GrpcServer
{
    /// <summary>
    /// Defined as a singleton in Ninject
    /// </summary>
    public class OpcUaGrpcServer
    {
        private Server _grpcServer;
        private readonly GrpcServices.GrpcServicesBase _scoutOpcUaGrpcService;
        private readonly ScoutInterceptor _scoutInterceptor;

        public OpcUaGrpcServer(GrpcServices.GrpcServicesBase scoutOpcUaGrpcService, ScoutInterceptor scoutInterceptor)
        {
            _scoutOpcUaGrpcService = scoutOpcUaGrpcService;
            _scoutInterceptor = scoutInterceptor;
        }

        public void StartServer()
        {
            try
            {
                _grpcServer = new Server();

                // Despite being able to add multiple interceptors here, only having a single interceptor will work in
                // conjunction with our Attributes. When you add more than 1 interceptor, the "continuation.Method" 
                // parameter in the override methods turns into another method which makes it not possible to check
                // for the custom attributes we use (PermissionAttribute, RequiresAutomationLockAttribute, etc).
                // Because of this, we will only use a single interceptor as a partial class to segregate the 
                // different checks (see ScoutInterceptor, LockInterceptor.cs, SecurityInterceptor.cs).
                _grpcServer.Services.Add(GrpcServices.BindService(_scoutOpcUaGrpcService)
                                                     .Intercept(_scoutInterceptor));
                
                _grpcServer.Ports.Add(new ServerPort(OpcConstants.GrpcHostIp, OpcConstants.GrpcPort, ServerCredentials.Insecure));
                _grpcServer.Start();

                Console.WriteLine($"OPC/UA gRPC server listening on  {OpcConstants.GrpcHostIp}:{OpcConstants.GrpcPort}");
                Console.WriteLine("Press any key to stop the server...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OPC/UA gRPC server exception during startup: {ex.Message}");
            }
        }

        public void ShutdownServer()
        {
            _grpcServer?.ShutdownAsync().Wait();
        }
    }
}