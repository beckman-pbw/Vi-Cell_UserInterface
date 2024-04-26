using ScoutDomains;
using ScoutServices.Service.ConcentrationSlope;
using ScoutUtilities.Enums;
using ScoutUtilities.Helper;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace ScoutViewModels.ViewModels.Service.ConcentrationSlope.DataTabs
{
    public class AcupSummaryTabViewModel : BaseViewModel, IHandlesCalibrationState
    {
        public AcupSummaryTabViewModel(IAcupConcentrationService acupConcentrationService,
            IConcentrationSlopeService concentrationSlopeService)
        {
            _acupConcentrationService = acupConcentrationService;
            _concentrationSlopeService = concentrationSlopeService;
            IsSingleton = true;
            HandleNewCalibrationState(CalibrationGuiState.NotStarted);
        }

        public void UpdateDatePicker()
        {
            if (ConcentrationTemplates != null)
            {
                foreach (var template in ConcentrationTemplates)
                {
                    template.ExpiryDate = DateTime.Now;
                }
            }
        }

        #region Properties & Fields

        private readonly IAcupConcentrationService _acupConcentrationService;
        private readonly IConcentrationSlopeService _concentrationSlopeService;

        public ObservableCollection<ICalibrationConcentrationListDomain> ConcentrationTemplates
        {
            get { return GetProperty<ObservableCollection<ICalibrationConcentrationListDomain>>(); }
            set { SetProperty(value); }
        }

        public string ACupConcentrationComment
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Interface Methods
        
        public void HandleNewCalibrationState(CalibrationGuiState state)
        {
            switch(state)
            {
                case CalibrationGuiState.Aborted:
                case CalibrationGuiState.NotStarted:
                case CalibrationGuiState.CalibrationApplied:
                case CalibrationGuiState.CalibrationRejected:
                    IsEnabled = true;
                    ConcentrationTemplates = _concentrationSlopeService.GetStandardConcentrationList()
                                                                       .ToObservableCollection();
                    ACupConcentrationComment = string.Empty;
                    break;
                case CalibrationGuiState.Ended:
                    break;
                case CalibrationGuiState.Started:
                    IsEnabled = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        #endregion
    }
}