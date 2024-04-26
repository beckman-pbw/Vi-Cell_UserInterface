using ScoutUtilities.Enums;
using System;

namespace GrpcServer.GrpcInterceptor.Attributes
{
    /// <summary>
    /// Annotation for gRPC methods. The ScoutInterceptor uses reflection to determine if the user has
    /// is allowed to execute the method based on the current state of the instrument.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class InstrumentStateAttribute : Attribute
    {
        public SystemStatus[] ValidSystemStates { get; set; }

        public InstrumentStateAttribute(SystemStatus[] validSystemStates)
        {
            ValidSystemStates = validSystemStates;
        }
    }
}