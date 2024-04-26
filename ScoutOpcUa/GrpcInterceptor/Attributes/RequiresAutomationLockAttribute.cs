using System;
using GrpcServer.Enums;

namespace GrpcServer.GrpcInterceptor.Attributes
{
    /// <summary>
    /// Annotation for gRPC methods. The ScoutInterceptor uses reflection to determine if the user has
    /// is allowed to execute the method based on the current state of the automation lock.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RequiresAutomationLockAttribute : Attribute
    {
        public LockRequirements LockStateRequirement { get; set; }

        public RequiresAutomationLockAttribute(LockRequirements lockStateRequirement = LockRequirements.RequiresLocked)
        {
            LockStateRequirement = lockStateRequirement;
        }
    }
}