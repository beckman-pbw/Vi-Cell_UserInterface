using ApiProxies;
using ApiProxies.Commands.Service;
using ApiProxies.Misc;
using JetBrains.Annotations;
using log4net;
using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;

namespace ScoutModels.Service
{
    public class DustReferenceModel : Disposable
    {
        private SampleImageRecordDomain _finalImage;
        private IList<SampleImageRecordDomain> _imageList;
        private List<ProgressDomain> _progressList;
        private bool _dustSubscribed;

        public DustReferenceModel()
        {
        }

        protected override void DisposeUnmanaged()
        {
            UnsubscribeFromApiCallback();
            base.DisposeUnmanaged();
        }

        public IList<SampleImageRecordDomain> ImageList
        {
            get { return _imageList ?? (_imageList = new List<SampleImageRecordDomain>()); }
            set { _imageList = value; }
        }

        public List<ProgressDomain> ProgressList
        {
            get { return _progressList ?? (_progressList = new List<ProgressDomain>(GetDustRefStepsList())); }
            set { _progressList = value; }
        }

        public SampleImageRecordDomain FinalImage
        {
            get { return _finalImage ?? (_finalImage = new SampleImageRecordDomain()); }
            set { _finalImage = value; }
        }

      
        private List<ProgressDomain> GetDustRefStepsList()
        {
            var dustRefList = new List<ProgressDomain>();
            dustRefList.Add(GetStatus(false, ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_AspiratingBuffer"), true));
            dustRefList.Add(GetStatus(false, ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_DispensingBuffer"), false));
            dustRefList.Add(GetStatus(false, ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_Acquiring"), false));
            dustRefList.Add(GetStatus(false, ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_AcceptNewRef"), false));
            return dustRefList;
        }

     
        private ProgressDomain GetStatus(bool isProgressComplete, string Label, bool isResponseAvailable)
        {
            var progressData = new ProgressDomain()
            {
                IsProgressComplete = isProgressComplete,
                ProgressLabel = Label,
                IsResponseAvailable = isResponseAvailable
            };
            return progressData;
        }

        [MustUseReturnValue("Use HawkeyeError")] public HawkeyeError StartBrightFieldDustSubtract(string dustImageNamePrefix)
        {
            try
            {
                SubscribeToApiCallback();

                var apiCommand = new StartBrightfieldDustSubtract(dustImageNamePrefix, FileSystem.DefaultImageDirectory);
                var result = apiCommand.Invoke();
                if (result != HawkeyeError.eSuccess)
                {
                    UnsubscribeFromApiCallback();
                }

                return result;
            }
            catch (Exception)
            {
                UnsubscribeFromApiCallback();
                throw;
            }
        }

        public void SubscribeToApiCallback()
        {
            ApiEventBroker.Instance.Subscribe<eBrightfieldDustSubtractionState, ImageDto, List<ImageDto>>(
                ApiEventType.Brightfield_Dust_Subtraction, HandleBrightfieldDustSubtractionCallback);
            _dustSubscribed = true;
        }

        public void UnsubscribeFromApiCallback()
        {
            if (_dustSubscribed)
            {
                ApiEventBroker.Instance.Unsubscribe<eBrightfieldDustSubtractionState, ImageDto, List<ImageDto>>(
                    ApiEventType.Brightfield_Dust_Subtraction, HandleBrightfieldDustSubtractionCallback);
                _dustSubscribed = false;
            }
        }

        public event EventHandler<ApiEventArgs<eBrightfieldDustSubtractionState, ImageDto, List<ImageDto>>> BrightfieldDustSubtractionOccurred;

        private void HandleBrightfieldDustSubtractionCallback(object sender, ApiEventArgs<eBrightfieldDustSubtractionState, ImageDto, List<ImageDto>> args)
        {
            BrightfieldDustSubtractionOccurred?.Invoke(this, args);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError CancelBrightfieldDustSubtract()
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.CancelBrightfieldDustSubtractAPI();
            Log.Debug("CancelBrightfieldDustSubtract:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }


        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError AcceptDustReference(bool accepted)
        {
            Log.Debug("AcceptDustReference:: accepted: " + accepted);
            var hawkeyeError = HawkeyeCoreAPI.Service.AcceptDustReferenceAPI(accepted);
            Log.Debug("AcceptDustReference:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }
    }
}