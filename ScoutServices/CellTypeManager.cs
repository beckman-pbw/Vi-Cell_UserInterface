using HawkeyeCoreAPI.Facade;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Ninject.Extensions.Logging;
using ScoutModels.Admin;
using ScoutModels.Interfaces;
using ScoutModels.Settings;

namespace ScoutServices
{
    public class CellTypeManager : BaseDisposableNotifyPropertyChanged, ICellTypeManager
    {
        private SystemStatusDomain _systemStatusDomain;
        private IDisposable _systemStatusSubscriber;

        private readonly Subject<List<CellTypeDomain>> _cellTypesRetrievedSubject;
        private readonly Subject<List<QualityControlDomain>> _qualityControlsRetrievedSubject;
        private readonly ILogger _logger;

        public CellTypeManager(IInstrumentStatusService instrumentStatusService, ILogger logger)
        {
            _systemStatusSubscriber = instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe(OnSystemStatusChanged);
            _logger = logger;

            _cellTypesRetrievedSubject = new Subject<List<CellTypeDomain>>();
            _qualityControlsRetrievedSubject = new Subject<List<QualityControlDomain>>();
        }

        protected override void DisposeManaged()
        {
            _cellTypesRetrievedSubject?.OnCompleted();
            _qualityControlsRetrievedSubject?.OnCompleted();
            base.DisposeManaged();
        }

        protected override void DisposeUnmanaged()
        {
            _systemStatusSubscriber?.Dispose();
            base.DisposeUnmanaged();
        }

        public IObservable<List<CellTypeDomain>> SubscribeCellTypeRetrieval()
        {
            return _cellTypesRetrievedSubject;
        }

        public IObservable<List<QualityControlDomain>> SubscribeQualityControlRetrieval()
        {
            return _qualityControlsRetrievedSubject;
        }

        public bool InstrumentStateAllowsEdit => _systemStatusDomain?.SystemStatus == SystemStatus.Idle ||
                                                 _systemStatusDomain?.SystemStatus == SystemStatus.Faulted ||
                                                 _systemStatusDomain?.SystemStatus == SystemStatus.Stopped;

        private void OnSystemStatusChanged(SystemStatusDomain systemStatus)
        {
            if (systemStatus == null)
                return;
            _systemStatusDomain = systemStatus;
            NotifyPropertyChanged(nameof(InstrumentStateAllowsEdit));
        }

        #region Create Cell Type

        public bool CreateCellType(string username, string password, CellTypeDomain selectedCellType, string retiredCellTypeName, bool showDialogPrompt)
        {
            var status = CellTypeFacade.Instance.Add(username, password, selectedCellType, retiredCellTypeName);
            if (status.Equals(HawkeyeError.eSuccess))
            {
                CellTypeModel.SpecializeAnalysisForCellType(selectedCellType.AnalysisDomain, selectedCellType.CellTypeIndex);
                var str = LanguageResourceHelper.Get("LID_StatusBar_AddedCellType");
                _logger.Debug(str);
                if (showDialogPrompt)
                    ApiHawkeyeMsgHelper.PublishHubMessage(str, MessageType.Normal);
                return true;
            }

            if (showDialogPrompt)
            {
	            selectedCellType.TempCellName = selectedCellType.CellTypeName;
	            ApiHawkeyeMsgHelper.ErrorCreateCellType(status);
            }

            return false;
        }


        public bool SaveCellTypeValidation(CellTypeDomain selectedCellType, bool showDialogPrompt)
        {
            return SaveCellTypeValidation(selectedCellType, showDialogPrompt, out var _);
        }

