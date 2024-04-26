using GrpcService;
using Moq;
using NUnit.Framework;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutServices.Enums;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf.WellKnownTypes;
using ScoutDataAccessLayer.IDAL;
using ScoutDomains.EnhancedSampleWorkflow;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class ScoutOpcUaGrpcServiceResponseTests : BaseInterceptorTest
    {
        #region AutomationLock

        [Test]
        public void AutomationLock_Success()
        {
            try
            {
                Init(LockStateEnum.Unlocked);
                var response = GrpcClient.SendRequestRequestLock(new RequestRequestLock());
                Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
                Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void AutomationLock_Failure()
        {
            var response = new VcbResultRequestLock();
            try
            {
                // There is no code path in SendRequestAutomationLock that sends a "proper" Failure 
                // response (LockManager has no failing condition for PublishAutomationLock).
                // We will just mock it to throw an exception.

                Init(LockStateEnum.Unlocked);
                MockLockManager.Setup(m => m.PublishAutomationLock(
                                   It.IsAny<LockResult>(), It.IsAny<string>()))
                               .Throws(new Exception("Mock should throw this"));
                response = GrpcClient.SendRequestRequestLock(new RequestRequestLock());
                Assert.Fail($"Exception should have been thrown");
            }
            catch (Exception)
            {
                Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            }
        }

        #endregion

        #region AutomationUnlock

        [Test]
        public void AutomationUnlock_Success()
        {
            try
            {
                Init(LockStateEnum.Locked);
                var response = GrpcClient.SendRequestReleaseLock(new RequestReleaseLock());
                Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
                Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void AutomationUnlock_Failure()
        {
            var response = new VcbResultReleaseLock();
            try
            {
                // There is no code path in SendRequestAutomationUnlock that sends a "proper" Failure 
                // response (LockManager has no failing condition for PublishAutomationLock).
                // We will just mock it to throw an exception.

                Init(LockStateEnum.Locked);
                MockLockManager.Setup(m => m.PublishAutomationLock(
                                   It.IsAny<LockResult>(), It.IsAny<string>()))
                               .Throws(new Exception("Mock should throw this"));
                response = GrpcClient.SendRequestReleaseLock(new RequestReleaseLock());
                Assert.Fail($"Exception should have been thrown");
            }
            catch (Exception)
            {
                Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            }
        }

        #endregion

        #region CreateCellType

        [Test]
        public void CreateCellType_Success()
        {
            Init(LockStateEnum.Locked);
            SetupMockCellTypeManager(true, true);
            var response = GrpcClient.SendRequestCreateCellType(new RequestCreateCellType
            {
                Cell = new GrpcService.CellType()
            });
            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }

        [Test]
        public void CreateCellType_Failure_CreateCellType()
        {
            Init(LockStateEnum.Locked);
            SetupMockCellTypeManager(false, true);
            var response = GrpcClient.SendRequestCreateCellType(new RequestCreateCellType
            {
                Cell = new GrpcService.CellType()
            });
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
            Assert.IsTrue(response.Description.Contains("Error creating CellType"));
        }

        [Test]
        public void CreateCellType_Failure_NullCellType()
        {
            Init(LockStateEnum.Locked);
            SetupMockCellTypeManager(true, true); // should fail before getting to ICellTypeManager checks
            var response = GrpcClient.SendRequestCreateCellType(new RequestCreateCellType());
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
            Assert.IsTrue(response.Description.Contains("No CellType found"));
        }

        [Test]
        public void CreateCellType_Failure_Validation()
        {
            Init(LockStateEnum.Locked);
            SetupMockCellTypeManager(false, false);
            var response = GrpcClient.SendRequestCreateCellType(new RequestCreateCellType()
            {
                Cell = new GrpcService.CellType()
            });
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
            Assert.IsTrue(response.Description.Contains("Invalid CellType"));
        }

        #endregion

        #region DeleteCellType

        [Test]
        public void DeleteCellType_Success()
        {
            Init(LockStateEnum.Locked);
            SetupMockCellTypeManager(true, true, true);
            var response = GrpcClient.SendRequestDeleteCellType(new RequestDeleteCellType
            {
                CellTypeName = "My Cell Type"
            });
            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }
        
        [Test]
        public void DeleteCellType_Failure_NullCellType()
        {
            Init(LockStateEnum.Locked);
            SetupMockCellTypeManager(true, true); // should fail before getting to ICellTypeManager checks
            var response = GrpcClient.SendRequestDeleteCellType(new RequestDeleteCellType());
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.RequiresUserInteraction, response.ErrorLevel);
        }

        [Test]
        public void DeleteCellType_Failure()
        {
            Init(LockStateEnum.Locked);
            SetupMockCellTypeManager(true, true, false);
            var response = GrpcClient.SendRequestDeleteCellType(new RequestDeleteCellType()
            {
                CellTypeName = "My Cell Type"
            });
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        #endregion

        #region CreateQualityControl

        [Test]
        [Ignore("Ignoring for now as it seems to fail finding the HawkeyeCore.dll, although it exists.")]
        public void CreateQualityControl_Success()
        {
            Init(LockStateEnum.Locked);
            SetupMockCellTypeManager(true, true, true, true, true);
            MockCellTypeManager.Setup(m => m.GetCellTypeDomain(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>())).Returns(CreateValidCellTypeDomain());
            var response = GrpcClient.SendRequestCreateQualityControl(new RequestCreateQualityControl
            {
                QualityControl = new GrpcService.QualityControl()
            });
            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }

        [Test]
        public void CreateQualityControl_Failure_CreateQc()
        {
            Init(LockStateEnum.Locked);
            SetupMockCellTypeManager(true, true, true, false, true);
            var response = GrpcClient.SendRequestCreateQualityControl(new RequestCreateQualityControl
            {
                QualityControl = new GrpcService.QualityControl()
            });
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        [Test]
        public void CreateQualityControl_Failure_NullQc()
        {
            Init(LockStateEnum.Locked);
            SetupMockCellTypeManager(true, true); // should fail before getting to ICellTypeManager checks
            var response = GrpcClient.SendRequestCreateQualityControl(new RequestCreateQualityControl());
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.RequiresUserInteraction, response.ErrorLevel);
        }

        [Test]
        [Ignore("Ignoring for now as it seems it cannot find the HawkeyeCore.dll though it exists.")]
        public void CreateQualityControl_Failure_Validation()
        {
            Init(LockStateEnum.Locked);
            SetupMockCellTypeManager(true, true, true, true, false);
            MockCellTypeManager.Setup(m => m.GetCellTypeDomain(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>())).Returns(CreateValidCellTypeDomain());
            var response = GrpcClient.SendRequestCreateQualityControl(new RequestCreateQualityControl()
            {
                QualityControl = new GrpcService.QualityControl()
            });
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.RequiresUserInteraction, response.ErrorLevel);
        }

        [Test]
        public void CreateQualityControl_Failure_NoCellType()
        {
            Init(LockStateEnum.Locked);
            SetupMockCellTypeManager(true, true, true, true, false);
            var response = GrpcClient.SendRequestCreateQualityControl(new RequestCreateQualityControl()
            {
                QualityControl = new GrpcService.QualityControl()
            });
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        #endregion

        #region DeleteSampleResults

        [Test]
        public void DeleteSampleResults_Success()
        {
            Init(LockStateEnum.Locked);
            SetupMockSampleResultsManager();
            var response = GrpcClient.SendRequestDeleteSampleResults(new RequestDeleteSampleResults
            {
                Uuids = {Guid.NewGuid().ToString()}
            });
            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }

        [Test]
        public void DeleteSampleResults_Failure_Null()
        {
            Init(LockStateEnum.Locked);
            SetupMockSampleResultsManager();
            var response = GrpcClient.SendRequestDeleteSampleResults(new RequestDeleteSampleResults());
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.RequiresUserInteraction, response.ErrorLevel);
        }

        [Test]
        public void DeleteSampleResults_Failure()
        {
            Init(LockStateEnum.Locked);
            SetupMockSampleResultsManager(false);
            var response = GrpcClient.SendRequestDeleteSampleResults(new RequestDeleteSampleResults
            {
                Uuids = { Guid.NewGuid().ToString() }
            });
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        #endregion

        #region EjectStageMethods

        [Test]
        public void EjectStageMethods_Success()
        {
            Init(LockStateEnum.Locked);
            MockSampleProcessingService.Setup(m => m.EjectStage(Username,Password, false)).Returns(true);
            var response = GrpcClient.SendRequestEjectStage(new RequestEjectStage());
            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }
        
        [Test]
        public void EjectStageMethods_Failure()
        {
            Init(LockStateEnum.Locked);
            MockSampleProcessingService.Setup(m => m.EjectStage(Username, Password, false)).Returns(false);
            var response = GrpcClient.SendRequestEjectStage(new RequestEjectStage());
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        #endregion

        #region ExportConfigFiles

        [Test]
        [Ignore("Ignoring due to backend hanging on initialization on jenkins")]
        public void ExportConfigFiles_Success()
        {
            Init(LockStateEnum.Locked);
            MockConfigurationManager.Setup(m => m.ExportConfig(Username, Password, It.IsAny<RequestExportConfig>()))
                                    .Returns<byte[]>(null);

            var req = new RequestExportConfig();            
            var response = GrpcClient.SendRequestExportConfig(req);

            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }

        [Test]
        public void ExportConfigFiles_Failure_Null()
        {
            Init(LockStateEnum.Locked);
            MockConfigurationManager.Setup(m => m.ExportConfig(Username, Password, It.IsAny<RequestExportConfig>()))
                                    .Returns<byte[]>(null);
            var response = GrpcClient.SendRequestExportConfig(new RequestExportConfig());
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        [Test]
        public void ExportConfigFiles_Failure()
        {
            Init(LockStateEnum.Locked);
            MockConfigurationManager.Setup(m => m.ExportConfig(Username, Password, It.IsAny<RequestExportConfig>()))
                                    .Returns<string>(null);
            var response = GrpcClient.SendRequestExportConfig(new RequestExportConfig());
            //{
            //   FilenameToSaveConfigTo = @"\Instrument\Export"
            //});
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        #endregion

        #region RetrieveSampleExport

        [Test]
        public void RetrieveSampleExport_Success()
        {
            Init(LockStateEnum.Locked);

            MockSampleResultsManager.Setup(m => m.StartExport(Username, Password, It.IsAny<RequestStartExport>())).Returns("");

            var response = GrpcClient.SendStartExport(new RequestStartExport
            {
                SampleListUuid = {Guid.NewGuid().ToString()}
            });
            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }

        //[Test]
        //public void RetrieveSampleExport_Failure_Null()
        //{
        //    Init(LockStateEnum.Locked);

        //    MockSampleResultsManager.Setup(m => m.RetrieveSampleExport(Username, Password, It.IsAny<RequestRetrieveSampleExport>())).Returns<byte[]>(null);

        //    var response = GrpcClient.SendRequestRetrieveSampleExport(new RequestRetrieveSampleExport());
        //    Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
        //    Assert.AreEqual(ErrorLevelEnum.RequiresUserInteraction, response.ErrorLevel);
        //}

        //[Test]
        //public void RetrieveSampleExport_Failure()
        //{
        //    Init(LockStateEnum.Locked);

        //    MockSampleResultsManager.Setup(m => m.RetrieveSampleExport(Username, Password, It.IsAny<RequestRetrieveSampleExport>())).Returns<byte[]>(null);

        //    var response = GrpcClient.SendRequestRetrieveSampleExport(new RequestRetrieveSampleExport
        //    {
        //        SampleUuid = { Guid.NewGuid().ToString() }
        //    });
        //    Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
        //    Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        //}

        #endregion

        #region GetSampleResults

        [Test]
        public void GetSampleResults_Success()
        {
            Init(LockStateEnum.Locked);
            MockSampleResultsManager.Setup(m => m.GetResultsValidation(It.IsAny<RequestGetSampleResults>())).Returns(true);
            MockSampleResultsManager.Setup(m => m.GetSampleResults(Username, Password, It.IsAny<RequestGetSampleResults>())).Returns(new List<SampleResult>());
            string errMsg;
            MockSampleResultsManager.Setup(m => m.CheckUserPrivilegedData(Username, String.Empty, out errMsg)).Returns(true);
            var response = GrpcClient.SendRequestGetSampleResults(new RequestGetSampleResults() {CellTypeQualityControlName = string.Empty, Username = string.Empty, FilterType = FilterOnEnum.FilterSample, FromDate = Timestamp.FromDateTime(DateTime.UtcNow), ToDate = Timestamp.FromDateTime(DateTime.UtcNow), SearchNameString = string.Empty, SearchTagString = string.Empty});

            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }

        [Test]
        public void GetSampleResults_UserPriviliges_Failure()
        {
            Init(LockStateEnum.Locked);
            MockSampleResultsManager.Setup(m => m.GetSampleResults(Username, Password, It.IsAny<RequestGetSampleResults>())).Returns(new List<SampleResult>());
            string errMsg;
            MockSampleResultsManager.Setup(m => m.CheckUserPrivilegedData(Username, "Admin User", out errMsg)).Returns(false);
            var response = GrpcClient.SendRequestGetSampleResults(new RequestGetSampleResults() { CellTypeQualityControlName = string.Empty, Username = string.Empty, FilterType = FilterOnEnum.FilterSample, FromDate = Timestamp.FromDateTime(DateTime.UtcNow), ToDate = Timestamp.FromDateTime(DateTime.UtcNow), SearchNameString = string.Empty, SearchTagString = string.Empty });

            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        [Test]
        public void GetSampleResults_Failure_GetSampleResults()
        {
            Init(LockStateEnum.Locked);
            MockSampleResultsManager.Setup(m => m.GetSampleResults(Username, Password, It.IsAny<RequestGetSampleResults>())).Returns<SampleResult>(null);

            var response = GrpcClient.SendRequestGetSampleResults(new RequestGetSampleResults() { CellTypeQualityControlName = string.Empty, Username = string.Empty, FilterType = FilterOnEnum.FilterSample, FromDate = Timestamp.FromDateTime(DateTime.UtcNow), ToDate = Timestamp.FromDateTime(DateTime.UtcNow), SearchNameString = string.Empty, SearchTagString = string.Empty });

            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        [Test]
        public void GetSampleResults_Success_NullGetSampleResultList()
        {
            Init(LockStateEnum.Locked);
            MockSampleResultsManager.Setup(m => m.GetResultsValidation(It.IsAny<RequestGetSampleResults>())).Returns(true);
            MockSampleResultsManager.Setup(m => m.GetSampleResults(Username, Password, It.IsAny<RequestGetSampleResults>())).Returns(new List<SampleResult>());
            string errMsg;
            MockSampleResultsManager.Setup(m => m.CheckUserPrivilegedData(Username, String.Empty, out errMsg)).Returns(true);
            var response = GrpcClient.SendRequestGetSampleResults(new RequestGetSampleResults() { CellTypeQualityControlName = string.Empty, Username = string.Empty, FilterType = FilterOnEnum.FilterSample, FromDate = Timestamp.FromDateTime(DateTime.UtcNow), ToDate = Timestamp.FromDateTime(DateTime.UtcNow), SearchNameString = string.Empty, SearchTagString = string.Empty });

            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }

        #endregion

        #region ImportConfigFiles

        [Test]
        public void ImportConfigFiles_Success()
        {
            Init(LockStateEnum.Locked);
            MockConfigurationManager.Setup(m => m.ImportConfig(Username, Password, It.IsAny<RequestImportConfig>()))
                                    .Returns(HawkeyeError.eSuccess);

            var guid = Guid.NewGuid();
            var file = @"\Instrument\Export\" + guid + ".cfg";
            File.WriteAllText(file, "test data");

            byte[] data = new byte[256];

            var response = GrpcClient.SendRequestImportConfig(new RequestImportConfig
            {
                FileData = Google.Protobuf.ByteString.CopyFrom(data)
            });

            if (File.Exists(file))
                File.Delete(file);

            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }

        [Test]
        public void ImportConfigFiles_Failure_Null()
        {
            Init(LockStateEnum.Locked);
            MockConfigurationManager.Setup(m => m.ImportConfig(Username, Password, It.IsAny<RequestImportConfig>()))
                                    .Returns(HawkeyeError.eNotPermittedAtThisTime);
            var response = GrpcClient.SendRequestImportConfig(new RequestImportConfig());
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.RequiresUserInteraction, response.ErrorLevel);
        }

        [Test]
        public void ImportConfigFiles_Failure()
        {
            Init(LockStateEnum.Locked);
            MockConfigurationManager.Setup(m => m.ImportConfig(Username, Password, It.IsAny<RequestImportConfig>()))
                                    .Returns(HawkeyeError.eDatabaseError);

            byte[] data = new byte[256];
            var response = GrpcClient.SendRequestImportConfig(new RequestImportConfig
            {
                FileData = Google.Protobuf.ByteString.CopyFrom(data)
            });

            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        #endregion

        #region Resume

        [Test]
        public void Resume_Success()
        {
            Init(LockStateEnum.Locked, systemStatus: SystemStatus.Paused);
            MockSampleProcessingService.Setup(m => m.ResumeProcessing(Username, Password)).Returns(true);
            var response = GrpcClient.SendRequestResume(new RequestResume());
            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }
        
        [Test]
        public void Resume_Failure()
        {
            Init(LockStateEnum.Locked, systemStatus: SystemStatus.Paused);
            MockSampleProcessingService.Setup(m => m.ResumeProcessing(Username, Password)).Returns(false);
            var response = GrpcClient.SendRequestResume(new RequestResume());
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        #endregion

        #region StartMultipleSamples

        [Test]
        public void StartMultipleSamples_Success()
        {
            Init(LockStateEnum.Locked);
            MockSampleProcessingService.Setup(m => m.CanProcessSamples(Username,
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<bool>())).Returns(SampleProcessingValidationResult.Valid);
            MockSampleProcessingService.Setup(m => m.ProcessSamples(
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<string>(), It.IsAny<SampleSetTemplateDomain>(), It.IsAny<IDataAccess>())).Returns(true);
            var response = GrpcClient.SendRequestStartSampleSet(new RequestStartSampleSet { SampleSetConfig = new SampleSetConfig() });
            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }

        [Test]
        public void StartMultipleSamples_Failure_Null()
        {
            Init(LockStateEnum.Locked);
            MockSampleProcessingService.Setup(m => m.CanProcessSamples(Username,
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<bool>())).Returns(SampleProcessingValidationResult.Valid);
            MockSampleProcessingService.Setup(m => m.ProcessSamples(
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<string>(), It.IsAny<SampleSetTemplateDomain>(), It.IsAny<IDataAccess>())).Returns(true);
            var response = GrpcClient.SendRequestStartSampleSet(new RequestStartSampleSet());
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.RequiresUserInteraction, response.ErrorLevel);
        }

        [Test]
        public void StartMultipleSamples_CanProcess_Failure()
        {
            Init(LockStateEnum.Locked);
            MockSampleProcessingService.Setup(m => m.CanProcessSamples(Username,
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<bool>())).Returns(SampleProcessingValidationResult.InvalidSamplePosition);
            MockSampleProcessingService.Setup(m => m.ProcessSamples(
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<string>(), It.IsAny<SampleSetTemplateDomain>(), It.IsAny<IDataAccess>())).Returns(true);
            var response = GrpcClient.SendRequestStartSampleSet(new RequestStartSampleSet { SampleSetConfig = new SampleSetConfig() });
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        [Test]
        public void StartMultipleSamples_Process_Failure()
        {
            Init(LockStateEnum.Locked);
            MockSampleProcessingService.Setup(m => m.CanProcessSamples(Username,
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<bool>())).Returns(SampleProcessingValidationResult.Valid);
            MockSampleProcessingService.Setup(m => m.ProcessSamples(
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<string>(), It.IsAny<SampleSetTemplateDomain>(), It.IsAny<IDataAccess>())).Returns(false);
            var response = GrpcClient.SendRequestStartSampleSet(new RequestStartSampleSet {SampleSetConfig = new SampleSetConfig()});
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        #endregion

        #region StartSingleSample

        [Test]
        public void StartSingleSample_Success()
        {
            Init(LockStateEnum.Locked);
            MockSampleProcessingService.Setup(m => m.CanProcessSamples(Username,
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<bool>())).Returns(SampleProcessingValidationResult.Valid);
            MockSampleProcessingService.Setup(m => m.ProcessSamples(
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<string>(), It.IsAny<SampleSetTemplateDomain>(), It.IsAny<IDataAccess>())).Returns(true);
            var response = GrpcClient.SendRequestStartSample(new RequestStartSample {SampleConfig = new SampleConfig()});
            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }

        [Test]
        public void StartSingleSample_Failure_Null()
        {
            Init(LockStateEnum.Locked);
            MockSampleProcessingService.Setup(m => m.CanProcessSamples(Username,
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<bool>())).Returns(SampleProcessingValidationResult.Valid);
            MockSampleProcessingService.Setup(m => m.ProcessSamples(
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<string>(), It.IsAny<SampleSetTemplateDomain>(), It.IsAny<IDataAccess>())).Returns(true);
            var response = GrpcClient.SendRequestStartSample(new RequestStartSample());
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.RequiresUserInteraction, response.ErrorLevel);
        }

        [Test]
        public void StartSingleSample_CanProcess_Failure()
        {
            Init(LockStateEnum.Locked);
            MockSampleProcessingService.Setup(m => m.CanProcessSamples(Username,
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<bool>())).Returns(SampleProcessingValidationResult.InvalidSamplePosition);
            MockSampleProcessingService.Setup(m => m.ProcessSamples(
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<string>(), It.IsAny<SampleSetTemplateDomain>(), It.IsAny<IDataAccess>())).Returns(true);
            var response = GrpcClient.SendRequestStartSample(new RequestStartSample {SampleConfig = new SampleConfig()});
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        [Test]
        public void StartSingleSample_Process_Failure()
        {
            Init(LockStateEnum.Locked);
            MockSampleProcessingService.Setup(m => m.CanProcessSamples(Username,
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<bool>())).Returns(SampleProcessingValidationResult.Valid);
            MockSampleProcessingService.Setup(m => m.ProcessSamples(
                It.IsAny<IList<SampleSetDomain>>(), It.IsAny<string>(), It.IsAny<SampleSetTemplateDomain>(), It.IsAny<IDataAccess>())).Returns(false);
            var response = GrpcClient.SendRequestStartSample(new RequestStartSample {SampleConfig = new SampleConfig()});
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        #endregion

        #region Stop

        [Test]
        public void Stop_Success()
        {
            Init(LockStateEnum.Locked, systemStatus: SystemStatus.ProcessingSample);
            MockSampleProcessingService.Setup(m => m.StopProcessing(Username, Password)).Returns(true);
            var response = GrpcClient.SendRequestStop(new RequestStop());
            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }

        [Test]
        public void Stop_Failure()
        {
            Init(LockStateEnum.Locked, systemStatus: SystemStatus.ProcessingSample);
            MockSampleProcessingService.Setup(m => m.StopProcessing(Username, Password)).Returns(false);
            var response = GrpcClient.SendRequestStop(new RequestStop());
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        #endregion

        #region Pause

        [Test]
        public void Pause_Success()
        {
            Init(LockStateEnum.Locked, systemStatus: SystemStatus.ProcessingSample);
            MockSampleProcessingService.Setup(m => m.PauseProcessing(Username, Password)).Returns(true);
            var response = GrpcClient.SendRequestPause(new RequestPause());
            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }

        [Test]
        public void Pause_Failure()
        {
            Init(LockStateEnum.Locked, systemStatus: SystemStatus.ProcessingSample);
            MockSampleProcessingService.Setup(m => m.PauseProcessing(Username, Password)).Returns(false);
            var response = GrpcClient.SendRequestPause(new RequestPause());
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        #endregion

        #region ValidateUser

        [Test]
        [Ignore("Ignoring for now as it seems it cannot find the HawkeyeCore.dll though it exists.")]
        public void ValidateUser_Success()
        {
            Init(LockStateEnum.Locked);
            MockSecurityService.Setup(m => m.LoginRemoteUser(
                It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            var response = GrpcClient.LoginRemoteUser(new RequestLoginUser {Username = Username, Password = Password});
            Assert.AreEqual(MethodResultEnum.Success, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.NoError, response.ErrorLevel);
        }

        [Test]
        public void ValidateUser_Failure()
        {
            Init(LockStateEnum.Locked);
            MockSecurityService.Setup(m => m.LoginRemoteUser(
                It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            var response = GrpcClient.LoginRemoteUser(new RequestLoginUser { Username = Username, Password = Password });
            Assert.AreEqual(MethodResultEnum.Failure, response.MethodResult);
            Assert.AreEqual(ErrorLevelEnum.Error, response.ErrorLevel);
        }

        #endregion

        #region Helper Methods

        private void SetupMockSampleResultsManager(bool delete_returns = true)
        {
            MockSampleResultsManager.Setup(m => m.DeleteSampleResults(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<uuidDLL>>(), It.IsAny<bool>())).Returns(delete_returns);
        }

        private void SetupMockCellTypeManager(bool cellType_create_returns, bool cellType_validation_returns,
            bool cellType_delete_returns = true, bool qc_create_returns = true, bool qc_validation_returns = true)
        {
            MockCellTypeManager.Reset();
            MockCellTypeManager.Setup(m => m.CreateCellType(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CellTypeDomain>(), "", It.IsAny<bool>())).Returns(cellType_create_returns);
            MockCellTypeManager.Setup(m => m.SaveCellTypeValidation(
                It.IsAny<CellTypeDomain>(), It.IsAny<bool>())).Returns(cellType_validation_returns);
            var failureReason = "";
            MockCellTypeManager.Setup(m => m.SaveCellTypeValidation(
                It.IsAny<CellTypeDomain>(), It.IsAny<bool>(), out failureReason)).Returns(cellType_validation_returns);
            MockCellTypeManager.Setup(m => m.CanAddAdjustmentFactor(It.IsAny<string>(), It.IsAny<CellTypeDomain>(),out failureReason))
                .Returns(true);
            MockCellTypeManager.Setup(m => m.DeleteCellType(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>())).Returns(cellType_delete_returns);
            
            MockCellTypeManager.Setup(m => m.CreateQualityControl(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<QualityControlDomain>(), It.IsAny<bool>())).Returns(qc_create_returns);
            MockCellTypeManager.Setup(m => m.QualityControlValidation(
                It.IsAny<QualityControlDomain>(), It.IsAny<bool>())).Returns(qc_validation_returns); 
            var qcFailureReason = "";
            MockCellTypeManager.Setup(m => m.QualityControlValidation(
                 It.IsAny<QualityControlDomain>(), It.IsAny<bool>(), out qcFailureReason, "", "")).Returns(qc_validation_returns);
        }

        private GrpcService.CellType CreateValidCellTypeObjectType()
        {
            return Mapper.Map<GrpcService.CellType>(CreateValidCellTypeDomain());
        }

        private CellTypeDomain CreateValidCellTypeDomain()
        {
            var ct = new CellTypeDomain();
            ct.CellTypeName = "My cell type";
            ct.QCCellTypeForDisplay = "My qc cell for display";
            ct.AnalysisDomain = new AnalysisDomain()
            {
                AnalysisIndex = 0,
                AnalysisParameter = new List<AnalysisParameterDomain>()
                {
                    new AnalysisParameterDomain()
                    {
                        AboveThreshold = false,
                        Characteristic = new Characteristic_t(),
                        Label = "new",
                        ThresholdValue = 1.5f
                    },
                    new AnalysisParameterDomain()
                    {
                        AboveThreshold = false,
                        Characteristic = new Characteristic_t(),
                        Label = "new",
                        ThresholdValue = 1.5f
                    }
                },
                MixingCycle = 1,
            };
            ct.AspirationCycles = 2;
            ct.CalculationAdjustmentFactor = 0;
            ct.CellSharpness = 17;
            ct.CellTypeIndex = 123456;
            ct.DeclusterDegree = eCellDeclusterSetting.eDCLow;
            ct.Images = 50;
            ct.MaximumDiameter = 12.0;
            ct.MinimumDiameter = 5.0;
            ct.MinimumCircularity = 0.5;

            return ct;
        }

        #endregion
    }
}