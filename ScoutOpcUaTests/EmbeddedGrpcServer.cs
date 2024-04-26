using System.Diagnostics;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcServer;
using GrpcServer.AutomationLock;
using GrpcServer.EventProcessors;
using GrpcServer.GrpcInterceptor;
using GrpcServer.GrpcInterceptor.Attributes;
using GrpcService;
using ScoutModels.Interfaces;
using ScoutServices.Enums;
using ScoutServices.Interfaces;
using ScoutUtilities.Enums;

namespace ScoutOpcUaTests
{
    public class EmbeddedGrpcServer : GrpcServices.GrpcServicesBase
    {
        private readonly GrpcBaseTestClass _testClass;
        private Server _grpcServer;
        private readonly ScoutInterceptor _securityInterceptor;
        private readonly ILockManager _lockManager;
        private readonly ISecurityService _securityService;

        public EmbeddedGrpcServer(ILockManager lockManager, ISecurityService securityService, ScoutInterceptor securityInterceptor, GrpcBaseTestClass testClass)
        {
            _lockManager = lockManager;
            _securityService = securityService;
            _securityInterceptor = securityInterceptor;
            _testClass = testClass;
        }

        public void Start()
        {
            _grpcServer = new Server
            {
                Ports = {new ServerPort(OpcConstants.GrpcHostIp, OpcConstants.GrpcPort, ServerCredentials.Insecure)},
                Services = {GrpcServices.BindService(this).Intercept(_securityInterceptor)}
            };

            _grpcServer.Start();
        }

        public void Stop()
        {
            _grpcServer?.ShutdownAsync().Wait();
        }

        [Permission(new UserPermissionLevel[] { UserPermissionLevel.eAdministrator })]
        public override Task<VcbResultRequestLock> RequestLock(RequestRequestLock request, ServerCallContext context)
        {
            Debug.WriteLine("EmbeddedGrpcServer.AutomationLock(): Entered");
            _lockManager.PublishAutomationLock(LockResult.Locked, "fakeUser");

            // Return success.
            return Task.FromResult(new VcbResultRequestLock { Description = "Success", ErrorLevel = ErrorLevelEnum.NoError, MethodResult = MethodResultEnum.Success, LockState = LockStateEnum.Locked});
        }

        public override Task<VcbResultReleaseLock> ReleaseLock(RequestReleaseLock request, ServerCallContext context)
        {
            return base.ReleaseLock(request, context);
        }

        public override Task<VcbResultGetSampleResults> GetSampleResults(RequestGetSampleResults request, ServerCallContext context)
        {
            return base.GetSampleResults(request, context);
        }

        [Permission(new UserPermissionLevel[] { UserPermissionLevel.eAdministrator })]
        public override Task SubscribeLockState(RegistrationRequest request, IServerStreamWriter<LockStateChangedEvent> responseStream, ServerCallContext context)
        {
            Debug.WriteLine("EmbeddedGrpcServer.SubscribeLockState(): Entered");
            var resultProcessor = _testClass.CreateEventProcessor<LockResultProcessor>();
            var task = Task.Run(() => resultProcessor.Subscribe(context, responseStream));
            return task;
        }

        public override Task<VcbResult> LoginRemoteUser(RequestLoginUser request, ServerCallContext context)
        {
            Debug.WriteLine("EmbeddedGrpcServer.LoginRemoteUser(): Entered");
            var result = _securityService.LoginRemoteUser(request.Username, request.Password);

            // Return success.
            return Task.FromResult(result? MakeSuccess() : MakeFailure());
        }
        public override Task<VcbResult> LogoutRemoteUser(RequestLogoutUser request, ServerCallContext context)
        {
            Debug.WriteLine("EmbeddedGrpcServer.LogoutRemoteUser(): Entered");
            _securityService.LogoutRemoteUser(request.Username);
            return Task.FromResult(MakeSuccess());
        }
        private VcbResult MakeSuccess()
        {
            return new VcbResult
            {
                Description = "Success",
                ErrorLevel = ErrorLevelEnum.NoError,
                MethodResult = MethodResultEnum.Success
            };
        }
        private VcbResult MakeFailure()
        {
            return new VcbResult
            {
                Description = "Failure",
                ErrorLevel = ErrorLevelEnum.Error,
                MethodResult = MethodResultEnum.Failure
            };
        }
    }
}