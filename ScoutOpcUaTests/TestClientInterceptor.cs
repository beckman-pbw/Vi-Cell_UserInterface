using Grpc.Core;
using Grpc.Core.Interceptors;

namespace ScoutOpcUaTests
{
    public class TestClientInterceptor : Interceptor
    {
        private static TestClientException TransformException(RpcException e)
        {
            var testStatusCode = TestClientException.ERROR_CODE_UNEXPECTED;
            var msg = TestClientException.ERROR_TEXT_UNEXPECTED;
            switch (e.StatusCode)
            {
                case StatusCode.PermissionDenied:
                    testStatusCode = TestClientException.ERROR_CODE_PERMISSION_DENIED;
                    msg = TestClientException.ERROR_TEXT_PERMISSION_DENIED;
                    break;
                case StatusCode.FailedPrecondition:
                    testStatusCode = TestClientException.ERROR_CODE_BAD_LOCK_STATE;
                    msg = string.IsNullOrEmpty(e.Status.Detail) ? TestClientException.ERROR_TEXT_BAD_LOCK_STATE : e.Status.Detail;
                    break;
                default:
                    break;
            }

            return new TestClientException(msg, testStatusCode);
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                return base.BlockingUnaryCall(request, context, continuation);
            }
            catch (RpcException e)
            {
                throw TransformException(e);
            }
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                return base.AsyncUnaryCall(request, context, continuation);
            }
            catch (RpcException e)
            {
                throw TransformException(e);
            }
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                return base.AsyncServerStreamingCall(request, context, continuation);
            }
            catch (RpcException e)
            {
                throw TransformException(e);
            }
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                return base.AsyncClientStreamingCall(context, continuation);
            }
            catch (RpcException e)
            {
                throw TransformException(e);
            }
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                return base.AsyncDuplexStreamingCall(context, continuation);
            }
            catch (RpcException e)
            {
                throw TransformException(e);
            }
        }
    }
}