        public bool SaveCellTypeValidation(CellTypeDomain selectedCellType, bool showDialogPrompt, out string failureReason)
        {
            failureReason = "";
            return CellTypeName(selectedCellType.TempCellName, showDialogPrompt, ref failureReason) &&
                CellTypeMinimumDiameter(selectedCellType.MinimumDiameter, selectedCellType.MaximumDiameter, showDialogPrompt, ref failureReason) &&
                CellTypeMaximumDiameter(selectedCellType.MinimumDiameter, selectedCellType.MaximumDiameter, showDialogPrompt, ref failureReason) &&
                CellTypeImages(selectedCellType.Images, showDialogPrompt, ref failureReason) &&
                CellTypeSharpness(selectedCellType.CellSharpness, showDialogPrompt, ref failureReason) &&
                CellTypeMinCircularity(selectedCellType.MinimumCircularity, showDialogPrompt, ref failureReason) &&
                CellTypeSpotBrightness(selectedCellType.AnalysisDomain.AnalysisParameter.Last().ThresholdValue, showDialogPrompt, ref failureReason) &&
                CellTypeSpotArea(selectedCellType.AnalysisDomain.AnalysisParameter.First().ThresholdValue, showDialogPrompt, ref failureReason) &&
                AdjustmentFactorValidation(selectedCellType.CalculationAdjustmentFactor, showDialogPrompt, ref failureReason) &&
                AspirationCycles(selectedCellType.AspirationCycles, ref failureReason) &&
                MixingCycles(selectedCellType.AnalysisDomain.MixingCycle, ref failureReason) &&
                DeclusterDegree(selectedCellType.DeclusterDegree, ref failureReason);
        }

        private bool CellTypeMinimumDiameter(double? minimumDiameter, double? maximumDiameter, bool showDialogPrompt, ref string failureReason)
        {
            if (minimumDiameter != null && maximumDiameter != null)
            {
                if (minimumDiameter >= ApplicationConstants.LowerDiameterLimit)
                {
                    if (minimumDiameter < maximumDiameter) return true;

                    failureReason = LanguageResourceHelper.Get("LID_MSGBOX_MinimumDiameterLength");
                    ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                    _logger.Debug(failureReason);
                }
                else
                {
                    failureReason = string.Format(
                        LanguageResourceHelper.Get("LID_Label_MinDiameterRange"),
                        Misc.UpdateTrailingPoint(ApplicationConstants.LowerDiameterLimit, TrailingPoint.Two),
                        Misc.UpdateTrailingPoint(ApplicationConstants.UpperDiameterLimit, TrailingPoint.Two));

                    ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                    _logger.Info(failureReason);
                }
            }
            else
            {
                if (!minimumDiameter.HasValue)
                {
                    failureReason = LanguageResourceHelper.Get("LID_ERRMSGBOX_MinimumDiameterBlank");
                    ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                    _logger.Debug(failureReason);
                }
                else if (!maximumDiameter.HasValue)
                {
                    failureReason = LanguageResourceHelper.Get("LID_ERRMSGBOX_MaximumDiameterBlank");
                    ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                    _logger.Debug(failureReason);
                }
            }

            return false;
        }

        private bool CellTypeMaximumDiameter(double? minimumDiameter, double? maximumDiameter, bool showDialogPrompt, ref string failureReason)
        {
            if (minimumDiameter != null && maximumDiameter != null)
            {
                if (maximumDiameter <= ApplicationConstants.UpperDiameterLimit)
                {
                    if (minimumDiameter < maximumDiameter) return true;

                    failureReason = LanguageResourceHelper.Get("LID_MSGBOX_MaximumDiameter");
                    ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                    _logger.Debug(failureReason);
                }
                else
                {
                    failureReason = string.Format(LanguageResourceHelper.Get("LID_Label_MaxDiameterRange"),
                        Misc.UpdateTrailingPoint(ApplicationConstants.LowerDiameterLimit, TrailingPoint.Two),
                        Misc.UpdateTrailingPoint(ApplicationConstants.UpperDiameterLimit, TrailingPoint.Two));

                    ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                    _logger.Info(failureReason);

                }
            }
            else
            {
                if (!maximumDiameter.HasValue)
                {
                    failureReason = LanguageResourceHelper.Get("LID_ERRMSGBOX_MaximumDiameterBlank");
                    ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                    _logger.Debug(failureReason);
                }
                else if (!minimumDiameter.HasValue)
                {
                    failureReason = LanguageResourceHelper.Get("LID_ERRMSGBOX_MinimumDiameterBlank");
                    ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                    _logger.Debug(failureReason);
                }
            }

            return false;
        }

