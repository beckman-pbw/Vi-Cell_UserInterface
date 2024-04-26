using ApiProxies.Generic;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutServices.Service.ConcentrationSlope;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ScoutViewModels.ViewModels.Service.ConcentrationSlope.DataTabs
{
    public class AcupHistoricalTabViewModel : BaseViewModel, IHandlesCalibrationState
    {
        public AcupHistoricalTabViewModel(IConcentrationSlopeService concentrationSlopeService)
        {
            _concentrationSlopeService = concentrationSlopeService;
            IsSingleton = true;
            ACupConcentrationOverTimeList = new ObservableCollection<CalibrationActivityLogDomain>();
            IsACupListAvailable = false;
        }

        public void UpdateDatePicker()
        {
            RetrieveCalibrationData();
            if (ACupConcentrationOverTimeList != null && ACupConcentrationOverTimeList.Any())
            {
                ConcentrationFromDate = ACupConcentrationOverTimeList.Min(a => a.Date);
                ConcentrationToDate = ACupConcentrationOverTimeList.Max(a => a.Date);
            }
            else
            {
                ConcentrationFromDate = DateTimeConversionHelper.DateTimeToStartOfDay(DateTime.Now);
                ConcentrationToDate = DateTimeConversionHelper.DateTimeToEndOfDay(DateTime.Now);
            }
        }

        #region Properties & Fields

        private readonly IConcentrationSlopeService _concentrationSlopeService;
        
        public ObservableCollection<CalibrationActivityLogDomain> ACupConcentrationOverTimeList
        {
            get { return GetProperty<ObservableCollection<CalibrationActivityLogDomain>>(); }
            set { SetProperty(value); }
        }

        public DateTime ConcentrationFromDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public DateTime ConcentrationToDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }
        
        public bool IsACupListAvailable
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
                case CalibrationGuiState.NotStarted:
                    break;
                case CalibrationGuiState.Started:
                    break;
                case CalibrationGuiState.Aborted:
                    break;
                case CalibrationGuiState.Ended:
                    break;
                case CalibrationGuiState.CalibrationApplied:
                    UpdateDatePicker();
                    break;
                case CalibrationGuiState.CalibrationRejected:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        #endregion

        #region Commands

        private RelayCommand<string> _getCalibrationsCommand;
        public RelayCommand<string> GetCalibrationsCommand => _getCalibrationsCommand ?? 
            (_getCalibrationsCommand = new RelayCommand<string>(PerformGetCalibrations, CanPerformGetCalibrations));

        private bool CanPerformGetCalibrations(string param)
        {
            return true;
        }

        private void PerformGetCalibrations(string param)
        {
            switch (param)
            {
                case "CleanConcentration":
                    var args = new DialogBoxEventArgs(LanguageResourceHelper.Get("LID_MSGBOX_DeleteConcentrationSlopeHistoryACup"), null, 
                        DialogButtons.YesNo, null, null, MessageBoxImage.None,DialogLocation.CenterApp, true, null, null, false, null, null, false);
                    if (DialogEventBus.DialogBoxRemoveACupConcentration(this, args) != true)
                    {
                        break;
                    }

                    var loginArgs = new LoginEventArgs(LoggedInUser.CurrentUserId,
                        LoggedInUser.CurrentUserId, LoginState.ValidateCurrentUserOnly,
                        DialogLocation.CenterApp, false, true);

                    if (DialogEventBus.Login(this, loginArgs) != LoginResult.CurrentUserLoginSuccess)
                    {
                        break;
                    }

                    var result = _concentrationSlopeService.ClearCalibrationActivityLog(
	                    calibration_type.cal_ACupConcentration, 
                        ConcentrationToDate,
                        loginArgs.Password, 
	                    args.DeleteAllACupData);
                    
                    if (result.Equals(HawkeyeError.eSuccess))
                    {
                        PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_ConLoghasBeenCleared"));
                    }
                    else
                    {
                        ApiHawkeyeMsgHelper.ErrorValidate(result); // opens dialog
                    }

                    RetrieveCalibrationData(ConcentrationFromDate, ConcentrationToDate);

                    break;
                case "SearchConcentration":
                    RetrieveCalibrationData(ConcentrationFromDate, ConcentrationToDate);
                    break;
            }
        }

        #endregion

        #region Private Helper Methods

        private void RetrieveCalibrationData()
        {
            RetrieveCalibrationData(0, 0);
        }

        private void RetrieveCalibrationData(DateTime startDate, DateTime endDate)
        {
            var startTime = DateTimeConversionHelper.DateTimeToUnixSecondRounded(startDate);
            var endTime = DateTimeConversionHelper.DateTimeToEndOfDayUnixSecondRounded(endDate);
            RetrieveCalibrationData(startTime, endTime);
        }

        private void RetrieveCalibrationData(ulong startTime, ulong endTime)
        {
            try
            {
                var calibrationErrorLog = _concentrationSlopeService.RetrieveCalibrationActivityLogRange(
                    calibration_type.cal_ACupConcentration, startTime, endTime);

                ACupConcentrationOverTimeList = calibrationErrorLog.ToObservableCollection();
                IsACupListAvailable = ACupConcentrationOverTimeList.Count > 0;
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_SET_LOG_ENABLE"));
            }
        }

        #endregion
    }
}