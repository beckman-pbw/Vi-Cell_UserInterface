using System;
using ScoutUtilities.Enums;

namespace GrpcServer.GrpcInterceptor.Attributes
{
    /// <summary>
    /// Annotation for gRPC methods. The ScoutInterceptor uses reflection to determine if the user has
    /// the right to execute the method.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class PermissionAttribute : Attribute
    {
        public UserPermissionLevel[] Permissions { get; set; }

        public PermissionAttribute(UserPermissionLevel[] permissions)
        {
            Permissions = permissions;
        }
    }
}