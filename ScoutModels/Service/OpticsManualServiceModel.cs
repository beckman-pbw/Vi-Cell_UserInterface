using ApiProxies;
using ApiProxies.Commands.Service;
using ApiProxies.Misc;
using JetBrains.Annotations;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.DataTransferObjects;
using ScoutModels.Common;
using ScoutModels.Review;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;

namespace ScoutModels.Service
{
    public class OpticsManualServiceModel : BaseDisposableNotifyPropertyChanged
    {
        #region Constructor
        
        public OpticsManualServiceModel()
        {
            DustRef = new DustReferenceModel();
            SetFocus = new SetFocusModel();
        }

        #endregion

        public event EventHandler<ApiEventArgs<HawkeyeError, BasicResultAnswers, ImageDto>> ServiceResultAnalysisOccurred;

        public DustReferenceModel DustRef { get; set; }
        public SetFocusModel SetFocus { get; set; }

        private IList<KeyValuePair<int, string>> _liveImageList;
        public IList<KeyValuePair<int, string>> LiveImageList
        {
            get
            {
                return _liveImageList ?? (_liveImageList = new List<KeyValuePair<int, string>>(LoadImageOptionList()));
            }
            set { _liveImageList = value; }
        }

        #region Public Method

        protected override void DisposeUnmanaged()
        {
            SubscribeToAnalysisResult(false, HandleContinuousAnalysisResult);
            DustRef?.Dispose();
            SetFocus?.Dispose();
            base.DisposeUnmanaged();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError svc_CameraFocusRestoreToSavedLocation()
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.CameraFocusRestoreToSavedLocationAPI();
            Log.Debug("svc_CameraFocusRestoreToSavedLocation:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError svc_ManualSampleLoad()
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.ManualSample_LoadAPI();
            Log.Debug("svc_ManualSampleLoad:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError svc_ManualSampleNudge()
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.ManualSample_NudgeAPI();
            Log.Debug("svc_ManualSampleNudge:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")] public HawkeyeError svc_ManualSampleExpel()
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.ManualSample_ExpelAPI();
            Log.Debug("svc_ManualSampleExpel:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")] public HawkeyeError svc_GetCameraLampState(out bool lamp_on, out float intensity_0_to_100)
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.GetCameraLampStateAPI(out lamp_on, out intensity_0_to_100);
            Log.Debug("svc_GetCameraLampState:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")] public HawkeyeError svc_SetCameraLampState(bool lamp_on, float intensity_0_to_100)
        {
            Log.Debug("svc_SetCameraLampState:: lamp_on: " + lamp_on + ", intensity: " + intensity_0_to_100);
            var hawkeyeError = HawkeyeCoreAPI.Service.SetCameraLampStateAPI(lamp_on, intensity_0_to_100);
            Log.Debug("svc_SetCameraLampState:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")] public HawkeyeError svc_SetCameraLampIntensity(float intensity_0_to_100)
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.SetCameraLampIntensityAPI(intensity_0_to_100);
            Log.Debug("- Output from Method:svc_SetCameraLampIntensity:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        public List<CellTypeDomain> svc_GetTemporaryCellType()
        {
            return CellTypeModel.svc_GetTemporaryCellType();
        }

        [MustUseReturnValue("Use HawkeyeError")] public HawkeyeError svc_SetTemporaryAnalysisDefinition(AnalysisDomain apDomain)
        {
            return CellTypeModel.svc_SetTemporaryAnalysisDefinition(apDomain);
        }

        [MustUseReturnValue("Use HawkeyeError")] public HawkeyeError svc_SetTemporaryCellType(CellTypeDomain cellList)
        {
            return CellTypeModel.svc_SetTemporaryCellType(cellList);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError svc_SetTemporaryCellTypeFromExisting(UInt32 cellIndex)
        {
            var hawkeyeError = HawkeyeCoreAPI.CellType.SetTemporaryCellTypeFromExistingAPI(cellIndex);
            Log.Debug("svc_SetTemporaryCellTypeFromExisting:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")] public HawkeyeError svc_GenerateSingleShotAnalysis()
        {
            try
            {
                // Subscribe to single-shot analysis event.  The matching unsubscription is in the event handler.
                SubscribeToAnalysisResult(true, HandleSingleAnalysisResult);

                var result = new GenerateSingleShotAnalysis().Invoke();
                if (result != HawkeyeError.eSuccess)
                {
                    SubscribeToAnalysisResult(false, HandleSingleAnalysisResult);
                }

                return result;
            }
            catch (Exception)
            {
                SubscribeToAnalysisResult(false, HandleSingleAnalysisResult);
                throw;
            }
        }

        [MustUseReturnValue("Use HawkeyeError")] public HawkeyeError svc_GenerateContinuousAnalysis()
        {
            try
            {
                // Subscribe to continuous analysis event.  The matching unsubscription is in the StopContinuousAnalysis method.
                SubscribeToAnalysisResult(true, HandleContinuousAnalysisResult);

                var result = new GenerateContinuousAnalysis().Invoke();
                if (result != HawkeyeError.eSuccess)
                {
                    SubscribeToAnalysisResult(false, HandleContinuousAnalysisResult);
                }

                return result;
            }
            catch (Exception)
            {
                SubscribeToAnalysisResult(false, HandleContinuousAnalysisResult);
                throw;
            }
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError svc_StopContinuousAnalysis()
        {
            SubscribeToAnalysisResult(false, HandleContinuousAnalysisResult);
            return new StopContinuousAnalysis().Invoke();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError svc_CameraFocusAdjust(bool direction_up, bool adjustment_fine)
        {
            Log.Debug("svc_CameraFocusAdjust:: Direction up: " + direction_up + ", adjustment fine: " + adjustment_fine);
            var hawkeyeError = HawkeyeCoreAPI.Service.CameraFocusAdjustAPI(direction_up, adjustment_fine);
            Log.Debug("svc_CameraFocusAdjust:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        public double svc_GetFlowCellDepthSettingInMillimeters()
        {
            ushort flow_cell_depth = 0;
            var hawkeyeError = HawkeyeCoreAPI.Service.GetFlowCellDepthSettingAPI(out flow_cell_depth);
            Log.Debug("svc_GetFlowCellDepthSetting:: hawkeyeError: " + hawkeyeError + ", flow_cell_depth" + flow_cell_depth);

            return (double)flow_cell_depth / 1000; 
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError svc_SetFlowCellDepthSetting(double flow_cell_depth)
        {
            flow_cell_depth = flow_cell_depth * 1000;
            flow_cell_depth = Math.Round(flow_cell_depth, MidpointRounding.AwayFromZero);
            Log.Debug("svc_SetFlowCellDepthSetting:: flow_cell_depth: " + flow_cell_depth);
            var hawkeyeError = HawkeyeCoreAPI.Service.SetFlowCellDepthSettingAPI((UInt16)flow_cell_depth);
            Log.Debug("svc_SetFlowCellDepthSetting:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        #endregion

        private void HandleSingleAnalysisResult(object sender, ApiEventArgs<HawkeyeError, BasicResultAnswers, ImageDto> args)
        {
            // Single-shot should only happen once, so unsubscribe on first callback
            SubscribeToAnalysisResult(false, HandleSingleAnalysisResult);
            HandleContinuousAnalysisResult(sender, args);
        }

        private void HandleContinuousAnalysisResult(object sender, ApiEventArgs<HawkeyeError, BasicResultAnswers, ImageDto> args)
        {
            ServiceResultAnalysisOccurred?.Invoke(this, args);
        }

        private void SubscribeToAnalysisResult(bool subscribe, EventHandler<ApiEventArgs<HawkeyeError, BasicResultAnswers, ImageDto>> handler)
        {
            if (subscribe)
            {
                ApiEventBroker.Instance.Subscribe<HawkeyeError, BasicResultAnswers, ImageDto>(
                    ApiEventType.Service_Analysis_Result, handler);
            }
            else
            {
                ApiEventBroker.Instance.Unsubscribe<HawkeyeError, BasicResultAnswers, ImageDto>(
                    ApiEventType.Service_Analysis_Result, handler);
            }
        }

        private List<KeyValuePair<int, string>> LoadImageOptionList()
        {
            var imgOption = new List<KeyValuePair<int, string>>();
            imgOption.Add(new KeyValuePair<int, string>(1, ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_Brightfield")));
            return imgOption;
        }

    }
}
