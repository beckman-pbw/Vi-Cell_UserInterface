using Grpc.Core;
using GrpcService;
using NUnit.Framework;
using System;
using ScoutUtilities.Enums;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class AutomationLockInterceptorTests : BaseInterceptorTest
    {
        protected override void AssertRpcExceptionDetailText(RpcException rpcException)
        {
            Assert.AreEqual(StatusCode.FailedPrecondition, rpcException.StatusCode);
            Assert.IsTrue(rpcException.Status.Detail.Contains("lock state"),
                "rpcException did not contain the expected string - 1");
            Assert.IsTrue(rpcException.Status.Detail.Contains("does not match the required locked state"),
                "rpcException did not contain the expected string - 2");
        }

        #region RequestAutomationLock

        [Test]
        public void RequestAutomationLock_SuccessWhenUnlocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked);
                GrpcClient.SendRequestRequestLock(new RequestRequestLock());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void RequestAutomationLock_FailureWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked);
                GrpcClient.SendRequestRequestLock(new RequestRequestLock());
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region RequestReleaseAutomationLock

        [Test]
        public void RequestReleaseAutomationLock_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked);
                GrpcClient.SendRequestReleaseLock(new RequestReleaseLock());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void RequestReleaseAutomationLock_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked);
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

        [Test]
        public void CreateCellType_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked);
                GrpcClient.SendRequestCreateCellType(new RequestCreateCellType() {Cell = new CellType()});
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void CreateCellType_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked);
                GrpcClient.SendRequestCreateCellType(new RequestCreateCellType() { Cell = new CellType() });
                Assert.Fail($"Request was supposed to throw an RpcException ( ._.)");
            }
            catch (Exception e)
            {
                HandleShouldFailException(e);
            }
        }

        #endregion

        #region DeleteCellType

        [Test]
        public void DeleteCellType_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked);
                GrpcClient.SendRequestDeleteCellType(new RequestDeleteCellType());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void DeleteCellType_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked);
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

        [Test]
        public void CreateQualityControl_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked);
                GrpcClient.SendRequestCreateQualityControl(
                    new RequestCreateQualityControl() {QualityControl = new QualityControl()});
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void CreateQualityControl_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked);
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

        [Test]
        public void DeleteSampleResults_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked);
                GrpcClient.SendRequestDeleteSampleResults(new RequestDeleteSampleResults());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void DeleteSampleResults_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked);
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

        [Test]
        public void EjectStageMethods_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked);
                GrpcClient.SendRequestEjectStage(new RequestEjectStage());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void EjectStageMethods_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked);
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

        [Test]
        public void ExportConfigFiles_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked);
                GrpcClient.SendRequestExportConfig(new RequestExportConfig());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ExportConfigFiles_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked);
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

        [Test]
        public void RetrieveSampleExport_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked);
                GrpcClient.SendStartExport(new RequestStartExport());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void RetrieveSampleExport_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked);
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

        [Test]
        public void GetSampleResults_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked);
                GrpcClient.SendRequestGetSampleResults(new RequestGetSampleResults());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void GetSampleResults_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked);
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

        [Test]
        public void ImportConfigFiles_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked);
                GrpcClient.SendRequestImportConfig(new RequestImportConfig { FileData = Google.Protobuf.ByteString.CopyFrom(new byte[256]) });
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ImportConfigFiles_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked);
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

        [Test]
        public void Resume_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked, systemStatus: SystemStatus.Paused);
                GrpcClient.SendRequestResume(new RequestResume());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void Resume_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked, systemStatus: SystemStatus.Paused);
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

        [Test]
        public void StartMultipleSamples_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked);
                GrpcClient.SendRequestStartSampleSet(new RequestStartSampleSet());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void StartMultipleSamples_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked);
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

        [Test]
        public void StartSingleSample_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked);
                GrpcClient.SendRequestStartSample(new RequestStartSample());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void StartSingleSample_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked);
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

        [Test]
        public void Stop_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked, systemStatus: SystemStatus.ProcessingSample);
                GrpcClient.SendRequestStop(new RequestStop());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void Stop_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked, systemStatus: SystemStatus.ProcessingSample);
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

        [Test]
        public void Pause_SuccessWhenLocked()
        {
            try
            {
                Init(LockStateEnum.Locked, systemStatus: SystemStatus.ProcessingSample);
                GrpcClient.SendRequestPause(new RequestPause());
                //No RpcException thrown so the lock interceptor did not find an issue with the lock state
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void Pause_FailureWhenUnLocked()
        {
            try
            {
                Init(LockStateEnum.Unlocked, systemStatus: SystemStatus.ProcessingSample);
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