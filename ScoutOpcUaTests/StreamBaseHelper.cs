using System;
using Grpc.Core;
using GrpcService;

namespace GrpcServer
{
    public static class StreamBaseHelper
    {
        public static void CallBaseBaseSubscribe<T>(object thisPtr, ServerCallContext context, IServerStreamWriter<T> responseStream)
        {
            var ptr = typeof(EventProcessor<T>)?.GetMethod("Subscribe")?.MethodHandle.GetFunctionPointer();
            var baseSubscribe =
                (Action<ServerCallContext, IServerStreamWriter<T>>) Activator.CreateInstance(
                    typeof(Action<ServerCallContext, IServerStreamWriter<T>>), thisPtr, ptr);
            baseSubscribe.DynamicInvoke(context, responseStream);
        }
    }
}