        private bool CellTypeImages(int? value, bool showDialogPrompt, ref string failureReason)
        {
            if (value != null)
            {

                if (value >= ApplicationConstants.MinimumCelltypeImageCount && value <= ApplicationConstants.MaximumCelltypeImageCount) return true;

                failureReason = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_NoOfImagesLimit"),
                    Misc.ConvertToString(ApplicationConstants.MinimumCelltypeImageCount),
                    Misc.ConvertToString(ApplicationConstants.MaximumCelltypeImageCount));

                ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                _logger.Debug(failureReason);
            }
            else
            {
                failureReason = LanguageResourceHelper.Get("LID_ERRMSGBOX_NoOfImagesBlank");
                ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                _logger.Debug(failureReason);
            }

            return false;
        }

        private bool CellTypeSharpness(float? value, bool showDialogPrompt, ref string failureReason)
        {
            if (value != null)
            {
                if (value >= ApplicationConstants.LowerSharpLimit && value <= ApplicationConstants.UpperSharpLimit) return true;

                failureReason = string.Format(LanguageResourceHelper.Get("LID_Label_CellSharpnessLimit"),
                    Misc.UpdateTrailingPoint(ApplicationConstants.LowerSharpLimit, TrailingPoint.One),
                    Misc.UpdateTrailingPoint(ApplicationConstants.UpperSharpLimit, TrailingPoint.One));

                ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                _logger.Debug(failureReason);
            }
            else
            {
                failureReason = LanguageResourceHelper.Get("LID_Label_CellSharpnessValueBlank");
                ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                _logger.Debug(failureReason);
            }

            return false;
        }

        private bool CellTypeMinCircularity(double? value, bool showDialogPrompt, ref string failureReason)
        {
            if (value != null)
            {
                if (value >= ApplicationConstants.LowerCircularityLimit && value <= ApplicationConstants.UpperCircularityLimit) return true;

                failureReason = string.Format(LanguageResourceHelper.Get("LID_Label_MinimumCircularityRange"),
                    Misc.UpdateTrailingPoint(ApplicationConstants.LowerCircularityLimit, TrailingPoint.Two),
                    Misc.UpdateTrailingPoint(ApplicationConstants.UpperCircularityLimit, TrailingPoint.Two));

                ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                _logger.Info(failureReason);
            }
            else
            {
                failureReason = LanguageResourceHelper.Get("LID_ERRMSGBOX_MinimumCircularityBlank");
                ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                _logger.Debug(failureReason);
            }

            return false;
        }

        private bool CellTypeSpotBrightness(float? value, bool showDialogPrompt, ref string failureReason)
        {
            if (value != null)
            {
                if (value >= ApplicationConstants.LowerCellTypeSpotBrightnessLimit && value <= ApplicationConstants.UpperCellTypeSpotBrightnessLimit) return true;

                failureReason = string.Format(LanguageResourceHelper.Get("LID_Label_SpotBrightnessRange"),
                    Misc.UpdateTrailingPoint(ApplicationConstants.LowerCellTypeSpotBrightnessLimit, TrailingPoint.Two),
                    Misc.UpdateTrailingPoint(ApplicationConstants.UpperCellTypeSpotBrightnessLimit, TrailingPoint.Two));

                ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                _logger.Info(failureReason);
            }
            else
            {
                failureReason = LanguageResourceHelper.Get("LID_ERRMSGBOX_ViableSpotBrightnessBlank");
                ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                _logger.Debug(failureReason);
            }
            return false;
        }

