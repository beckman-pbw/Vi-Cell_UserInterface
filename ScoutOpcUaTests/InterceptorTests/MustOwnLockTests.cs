using Grpc.Core;
using GrpcService;
using NUnit.Framework;
using System;
using ScoutUtilities.Enums;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class MustOwnLockTests : BaseInterceptorTest
    {
        protected override void AssertRpcExceptionDetailText(RpcException rpcException)
        {
            Assert.AreEqual(StatusCode.PermissionDenied, rpcException.StatusCode);
            Assert.IsTrue(rpcException.Status.Detail.Contains("does not own the current automation lock."),
                "rpcException did not contain the expected string");
        }
        
        #region AutomationLock

        [Test]
        public void AutomationLock_SuccessWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Unlocked, ownsLockReturnsTrue: false);
                GrpcClient.SendRequestRequestLock(new RequestRequestLock());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void AutomationLock_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Unlocked, ownsLockReturnsTrue: true);
                GrpcClient.SendRequestRequestLock(new RequestRequestLock());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region AutomationUnlock

        [Test]
        public void AutomationUnlock_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false);
                GrpcClient.SendRequestReleaseLock(new RequestReleaseLock());
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        public void AutomationUnlock_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true);
                GrpcClient.SendRequestReleaseLock(new RequestReleaseLock());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region CreateCellType

        [Test]
        public void CreateCellType_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false);
                GrpcClient.SendRequestCreateCellType(new RequestCreateCellType());
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        public void CreateCellType_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true);
                GrpcClient.SendRequestCreateCellType(new RequestCreateCellType());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region DeleteCellType

        [Test]
        public void DeleteCellType_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false);
                GrpcClient.SendRequestDeleteCellType(new RequestDeleteCellType());
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        public void DeleteCellType_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true);
                GrpcClient.SendRequestDeleteCellType(new RequestDeleteCellType());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region CreateQualityControl

        [Test]
        public void CreateQualityControl_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false);
                GrpcClient.SendRequestCreateQualityControl(new RequestCreateQualityControl());
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        public void CreateQualityControl_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true);
                GrpcClient.SendRequestCreateQualityControl(new RequestCreateQualityControl());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region DeleteSampleResults

        [Test]
        public void DeleteSampleResults_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false);
                GrpcClient.SendRequestDeleteSampleResults(new RequestDeleteSampleResults());
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        public void DeleteSampleResults_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true);
                GrpcClient.SendRequestDeleteSampleResults(new RequestDeleteSampleResults());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region EjectStageMethods

        [Test]
        public void EjectStageMethods_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false);
                GrpcClient.SendRequestEjectStage(new RequestEjectStage());
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        public void EjectStageMethods_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true);
                GrpcClient.SendRequestEjectStage(new RequestEjectStage());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region ExportConfigFiles

        [Test]
        public void ExportConfigFiles_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false);
                GrpcClient.SendRequestExportConfig(new RequestExportConfig());
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        public void ExportConfigFiles_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true);
                GrpcClient.SendRequestExportConfig(new RequestExportConfig());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region RetrieveSampleExport

        [Test]
        [Ignore("Ignoring due to backend hanging on initialization on jenkins")]
        public void RetrieveSampleExport_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false);
                GrpcClient.SendStartExport(new RequestStartExport { SampleListUuid = { Guid.NewGuid().ToString() } });
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        [Ignore("Ignoring due to backend hanging on initialization on jenkins")]
        public void RetrieveSampleExport_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true);
                GrpcClient.SendStartExport(new RequestStartExport { SampleListUuid = { Guid.NewGuid().ToString() } });
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region GetSampleResults

        [Test]
        public void GetSampleResults_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false);
                GrpcClient.SendRequestGetSampleResults(new RequestGetSampleResults());
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        public void GetSampleResults_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true);
                GrpcClient.SendRequestGetSampleResults(new RequestGetSampleResults());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region ImportConfigFiles

        [Test]
        public void ImportConfigFiles_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false);
                GrpcClient.SendRequestImportConfig(new RequestImportConfig());
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        public void ImportConfigFiles_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true);
                GrpcClient.SendRequestImportConfig(new RequestImportConfig());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region Resume

        [Test]
        public void Resume_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false, systemStatus: SystemStatus.Paused);
                GrpcClient.SendRequestResume(new RequestResume());
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        public void Resume_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true, systemStatus: SystemStatus.Paused);
                GrpcClient.SendRequestResume(new RequestResume());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region StartMultipleSamples

        [Test]
        public void StartMultipleSamples_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false);
                GrpcClient.SendRequestStartSampleSet(new RequestStartSampleSet());
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        public void StartMultipleSamples_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true);
                GrpcClient.SendRequestStartSampleSet(new RequestStartSampleSet());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region StartSingleSample

        [Test]
        public void StartSingleSample_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false);
                GrpcClient.SendRequestStartSample(new RequestStartSample());
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        public void StartSingleSample_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true);
                GrpcClient.SendRequestStartSample(new RequestStartSample());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region Stop

        [Test]
        public void Stop_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false, systemStatus: SystemStatus.ProcessingSample);
                GrpcClient.SendRequestStop(new RequestStop());
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        public void Stop_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true, systemStatus: SystemStatus.ProcessingSample);
                GrpcClient.SendRequestStop(new RequestStop());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion

        #region Pause

        [Test]
        public void Pause_FailureWhenLockIsNotOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: false, systemStatus: SystemStatus.ProcessingSample);
                GrpcClient.SendRequestPause(new RequestPause());
                Assert.Fail($"AutomationUnlock should have thrown an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        [Test]
        public void Pause_SuccessWhenLockIsOwned()
        {
            try
            {
                Init(LockStateEnum.Locked, ownsLockReturnsTrue: true, systemStatus: SystemStatus.ProcessingSample);
                GrpcClient.SendRequestPause(new RequestPause());
                //No RpcException thrown so no issue with the lock state owner
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #endregion
    }
}