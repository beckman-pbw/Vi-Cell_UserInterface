using System;

namespace GrpcServer.GrpcInterceptor.Attributes
{
    /// <summary>
    /// Annotation for gRPC methods. The ScoutInterceptor uses reflection to determine if the user has
    /// owns the current automation lock and is allowed to execute the method.
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Method)]
    public class MustOwnLockAttribute : Attribute
    {
        public MustOwnLockAttribute()
        {
        }
    }
}