        private bool CellTypeSpotArea(float? value, bool showDialogPrompt, ref string failureReason)
        {
            if (value != null)
            {
                if (value >= ApplicationConstants.LowerCellTypeSpotAreaLimit && value <= ApplicationConstants.UpperCellTypeSpotAreaLimit) return true;

                failureReason = string.Format(LanguageResourceHelper.Get("LID_Label_SpotAreaRange"),
                    Misc.UpdateTrailingPoint(ApplicationConstants.LowerCellTypeSpotAreaLimit, TrailingPoint.Two),
                    Misc.UpdateTrailingPoint(ApplicationConstants.UpperCellTypeSpotAreaLimit, TrailingPoint.Two));

                ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                _logger.Info(failureReason);
            }
            else
            {
                failureReason = LanguageResourceHelper.Get("LID_ERRMSGBOX_ViableSpotAreaBlank");
                ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                _logger.Debug(failureReason);
            }
            return false;
        }

        //Only called from opc. On the GUI the Concentration Adjustment Factor field is disabled for non-admins
        public bool CanAddAdjustmentFactor(string loggedInUser, CellTypeDomain selectedCellType, out string failureReason)
        {
            failureReason = string.Empty;
            if (UserModel.GetUserRole(loggedInUser) == UserPermissionLevel.eAdministrator ||
                selectedCellType.CalculationAdjustmentFactor == 0.0)
                return true;
            failureReason = "User is not permitted to add a Concentration Adjustment Factor";
            return false;
        }

        private bool AdjustmentFactorValidation(float? value, bool showDialogPrompt, ref string failureReason)
        {
            if (value == null)
            {
                failureReason = string.Format(LanguageResourceHelper.Get("LID_ERRMSGBOX_AdjustmentFactorBlank"),
                    Misc.UpdateTrailingPoint(ApplicationConstants.DefaultAdjustmentFactorValue, TrailingPoint.One));
                ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                return false;
            }

            if (value >= ApplicationConstants.LowerCellTypeAdjustmentFactorValue &&
                value <= ApplicationConstants.UpperCellTypeAdjustmentFactorValue)
                return true;

            failureReason = string.Format(LanguageResourceHelper.Get("LID_Label_AdjustmentFactorRange"),
                Misc.UpdateTrailingPoint(ApplicationConstants.LowerCellTypeAdjustmentFactorValue, TrailingPoint.One),
                Misc.UpdateTrailingPoint(ApplicationConstants.UpperCellTypeAdjustmentFactorValue, TrailingPoint.One));

            ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
            _logger.Info(failureReason);
            return false;
        }

        private bool CellTypeName(string name, bool showDialogPrompt, ref string failureReason)
        {
            if (string.IsNullOrEmpty(name))
            {
                failureReason = LanguageResourceHelper.Get("LID_ERRMSGBOX_CellTypeNameBlank");
                ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                _logger.Debug(failureReason);
                return false;
            }

            if (name.Length > ApplicationConstants.CellTypeNameLimit)
            {
                failureReason = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_CelltypeLimit"), Misc.ConvertToString(ApplicationConstants.CellTypeNameLimit));
                ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                _logger.Info(failureReason);
                return false;
            }

            /* This check is only needed for OpcUa calls.  The UI doesn't allow the user to enter certain characters
             * whereas a user can type in a bad value using OpcUa, therefore, no UI dialog messages are necessary
             */
            if (Misc.ContainsInvalidCharacter(name))
            {
                failureReason = "Cell Type name contains an invalid character";
                _logger.Debug(failureReason);
                return false;
            }

            return true;
        }

        /* This check is only needed for OpcUa calls.  The UI forces the use to select a value within range
         * whereas a user can type in a bad value using OpcUa, therefore, no UI dialog messages are necessary
         */
        private bool AspirationCycles(int? value, ref string failureReason)
        {
            if (value != null)
            {
                if (value >= ApplicationConstants.LowerAspirationCyclesLimit && value <= ApplicationConstants.UpperAspirationCyclesLimit)
                    return true;
                failureReason = $"Aspiration Cycles must be between {ApplicationConstants.LowerAspirationCyclesLimit} and {ApplicationConstants.UpperAspirationCyclesLimit}";
                _logger.Debug(failureReason);
            }
            else
            {
                failureReason = "Aspiration Cycles cannot be blank";
                _logger.Debug(failureReason);
            }
            return false;
        }

