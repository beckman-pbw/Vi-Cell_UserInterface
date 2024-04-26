using System.Threading;

namespace GrpcServer
{
    public interface ITestRegisterMutex
    {
        AutoResetEvent RegisterMutex { get; }
    }
}