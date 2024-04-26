using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Configuration;
using ScoutModels.Interfaces;
using ScoutServices.Interfaces;
using System.Reflection;
using System.Threading.Tasks;

namespace GrpcServer.GrpcInterceptor
{
    /// <summary>
    /// This partial class has several other files to complete and segregate code.
    /// Despite being able to add multiple interceptors to the gRPC server, only
    /// having a single interceptor will work in conjunction with our Attributes.
    /// When you add more than 1 interceptor, the "continuation.Method" 
    /// parameter in the override methods turns into another method (named "<AddMethod>b__0")
    /// which makes it impossible to check for the custom attributes we use
    /// (PermissionAttribute, RequiresAutomationLockAttribute, etc).
    /// Because of this, we will only use a single interceptor as a partial class to segregate the 
    /// different checks (see ScoutInterceptor, LockInterceptor.cs, SecurityInterceptor.cs).
    /// </summary>
    public partial class ScoutInterceptor : Interceptor
    {
        #region Constructor

        public ScoutInterceptor(IConfiguration configuration, ISecurityService securityService, ILockManager lockManager,
            IInstrumentStatusService instrumentStatusService)
        {
            _configuration = configuration;
            _securityService = securityService;
            _lockManager = lockManager;
            _instrumentStatusService = instrumentStatusService;
        }

        #endregion

        #region Properties & Fields

        private readonly IConfiguration _configuration;
        private readonly ILockManager _lockManager;
        private readonly ISecurityService _securityService;
        private readonly IInstrumentStatusService _instrumentStatusService;

        #endregion

        #region Override Interceptor Methods

        private void CheckMethodCallRequirements(Metadata context, MethodInfo method)
        {
            CheckAutomationLockRequirements(method);
            CheckLockOwnerRequirements(context, method);
            CheckInstrumentStateRequirements(method);
        }

        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, 
            ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            CheckMethodCallRequirements(context.RequestHeaders, continuation.Method);
            try
            {
                if (CheckHasPermission(context.RequestHeaders, continuation.Method))
                {
                    return base.UnaryServerHandler(request, context, continuation);
                }
            }
            finally
            {
            }
            var status = new Status(StatusCode.PermissionDenied, "User not valid for requested operation");
            throw new RpcException(status, "User not valid for requested operation");
        }
        
        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
            DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            CheckMethodCallRequirements(context.RequestHeaders, continuation.Method);
            try
            {
                if (CheckHasPermission(context.RequestHeaders, continuation.Method))
                {
                    return base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
                }
            }
            finally
            {
            }
            var status = new Status(StatusCode.PermissionDenied, "User not valid for requested operation");
            throw new RpcException(status, "User not valid for requested operation");
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            CheckMethodCallRequirements(context.RequestHeaders, continuation.Method);
            try
            {
                if (CheckHasPermission(context.RequestHeaders, continuation.Method))
                {
                    return base.ServerStreamingServerHandler(request, responseStream, context, continuation);
                }
            }
            finally
            {
            }
            var status = new Status(StatusCode.PermissionDenied, "User not valid for requested operation");
            throw new RpcException(status, "User not valid for requested operation");
        }

        #endregion
    }
}