        /* This check is only needed for OpcUa calls.  The UI forces the use to select a value within range
         * whereas a user can type in a bad value using OpcUa, therefore, no UI dialog messages are necessary
         */
        private bool MixingCycles(int? value, ref string failureReason)
        {
            if (value != null)
            {
                if (value >= ApplicationConstants.LowerMixingCyclesLimit && value <= ApplicationConstants.UpperMixingCyclesLimit)
                    return true;
                failureReason = $"Mixing Cycles must be between {ApplicationConstants.LowerMixingCyclesLimit} and {ApplicationConstants.UpperMixingCyclesLimit}";
                _logger.Debug(failureReason);
            }
            else
            {
                failureReason = "Mixing Cycles cannot be blank";
                _logger.Debug(failureReason);
            }

            return false;
        }

        /* This check is only needed for OpcUa calls.  The UI forces the use to select a value within range
         * whereas a user can type in a bad value using OpcUa, therefore, no UI dialog messages are necessary
         */
        private bool DeclusterDegree(eCellDeclusterSetting? value, ref string failureReason)
        {
            if (value != null)
            {
                if (Enum.IsDefined(typeof(eCellDeclusterSetting), value))
                    return true;
                failureReason = $"DeclusterDegree is out of range";
                _logger.Debug(failureReason);
            }
            else
            {
                failureReason = "DeclusterDegree cannot be blank";
                _logger.Debug(failureReason);
            }
            return false;
        }

        #endregion

        #region DeleteCellType

        public bool DeleteCellType(string username, string password, string cellTypeName, bool showDialogPrompt)
        {
            if (cellTypeName == null)
                return false;

            var cellType = GetCellTypeDomain(username, password, cellTypeName);
            if (cellType == null)
                return false;

            if (!CanPerformDelete(cellType))
                return false;

            return DeleteCellTypeWorker(username, password, cellType, showDialogPrompt);
        }

        public bool DeleteCellType(string username, string password, CellTypeDomain cellType, bool showDialogPrompt)
        {
            return DeleteCellTypeWorker(username, password, cellType, showDialogPrompt);
        }

        public bool CanPerformDelete(CellTypeDomain cellType)
        {
            if(cellType == null)
            {
                return false;
            }
            return InstrumentStateAllowsEdit &&
                    !cellType.IsQualityControlCellType &&
                    cellType.IsUserDefineCellType;
        }

        private bool DeleteCellTypeWorker(string username, string password, CellTypeDomain cellType, bool showDialogPrompt)
        {
            var deleteCellResult = CellTypeFacade.Instance.Remove(username, password, cellType);
            if (deleteCellResult.Equals(HawkeyeError.eSuccess))
            {
                var str = LanguageResourceHelper.Get("LID_StatusBar_CellhasBeenDeleted");
                if (showDialogPrompt)
                    ApiHawkeyeMsgHelper.PublishHubMessage(str, MessageType.Normal);
                _logger.Debug(str);
                return true;
            }
            else
            {
                if (showDialogPrompt)
                    ApiHawkeyeMsgHelper.ErrorCommon(deleteCellResult);
            }

            return false;
        }

        #endregion

        #region CreateQualityControl

