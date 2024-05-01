using AutoMapper;
using Grpc.Core;
using GrpcServer.Enums;
using GrpcServer.GrpcInterceptor.Attributes;
using GrpcService;
using Ninject.Extensions.Logging;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Interfaces;
using ScoutServices.Enums;
using ScoutServices.Interfaces;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using HawkeyeCoreAPI.Facade;
using HawkeyeCoreAPI.Interfaces;
using ScoutModels.Settings;
using ScoutUtilities.Events;
using ScoutUtilities.UIConfiguration;
using CellType = GrpcService.CellType;
using DateTime = System.DateTime;
using Notification = GrpcService.Notification;
using ShutdownOrRebootEnum = GrpcService.ShutdownOrRebootEnum;

namespace GrpcServer
{
    public class ScoutOpcUaGrpcService : GrpcServices.GrpcServicesBase
    {
        private readonly ILogger _logger;
        private readonly GrpcClientManager _clientManager;
        private readonly ILockManager _lockManager;
        private readonly ISampleResultsManager _sampleResultsManager;
        private readonly ISampleProcessingService _sampleProcessingService;
        private readonly IMapper _mapper;
        private readonly ICellTypeManager _cellTypeManager;
        private readonly IConfigurationManager _configurationManager;
        private readonly IScoutModelsFactory _scoutModelsFactory;
        private readonly ISecurityService _securityService;
        private readonly IInstrumentStatusService _instrumentStatusService;
        private readonly IAuditLog _auditLog;
        private readonly IAutomationSettingsService _automationSettingsService;
        private readonly IMaintenanceService _maintenanceService;

        private bool _carouselIsInstalled;

        public ScoutOpcUaGrpcService(GrpcClientManager clientManager, ILogger logger, ILockManager lockManager,
            ISampleResultsManager sampleResultsManager, ISampleProcessingService sampleProcessingService,
            IMapper mapper, ICellTypeManager cellTypeManager, IConfigurationManager configurationManager,
            IScoutModelsFactory scoutModelsFactory, ISecurityService securityService,
            IInstrumentStatusService instrumentStatusService, IAuditLog auditLog, IAutomationSettingsService automationSettingsService,
            IMaintenanceService maintenanceService)
        {
            _clientManager = clientManager;
            _logger = logger;
            _lockManager = lockManager;
            _sampleResultsManager = sampleResultsManager;
            _sampleProcessingService = sampleProcessingService;
            _configurationManager = configurationManager;
            _scoutModelsFactory = scoutModelsFactory;
            _securityService = securityService;
            _mapper = mapper;
            _cellTypeManager = cellTypeManager;
            _instrumentStatusService = instrumentStatusService;
            _auditLog = auditLog;
            _automationSettingsService = automationSettingsService;
            _maintenanceService = maintenanceService;

            _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe(HandleSystemStatusCallback);
        }

	#region OPC Methods

        [RequiresAutomationLock(LockRequirements.RequiresUnlocked)]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResultRequestLock> RequestLock(RequestRequestLock request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(RequestLock)}");

            string msg;

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                VcbResultRequestLock res = new VcbResultRequestLock()
                {
                    ErrorLevel = ErrorLevelEnum.Error,
                    LockState = LockStateEnum.Unlocked,
                    MethodResult = MethodResultEnum.Failure,
                    Description = "No user credentials"
                };

                msg = " Lock Failed: " + res.Description;
                _auditLog.WriteToAuditLogAPI(username, audit_event_type.evt_automation, msg);
				ApiHawkeyeMsgHelper.PublishHubMessage(ScoutUtilities.Misc.AuditEventString(audit_event_type.evt_automation) + msg, MessageType.Warning);

				return Task.FromResult(res);
            }

			// Log and perform action.
			_logger.Info($"{nameof(RequestLock)}: user '{username}'.");
            _lockManager.PublishAutomationLock(LockResult.Locked, username);

            msg = " Locked by " + username;
            _auditLog.WriteToAuditLogAPI(username, audit_event_type.evt_automationlocked, msg);
            ApiHawkeyeMsgHelper.PublishHubMessage(ScoutUtilities.Misc.AuditEventString(audit_event_type.evt_automationlocked) + msg, MessageType.Normal);

