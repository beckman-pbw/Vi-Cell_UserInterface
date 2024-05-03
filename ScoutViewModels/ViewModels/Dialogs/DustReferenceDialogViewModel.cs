using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutModels.Service;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.UIConfiguration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ScoutModels;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public enum DustImageType
    {
        FinalImage,
        ImageList
    }

    public class DustReferenceDialogViewModel : BaseDialogViewModel
    {
        public DustReferenceDialogViewModel(DustReferenceEventArgs args, Window parentWindow) : base(args, parentWindow)
        {
            ShowDialogTitleBar = true;
            DialogTitle = LanguageResourceHelper.Get("LID_POPUPHeader_DustReference");

            ImageIndexList = new ObservableCollection<KeyValuePair<int, string>>();
            SampleImageDomain = new SampleImageRecordDomain();

            _dustReferenceModel = new DustReferenceModel();
            _dustReferenceModel.BrightfieldDustSubtractionOccurred += HandleBrightFieldDustSubtractionOccurred;

            _dustReferenceTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, UISettings.DustReffTimeOutSec) };
            _dustReferenceTimer.Tick += OnDustReferenceTick;

            ShowSlideShowButtons = false;
            IsStepOneEnable = true;
            ImageViewType = ApplicationConstants.ImageViewRightClickMenuImageFitSize;
            FinalImageViewType = ApplicationConstants.ImageViewRightClickMenuImageFitSize;
            ImageList = new ObservableCollection<SampleImageRecordDomain>(_dustReferenceModel.ImageList);

        }

        protected override void DisposeManaged()
        {
            _dustReferenceTimer?.Stop();
            _dustReferenceTimer.Tick -= OnDustReferenceTick;
            _dustReferenceModel.BrightfieldDustSubtractionOccurred -= HandleBrightFieldDustSubtractionOccurred;
            _dustReferenceModel?.Dispose();
            base.DisposeManaged();
        }

        #region Properites & Fields

        private DustReferenceModel _dustReferenceModel;
        private CallBackProgressStatus _progressStatus;
        private DispatcherTimer _dustReferenceTimer;
        private CancellationTokenSource _cancelTokenSource;
        private bool _enableDustSubtractListener;
        private bool _isCancelRequestAccepted;

        private const string IMG_LIST = "ImgList";

        public ObservableCollection<ProgressDomain> ProgressList => new ObservableCollection<ProgressDomain>(_dustReferenceModel.ProgressList);

        public bool ShowSlideShowButtons
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsStepOneEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsStepTwoEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsStepThreeEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsFinalImgEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsImgActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsImageActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsAcceptEnable
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
                DeclineCommand.RaiseCanExecuteChanged();
            }
        }

        public string ImageViewType
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string FinalImageViewType
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<KeyValuePair<int, string>> ImageIndexList
        {
            get { return GetProperty<ObservableCollection<KeyValuePair<int, string>>>(); }
            set { SetProperty(value); }
        }

        public KeyValuePair<int, string> SelectedImageIndex
        {
            get { return GetProperty<KeyValuePair<int, string>>(); }
            set
            {
                if (!GetProperty<KeyValuePair<int, string>>().Equals(value))
                {
                    SetProperty(value);
                    SetImage(value.Key - 1);
                }
            }
        }

        public AdjustValue AdjustState
        {
            get { return GetProperty<AdjustValue>(); }
            set
            {
                SetProperty(value);
                
                var index = Convert.ToInt32(SampleImageDomain.SequenceNumber);
                switch (value)
                {
                    case AdjustValue.Idle:
                        break;
                    case AdjustValue.Left:
                        if (index.Equals(1)) return;
                        SetImage(index - 2);
                        break;
                    case AdjustValue.Right:
                        if (index.Equals(ImageList.Count)) return;
                        SetImage(index);
                        break;
                }
            }
        }

        public SampleImageRecordDomain FinalImage
        {
            get { return _dustReferenceModel.FinalImage; }
            set
            {
                _dustReferenceModel.FinalImage = value;
                NotifyPropertyChanged(nameof(FinalImage));
            }
        }

        public ObservableCollection<SampleImageRecordDomain> ImageList
        {
            get { return GetProperty<ObservableCollection<SampleImageRecordDomain>>(); }
            set { SetProperty(value); }
        }

        public SampleImageRecordDomain SampleImageDomain
        {
            get { return GetProperty<SampleImageRecordDomain>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Event Handlers

        private void OnDustReferenceTick(object sender, EventArgs e)
        {
            _cancelTokenSource?.Cancel();
            Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_CancelDustError"));
            
            var result = DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_CancelDustError"));
            if (result != true) return;

            Close(true);
        }

        private void HandleBrightFieldDustSubtractionOccurred(object sender, ApiEventArgs<eBrightfieldDustSubtractionState, ImageDto, List<ImageDto>> args)
        {
            if (!_enableDustSubtractListener)
            {
                return;
            }

            Log.Info($"eBrightfieldDustSubtractionState : {args.Arg1}");
            DustReferenceFieldState(args.Arg1, args.Arg2, args.Arg3);
        }

        #endregion

        #region Commands

        #region Image Expand Commands

        private RelayCommand _imageExpandCommand;
        public RelayCommand ImageExpandCommand => _imageExpandCommand ?? (_imageExpandCommand = new RelayCommand(OnExpandImage));

        private void OnExpandImage()
        {
            OnExpandView(string.Empty);
        }

        private RelayCommand _finalImageExpandCommand;
        public RelayCommand FinalImageExpandCommand => _finalImageExpandCommand ?? (_finalImageExpandCommand = new RelayCommand(OnExpandFinalImage));

        private void OnExpandFinalImage()
        {
            OnExpandView(ApplicationConstants.FinalImageStr);
        }

        private void OnExpandView(string image)
        {
            if (image.Equals(ApplicationConstants.FinalImageStr))
            {
                var args = new ExpandedImageGraphEventArgs(ImageType.Raw, FinalImageViewType, 
                    new List<object> { FinalImage }, FinalImage, 0, false);
                args.IsHorizontalPaginationVisible = false;
                args.IsQueueManagementDialog = false;

                DialogEventBus.ExpandedImageGraph(this, args);
            }
            else
            {
                var imageList = ImageList?.Cast<object>()?.ToList();
                var index = imageList?.Contains(SampleImageDomain) == true ? imageList.IndexOf(SampleImageDomain) : -1;
                var args = new ExpandedImageGraphEventArgs(ImageType.Raw, ImageViewType, imageList, 
                    SampleImageDomain, index, false);
                args.IsHorizontalPaginationVisible = true;
                args.IsQueueManagementDialog = true;

                DialogEventBus.ExpandedImageGraph(this, args);
                ImageViewType = args.SelectedRightClickImageType;
                SampleImageDomain = ImageList?.ElementAt(args.SelectedImageIndex) ?? SampleImageDomain;
            }
        }

        #endregion

        #region Image Command (Switch to Image)

        private RelayCommand _imageCommand;
        public RelayCommand ImageCommand => _imageCommand ?? (_imageCommand = new RelayCommand(PerformImageSwitch));

        private void PerformImageSwitch(object parameter)
        {
            if (parameter is string str)
            {
                SwitchToImage(str.Equals(IMG_LIST) ? DustImageType.ImageList : DustImageType.FinalImage);
            }
        }

        #endregion

        #region Start Dust Reference Command

        private RelayCommand _startDusReferenceCommand;
        public RelayCommand StartDusReferenceCommand => _startDusReferenceCommand ?? (_startDusReferenceCommand = new RelayCommand(OnStartDustReference, null));

        private void OnStartDustReference()
        {
            try
            {
                _enableDustSubtractListener = true;
                // There's not need to save the final image. For dust reference image list we do need to save, so pass in file name
                var startDustStatus = _dustReferenceModel.StartBrightFieldDustSubtract(ApplicationConstants.ImageFileName);
                _progressStatus = CallBackProgressStatus.IsStart;
                if (startDustStatus.Equals(HawkeyeError.eSuccess))
                {
                    IsStepOneEnable = IsStepThreeEnable = false;
                    IsStepTwoEnable = true;
                    _progressStatus = CallBackProgressStatus.IsRunning;
                }
                else
                {
                    _enableDustSubtractListener = false;
                    ApiHawkeyeMsgHelper.ErrorCommon(startDustStatus);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_START_BRIGHT_FIELD_DUST_SUBTRACT"));
            }
        }

        #endregion

        #region Decline

        public override bool CanDecline()
        {
            return IsAcceptEnable;
        }

        protected override void OnDecline()
        {
            try
            {
                var setFocusAccept = _dustReferenceModel.AcceptDustReference(false);
                ApiHawkeyeMsgHelper.ErrorCommon(setFocusAccept);
                Close(false);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_DUST_REFERENCE_CANCEL"));
            }
        }

        #endregion

        #region Accept

        public override bool CanAccept()
        {
            return IsAcceptEnable;
        }

        protected override void OnAccept()
        {
            try
            {
                var setFocusAccept = _dustReferenceModel.AcceptDustReference(true);
                ApiHawkeyeMsgHelper.ErrorCommon(setFocusAccept);
                Close(true);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_DUST_REFERENCE_ACCEPT"));
            }
        }

        #endregion

        #region Cancel/Close

        protected override void OnCancel()
        {
            if (_progressStatus.Equals(CallBackProgressStatus.IsRunning))
            {
                if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_DustRefWarning")) == true)
                {
                    var cancelResult = _dustReferenceModel.CancelBrightfieldDustSubtract();
                    if (!cancelResult.Equals(HawkeyeError.eSuccess))
                    {
                        ApiHawkeyeMsgHelper.ErrorCommon(cancelResult);
                    }
                    else
                    {
                        _isCancelRequestAccepted = true;
                    }

                    Close(null);
                }
                else
                {
                    // user does NOT want to cancel
                    return;
                }
            }

            Close(null);
        }

        public override bool Close(bool? result)
        {
            _dustReferenceModel.UnsubscribeFromApiCallback();
            return base.Close(result);
        }

        #endregion

        #endregion

        #region Private Methods

        private void DustReferenceFieldState(eBrightfieldDustSubtractionState status, ImageDto dustRefImg, List<ImageDto> sourceDustImgs)
        {
            switch (status)
            {
                case eBrightfieldDustSubtractionState.bds_Idle:
                    if (_progressStatus.Equals(CallBackProgressStatus.IsCanceling) || _isCancelRequestAccepted)
                    {
                        _progressStatus = CallBackProgressStatus.IsCanceled;
                        _dustReferenceTimer?.Stop();
                        _cancelTokenSource?.Cancel();
                        Close(true);
                    }
                    break;
                case eBrightfieldDustSubtractionState.bds_AspiratingBuffer:
                    break;
                case eBrightfieldDustSubtractionState.bds_DispensingBufferToFlowCell:
                    _dustReferenceModel.ProgressList[0].IsProgressComplete = true;
                    _dustReferenceModel.ProgressList[1].IsResponseAvailable = true;
                    NotifyPropertyChanged(nameof(ProgressList));
                    break;
                case eBrightfieldDustSubtractionState.bds_AcquiringImages:
                    _dustReferenceModel.ProgressList[1].IsProgressComplete = true;
                    _dustReferenceModel.ProgressList[2].IsResponseAvailable = true;
                    NotifyPropertyChanged(nameof(ProgressList));
                    break;
                case eBrightfieldDustSubtractionState.bds_ProcessingImages:
                    _dustReferenceModel.ProgressList[2].IsProgressComplete = true;
                    _dustReferenceModel.ProgressList[3].IsResponseAvailable = true;
                    NotifyPropertyChanged(nameof(ProgressList));
                    break;
                case eBrightfieldDustSubtractionState.bds_WaitingOnUserApproval:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        IsImageActive = true;
                        SwitchToImage(DustImageType.FinalImage);
                        if (dustRefImg.Rows != 0)
                        {
                            FinalImage = new SampleImageRecordDomain
                            {
                                ImageSet = new ImageSetDto { BrightfieldImage = dustRefImg },
                                SequenceNumber = 1,
                                TotalCumulativeImage = 1
                            };
                        }

                        SwitchToImage(DustImageType.ImageList);
                        if (sourceDustImgs != null && sourceDustImgs.Any())
                        {
                            UpdateImageList(sourceDustImgs);
                        }
                    });
                    break;
                case eBrightfieldDustSubtractionState.bds_SettingReferenceImage:
                    break;
                case eBrightfieldDustSubtractionState.bds_Cancelling:
                    _progressStatus = CallBackProgressStatus.IsCanceling;
                    _dustReferenceTimer.Start();
                    StartLoadingIndicator();
                    break;
                case eBrightfieldDustSubtractionState.bds_Completed:
                    if (_progressStatus.Equals(CallBackProgressStatus.IsCanceling) || _isCancelRequestAccepted)
                    {
                        _progressStatus = CallBackProgressStatus.IsCanceled;
                        _dustReferenceTimer?.Stop();
                        _cancelTokenSource?.Cancel();
                        Close(true);
                        return;
                    }
                    _progressStatus = CallBackProgressStatus.IsFinish;
                    IsStepOneEnable = IsStepTwoEnable = false;
                    IsStepThreeEnable = true;
                    break;
                case eBrightfieldDustSubtractionState.bds_Failed:
                    _progressStatus = CallBackProgressStatus.IsError;
                    _dustReferenceTimer?.Stop();
                    _cancelTokenSource?.Cancel();
                    OnFaultError();
                    break;
            }

            var msg = new Notification<ReagentContainerStateDomain>(
                ReagentModel.GetReagentContainerStatusAll().FirstOrDefault(),
                MessageToken.RefreshReagentStatus, "");
            MessageBus.Default.Publish(msg);
        }

        private void StartLoadingIndicator()
        {
            var msg = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_DustReffCancelMsg"), ScoutUtilities.Misc.ConvertToString(ApplicationConstants.DustRefTimer));
            _cancelTokenSource = new CancellationTokenSource();
            DialogEventBus.LargeLoadingScreen(this, new LargeLoadingScreenEventArgs(msg, null, true, _cancelTokenSource.Token));
        }

        private void OnFaultError()
        {
            if (DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_FailedDustReference")) != true)
            {
                return;
            }

            Close(true);
        }

        private void SwitchToImage(DustImageType type)
        {
            switch (type)
            {
                case DustImageType.FinalImage:
                    IsFinalImgEnable = true;
                    IsImgActive = false;
                    break;
                case DustImageType.ImageList:
                    IsFinalImgEnable = false;
                    IsImgActive = true;
                    break;
            }
        }

        private void UpdateImageList(List<ImageDto> images)
        {
            if (images == null || !images.Any())
            {
                return;
            }

            var imageList = new List<SampleImageRecordDomain>();
            var imageIndexList = new List<KeyValuePair<int, string>>();

            for (var i = 0; i < images.Count; i++)
            {
                var sampleImage = new SampleImageRecordDomain
                {
					ImageSet = StructMarshallingExts.MarshalToImageSetDto(images[i]),
                    SequenceNumber = (uint)i + 1,
                    TotalCumulativeImage = images.Count,
                    ImageID = 1
                };

                imageList.Add(sampleImage);
                imageIndexList.Add(new KeyValuePair<int, string>(i + 1, ScoutUtilities.Misc.ConvertToString(i + 1)));
            }

            ImageIndexList = new ObservableCollection<KeyValuePair<int, string>>(imageIndexList);
            ImageList = new ObservableCollection<SampleImageRecordDomain>(imageList);
            SelectedImageIndex = ImageIndexList.FirstOrDefault(kvp => kvp.Key == 1);
            SampleImageDomain = ImageList.FirstOrDefault(i => i.SequenceNumber == 1); // setting SelectedImageIndex  will set SampleImageDomain

            IsAcceptEnable = true;
        }

        private void SetImage(int selectedImageIndex)
        {
            if (ImageList.Count > selectedImageIndex)
            {
                var sampleRecord = ImageList[selectedImageIndex];
                SampleImageDomain = sampleRecord;
            }
        }

        #endregion
    }
}
