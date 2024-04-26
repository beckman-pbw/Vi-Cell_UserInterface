using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutDomains.RunResult;
using ScoutModels.Review;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Structs;
using ScoutViewModels.Common;
using System;
using System.Linq;

namespace ScoutViewModels.ViewModels.Service.ConcentrationSlope.DataTabs
{
    public class AcupImagesTabViewModel : BaseViewModel, IHandlesCalibrationState, IHandlesImageReceived
    {
        public AcupImagesTabViewModel(ICellTypeManager cellTypeManager, IRunSampleHelper runSampleHelper)
        {
            _cellTypeManager = cellTypeManager;
            _runSampleHelper = runSampleHelper;
            IsSingleton = true;
            HandleNewCalibrationState(CalibrationGuiState.NotStarted);
        }

        #region Properties & Fields

        private readonly ICellTypeManager _cellTypeManager;
        private readonly IRunSampleHelper _runSampleHelper;

        public bool ImagesAreAvailable
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                DispatcherHelper.ApplicationExecute(() => ImageExpandCommand.RaiseCanExecuteChanged());
            }
        }

        public bool IsPaginationButtonEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public SampleRecordDomain SelectedSampleRecord
        {
            get { return GetProperty<SampleRecordDomain>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Interface Methods
        
        public void HandleNewCalibrationState(CalibrationGuiState state)
        {
            switch(state)
            {
                case CalibrationGuiState.NotStarted:
                case CalibrationGuiState.CalibrationApplied:
                case CalibrationGuiState.CalibrationRejected:
                case CalibrationGuiState.Aborted:
                case CalibrationGuiState.Started:
                    ImagesAreAvailable = false;
                    IsPaginationButtonEnable = false;
                    SelectedSampleRecord = new SampleRecordDomain();
                    break;
                case CalibrationGuiState.Ended:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        public void HandleImageReceived(SampleEswDomain sample, ushort imageNum, BasicResultAnswers cumulativeResults,
            ImageSetDto image, BasicResultAnswers imageResults, AcupCalibrationConcentrationListDomain concentration)
        {
            if (!ImagesAreAvailable)
            {
                DispatcherHelper.ApplicationExecute(() => { ImagesAreAvailable = true; });
            }

            if (string.IsNullOrEmpty(SelectedSampleRecord?.SampleIdentifier))
            {
                var sampleResult = new SampleRecordDomain
                {
                    Tag = sample.SampleTag,
                    BpQcName = sample.CellTypeQcName,
                    SampleIdentifier = sample.SampleName,
                    WashName = sample.WashType,
                    DilutionName = Misc.ConvertToString(sample.Dilution),
                    Position = sample.SamplePosition,
                    SelectedResultSummary = new ResultSummaryDomain
                    {
                        CellTypeDomain = _cellTypeManager.GetCellType(sample.CellTypeIndex),
                        CumulativeResult = cumulativeResults.MarshalToBasicResultDomain()
					}
                };

                DispatcherHelper.ApplicationExecute(() => { SelectedSampleRecord = sampleResult; });
            }
            
            DispatcherHelper.ApplicationExecute(() =>
            {
                _runSampleHelper.UpdateSampleRecord(sample, SelectedSampleRecord, imageResults, image, imageNum);
            });
        }
        
        #endregion

        #region Commands

        private RelayCommand _imageExpandCommand;
        public RelayCommand ImageExpandCommand => _imageExpandCommand ?? (_imageExpandCommand = new RelayCommand(PerformExpandImage, CanPerformExpandImage));

        private bool CanPerformExpandImage()
        {
            return ImagesAreAvailable;
        }

        private void PerformExpandImage()
        {
            var imageList = SelectedSampleRecord?.SampleImageList?.Cast<object>()?.ToList();
            var index = SelectedSampleRecord?.SelectedSampleImageRecord == null ? -1 : (int)SelectedSampleRecord.SelectedSampleImageRecord.SequenceNumber;
            
            var args = new ExpandedImageGraphEventArgs(ImageType.Annotated, ApplicationConstants.ImageViewRightClickMenuImageFitSize, imageList, SelectedSampleRecord, index);
            DialogEventBus.ExpandedImageGraph(this, args);
        }

        #endregion
    }
}