using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcServer.Enums;
using Microsoft.Extensions.Configuration;
using ScoutServices.Interfaces;
using ScoutUtilities.Helper;
using System.Reflection;
using System.Threading.Tasks;

namespace GrpcServer.AutomationLock
{
    public class AutomationLockInterceptor : Interceptor
    {
        #region Constructor

        public AutomationLockInterceptor(IConfiguration configuration, ILockManager lockManager)
        {
            _configuration = configuration;
            _lockManager = lockManager;
        }

        #endregion

        #region Properties & Fields

        private readonly IConfiguration _configuration;
        private readonly ILockManager _lockManager;

        #endregion

        #region Helper Methods

        private void CheckAutomationLockRequirements(MethodInfo method)
        {
            var methodLockAttribute = ReflectionHelpers.GetCustomAttribute<RequiresAutomationLockAttribute>(method);
            // No RequiresAutomationLockAttribute means 'method' does NOT require a lock before use
            
            if (!LockMatchesLockRequirements(methodLockAttribute))
            {
                var lockedStr = _lockManager.IsLocked() ? "Locked" : "Unlocked";
                var msg = $"Cannot perform action '{method?.Name}'. The Instrument's lock state ({lockedStr}) does not match the required locked state ({methodLockAttribute.LockStateRequirement}) for the requested operation.";
                
                var status = new Status(StatusCode.FailedPrecondition, msg);
                throw new RpcException(status, "LockState not valid for requested operation");
            }
        }

        private bool LockMatchesLockRequirements(RequiresAutomationLockAttribute attribute)
        {
            if (attribute == null)
                return true;
            if (attribute.LockStateRequirement == LockRequirements.NoRequirements)
                return true;
            if (attribute.LockStateRequirement == LockRequirements.RequiresLocked)
                return _lockManager.IsLocked();
            if (attribute.LockStateRequirement == LockRequirements.RequiresUnlocked)
                return !_lockManager.IsLocked();

            return false;
        }
        
        #endregion

        #region Override Interceptor Methods

        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            CheckAutomationLockRequirements(continuation.Method);
            return base.UnaryServerHandler(request, context, continuation);
        }

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream, ServerCallContext context, 
            DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            CheckAutomationLockRequirements(continuation.Method);
            return base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, 
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            CheckAutomationLockRequirements(continuation.Method);
            return base.ServerStreamingServerHandler(request, responseStream, context, continuation);
        }

        #endregion
    }
}