			return MakeSuccessRequestLock(LockResult.Locked);
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[]
        { 
            // don't allow during Pausing, or Paused states
            SystemStatus.Idle,
            SystemStatus.ProcessingSample,
            SystemStatus.Stopping,
            SystemStatus.Stopped,
            SystemStatus.Faulted,
            SystemStatus.SearchingTube
        })]
        public override Task<VcbResultReleaseLock> ReleaseLock(RequestReleaseLock request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(ReleaseLock)}");

            string msg;
            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                VcbResultReleaseLock res = new VcbResultReleaseLock()
                {
                    ErrorLevel = ErrorLevelEnum.Error,
                    LockState = LockStateEnum.Unlocked,
                    MethodResult = MethodResultEnum.Failure,
                    Description = "No user credentials"
                };

                msg = " Unlock Failed" + res.Description;
                _auditLog.WriteToAuditLogAPI(username, audit_event_type.evt_automation, msg );
                ApiHawkeyeMsgHelper.PublishHubMessage(ScoutUtilities.Misc.AuditEventString(audit_event_type.evt_automation) + msg, MessageType.Warning);

				return Task.FromResult(res);
            }

            _logger.Info($"{nameof(ReleaseLock)}: from user '{username}'.");

            _lockManager.PublishAutomationLock(LockResult.Unlocked, username);

			msg = " Unlocked by " + username;
            _auditLog.WriteToAuditLogAPI(username, audit_event_type.evt_automationunlocked, msg);
            ApiHawkeyeMsgHelper.PublishHubMessage(ScoutUtilities.Misc.AuditEventString(audit_event_type.evt_automationunlocked) + msg, MessageType.Normal);

			return MakeSuccessReleaseLock(LockResult.Unlocked);
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        [Permission(new[] { UserPermissionLevel.eElevated, UserPermissionLevel.eAdministrator })]
        public override Task<VcbResultCreateCellType> CreateCellType(RequestCreateCellType request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(CreateCellType)}");

            if (request?.Cell == null)
            {
	            _logger.Error($"{nameof(CreateCellType)}: no CellType found");
                return MakeFailureCreateCellType("No CellType found");
            }

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                return MakeFailureCreateCellType("No user credentials");
            }

            var cellDomain = _mapper.Map<CellTypeDomain>(request.Cell);

			if (cellDomain.AnalysisDomain.AnalysisParameter.Count >= 1)
			{
				cellDomain.AnalysisDomain.AnalysisParameter[0].Label = ApplicationConstants.CellSpotAreaText;
				Characteristic_t temp = cellDomain.AnalysisDomain.AnalysisParameter[0].Characteristic;
				temp.key = ApplicationConstants.CellSpotAreaKey;
				cellDomain.AnalysisDomain.AnalysisParameter[0].Characteristic = temp;
				cellDomain.AnalysisDomain.AnalysisParameter[0].AboveThreshold = true;
			}
			if (cellDomain.AnalysisDomain.AnalysisParameter.Count == 2)
			{
				cellDomain.AnalysisDomain.AnalysisParameter[1].Label = ApplicationConstants.AvgSpotBrightnessText;
				Characteristic_t temp = cellDomain.AnalysisDomain.AnalysisParameter[1].Characteristic;
				temp.key = ApplicationConstants.AvgSpotBrightnessKey;
				cellDomain.AnalysisDomain.AnalysisParameter[1].Characteristic = temp;
				cellDomain.AnalysisDomain.AnalysisParameter[1].AboveThreshold = true;
			}

			// Need to add the AnalysisDomain Label to the new cell type (the Label is the
			// same for all cell types AnalysisDomains). When creating a new cell type via the UI, the cell type is
			// cloned from the original and this information is copied as part of the clone, but we can't clone here
			// because we don't have the source cell type for the clone
			var allCellTypes = CellTypeFacade.Instance.GetAllCellTypes_BECall();
            if (allCellTypes != null && allCellTypes.Count > 0)
                cellDomain.AnalysisDomain.Label = allCellTypes[0].AnalysisDomain.Label;

            var retiredName = string.Empty;

            // Check for a pre-existing cell type with the same name.  This is a sign that we are EDITING.
            // If we are EDITING, then we must retire the existing definition by renaming it out of the way.
            if (allCellTypes.Any(x => x.CellTypeName.Equals(cellDomain.CellTypeName)))
            {
                retiredName = cellDomain.CellTypeName  + " (" +DateTime.Now.ToString("G", System.Globalization.CultureInfo.CurrentCulture) + ")";
            }

            if (!_cellTypeManager.SaveCellTypeValidation(cellDomain, false, out var invalidParam))
            {
	            _logger.Error(string.Format("CreateCellType: invalid CellType: {0}", invalidParam));
	            return MakeFailureCreateCellType(string.Format("Invalid CellType: {0}", invalidParam));          
            }
            else if (!_cellTypeManager.CanAddAdjustmentFactor(username, cellDomain, out var invalidPermissionLevel))
            {
                _logger.Error(string.Format("CreateCellType: error creating CellType {0}", invalidPermissionLevel));
                return MakeFailureCreateCellType(string.Format("Error creating CellType: {0}",invalidPermissionLevel));
            }
            else if (!_cellTypeManager.CreateCellType(username, password, cellDomain, retiredName, false))
            {
	            _logger.Error(string.Format("CreateCellType: error creating CellType {0}", invalidParam));
                return MakeFailureCreateCellType(string.Format("Error creating CellType"));
            }

            // Notify the Console user the Cell Types have changed
            var tmpEvent = new ScoutUtilities.Events.Notification(ScoutUtilities.Events.MessageToken.CellTypesUpdated);
            ScoutUtilities.Events.MessageBus.Default.Publish(tmpEvent);

            return MakeSuccessCreateCellType();
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        [Permission(new[] { UserPermissionLevel.eElevated, UserPermissionLevel.eAdministrator })]
        public override Task<VcbResultDeleteCellType> DeleteCellType(RequestDeleteCellType request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(DeleteCellType)}");

            if (string.IsNullOrEmpty(request?.CellTypeName))
            {
	            _logger.Error($"{nameof(DeleteCellType)}: no CellType name specified");
				return CreateInvalidParametersDeleteCellTypeResponse("No CellType name specified");
            }

			_logger.Info($"{nameof(DeleteCellType)}: CellType name='{request.CellTypeName}'");

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
	            return MakeFailureDeleteCellType("No user credentials");
            }

            var cellTypeName = request.CellTypeName;
            if (!_cellTypeManager.DeleteCellType(username, password, cellTypeName, false))
            {
	            _logger.Error("DeleteCellType: error deleting CellType");
	            return MakeFailureDeleteCellType("Error deleting CellType");
            }

            // Notify the Console user the Cell Types have changed
            var tmpEvent = new ScoutUtilities.Events.Notification(ScoutUtilities.Events.MessageToken.CellTypesUpdated);
            ScoutUtilities.Events.MessageBus.Default.Publish(tmpEvent);

            return MakeSuccessDeleteCellType();
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        [Permission(new[] { UserPermissionLevel.eElevated, UserPermissionLevel.eAdministrator })]
        public override Task<VcbResult> CreateQualityControl(RequestCreateQualityControl request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(CreateQualityControl)}");

            if (request?.QualityControl == null)
            {
                _logger.Error($"{nameof(CreateQualityControl)}: no QC found");
                return CreateInvalidParametersCreateQualityControlResponse("No QC found");
            }

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                return MakeFailureCreateQualityControl("No user credentials");
            }

            var qualityControl = _mapper.Map<QualityControlDomain>(request.QualityControl);

            if(string.IsNullOrEmpty(qualityControl.CellTypeName))
            {
                _logger.Error("CreateQualityControl : Cell Type Name must not be empty.");
                return MakeFailure("CreateQualityControl: Cell Type Name must not be empty.");
            }

            var cellType = _cellTypeManager.GetCellTypeDomain(username, password, qualityControl.CellTypeName);
            if (cellType != null)
            {
                qualityControl.CellTypeIndex = cellType.CellTypeIndex;
            }

            if (cellType == null)
            {
                _logger.Error("CreateQualityControl: Cell Type Name was not found. Ensure you specified an existing name (case-sensitive).");
                return MakeFailure("Cell Type Name was not found. Ensure you specified an existing name (case-sensitive).");
            }
            else if (!_cellTypeManager.QualityControlValidation(qualityControl, false, out var failureReason, username, password))
            {
                _logger.Error(string.Format("CreateQualityControl: {0}", failureReason));
                return CreateInvalidParametersCreateQualityControlResponse(string.Format("Failed to validate QC: {0}", failureReason));
            }
            else if (!_cellTypeManager.CreateQualityControl(username, password, qualityControl, false))
            {
                _logger.Error("CreateQualityControl: error creating QC");
                return MakeFailureCreateQualityControl("Error creating QC");
            }

            // Notify the Console user the QCs have changed
            var tmpEvent = new ScoutUtilities.Events.Notification(ScoutUtilities.Events.MessageToken.QualityControlsUpdated);
            ScoutUtilities.Events.MessageBus.Default.Publish(tmpEvent);

            return MakeSuccess();
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        [Permission(new[] { UserPermissionLevel.eAdministrator })]
        public override Task<VcbResult> DeleteSampleResults(RequestDeleteSampleResults request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(DeleteSampleResults)}");

            if (request?.Uuids == null || request.Uuids.Count < 1)
                return CreateInvalidParametersResponse($"{nameof(DeleteSampleResults)} request.Uuids is null/Empty.");

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                return MakeFailure("No user credentials");
            }

            var uuids = _mapper.Map<List<uuidDLL>>(request.Uuids);
            _logger.Info($"{nameof(DeleteSampleResults)}: user '{username}', " +
                         $"UUIDs='{string.Join(", ", uuids)}';" +
                         $" retainResults='{request.RetainResultsAndFirstImage}'");

            if (!_sampleResultsManager.DeleteSampleResults(username, password, uuids, request.RetainResultsAndFirstImage))
            {
                return MakeFailure();
            }

            return MakeSuccess();
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResultEjectStage> EjectStage(RequestEjectStage request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(EjectStage)}");

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                return MakeFailureEjectStage("No user credentials");
            }

            // Log and perform action.
            var result = _sampleProcessingService.EjectStage(username, password, false); //show error dialog = false
            return result == false ?
                MakeFailureEjectStage("Error ejecting stage") :
                MakeSuccessEjectStage();
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        [Permission(new[] { UserPermissionLevel.eService, UserPermissionLevel.eAdministrator })]
        public override Task<VcbResultExportConfig> ExportConfig(RequestExportConfig request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(ExportConfig)}");

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
	            return MakeFailureExportConfig("No user credentials");
            }

            var exportBytes = _configurationManager.ExportConfig(username, password, request);

            if (exportBytes == null)
            {
                return MakeFailureExportConfig();
            }
            else
            {
                ApiHawkeyeMsgHelper.PublishHubMessage(LanguageResourceHelper.Get("LID_Icon_ExportInstrumentStatusSuccessful"), MessageType.Normal);
                return MakeSuccessExportConfig(exportBytes);
            }
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResultStartExport> StartExport(RequestStartExport request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(StartExport)}");

            if (request?.SampleListUuid == null || !request.SampleListUuid.Any())
                return CreateInvalidParametersStartExportResponse($"{nameof(StartExport)} request or request.SampleUuid is null/empty.");

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                return MakeFailureStartExport();
            }

            _logger.Info($"{nameof(StartExport)} :: Request :: UUIDs='{string.Join(", ", request.SampleListUuid)}'");
            var bulkDataId = _sampleResultsManager.StartExport(username, password, request);
            if(string.IsNullOrEmpty(bulkDataId))
            {
                MakeFailureStartExport();
            }
            _logger.Debug("Bulk Data ID: " + bulkDataId);
            return MakeSuccessStartExport(bulkDataId);
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResultStartExport> StartLogDataExport(RequestStartLogDataExport request, ServerCallContext context)
        {
	        _logger.Info($"Received request: {nameof(StartLogDataExport)}");

	        if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
	        {
		        return MakeFailureStartExport();
	        }

	        var bulkDataId = _sampleResultsManager.StartLogDataExport(request);
			if (string.IsNullOrEmpty(bulkDataId))
	        {
		        MakeFailureStartExport();
	        }
	        _logger.Info("Bulk Data ID: " + bulkDataId);
	        return MakeSuccessStartExport(bulkDataId);
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResultRetrieveBulkDataBlock> RetrieveBulkDataBlock(RequestRetrieveBulkDataBlock request, ServerCallContext context)
        {
	        // Keep for debugging: _logger.Info($"Received request: {nameof(RetrieveBulkDataBlock)} :: Request :: block idx='{request.BlockIndex}'");

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                return MakeFailureRetrieveBulkData(string.Format("No user credentials", "RetrieveBulkDataBlock()"));
            }

            byte[] data;
            var res = _sampleResultsManager.GetBulkDataBlock(request.BlockIndex, request.BulkDataId, out data);
            if ((res == RetrieveBulkBlockStatusEnum.RbsDone) ||
                (res == RetrieveBulkBlockStatusEnum.RbsSuccess))
            {
                return MakeSuccessRetrieveExportBlock(request.BlockIndex, res, data);
            }
            return MakeFailureRetrieveBulkData(string.Format("Failed to get block", "RetrieveBulkDataBlock()"));
        }


        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResultGetSampleResults> GetSampleResults(RequestGetSampleResults request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(GetSampleResults)}");

            if (request == null) // todo: do we want to do some base-level validation on the parameters?
                return CreateInvalidParametersGetSampleResultsResponse($"{nameof(GetSampleResults)} request is null.");

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
	            return MakeFailureGetSampleResults("No user credentials");
            }

            // Perform action.
			if (!_sampleResultsManager.CheckUserPrivilegedData(username, request.Username, out var errMsg))
                return MakeFailureGetSampleResults(errMsg);

            if (!_sampleResultsManager.GetResultsValidation(request))
            {
                return MakeFailureGetSampleResults("Error validating results");
            }

            var result = _sampleResultsManager.GetSampleResults(username, password, request);
            return result == null ?
                MakeFailureGetSampleResults("Error getting sample results") :
                MakeSuccessGetSampleResults(result);
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        [Permission(new[] { UserPermissionLevel.eService, UserPermissionLevel.eAdministrator })]
        public override Task<VcbResult> ImportConfig(RequestImportConfig request, ServerCallContext context)
        {
            // Log initial interaction.
            _logger.Info($"Received request: {nameof(ImportConfig)}");

            // Validate parameters.
            if ((request?.FileData == null) || (request?.FileData.Length == 0))
            {
                return CreateInvalidParametersResponse("No import data specified");
            }

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                return MakeFailure("No user credentials");
            }

            var importBytes = _configurationManager.ImportConfig(username, password, request);

            if (importBytes != HawkeyeError.eSuccess)
            {
                return MakeFailure();
            }
            else
            {
                ApiHawkeyeMsgHelper.PublishHubMessage(LanguageResourceHelper.Get("LID_FrameLabel_ImportSuccessful"), MessageType.Normal);
                return MakeSuccess();
            }
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Paused })]
        public override Task<VcbResult> Resume(RequestResume request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(Resume)}");

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                return MakeFailure("No user credentials");
            }

            var result = _sampleProcessingService.ResumeProcessing(username, password);
            return result == false ?
                MakeFailure("Error resuming processing") :
                MakeSuccess();
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResult> StartSampleSet(RequestStartSampleSet request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(StartSampleSet)}");

            var requestSampleSet = request?.SampleSetConfig;
            if (requestSampleSet == null)
                return CreateInvalidParametersResponse("No sample set specified");

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                return MakeFailure("No user credentials");
            }

            _logger.Info($"{nameof(StartSampleSet)}: user: {username}, SampleSet: '{requestSampleSet.SampleSetName}'");

            var mappedSampleSet = _mapper.Map<SampleSetDomain>(requestSampleSet);
            var sampleSet = _sampleProcessingService.CreateSampleSetFromAutomation(mappedSampleSet.Samples,
                username, request.SampleSetConfig.SampleSetName, true, out var validationResult,
                ApplicationConstants.FirstNonOrphanSampleSetIndex);
            var sampleSetList = new List<SampleSetDomain> { sampleSet };

            return ProcessSamples(sampleSetList, username, sampleSet, ref validationResult);
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResult> StartSample(RequestStartSample request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(StartSample)}");

            // Validate parameters
            var sampleToStart = request?.SampleConfig;
            if (sampleToStart == null)
                return CreateInvalidParametersResponse($"{nameof(StartSample)}'s parameter 'SampleConfig' is NULL.");

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                return MakeFailure("No user credentials");
            }

            _logger.Info($"{nameof(StartSample)}: user: {username}, Sample name: '{sampleToStart.SampleName}'.");

            // Map the sample and create a sample set
            var sample = _mapper.Map<SampleEswDomain>(sampleToStart);
            var setName = LanguageResourceHelper.Get("LID_Default_AutomationCupSampleSetName");
            var sampleList = new List<SampleEswDomain> { sample };
            var sampleSet = _sampleProcessingService.CreateSampleSetFromAutomation(sampleList, username, setName, false, out var validationResult,
                ApplicationConstants.FirstNonOrphanSampleSetIndex);
            var sampleSetList = new List<SampleSetDomain> { sampleSet };

            // validate and process the sample set
            return ProcessSamples(sampleSetList, username, sampleSet, ref validationResult);
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.ProcessingSample, SystemStatus.Paused })]
        public override Task<VcbResult> Stop(RequestStop request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(Stop)}");

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                return MakeFailure("No user credentials");
            }

            // Try to stop sample processing
            var result = _sampleProcessingService.StopProcessing(username, password);
            return result ? MakeSuccess() : MakeFailure("Stop error");
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.ProcessingSample })]
        public override Task<VcbResult> Pause(RequestPause request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(Pause)}");

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                return MakeFailure("No user credentials");
            }

            // Try to pause sample processing
            var result = _sampleProcessingService.PauseProcessing(username, password);
            return result == false ?
                MakeFailure("Pause error") :
                MakeSuccess();
        }

        [RequiresAutomationLock(LockRequirements.NoRequirements)]
        public override Task<VcbResultGetCellTypes> GetCellTypes(RequestGetCellTypes request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(GetCellTypes)}");

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                return MakeFailureGetCellTypes();
            }

            var cellTypes = _cellTypeManager.GetAllowedCellTypes(username);
            return cellTypes != null ? MakeSuccessGetCellTypes(cellTypes) : MakeFailureGetCellTypes();
        }

        [RequiresAutomationLock(LockRequirements.NoRequirements)]
        public override Task<VcbResultGetQualityControls> GetQualityControls(RequestGetQualityControls request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(GetQualityControls)}");

            if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
            {
                return MakeFailureGetQualityControls("No user credentials");
            }
            var cellTypes = _cellTypeManager.GetAllowedCellTypes(username);
            var qualityControls = _cellTypeManager.GetAllowedQualityControls(username, cellTypes);
            return qualityControls != null ? MakeSuccessGetQualityControls(qualityControls) : MakeFailureGetQualityControls();
        }

        [RequiresAutomationLock(LockRequirements.NoRequirements)]
        public override Task<VcbResultGetDiskSpace> GetAvailableDiskSpace(RequestGetAvailableDiskSpace request, ServerCallContext context)
        {
            _logger.Info($"Received request: {nameof(GetAvailableDiskSpace)}");
            double total_size, free_size, other_size, data_size, export_size;
            var success = _configurationManager.GetDiskSpaceVariables(out total_size, out free_size, out other_size, out data_size, out export_size);
            return success ? MakeSuccessGetDiskSpace(total_size, free_size, other_size, data_size, export_size) : MakeFailureGetDiskSpace();
        }

        [RequiresAutomationLock(LockRequirements.NoRequirements)]
        public override Task<VcbResult> LoginRemoteUser(RequestLoginUser request, ServerCallContext context)
        {
            _logger.Info($"LoginRemoteUser: {request.Username}");

            if(_automationSettingsService.IsAutomationEnabled() == false)
                return MakeFailure("Automation is not currently enabled. Ensure you have enabled automation and try again or contact Beckman Services."); 

            string msg;
            var result = _securityService.LoginRemoteUser(request.Username, request.Password);
            if (result)
            {
                msg = request.Username + " Connected";
                _auditLog.WriteToAuditLogAPI(request.Username, audit_event_type.evt_automation, msg);
                ApiHawkeyeMsgHelper.PublishHubMessage(ScoutUtilities.Misc.AuditEventString(audit_event_type.evt_automation) + msg, MessageType.Normal);
            }
            else
            {
                msg = request.Username + " Connect Failed";
                _auditLog.WriteToAuditLogAPI(request.Username, audit_event_type.evt_automation, msg);
                ApiHawkeyeMsgHelper.PublishHubMessage(ScoutUtilities.Misc.AuditEventString(audit_event_type.evt_automation) + msg, MessageType.Warning);
            }

            _instrumentStatusService.PublishSoftwareVersionCallback(UISettings.SoftwareVersion);

            var hardwareInfo = new HardwareSettingsModel();
            var versionInfo = hardwareInfo.GetVersionInformation();
            _instrumentStatusService.PublishFirmwareVersionCallback(versionInfo.FirmwareVersion);

			return result ? MakeSuccess() : MakeFailure("Check validity of username/password.");
        }

        [RequiresAutomationLock(LockRequirements.NoRequirements)]
        public override Task<VcbResult> LogoutRemoteUser(RequestLogoutUser request, ServerCallContext context)
        {
            _logger.Info($"LogoutRemoteUser: {request.Username}");

            _securityService.LogoutRemoteUser(request.Username);

            string msg = request.Username + " Disconnected";
            _auditLog.WriteToAuditLogAPI(request.Username, audit_event_type.evt_automation, msg);
            ApiHawkeyeMsgHelper.PublishHubMessage(ScoutUtilities.Misc.AuditEventString(audit_event_type.evt_automation) + msg, MessageType.Normal);

			return MakeSuccess();
        }

		[MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResult> CleanFluidics(RequestCleanFluidics request, ServerCallContext context)
        {
	        _logger.Info($"Received request: {nameof(CleanFluidics)}");

	        if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
	        {
		        return MakeFailure("No user credentials");
	        }

	        var result = _maintenanceService.StartCleanFluidics();
			return result ? MakeSuccess() : MakeFailure("CleanFluidics error");
        }

        [RequiresAutomationLock]
        public override Task<VcbResultReagentVolume> GetReagentVolume (RequestGetReagentVolume request, ServerCallContext context)
        {
	        _logger.Info($"Received request: {nameof(GetReagentVolume)}");

	        if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
	        {
		        return MakeFailureReagentVolume("No user credentials");
	        }

	        var type = _mapper.Map<CellHealthFluidType>(request.Type);

	        Int32 volume = 0;
	        var result = HawkeyeCoreAPI.Reagent.GetReagentVolumeAPI (type, ref volume);
	        return result ? MakeSuccessReagentVolume(volume) : MakeFailureReagentVolume("GetReagentVolume error");
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        public override Task<VcbResult> SetReagentVolume(RequestSetReagentVolume request, ServerCallContext context)
        {
	        _logger.Info($"Received request: {nameof(SetReagentVolume)}");

	        if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
	        {
		        return MakeFailure("No user credentials");
	        }

	        var type = _mapper.Map<CellHealthFluidType>(request.Type);

	        var result = HawkeyeCoreAPI.Reagent.SetReagentVolumeAPI(type, request.Volume);
	        return result ? MakeSuccess() : MakeFailure("SetReagentVolume error");
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        public override Task<VcbResult> AddReagentVolume(RequestAddReagentVolume request, ServerCallContext context)
        {
	        _logger.Info($"Received request: {nameof(AddReagentVolume)}");

	        if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
	        {
		        return MakeFailure("No user credentials");
	        }

	        var type = _mapper.Map<CellHealthFluidType>(request.Type);

			var result = HawkeyeCoreAPI.Reagent.AddReagentVolumeAPI(type, request.Volume);
	        return result ? MakeSuccess() : MakeFailure("AddReagentVolume error");
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped, SystemStatus.Faulted })]
        public override Task<VcbResult> ShutdownOrReboot(RequestShutdownOrReboot request, ServerCallContext context)
        {
	        _logger.Info($"Received request: {nameof(ShutdownOrReboot)}");

	        if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
	        {
		        return MakeFailure("No user credentials");
	        }

	        var operation = _mapper.Map<ScoutUtilities.Enums.ShutdownOrRebootEnum>(request.Operation);
			HawkeyeCoreAPI.InitializeShutdown.ShutdownOrRebootAPI (operation);

	        return MakeSuccess();
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResult> DeleteCampaignData (RequestDeleteCampaignData request, ServerCallContext context)
        {
	        _logger.Info($"Received request: {nameof(DeleteCampaignData)}");

	        if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
	        {
		        return MakeFailure("No user credentials");
	        }

	        var result = HawkeyeCoreAPI.Result.DeleteCampaignDataAPI();
	        return result == HawkeyeError.eSuccess ? MakeSuccess() : MakeFailure("DeleteCampaignData error");
        }


		[MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResult> PrimeReagents(RequestPrimeReagents request, ServerCallContext context)
        {
	        _logger.Info($"Received request: {nameof(PrimeReagents)}");

	        if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
	        {
		        return MakeFailure("No user credentials");
	        }

	        var result = _maintenanceService.PrimeReagents();
	        return result ? MakeSuccess() : MakeFailure("Prime Reagents error");
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResult> CancelPrimeReagents(RequestCancelPrimeReagents request, ServerCallContext context)
        {
	        _logger.Info($"Received request: {nameof(CancelPrimeReagents)}");

	        if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
	        {
		        return MakeFailure("No user credentials");
	        }

	        var result = _maintenanceService.CancelPrimeReagents();
	        return result ? MakeSuccess() : MakeFailure("Cancel Prime Reagents error");
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResult> PurgeReagents(RequestPurgeReagents request, ServerCallContext context)
        {
	        _logger.Info($"Received request: {nameof(PurgeReagents)}");

	        if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
	        {
		        return MakeFailure("No user credentials");
	        }

	        var result = _maintenanceService.PurgeReagents();
	        return result ? MakeSuccess() : MakeFailure("Purge Reagents error");
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResult> CancelPurgeReagents(RequestCancelPurgeReagents request, ServerCallContext context)
        {
	        _logger.Info($"Received request: {nameof(CancelPurgeReagents)}");

	        if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
	        {
		        return MakeFailure("No user credentials");
	        }

	        var result = _maintenanceService.CancelPurgeReagents();
	        return result ? MakeSuccess() : MakeFailure("Cancel Purge Reagents error");
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResult> Decontaminate(RequestDecontaminate request, ServerCallContext context)
        {
	        _logger.Info($"Received request: {nameof(Decontaminate)}");

	        if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
	        {
		        return MakeFailure("No user credentials");
	        }

	        var result = _maintenanceService.Decontaminate();
	        return result ? MakeSuccess() : MakeFailure("Decontaminate error");
        }

        [MustOwnLock]
        [RequiresAutomationLock]
        [InstrumentState(new[] { SystemStatus.Idle, SystemStatus.Stopped })]
        public override Task<VcbResult> CancelDecontaminate(RequestCancelDecontaminate request, ServerCallContext context)
        {
	        _logger.Info($"Received request: {nameof(CancelDecontaminate)}");

	        if (!GrpcServer.GrpcInterceptor.ScoutInterceptor.ExtractBasicAuthCredentials(context.RequestHeaders, out var cnxId, out var username, out var password))
	        {
		        return MakeFailure("No user credentials");
	        }

	        var result = _maintenanceService.CancelDecontaminate();
	        return result ? MakeSuccess() : MakeFailure("Cancel Decontaminate error");
        }

	#endregion

	#region OPC Event Subscriptions

        private static GrpcClient _myClient = null;

        public override Task SubscribeLockState(RegistrationRequest request, IServerStreamWriter<LockStateChangedEvent> responseStream, ServerCallContext context)
        {
            if (_myClient == null)
                _myClient = _clientManager.AddClient(request.ClientId, "", "");
            _myClient.SubscribeLockResult(context, responseStream);
            return MakeSuccess();
        }

        public override Task SubscribeSampleStatus(RegistrationRequest request, 
	        IServerStreamWriter<SampleStatusChangedEvent> responseStream, ServerCallContext context)
        {
            if (_myClient == null)            
                _myClient = _clientManager.AddClient(request.ClientId, "", "");
            _myClient.SubscribeSampleStatusChanged(context, responseStream);            
            return MakeSuccess();
        }

        public override Task SubscribeSampleComplete(RegistrationRequest request, IServerStreamWriter<SampleCompleteEvent> responseStream, ServerCallContext context)
        {
            if (_myClient == null)
                _myClient = _clientManager.AddClient(request.ClientId, "", "");
            _myClient.SubscribeSampleComplete(context, responseStream);
            return MakeSuccess();
        }

        public override Task SubscribeWorkListComplete(RegistrationRequest request, IServerStreamWriter<WorkListCompleteEvent> responseStream, ServerCallContext context)
        {
            if (_myClient == null)
                _myClient = _clientManager.AddClient(request.ClientId, "", "");
            _myClient.SubscribeWorkListCompleted(context, responseStream);
            return MakeSuccess();
        }

        public override Task SubscribeViCellStatus(RegistrationRequest request, IServerStreamWriter<ViCellStatusChangedEvent> responseStream, ServerCallContext context)
        {
            if (_myClient == null)
                _myClient = _clientManager.AddClient(request.ClientId, "", "");
            _myClient.SubscribeSystemStatusChanged(context, responseStream);
            return MakeSuccess();
        }

        public override Task SubscribeViCellIdentifier(RegistrationRequest request, IServerStreamWriter<ViCellIdentifierChangedEvent> responseStream, ServerCallContext context)
        {
            if (_myClient == null)
                _myClient = _clientManager.AddClient(request.ClientId, "", "");
            _myClient.SubscribeViCellIdChanged(context, responseStream);
            return MakeSuccess();
        }

        public override Task SubscribeReagentUsesRemaining(RegistrationRequest request,
            IServerStreamWriter<ReagentUsesRemainingChangedEvent> responseStream, ServerCallContext context)
        {
            if (_myClient == null)
                _myClient = _clientManager.AddClient(request.ClientId, "", "");
            _myClient.SubscribeReagentUseRemainingChanged(context, responseStream);
            return MakeSuccess();
        }

        public override Task SubscribeWasteTubeCapacity(RegistrationRequest request,
            IServerStreamWriter<WasteTubeCapacityChangedEvent> responseStream, ServerCallContext context)
        {
            if (_myClient == null)
                _myClient = _clientManager.AddClient(request.ClientId, "", "");
            _myClient.SubscribeWasteTubeCapacityChanged(context, responseStream);
            return MakeSuccess();
        }

        public override Task SubscribeDeleteSampleResultsProgress(RegistrationRequest request,
	        IServerStreamWriter<DeleteSampleResultsProgressEvent> responseStream, ServerCallContext context)
        {
            var myProps = context.AuthContext.Properties;
            // ToDo: extract username and password - context.AuthContext.Properties
            if (_myClient == null)
                _myClient = _clientManager.AddClient(request.ClientId, "", "");
            _myClient.SubscribeDeleteSampleResultsProgress(context, responseStream);
            return MakeSuccess();
        }

        public override Task SubscribeExportStatus(RegistrationRequest request, 
	        IServerStreamWriter<ExportStatusEvent> responseStream, ServerCallContext context)
        {
            if (_myClient == null)
                _myClient = _clientManager.AddClient(request.ClientId, "", "");
            _myClient.SubscribeExportStatus(context, responseStream);
            return MakeSuccess();
        }

		public override Task SubscribePrimeReagentsStatus(RegistrationRequest request,
		 IServerStreamWriter<PrimeReagentsStatusEvent> responseStream, ServerCallContext context)
		{
			if (_myClient == null)
				_myClient = _clientManager.AddClient(request.ClientId, "", "");
			_myClient.SubscribePrimeReagentsStatusChanged(context, responseStream);
			return MakeSuccess();
		}

		public override Task SubscribePurgeReagentsStatus(RegistrationRequest request,
		 IServerStreamWriter<PurgeReagentsStatusEvent> responseStream, ServerCallContext context)
		{
			if (_myClient == null)
				_myClient = _clientManager.AddClient(request.ClientId, "", "");
			_myClient.SubscribePurgeReagentsStatusChanged(context, responseStream);
			return MakeSuccess();
		}

        public override Task SubscribeCleanFluidicsStatus(RegistrationRequest request,
	        IServerStreamWriter<CleanFluidicsStatusEvent> responseStream, ServerCallContext context)
        {
	        if (_myClient == null)
		        _myClient = _clientManager.AddClient(request.ClientId, "", "");
			_myClient.SubscribeCleanFluidicsStatusChanged(context, responseStream);
			return MakeSuccess();
        }

        public override Task SubscribeDecontaminateStatus(RegistrationRequest request,
	        IServerStreamWriter<DecontaminateStatusEvent> responseStream, ServerCallContext context)
        {
	        if (_myClient == null)
		        _myClient = _clientManager.AddClient(request.ClientId, "", "");
	        _myClient.SubscribeDecontaminateStatusChanged(context, responseStream);
	        return MakeSuccess();
        }

        public override Task SubscribeSoftwareVersion(RegistrationRequest request,
	        IServerStreamWriter<SoftwareVersionChangedEvent> responseStream, ServerCallContext context)
        {
	        if (_myClient == null)
		        _myClient = _clientManager.AddClient(request.ClientId, "", "");
	        _myClient.SubscribeSoftwareVersionChanged(context, responseStream);
			return MakeSuccess();
        }

        public override Task SubscribeFirmwareVersion(RegistrationRequest request,
	        IServerStreamWriter<FirmwareVersionChangedEvent> responseStream, ServerCallContext context)
        {
	        if (_myClient == null)
		        _myClient = _clientManager.AddClient(request.ClientId, "", "");
	        _myClient.SubscribeFirmwareVersionChanged(context, responseStream);
	        return MakeSuccess();
        }

        public override Task SubscribeErrorStatus(RegistrationRequest request,
	        IServerStreamWriter<ErrorStatusEvent> responseStream, ServerCallContext context)
        {
	        if (_myClient == null)
		        _myClient = _clientManager.AddClient(request.ClientId, "", "");
	        _myClient.SubscribeErrorStatusChanged(context, responseStream);
	        return MakeSuccess();
        }

	#endregion

	#region Private Methods

        private Task<VcbResult> CreateInvalidParametersResponse(string responseDescription)
        {
            _logger.Info($"Parameters were invalid. Description: {responseDescription}");
            var result = new VcbResult
            {
                Description = string.IsNullOrEmpty(responseDescription)
                    ? "Invalid Parameters"
                    : responseDescription,
                ErrorLevel = ErrorLevelEnum.RequiresUserInteraction,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResult> CreateInvalidParametersCreateQualityControlResponse(string responseDescription)
        {
            _logger.Info($"Parameters were invalid. Description: {responseDescription}");
            var result = new VcbResult
            {
                Description = string.IsNullOrEmpty(responseDescription)
                    ? "Invalid Parameters"
                    : responseDescription,
                ErrorLevel = ErrorLevelEnum.RequiresUserInteraction,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultDeleteCellType> CreateInvalidParametersDeleteCellTypeResponse(string responseDescription)
        {
            _logger.Info($"Parameters were invalid. Description: {responseDescription}");
            var result = new VcbResultDeleteCellType
            {
                Description = string.IsNullOrEmpty(responseDescription)
                    ? "Invalid Parameters"
                    : responseDescription,
                ErrorLevel = ErrorLevelEnum.RequiresUserInteraction,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultGetSampleResults> CreateInvalidParametersGetSampleResultsResponse(string responseDescription)
        {
            _logger.Info($"Parameters were invalid. Description: {responseDescription}");
            var result = new VcbResultGetSampleResults
            {
                Description = string.IsNullOrEmpty(responseDescription)
                    ? "Invalid Parameters"
                    : responseDescription,
                ErrorLevel = ErrorLevelEnum.RequiresUserInteraction,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultStartExport> CreateInvalidParametersStartExportResponse(string responseDescription)
        {
            _logger.Info($"Parameters were invalid. Description: {responseDescription}");
            var result = new VcbResultStartExport
            {
                Description = string.IsNullOrEmpty(responseDescription)
                    ? "Invalid Parameters"
                    : responseDescription,
                ErrorLevel = ErrorLevelEnum.RequiresUserInteraction,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultRetrieveBulkDataBlock> CreateInvalidParametersRetrieveBulkDataResponse(string responseDescription)
        {
            _logger.Info($"Parameters were invalid. Description: {responseDescription}");
            var result = new VcbResultRetrieveBulkDataBlock
            {
                Description = string.IsNullOrEmpty(responseDescription)
                    ? "Invalid Parameters"
                    : responseDescription,
                ErrorLevel = ErrorLevelEnum.RequiresUserInteraction,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultExportConfig> CreateInvalidParametersExportConfigResponse(string responseDescription)
        {
            _logger.Info($"Parameters were invalid. Description: {responseDescription}");
            var result = new VcbResultExportConfig
            {
                Description = string.IsNullOrEmpty(responseDescription)
                    ? "Invalid Parameters"
                    : responseDescription,
                ErrorLevel = ErrorLevelEnum.RequiresUserInteraction,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResult> MakeSuccess()
        {
            var result = new VcbResult
            {
                Description = "Success",
                ErrorLevel = ErrorLevelEnum.NoError,
                MethodResult = MethodResultEnum.Success
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultGetDiskSpace> MakeSuccessGetDiskSpace(double total, double free, double other, double data, double export)
        {
            var result = new VcbResultGetDiskSpace
            {
                Description = "Success",
                ErrorLevel = ErrorLevelEnum.NoError,
                MethodResult = MethodResultEnum.Success,
                TotalSizeBytes = total,
                TotalFreeBytes = free,
                DiskSpaceOtherBytes = other,
                DiskSpaceDataBytes = data,
                DiskSpaceExportBytes = export
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultGetQualityControls> MakeSuccessGetQualityControls(List<QualityControlDomain> qualityControls)
        {
            var grpcQualityControls = new RepeatedField<QualityControl>();
            foreach (var qualityControl in qualityControls)
                grpcQualityControls.Add(_mapper.Map<QualityControl>(qualityControl));

            var result = new VcbResultGetQualityControls
            {
                Description = "Success",
                ErrorLevel = ErrorLevelEnum.NoError,
                MethodResult = MethodResultEnum.Success,
                QualityControls = { grpcQualityControls }
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultGetCellTypes> MakeSuccessGetCellTypes(List<CellTypeDomain> cellTypes)
        {
            var grpcCellTypes = new RepeatedField<CellType>();
            foreach (var cellType in cellTypes)
                grpcCellTypes.Add(_mapper.Map<CellType>(cellType));

            var result = new VcbResultGetCellTypes
            {
                Description = "Success",
                ErrorLevel = ErrorLevelEnum.NoError,
                MethodResult = MethodResultEnum.Success,
                CellTypes = { grpcCellTypes }
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultExportConfig> MakeSuccessExportConfig(byte[] encryptedConfigBytes)
        {
            var result = new VcbResultExportConfig
            {
                Description = "Success",
                ErrorLevel = ErrorLevelEnum.NoError,
                MethodResult = MethodResultEnum.Success,
                FileData = ByteString.CopyFrom(encryptedConfigBytes)
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultEjectStage> MakeSuccessEjectStage()
        {
            var result = new VcbResultEjectStage
            {
                Description = "Success",
                ErrorLevel = ErrorLevelEnum.NoError,
                MethodResult = MethodResultEnum.Success
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultStartExport> MakeSuccessStartExport(string bulkDataId)
        {
            var result = new VcbResultStartExport
            {
                Description = "Success",
                ErrorLevel = ErrorLevelEnum.NoError,
                MethodResult = MethodResultEnum.Success,
                BulkDataId = bulkDataId
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultRetrieveBulkDataBlock> MakeSuccessRetrieveExportBlock(uint blockIdx, RetrieveBulkBlockStatusEnum status, byte[] data)
        {
            var result = new VcbResultRetrieveBulkDataBlock();

            result.Description = "Success";
            result.ErrorLevel = ErrorLevelEnum.NoError;
            result.MethodResult = MethodResultEnum.Success;
            result.BlockIndex = blockIdx;
            result.Status = status;
            result.SampleResultsZipFileBytes = ByteString.Empty;

            if ((data != null) && data.Length > 0)
            {
                result.SampleResultsZipFileBytes = ByteString.CopyFrom(data);
            }
            return Task.FromResult(result);
        }

        private Task<VcbResultDeleteCellType> MakeSuccessDeleteCellType()
        {
            var result = new VcbResultDeleteCellType
            {
                Description = "Success",
                ErrorLevel = ErrorLevelEnum.NoError,
                MethodResult = MethodResultEnum.Success
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultCreateCellType> MakeSuccessCreateCellType()
        {
            var result = new VcbResultCreateCellType
            {
                Description = "Success",
                ErrorLevel = ErrorLevelEnum.NoError,
                MethodResult = MethodResultEnum.Success
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultRequestLock> MakeSuccessRequestLock(LockResult status)
        {
            var result = new VcbResultRequestLock
            {
                Description = "Success",
                ErrorLevel = ErrorLevelEnum.NoError,
                MethodResult = MethodResultEnum.Success,
                LockState = status == LockResult.Locked ? LockStateEnum.Locked : LockStateEnum.Unlocked
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultReleaseLock> MakeSuccessReleaseLock(LockResult status)
        {
            var result = new VcbResultReleaseLock
            {
                Description = "Success",
                ErrorLevel = ErrorLevelEnum.NoError,
                MethodResult = MethodResultEnum.Success,
                LockState = status == LockResult.Locked ? LockStateEnum.Locked : LockStateEnum.Unlocked
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultGetSampleResults> MakeSuccessGetSampleResults(List<SampleResult> sampleResults)
        {
            var grpcSampleResults = new RepeatedField<SampleResult>();
            foreach (var sampleResult in sampleResults)
                grpcSampleResults.Add(_mapper.Map<SampleResult>(sampleResult));

            var result = new VcbResultGetSampleResults
            {
                Description = "Success",
                ErrorLevel = ErrorLevelEnum.NoError,
                MethodResult = MethodResultEnum.Success,
                SampleResults = { grpcSampleResults }
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultReagentVolume> MakeSuccessReagentVolume (Int32 volume)
        {
	        var result = new VcbResultReagentVolume
			{
		        Description = "Success",
		        ErrorLevel = ErrorLevelEnum.NoError,
		        MethodResult = MethodResultEnum.Success,
				Volume = volume
	        };
	        return Task.FromResult(result);
        }

        private Task<VcbResult> MakeFailure(string responseDescription = null)
        {
            if (!string.IsNullOrEmpty(responseDescription))
                _logger.Debug(responseDescription);

            var result = new VcbResult
            {
                Description = string.IsNullOrEmpty(responseDescription) ? "Failure" : responseDescription,
                ErrorLevel = ErrorLevelEnum.Error,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultGetDiskSpace> MakeFailureGetDiskSpace(string responseDescription = null)
        {
            if (!string.IsNullOrEmpty(responseDescription))
                _logger.Debug(responseDescription);

            var result = new VcbResultGetDiskSpace
            {
                Description = string.IsNullOrEmpty(responseDescription) ? "Failure" : responseDescription,
                ErrorLevel = ErrorLevelEnum.Error,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }


        private Task<VcbResultGetQualityControls> MakeFailureGetQualityControls(string responseDescription = null)
        {
            if (!string.IsNullOrEmpty(responseDescription))
                _logger.Debug(responseDescription);

            var result = new VcbResultGetQualityControls
            {
                Description = string.IsNullOrEmpty(responseDescription) ? "Failure" : responseDescription,
                ErrorLevel = ErrorLevelEnum.Error,
                MethodResult = MethodResultEnum.Failure,
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultGetCellTypes> MakeFailureGetCellTypes(string responseDescription = null)
        {
            if (!string.IsNullOrEmpty(responseDescription))
                _logger.Debug(responseDescription);

            var result = new VcbResultGetCellTypes
            {
                Description = string.IsNullOrEmpty(responseDescription) ? "Failure" : responseDescription,
                ErrorLevel = ErrorLevelEnum.Error,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }


        private Task<VcbResultEjectStage> MakeFailureEjectStage(string responseDescription = null)
        {
            if (!string.IsNullOrEmpty(responseDescription))
                _logger.Debug(responseDescription);

            var result = new VcbResultEjectStage
            {
                Description = string.IsNullOrEmpty(responseDescription) ? "Failure" : responseDescription,
                ErrorLevel = ErrorLevelEnum.Error,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }


        private Task<VcbResult> MakeFailureCreateQualityControl(string responseDescription = null)
        {
            if (!string.IsNullOrEmpty(responseDescription))
                _logger.Debug(responseDescription);

            var result = new VcbResult
			{
                Description = string.IsNullOrEmpty(responseDescription) ? "Failure" : responseDescription,
                ErrorLevel = ErrorLevelEnum.Error,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultDeleteCellType> MakeFailureDeleteCellType(string responseDescription = null)
        {
            if (!string.IsNullOrEmpty(responseDescription))
                _logger.Debug(responseDescription);

            var result = new VcbResultDeleteCellType
            {
                Description = string.IsNullOrEmpty(responseDescription) ? "Failure" : responseDescription,
                ErrorLevel = ErrorLevelEnum.Error,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultStartExport> MakeFailureStartExport(string responseDescription = null)
        {
            if (!string.IsNullOrEmpty(responseDescription))
                _logger.Debug(responseDescription);

            var result = new VcbResultStartExport
            {
                Description = string.IsNullOrEmpty(responseDescription) ? "Failure" : responseDescription,
                ErrorLevel = ErrorLevelEnum.Error,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultRetrieveBulkDataBlock> MakeFailureRetrieveBulkData(string responseDescription = null)
        {
            if (!string.IsNullOrEmpty(responseDescription))
                _logger.Debug(responseDescription);

            var result = new VcbResultRetrieveBulkDataBlock
            {
                Description = string.IsNullOrEmpty(responseDescription) ? "Failure" : responseDescription,
                ErrorLevel = ErrorLevelEnum.Error,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultExportConfig> MakeFailureExportConfig(string responseDescription = null)
        {
            if (!string.IsNullOrEmpty(responseDescription))
                _logger.Debug(responseDescription);

            var result = new VcbResultExportConfig
            {
                Description = string.IsNullOrEmpty(responseDescription) ? "Failure" : responseDescription,
                ErrorLevel = ErrorLevelEnum.Error,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultCreateCellType> MakeFailureCreateCellType(string responseDescription = null)
        {
            if (!string.IsNullOrEmpty(responseDescription))
                _logger.Debug(responseDescription);

            var result = new VcbResultCreateCellType
            {
                Description = string.IsNullOrEmpty(responseDescription) ? "Failure" : responseDescription,
                ErrorLevel = ErrorLevelEnum.Error,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultGetSampleResults> MakeFailureGetSampleResults(string responseDescription = null)
        {
            if (!string.IsNullOrEmpty(responseDescription))
                _logger.Debug(responseDescription);

            var result = new VcbResultGetSampleResults
            {
                Description = string.IsNullOrEmpty(responseDescription) ? "Failure" : responseDescription,
                ErrorLevel = ErrorLevelEnum.Error,
                MethodResult = MethodResultEnum.Failure
            };
            return Task.FromResult(result);
        }

        private Task<VcbResultReagentVolume> MakeFailureReagentVolume(string responseDescription = null)
        {
	        if (!string.IsNullOrEmpty(responseDescription))
		        _logger.Debug(responseDescription);

	        var result = new VcbResultReagentVolume
			{
		        Description = string.IsNullOrEmpty(responseDescription) ? "Failure" : responseDescription,
		        ErrorLevel = ErrorLevelEnum.Error,
		        MethodResult = MethodResultEnum.Failure,
				Volume = 0
	        };
	        return Task.FromResult(result);
        }

        private void HandleSystemStatusCallback(SystemStatusDomain obj)
        {
            _carouselIsInstalled = obj.CarouselDetect == eSensorStatus.ssStateActive;
        }

        private Task<VcbResult> ProcessSamples(List<SampleSetDomain> sampleSetList, string username, SampleSetDomain sampleSet, ref SampleProcessingValidationResult validationResult)
        {
            // run the sample set on the instrument

            var vResult = _sampleProcessingService.CanProcessSamples(username, sampleSetList, _carouselIsInstalled);
            validationResult |= vResult;
            if (_sampleProcessingService.HasPendingDeviceSamples)
            {
                validationResult |= SampleProcessingValidationResult.WorkListStatusNotIdle;
            }

            if (validationResult == SampleProcessingValidationResult.Valid)
            {
                _logger.Debug($"ProcessSamples: {validationResult}");

                var template = new SampleSetTemplateDomain(); // this is only used with carousel (not automation), so it can be empty
                
                var processResult = _scoutModelsFactory.CreateSecuredTask().Run(() => _sampleProcessingService.ProcessSamples(sampleSetList, username, template, XMLDataAccess.Instance));
                if (processResult?.Result != true)
                {
                    return MakeFailure($"Error processing samples");
                }

                return MakeSuccess();
            }
            else
            {
                return MakeFailure($"Failed to process SampleSet '{sampleSet?.SampleSetName}': {validationResult}");
            }
        }

	#endregion

    }
}