        public bool QualityControlValidation(QualityControlDomain qualityControl, bool showDialogPrompt, out string failureReason, string username = "", string password = "")
        {
            failureReason = "";
            if (string.IsNullOrWhiteSpace(qualityControl.QcName))
            {
                ApiHawkeyeMsgHelper.CustomDialogMessage(LanguageResourceHelper.Get("LID_ERRMSGBOX_QCNameBlank"), showDialogPrompt);
                failureReason = LanguageResourceHelper.Get("LID_ERRMSGBOX_QCNameBlank");
                _logger.Debug(failureReason);
                return false;
            }

            /* This check is only needed for OpcUa calls.  The UI doesn't allow the user to enter certain characters
             * whereas a user can type in a bad value using OpcUa, therefore, no UI dialog messages are necessary
             */
            if (Misc.ContainsInvalidCharacter(qualityControl.QcName))
            {
                failureReason = "Quality Control name contains an invalid character";
                _logger.Debug(failureReason);
                return false;
            }
            if (qualityControl.QcName.Length > ApplicationConstants.CellTypeNameLimit)
            {
                // Can't get this error from UI since the user can't type more than the max number of chars so no need to display or have translated message.
                failureReason = $"Quality Control should not be more than {ApplicationConstants.CellTypeNameLimit} characters";
                _logger.Debug(failureReason);
                return false;
            }

            // Since a username and password were passed, it's likely an OPC request.
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var qcDomain = GetQualityControlDomain(username, password, qualityControl.QcName);

                if (qcDomain != null && HardwareManager.HardwareSettingsModel.InstrumentType != InstrumentType.CellHealth_ScienceModule)
                {
                //TODO: should a message be sent ??? failureReason = LanguageResourceHelper.Get()
                    // This QC name already exists... do not attempt to create it.
                    failureReason = $"Quality Control of this name already exists. You must choose a unique name.";
                    _logger.Debug(failureReason);
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(qualityControl.LotInformation))
            {
                ApiHawkeyeMsgHelper.CustomDialogMessage(LanguageResourceHelper.Get("LID_ERRMSGBOX_LotNumberBlank"), showDialogPrompt);

                failureReason = LanguageResourceHelper.Get("LID_ERRMSGBOX_LotNumberBlank");
                _logger.Debug(failureReason);
                return false;
            }
            if (qualityControl.LotInformation.Length > ApplicationConstants.QualityControlLotLimit)
            {
                // Can't get this error from UI since the user can't type more than the max number of chars so no need to display or have translated message.
                failureReason = $"Lot should not be more than {ApplicationConstants.QualityControlLotLimit}";
                _logger.Debug(failureReason);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(qualityControl.CommentText))
            {
                if (qualityControl.CommentText.Length > ApplicationConstants.QualityControlCommentLimit)
                {
                    // Can't get this error from UI since the user can't type more than the max number of chars so no need to display or have translated message.
                    failureReason = $"Comment should not be more than {ApplicationConstants.QualityControlCommentLimit} characters";
                    _logger.Debug(failureReason);
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(qualityControl.AssayParameter.ToString()))
            {
                ApiHawkeyeMsgHelper.CustomDialogMessage(LanguageResourceHelper.Get("LID_ERRMSGBOX_AssayValueBlank"), showDialogPrompt);
                failureReason = LanguageResourceHelper.Get("LID_ERRMSGBOX_AssayValueBlank");
                _logger.Debug(failureReason);
                return false;
            }

            if (!Enum.IsDefined(typeof(assay_parameter), qualityControl.AssayParameter))
            {
                // Can't get this error from UI since user selects this from a dropdown menu so no need to display or have translated message.
                failureReason = "QualityControl AssayParameter out of range";
                _logger.Debug(failureReason);
                return false;
            }

            if (qualityControl.AssayValue == null)
            {
                ApiHawkeyeMsgHelper.CustomDialogMessage(LanguageResourceHelper.Get("LID_ERRMSGBOX_AssayValueBlank"), showDialogPrompt);
                failureReason = LanguageResourceHelper.Get("LID_ERRMSGBOX_AssayValueBlank");
                _logger.Debug(failureReason);
                return false;
            }

            switch (qualityControl.AssayParameter)
            {
                case assay_parameter.ap_Size:
                    if (qualityControl.AssayValue < ApplicationConstants.LowerConcentrationAssayLimit ||
                        qualityControl.AssayValue > ApplicationConstants.UpperConcentrationAssayLimit)
                    {
                        var message = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_AssayRange"),
                            Misc.UpdateTrailingPoint(ApplicationConstants.LowerConcentrationAssayLimit, TrailingPoint.Two),
                            Misc.UpdateTrailingPoint(ApplicationConstants.UpperConcentrationAssayLimit, TrailingPoint.Two), "µm");

                        ApiHawkeyeMsgHelper.CustomDialogMessage(message, showDialogPrompt);
                        failureReason = message;
                        _logger.Debug(failureReason);
                        return false;
                    }
                    break;
                case assay_parameter.ap_PopulationPercentage:
                    if (qualityControl.AssayValue < ApplicationConstants.LowerViabilityAssayLimit ||
                        qualityControl.AssayValue > ApplicationConstants.UpperViabilityAssayLimit)
                    {
                        var message = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_AssayRange"),
                            Misc.UpdateTrailingPoint(ApplicationConstants.LowerViabilityAssayLimit, TrailingPoint.Two),
                            Misc.UpdateTrailingPoint(ApplicationConstants.UpperViabilityAssayLimit, TrailingPoint.Two), "%");

                        ApiHawkeyeMsgHelper.CustomDialogMessage(message, showDialogPrompt);
                        failureReason = message;
                        _logger.Debug(failureReason);
                        return false;
                    }
                    break;
                case assay_parameter.ap_Concentration:
                    if (qualityControl.AssayValue < ApplicationConstants.LowerConcentrationAssayValueLimit ||
                        qualityControl.AssayValue > ApplicationConstants.UpperConcentrationAssayValueLimit)
                    {
                        var message = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_AssayRange"),
                            Misc.UpdateTrailingPoint(ApplicationConstants.LowerConcentrationAssayValueLimit, TrailingPoint.Two),
                            Misc.UpdateTrailingPoint(ApplicationConstants.UpperConcentrationAssayValueLimit, TrailingPoint.Two), "%");

                        ApiHawkeyeMsgHelper.CustomDialogMessage(message, showDialogPrompt);
                        failureReason = message;
                        _logger.Debug(failureReason);
                        return false;
                    }
                    break;
            }

            if (qualityControl.AcceptanceLimit == null)
            {
                ApiHawkeyeMsgHelper.CustomDialogMessage(LanguageResourceHelper.Get("LID_ERRMSGBOX_AcceptancelimitsBlank"), showDialogPrompt);
                failureReason = LanguageResourceHelper.Get("LID_ERRMSGBOX_AcceptancelimitsBlank");
                _logger.Debug(failureReason);
                return false;
            }

            if (qualityControl.AcceptanceLimit < ApplicationConstants.MinimumQualityControlAcceptanceLimit ||
                qualityControl.AcceptanceLimit > ApplicationConstants.MaximumQualityControlAcceptanceLimit)
            {
                var message = string.Format(LanguageResourceHelper.Get("LID_QCHeader_AcceptanceLimitsRange"),
                    Misc.ConvertToString(ApplicationConstants.MinimumQualityControlAcceptanceLimit),
                    Misc.ConvertToString(ApplicationConstants.MaximumQualityControlAcceptanceLimit));

                ApiHawkeyeMsgHelper.CustomDialogMessage(message, showDialogPrompt);
                failureReason = message;
                _logger.Debug(failureReason);
                return false;
            }
            if (!qualityControl.NotExpired)
            {
                failureReason = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_Calibration_ExpirationDate"));
                ApiHawkeyeMsgHelper.CustomDialogMessage(failureReason, showDialogPrompt);
                _logger.Debug(failureReason);
                return false;
            }

            return true;
        }

        public bool QualityControlValidation(QualityControlDomain qualityControl, bool showDialogPrompt)
        {
            return QualityControlValidation(qualityControl, showDialogPrompt, out var _);
        }

        public bool CreateQualityControl(string username, string password, QualityControlDomain qualityControl, bool showDialogPrompt)
        {
	        string retiredQCName = "";
	        List<QualityControlDomain> qcList = new List<QualityControlDomain>();

			CellTypeFacade.Instance.GetAllQcs(ref qcList);

			var dupQC = qcList.FirstOrDefault(q => q.QcName.Equals(qualityControl.QcName));
			if (dupQC != null)
			{ // Setup to rename existing QC and create a new QC (Backend will mark existing QC as retired).
				retiredQCName = qualityControl.QcName + " (" + Misc.DateFormatConverter(DateTime.Now, "LongDate") + ")";
			}

			var addResult = CellTypeFacade.Instance.AddQc(username, password, qualityControl, retiredQCName);
            if (addResult.Equals(HawkeyeError.eSuccess))
            {
                if (showDialogPrompt)
                    ApiHawkeyeMsgHelper.PublishHubMessage(LanguageResourceHelper.Get("LID_StatusBar_QChasBeenAdded"), MessageType.Normal);
                _logger.Debug(LanguageResourceHelper.Get("LID_MSGBOX_QCAddedSuccess"));
                return true;
            }
            else
            {
                if (showDialogPrompt)
                    ApiHawkeyeMsgHelper.ErrorCreateQualityControl(addResult);
                return false;
            }
        }
        #endregion

        public CellTypeDomain GetCellTypeDomain(string username, string password, string cellTypeName)
        {
            List<CellTypeDomain> ctList = new List<CellTypeDomain>();
            List<QualityControlDomain> qcList = new List<QualityControlDomain>();
            List<CellTypeQualityControlGroupDomain> bpqcGroupList = new List<CellTypeQualityControlGroupDomain>();
            CellTypeFacade.Instance.GetAllowedCtQc(username, ref ctList, ref qcList, ref bpqcGroupList);
            var ct = ctList.FirstOrDefault(c => c.CellTypeName.Equals(cellTypeName));
            return ct;
        }

        public QualityControlDomain GetQualityControlDomain(string username, string password, string qcName)
        {
            List<CellTypeDomain> ctList = new List<CellTypeDomain>();
            List<QualityControlDomain> qcList = new List<QualityControlDomain>();
            List<CellTypeQualityControlGroupDomain> bpqcGroupList = new List<CellTypeQualityControlGroupDomain>();
            CellTypeFacade.Instance.GetAllowedCtQc(username, ref ctList, ref qcList, ref bpqcGroupList);
            var qc = qcList.FirstOrDefault(c => c.QcName.Equals(qcName));
            return qc;
        }

        public List<CellTypeDomain> GetAllowedCellTypes(string username)
        {
            var cellList = new List<CellTypeDomain>();
            try
            {
                cellList = CellTypeFacade.Instance.GetAllowedCellTypes_BECall(username);
                PublishCellTypesRetrieved(cellList);
            }
            catch (Exception)
            {
                PublishCellTypesRetrieved(null);
                return null;
            }
            return cellList;
        }

        /// <summary>
        /// Returns the list of cell types in the cache. This does not call the backend.
        /// </summary>
        /// <returns></returns>
        public List<CellTypeDomain> GetAllCellTypes()
        {
            var allCellTypes = CellTypeFacade.Instance.GetAllCellTypes_BECall();
            return allCellTypes;
        }

        public CellTypeDomain GetCellType(uint cellTypeIndex)
        {
            try
            {
                var cellType = CellTypeFacade.Instance.GetCellTypeCopy_BECall(cellTypeIndex);
                return cellType;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to get cell type with index '{cellTypeIndex}'");
                return null;
            }
        }

        public List<QualityControlDomain> GetAllowedQualityControls(string username,  List<CellTypeDomain> allCells)
        {
            var qualityControlList = new List<QualityControlDomain>();
            try
            {
                qualityControlList = CellTypeFacade.Instance.GetAllowedQualityControls_BECall(username, allCells);
                PublishQualityControlsRetrieved(qualityControlList);
            }
            catch (Exception)
            {
                PublishQualityControlsRetrieved(null);
                return null;
            }

            return qualityControlList;
        }

        public void PublishCellTypesRetrieved(List<CellTypeDomain> cellTypes)
        {
            if (cellTypes == null)
                return;

            try
            {
                _cellTypesRetrievedSubject.OnNext(cellTypes);
            }
            catch (Exception e)
            {
                _cellTypesRetrievedSubject.OnError(e);
            }
        }

        public void PublishQualityControlsRetrieved(List<QualityControlDomain> qualityControls)
        {
            if (qualityControls == null)
                return;

            try
            {
                _qualityControlsRetrievedSubject.OnNext(qualityControls);
            }
            catch (Exception e)
            {
                _qualityControlsRetrievedSubject.OnError(e);
            }
        }
    }
}
