using GrpcService;
using NUnit.Framework;
using ScoutUtilities.Enums;
using System;
using Grpc.Core;
using SystemStatus = ScoutUtilities.Enums.SystemStatus;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class InstrumentStateInterceptorTests : BaseInterceptorTest
    {
        protected override void AssertRpcExceptionDetailText(RpcException rpcException)
        {
            Assert.AreEqual(StatusCode.FailedPrecondition, rpcException.StatusCode);
            Assert.IsTrue(rpcException.Status.Detail.Contains("The Instrument's State"),
                "rpcException did not contain the expected string - 1");
            Assert.IsTrue(rpcException.Status.Detail.Contains("does not allow it to execute."),
                "rpcException did not contain the expected string - 2");
        }

        #region AutomationLock

        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopped)]
        public void AutomationLock_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Unlocked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestRequestLock(new RequestRequestLock());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Paused)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void AutomationLock_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Unlocked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestRequestLock(new RequestRequestLock());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region AutomationUnlock

        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Stopped)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void AutomationUnlock_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestReleaseLock(new RequestReleaseLock());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Paused)]
        public void AutomationUnlock_FailureWhenInValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestReleaseLock(new RequestReleaseLock());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region CreateCellType

        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopped)]
        public void CreateCellType_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestCreateCellType(new RequestCreateCellType());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Paused)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void CreateCellType_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestCreateCellType(new RequestCreateCellType());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region DeleteCellType

        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopped)]
        public void DeleteCellType_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestDeleteCellType(new RequestDeleteCellType());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Paused)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void DeleteCellType_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestDeleteCellType(new RequestDeleteCellType());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region CreateQualityControl

        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopped)]
        public void CreateQualityControl_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestCreateQualityControl(new RequestCreateQualityControl());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Paused)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void CreateQualityControl_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestCreateQualityControl(new RequestCreateQualityControl());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region DeleteSampleResults

        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopped)]
        public void DeleteSampleResults_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestDeleteSampleResults(new RequestDeleteSampleResults());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Paused)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void DeleteSampleResults_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestDeleteSampleResults(new RequestDeleteSampleResults());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region EjectStageMethods

        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopped)]
        public void EjectStageMethods_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestEjectStage(new RequestEjectStage());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Paused)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void EjectStageMethods_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestEjectStage(new RequestEjectStage());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region ExportConfigFiles

        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopped)]
        public void ExportConfigFiles_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestExportConfig(new RequestExportConfig());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Paused)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void ExportConfigFiles_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestExportConfig(new RequestExportConfig());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region RetrieveSampleExport

        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopped)]
        [Ignore("Jenkins build hangs on this tests call to backend")]
        public void RetrieveSampleExport_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendStartExport(new RequestStartExport { SampleListUuid = { Guid.NewGuid().ToString() } });
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Paused)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void RetrieveSampleExport_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendStartExport(new RequestStartExport());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region GetSampleResults

        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopped)]
        public void GetSampleResults_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestGetSampleResults(new RequestGetSampleResults());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Paused)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void GetSampleResults_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestGetSampleResults(new RequestGetSampleResults());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region ImportConfigFiles

        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopped)]
        public void ImportConfigFiles_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestImportConfig(new RequestImportConfig());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Paused)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void ImportConfigFiles_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestImportConfig(new RequestImportConfig());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region Resume

        [TestCase(SystemStatus.Paused)]
        public void Resume_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestResume(new RequestResume());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Stopped)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void Resume_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestResume(new RequestResume());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region StartMultipleSamples

        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopped)]
        public void StartMultipleSamples_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestStartSampleSet(new RequestStartSampleSet());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Paused)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void StartMultipleSamples_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestStartSampleSet(new RequestStartSampleSet());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region StartSingleSample

        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopped)]
        public void StartSingleSample_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestStartSample(new RequestStartSample());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Paused)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void StartSingleSample_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestStartSample(new RequestStartSample());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region Stop

        [TestCase(SystemStatus.ProcessingSample)]
        [TestCase(SystemStatus.Paused)]
        public void Stop_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestStop(new RequestStop());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Stopped)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void Stop_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestStop(new RequestStop());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region Pause

        [TestCase(SystemStatus.ProcessingSample)]
        public void Pause_SuccessWhenValidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestPause(new RequestPause());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(SystemStatus.Paused)]
        [TestCase(SystemStatus.Pausing)]
        [TestCase(SystemStatus.Idle)]
        [TestCase(SystemStatus.Stopping)]
        [TestCase(SystemStatus.Stopped)]
        [TestCase(SystemStatus.Faulted)]
        [TestCase(SystemStatus.SearchingTube)]
        public void Pause_FailureWhenInvalidState(SystemStatus status)
        {
            try
            {
                Init(LockStateEnum.Locked, UserPermissionLevel.eAdministrator, status);
                GrpcClient.SendRequestPause(new RequestPause());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion
    }
}