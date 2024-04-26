using GrpcServer.Enums;
using System;

namespace GrpcServer.AutomationLock
{
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