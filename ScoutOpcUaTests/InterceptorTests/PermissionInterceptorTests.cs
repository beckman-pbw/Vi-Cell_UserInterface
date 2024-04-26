using Grpc.Core;
using GrpcService;
using NUnit.Framework;
using ScoutUtilities.Enums;
using System;

namespace ScoutOpcUaTests
{
    public class PermissionInterceptorTests : BaseInterceptorTest
    {
        protected override void AssertRpcExceptionDetailText(RpcException rpcException)
        {
            Assert.AreEqual(StatusCode.PermissionDenied, rpcException.StatusCode);
            Assert.IsTrue(rpcException.Status.Detail.Contains("User not valid for requested operation"),
                "rpcException did not contain the expected string");
        }

        #region RequestAutomationLock

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eElevated)]
        [TestCase(UserPermissionLevel.eService)]
        public void RequestAutomationLock_SuccessWithAllUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Unlocked, permissionLevel);
                GrpcClient.SendRequestRequestLock(new RequestRequestLock());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region RequestAutomationUnlock

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eElevated)]
        [TestCase(UserPermissionLevel.eService)]
        public void RequestAutomationUnlock_SuccessWithAllUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestReleaseLock(new RequestReleaseLock());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region CreateCellType

        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eElevated)]
        public void CreateCellType_SuccessWithValidUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestCreateCellType(new RequestCreateCellType() { Cell = new CellType() });
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eService)]
        public void CreateCellType_FailureWithInvalidUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestCreateCellType(new RequestCreateCellType() {Cell = new CellType()});
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region DeleteCellType

        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eElevated)]
        public void DeleteCellType_SuccessWithValidUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestDeleteCellType(new RequestDeleteCellType() {CellTypeName = "none"});
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eService)]
        public void DeleteCellType_FailureWithInvalidUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestDeleteCellType(new RequestDeleteCellType() { CellTypeName = "none" });
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region CreateQualityControl

        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eElevated)]
        public void CreateQualityControl_SuccessWithValidUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestCreateQualityControl(
                    new RequestCreateQualityControl {QualityControl = new QualityControl()});
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eService)]
        public void CreateQualityControl_FailureWithInvalidUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestCreateQualityControl(
                    new RequestCreateQualityControl { QualityControl = new QualityControl() });
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region DeleteSampleResults

        [TestCase(UserPermissionLevel.eAdministrator)]
        public void DeleteSampleResults_SuccessWithValidUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestDeleteSampleResults(new RequestDeleteSampleResults
                {
                    Uuids = {Guid.Empty.ToString()},
                    RetainResultsAndFirstImage = false
                });
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eElevated)]
        [TestCase(UserPermissionLevel.eService)]
        public void DeleteSampleResults_FailureWithInvalidUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestDeleteSampleResults(new RequestDeleteSampleResults
                {
                    Uuids = { Guid.Empty.ToString() },
                    RetainResultsAndFirstImage = false
                });
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region EjectStageMethods

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eElevated)]
        [TestCase(UserPermissionLevel.eService)]
        public void EjectStageMethods_SuccessWithAllUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestEjectStage(new RequestEjectStage());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region ExportConfigFiles

        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eService)]
        public void ExportConfigFiles_SuccessWithValidUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestExportConfig(new RequestExportConfig());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eElevated)]
        public void ExportConfigFiles_FailureWithInvalidUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
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

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eElevated)]
        [TestCase(UserPermissionLevel.eService)]
        [Ignore("Ignoring due to backend hanging on initialization on jenkins")]
        public void RetrieveSampleExport_SuccessWithAllUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendStartExport(new RequestStartExport { SampleListUuid = { Guid.NewGuid().ToString() } });
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region GetSampleResults

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eElevated)]
        [TestCase(UserPermissionLevel.eService)]
        public void GetSampleResults_SuccessWithAllUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestGetSampleResults(new RequestGetSampleResults());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region ImportConfigFiles

        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eService)]
        public void ImportConfigFiles_SuccessWithValidUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestImportConfig(new RequestImportConfig());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eElevated)]
        public void ImportConfigFiles_FailureWithInvalidUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
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

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eElevated)]
        [TestCase(UserPermissionLevel.eService)]
        public void Resume_SuccessWithAllUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel, SystemStatus.Paused);
                GrpcClient.SendRequestResume(new RequestResume());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region StartMultipleSamples

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eElevated)]
        [TestCase(UserPermissionLevel.eService)]
        public void StartMultipleSamples_SuccessWithAllUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestStartSampleSet(new RequestStartSampleSet());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region StartSingleSample

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eElevated)]
        [TestCase(UserPermissionLevel.eService)]
        public void StartSingleSample_SuccessWithAllUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel);
                GrpcClient.SendRequestStartSample(new RequestStartSample());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region Stop

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eElevated)]
        [TestCase(UserPermissionLevel.eService)]
        public void Stop_SuccessWithAllUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel, SystemStatus.ProcessingSample);
                GrpcClient.SendRequestStop(new RequestStop());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region Pause

        [TestCase(UserPermissionLevel.eNormal)]
        [TestCase(UserPermissionLevel.eAdministrator)]
        [TestCase(UserPermissionLevel.eElevated)]
        [TestCase(UserPermissionLevel.eService)]
        public void Pause_SuccessWithAllUsers(UserPermissionLevel permissionLevel)
        {
            try
            {
                Init(LockStateEnum.Locked, permissionLevel, SystemStatus.ProcessingSample);
                GrpcClient.SendRequestPause(new RequestPause());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion
    }
}