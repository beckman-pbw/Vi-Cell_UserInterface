using ApiProxies;
using ApiProxies.Commands.Service;
using ApiProxies.Misc;
using JetBrains.Annotations;
using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutModels.Common;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScoutModels.Service
{
    public class SetFocusModel : BaseDisposableNotifyPropertyChanged
    {
        #region Constructor

        public SetFocusModel()
        {
            GraphHelper = new GraphHelper();
            SetFocusProperties = new SetFocusDomain();
        }

        protected override void DisposeUnmanaged()
        {
            UnsubscribeFromLiveImages();
            UnsubscribeFromAutoFocusStateChanges();
            UnsubscribeFromAutoFocusTimerTicks();
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        public event EventHandler<ApiEventArgs<eAutofocusState, AutofocusResultsDto>> AutoFocusStateChanged;
        public event EventHandler<ApiEventArgs<uint>> AutoFocusTimerTick;
        public event EventHandler<ApiEventArgs<HawkeyeError, ImageDto, List<LiveImageDataDomain>>> ServiceLiveImageOccurred;

        private bool _liveImageSubscribed;
        private bool _autoFocusStateSubscribed;
        private bool _autoFocusTimerSubscribed;

        public GraphHelper GraphHelper { get; set; }

        public SetFocusDomain SetFocusProperties
        {
            get { return GetProperty<SetFocusDomain>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Event Handlers

        private void HandleServiceLiveImageCallback(object sender, ApiEventArgs<HawkeyeError, ImageDto, List<LiveImageDataDomain>> args)
        {
            ServiceLiveImageOccurred?.Invoke(this, args);
        }

        private void HandleAutofocusCountdownTimerTick(object sender, ApiEventArgs<uint> e)
        {
            AutoFocusTimerTick?.Invoke(this, e);
        }

        private void HandleAutofocusStateChanged(object sender, ApiEventArgs<eAutofocusState, AutofocusResultsDto> e)
        {
            Log.Debug($"SetFocusModel::HandleAutofocusStateChanged:: '{e.Arg1}'");
            AutoFocusStateChanged?.Invoke(this, e);
        }

        #endregion

        #region Public Methods

        public void LogFocusResults(AutofocusResultsDto autoFocusResults, List<AutofocusDatapoint> dataPointList)
        {
            if (autoFocusResults == null || dataPointList == null)
                return;
            Log.Debug("SetFocusModel::LogFocusResults:: nFocusDatapoints: " + autoFocusResults.NumFocusDatapoints +
                      ", bestfocus_af_position: " + autoFocusResults.BestAutofocusPosition +
                      ", offset_from_bestfocus_um: " + autoFocusResults.BestFocusOffsetMicrons +
                      ", final_af_position: " + autoFocusResults.FinalAutofocusPosition +
                      ", focus_successful: " + autoFocusResults.IsFocusSuccessful);

            Log.Debug("Autofocus Datapoints::");
            StringBuilder s = new StringBuilder();
            dataPointList.ForEach(data => { s.Append(data.position + ","); });
            Log.Info("Position: " + s);
            s.Clear();
            dataPointList.ForEach(data => { s.Append(data.focalvalue + ","); });
            Log.Info("Focalvalue: " + s);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError StartLiveImageFeed()
        {
            try
            {
                // Subscribe to the Live Image API event
                SubscribeToLiveImages();

                var result = new StartLiveImageFeed().Invoke();
                if (result != HawkeyeError.eSuccess)
                {
                    UnsubscribeFromLiveImages();
                }

                return result;
            }
            catch (Exception)
            {
                UnsubscribeFromLiveImages();
                throw;
            }
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError StopLiveImageFeed()
        {
            UnsubscribeFromLiveImages();
            return new StopLiveImageFeed().Invoke();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError svc_CameraAutoFocus_ServiceSkipDelay()
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.CameraAutoFocus_ServiceSkipDelayAPI();
            Log.Debug("svc_CameraAutoFocus_ServiceSkipDelay:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError svc_CameraAutoFocus_FocusAcceptance(eAutofocusCompletion decision)
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.CameraAutoFocus_FocusAcceptanceAPI(decision);
            Log.Debug("svc_CameraAutoFocus_FocusAcceptance:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError svc_CameraAutoFocus_Cancel()
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.CameraAutoFocus_CancelAPI();
            Log.Debug("svc_CameraAutoFocus_Cancel:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError svc_CameraAutoFocus(SamplePosition samplePosition)
        {
            try
            {
                SubscribeToAutoFocusStateChanges();
                SubscribeToAutoFocusTimerTicks();

                var apiCommand = new CameraAutoFocus(samplePosition);
                var result = apiCommand.Invoke();
                if (result != HawkeyeError.eSuccess)
                {
                    UnsubscribeFromAutoFocusStateChanges();
                    UnsubscribeFromAutoFocusTimerTicks();
                }

                return result;
            }
            catch (Exception)
            {
                UnsubscribeFromAutoFocusStateChanges();
                UnsubscribeFromAutoFocusTimerTicks();
                throw;
            }
        }

        #endregion

        #region Private Methods

        private void SubscribeToLiveImages()
        {
            if (!_liveImageSubscribed)
            {
                ApiEventBroker.Instance.Subscribe<HawkeyeError, ImageDto, List<LiveImageDataDomain>>(ApiEventType.Service_Live_Image, HandleServiceLiveImageCallback);
                _liveImageSubscribed = true;
            }
        }

        private void UnsubscribeFromLiveImages()
        {
            if (_liveImageSubscribed)
            {
                ApiEventBroker.Instance.Unsubscribe<HawkeyeError, ImageDto, List<LiveImageDataDomain>>(ApiEventType.Service_Live_Image, HandleServiceLiveImageCallback);
                _liveImageSubscribed = false;
            }
        }

        private void SubscribeToAutoFocusStateChanges()
        {
            if (!_autoFocusStateSubscribed)
            {
                // Subscribe to the autofocus state change event
                ApiEventBroker.Instance.Subscribe<eAutofocusState, AutofocusResultsDto>(ApiEventType.Autofocus_State, HandleAutofocusStateChanged);
                _autoFocusStateSubscribed = true;
                Log.Debug($"SetFocusModel::SubscribeToAutoFocusStateChanges() - Complete");
            }
        }

        private void UnsubscribeFromAutoFocusStateChanges()
        {
            if (_autoFocusStateSubscribed)
            {
                ApiEventBroker.Instance.Unsubscribe<eAutofocusState, AutofocusResultsDto>(ApiEventType.Autofocus_State, HandleAutofocusStateChanged);
                _autoFocusStateSubscribed = false;
                Log.Debug($"SetFocusModel::UnsubscribeFromAutoFocusStateChanges() - Complete");
            }
        }

        private void SubscribeToAutoFocusTimerTicks()
        {
            if (!_autoFocusTimerSubscribed)
            {
                // Subscribe to the autofocus countdown timer tick event
                ApiEventBroker.Instance.Subscribe<UInt32>(ApiEventType.Autofocus_Countdown_Timer, HandleAutofocusCountdownTimerTick);
                _autoFocusTimerSubscribed = true;
            }
        }

        private void UnsubscribeFromAutoFocusTimerTicks()
        {
            if (_autoFocusTimerSubscribed)
            {
                ApiEventBroker.Instance.Unsubscribe<UInt32>(ApiEventType.Autofocus_Countdown_Timer, HandleAutofocusCountdownTimerTick);
                _autoFocusTimerSubscribed = false;
            }
        }

        #endregion
    }
}
