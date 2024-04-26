using Grpc.Core;
using GrpcServer.Enums;
using GrpcServer.GrpcInterceptor.Attributes;
using ScoutUtilities.Helper;
using System.Reflection;

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
    public partial class ScoutInterceptor // Handles the Lock State requirements for the OPC Methods
    {
        private void CheckLockOwnerRequirements(Metadata metadata, MethodInfo method)
        {
            var mustOwnLockAttribute = ReflectionHelpers.GetCustomAttribute<MustOwnLockAttribute>(method);
            // No MustOwnLockAttribute means 'method' does NOT require user to own the lock before use

            if (mustOwnLockAttribute != null)
            {
                var hasCredentials = ExtractBasicAuthCredentials(metadata, out var cnxId, out var username, out _);
                if (!hasCredentials)
                {
                    var msg = $"Unable to find the username for the requested action ('{method.Name}'). " +
                              $"This action requires that the user who makes the call owns the current automation lock.";
                    var status = new Status(StatusCode.PermissionDenied, msg);
                    throw new RpcException(status, "Unable to find the username for the requested action.");
                }

                if (!_lockManager.OwnsLock(username))
                {
                    var msg = $"User '{username}' does not own the current automation lock. " +
                              $"'{method.Name}' cannot be performed.";
                    var status = new Status(StatusCode.PermissionDenied, msg);
                    throw new RpcException(status, "User cannot perform requested action.");
                }
            }
        }

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
    }
}