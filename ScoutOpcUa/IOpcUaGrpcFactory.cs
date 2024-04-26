namespace GrpcServer
{
    public interface IOpcUaGrpcFactory
    {
        GrpcClient CreateGrpcClient(string clientId, string username, string password);
    }
}