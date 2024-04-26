using System.Linq;
using System.Reflection;
using Grpc.Core;
using GrpcServer.GrpcInterceptor.Attributes;
using ScoutUtilities.Helper;

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
    public partial class ScoutInterceptor // Handles the Instrument State requirements for the OPC Methods
    {
        private void CheckInstrumentStateRequirements(MethodInfo method)
        {
            var instrumentStateAttribute = ReflectionHelpers.GetCustomAttribute<InstrumentStateAttribute>(method);
            // No InstrumentStateAttribute means 'method' does NOT require a particular instrument state before use

            if (instrumentStateAttribute == null)
                return;

            var validStates = instrumentStateAttribute.ValidSystemStates;
            var currentState = _instrumentStatusService.SystemStatus;
            if (!validStates.Contains(currentState))
            {
                var msg = $"Cannot perform action '{method.Name}'. The Instrument's State ({currentState}) does not allow it to execute.";

                var status = new Status(StatusCode.FailedPrecondition, msg);
                throw new RpcException(status, "Instrument State not valid for requested operation");
            }
        }
    }
}