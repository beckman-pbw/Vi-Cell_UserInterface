using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.Enums;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using ScoutViewModels.Interfaces;
using ScoutViewModels.ViewModels.Service.ConcentrationSlope.DataTabs;
using System.Collections.Generic;

namespace ScoutViewModels.ViewModels.Service.ConcentrationSlope
{
    public class AcupDataPanelViewModel : BaseViewModel, IHandlesCalibrationState, IHandlesSampleStatus,
        IHandlesImageReceived, IHandlesConcentrationCalculated
    {
        public AcupDataPanelViewModel(IScoutViewModelFactory viewModelFactory)
        {
            SummaryTabViewModel = viewModelFactory.CreateAcupSummaryTabViewModel();
            ConcentrationResultsViewModel = viewModelFactory.CreateAcupConcentrationResultsViewModel();
            ImagesTabViewModel = viewModelFactory.CreateAcupImagesTabViewModel();
            GraphsTabViewModel = viewModelFactory.CreateAcupGraphsTabViewModel();
            HistoricalTabViewModel = viewModelFactory.CreateAcupHistoricalTabViewModel();
            IsSingleton = true;

            HandleNewCalibrationState(CalibrationGuiState.NotStarted);
        }

        public void OnViewLoaded()
        {
            SummaryTabViewModel.UpdateDatePicker();
            HistoricalTabViewModel.UpdateDatePicker();
        }

        #region Properties & Fields

        public AcupSummaryTabViewModel SummaryTabViewModel
        {
            get { return GetProperty<AcupSummaryTabViewModel>(); }
            set { SetProperty(value); }
        }

        public AcupImagesTabViewModel ImagesTabViewModel
        {
            get { return GetProperty<AcupImagesTabViewModel>(); }
            set { SetProperty(value); }
        }

        public AcupGraphsTabViewModel GraphsTabViewModel
        {
            get { return GetProperty<AcupGraphsTabViewModel>(); }
            set { SetProperty(value); }
        }

        public AcupHistoricalTabViewModel HistoricalTabViewModel
        {
            get { return GetProperty<AcupHistoricalTabViewModel>(); }
            set { SetProperty(value); }
        }

        public AcupConcentrationResultsViewModel ConcentrationResultsViewModel
        {
            get { return GetProperty<AcupConcentrationResultsViewModel>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Interface Methods

        public void HandleNewCalibrationState(CalibrationGuiState state)
        {
            SummaryTabViewModel.HandleNewCalibrationState(state);
            ImagesTabViewModel.HandleNewCalibrationState(state);
            GraphsTabViewModel.HandleNewCalibrationState(state);
            HistoricalTabViewModel.HandleNewCalibrationState(state);
            ConcentrationResultsViewModel.HandleNewCalibrationState(state);
        }
        
        public void HandleSampleStatusChanged(SampleEswDomain sample, AcupCalibrationConcentrationListDomain concentration)
        {
            ConcentrationResultsViewModel.HandleSampleStatusChanged(sample, concentration);
        }

        public void HandleImageReceived(SampleEswDomain sample, ushort imageNum, BasicResultAnswers cumulativeResults,
            ImageSetDto image, BasicResultAnswers imageResults, AcupCalibrationConcentrationListDomain concentration)
        {
            ImagesTabViewModel.HandleImageReceived(sample, imageNum, cumulativeResults, image, imageResults, concentration);
            ConcentrationResultsViewModel.HandleImageReceived(sample, imageNum, cumulativeResults, image, imageResults, concentration);
        }
        
        public void HandleConcentrationCalculated(CalibrationData totalCells, CalibrationData originalConcentration, CalibrationData adjustedConcentration)
        {
            GraphsTabViewModel.HandleConcentrationCalculated(totalCells, originalConcentration, adjustedConcentration);
            ConcentrationResultsViewModel.HandleConcentrationCalculated(totalCells, originalConcentration, adjustedConcentration);
        }

        #endregion
    }
}