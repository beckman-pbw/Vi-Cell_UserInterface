// ***********************************************************************
// Assembly         : ScoutViewModels
// Author           : 20126416
// Created          : 04-07-2017
//
// Last Modified By : 40001533
// Last Modified On : 07-01-2019
// ***********************************************************************
// <copyright file="LowLevelViewModel.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using ApiProxies.Generic;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutModels.Home.QueueManagement;
using ScoutModels.Service.Manual;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Structs;
using ScoutViewModels.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ScoutViewModels.ViewModels.Service.Manual
{
    public class LowLevelViewModel : BaseValidation
    {
        #region Private Property

        private bool _isLoading;

        private bool _isAspirateActive;

        private bool _isDispenseActive;

        private bool _isRepetitionRunning;

        private LowLevelManualServiceModel _lowLevelManualService;

        private string _probValue;
        
        private ICommand _probeUpDownCommand;

        private ICommand _carouselPositionUpdateCommand;

        private ICommand _valveSelectionCommand;

        private ICommand _repetitionSectionCommand;

        private ICommand _reagentArmMoveCommand;

        private List<KeyValuePair<int, string>> _gridRowPositionList;

        private List<KeyValuePair<int, string>> _gridColumnPositionList;
        
        private List<KeyValuePair<int, string>> _positionList;

        private KeyValuePair<int, string> _selectedGridRowPosition;
        
        private KeyValuePair<int, string> _selectedColumnPosition;

        private KeyValuePair<int, string> _selectedPosition;

        private const int MinProbeVolume = 0;
        private const int MaxProbeVolume = 1000;

        #endregion

        #region Public Property

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                NotifyPropertyChanged(nameof(IsLoading));
            }
        }

        public bool IsAspirateActive
        {
            get { return _isAspirateActive; }
            set
            {
                _isAspirateActive = value;
                NotifyPropertyChanged(nameof(IsAspirateActive));
            }
        }

        public bool IsDispenseActive
        {
            get { return _isDispenseActive; }
            set
            {
                _isDispenseActive = value;
                NotifyPropertyChanged(nameof(IsDispenseActive));
            }
        }

        public bool IsRepetitionRunning
        {
            get { return _isRepetitionRunning; }
            set
            {
                _isRepetitionRunning = value;
                NotifyPropertyChanged(nameof(IsRepetitionRunning));
            }
        }

        public LowLevelManualServiceModel LowLevelManualService
        {
            get { return _lowLevelManualService; }
            set
            {
                _lowLevelManualService = value;
                NotifyPropertyChanged(nameof(LowLevelManualService));
            }
        }

        public string ProbValue
        {
            get { return _probValue; }
            set
            {
                _probValue = value;
                NotifyPropertyChanged(nameof(ProbValue));
            }
        }

        public List<KeyValuePair<int, string>> GridRowPositionList
        {
            get
            {
                return _gridRowPositionList ??
                       (_gridRowPositionList = new List<KeyValuePair<int, string>>(SetGridRowPosition()));
            }
            set { _gridRowPositionList = value; }
        }

      
        public List<KeyValuePair<int, string>> GridColumnPositionList
        {
            get
            {
                return _gridColumnPositionList ?? (_gridColumnPositionList =
                           new List<KeyValuePair<int, string>>(SetGridColumnPosition()));
            }
            set { _gridColumnPositionList = value; }
        }

        public List<KeyValuePair<int, string>> PositionList
        {
            get { return _positionList ?? (_positionList = new List<KeyValuePair<int, string>>(SetPosition())); }
            set { _positionList = value; }
        }

     
        public KeyValuePair<int, string> SelectedGridRowPosition
        {
            get { return _selectedGridRowPosition; }
            set
            {
                _selectedGridRowPosition = value;
                NotifyPropertyChanged(nameof(SelectedGridRowPosition));
            }
        }

     
        public KeyValuePair<int, string> SelectedColumnPosition
        {
            get { return _selectedColumnPosition; }
            set
            {
                _selectedColumnPosition = value;
                NotifyPropertyChanged(nameof(SelectedColumnPosition));
            }
        }

       
        public KeyValuePair<int, string> SelectedPosition
        {
            get { return _selectedPosition; }
            set
            {
                _selectedPosition = value;
                NotifyPropertyChanged(nameof(SelectedPosition));
            }
        }

        private bool _isProbeBtnEnabled;
      
        public bool IsProbeBtnEnabled
        {
            get { return _isProbeBtnEnabled; }
            set
            {
                _isProbeBtnEnabled = value;
                NotifyPropertyChanged(nameof(IsProbeBtnEnabled));
            }
        }

        #endregion

        #region Public Command

        public ICommand ProbUpDownCommand => _probeUpDownCommand ?? (_probeUpDownCommand = new RelayCommand(OnChangeProbeUpDown));


        public ICommand RepetitionSectionCommand => _repetitionSectionCommand ?? (_repetitionSectionCommand = new RelayCommand(RepetitionSection, null));

        public ICommand CarouselPositionCommand => _carouselPositionUpdateCommand ?? (_carouselPositionUpdateCommand = new RelayCommand(
               CarouselPositionUpdate =>
               {
                   var column = SelectedColumnPosition;
                   var row = SelectedGridRowPosition;
                   switch (LowLevelManualService.LowLevelManualServiceDomain.Carousel)
                   {
                       case eSensorStatus.ssStateUnknown:
                           PublishEventAggregator(LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CAROUSEL_POSITION_COMMAND"), MessageType.System);
                           Log.Warn("Carousel sensor state unknown :" + LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CAROUSEL_POSITION_COMMAND"));
                           break;
                       case eSensorStatus.ssStateActive:
                           try
                           {
                               if (SelectedPosition.Value != null)
                               {
                                   var hawkeyeError = LowLevelManualServiceModel.svc_SetSampleWellPosition('Z', uint.Parse(SelectedPosition.Value));
                                   if (hawkeyeError.Equals(HawkeyeError
                                       .eSuccess))
                                   {
                                       var samplePosition = QueueManagementModel.GetSampleWellPosition();
                                       var result = PositionList.Find(x => x.Key == samplePosition.col);
                                       SelectedPosition = result;
                                       LowLevelManualService.LowLevelManualServiceDomain.CarouselPositionValue = Global.ConvertToString(samplePosition.col);
                                   }
                                   else
                                       ShowMessageBoxInformation(hawkeyeError);
                               }
                           }
                           catch (Exception ex)
                           {
                               ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CAROUSEL_POSITION_COMMAND"));
                           }

                           break;
                       case eSensorStatus.ssStateInactive:
                           try
                           {
                               if (SelectedColumnPosition.Value != null &&
                                   SelectedGridRowPosition.Value != null)
                               {
                                   var hawkeyeError = LowLevelManualServiceModel.svc_SetSampleWellPosition(char.Parse(column.Value), uint.Parse(row.Value));
                                   if (hawkeyeError.Equals(HawkeyeError
                                       .eSuccess))
                                   {
                                       var samplePosition = QueueManagementModel.GetSampleWellPosition();
                                       var result = GridRowPositionList.Find(x => x.Key == samplePosition.col);
                                       column = result;
                                       result = GridColumnPositionList.Find(x => x.Value.Contains(Global.ConvertToString(samplePosition.row)));
                                       row = result;
                                       if (column.Value != null && row.Value != null)
                                           LowLevelManualService.LowLevelManualServiceDomain.CarouselPositionValue = column.Value + "-" + row.Value;
                                   }
                                   else
                                       ShowMessageBoxInformation(hawkeyeError);
                               }
                           }
                           catch (Exception ex)
                           {
                               ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CAROUSEL_POSITION_COMMAND"));
                           }

                           break;
                       default:
                           break;
                   }
               }));

      
        public ICommand ValveSelectionCommand => _valveSelectionCommand ?? (_valveSelectionCommand = new RelayCommand(param =>
             {
                 Valve valve = (Valve) param;

                 var hawkeyeError = LowLevelManualServiceModel.SetValvePort(GetValvePosition(valve));
                 if (hawkeyeError.Equals(HawkeyeError.eSuccess))
                 {
                     PublishEventAggregator(LanguageResourceHelper.Get("LID_StatusBar_ValvePositionChanged"));
                     AspirateDispenseStatus(valve);
                     LowLevelManualService.LowLevelManualServiceDomain.SelectedValve = LowLevelManualService.svc_GetValvePort();
                     char selectedValve = char.Parse(LowLevelManualService.LowLevelManualServiceDomain.SelectedValve);
                     LowLevelManualService.ValveStatus = SetValvePosition(selectedValve);
                 }
                 else
                     ShowMessageBoxInformation(hawkeyeError);
             }));

        private void ShowMessageBoxInformation(HawkeyeError message)
        {
            ApiHawkeyeMsgHelper.ErrorCommon(message);
        }

        private async void OnChangeProbeUpDown(object param)
        {
            try
            {
                if (param == null)
                    return;
                var hawkeyeError = new HawkeyeError();
                IsProbeBtnEnabled = false;              
                switch (param.ToString())
                {
                    case "Top":
                        hawkeyeError = await Task.Run(() => LowLevelManualServiceModel.svc_MoveProbe(true));
                        break;
                    case "Bottom":
                        hawkeyeError = await Task.Run(() => LowLevelManualServiceModel.svc_MoveProbe(false));
                        break;
                    case "StepUp":
                        hawkeyeError = await Task.Run(() => LowLevelManualServiceModel.svc_SetProbePosition(true, ApplicationConstants.ProbePositionValue));
                        break;
                    case "StepDown":
                        hawkeyeError = await Task.Run(() => LowLevelManualServiceModel.svc_SetProbePosition(false, ApplicationConstants.ProbePositionValue));
                        break;
                    default:
                        break;
                }
                if (hawkeyeError.Equals(HawkeyeError.eSuccess))
                {
                    await Task.Run(() => LowLevelManualService.svc_GetProbePosition());
                    PublishEventAggregator(LanguageResourceHelper.Get("LID_StatusBar_MovingProbeSuccessful"));
                }
                else
                    ShowMessageBoxInformation(hawkeyeError);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CHANGE_PROBE_UP_DOWN"));
            }
            finally
            {
                IsProbeBtnEnabled = true;
            }
        }

      
        private char GetValvePosition(Valve valve)
        {
            switch (valve)
            {
                case Valve.ValveA:
                    return 'A';
                case Valve.ValveB:
                    return 'B';
                case Valve.ValveC:
                    return 'C';
                case Valve.ValveD:
                    return 'D';
                case Valve.ValveE:
                    return 'E';
                case Valve.ValveF:
                    return 'F';
                case Valve.ValveG:
                    return 'G';
                case Valve.ValveH:
                    return 'H';
                default:
                    return 'A';
            }
        }

        private Valve SetValvePosition(char valvePosition)
        {
            switch (valvePosition)
            {
                case 'A':
                    return Valve.ValveA;
                case 'B':
                    return Valve.ValveB;
                case 'C':
                    return Valve.ValveC;
                case 'D':
                    return Valve.ValveD;
                case 'E':
                    return Valve.ValveE;
                case 'F':
                    return Valve.ValveF;
                case 'G':
                    return Valve.ValveG;
                case 'H':
                    return Valve.ValveH;
                default:
                    return Valve.ValveA;
            }
        }

        public ICommand ReagentArmMoveCommand => _reagentArmMoveCommand ??
            (_reagentArmMoveCommand = new RelayCommand(param => ReagentArmMovement(param.ToString())));

        private void ReagentArmMovement(string param)
        {
            try
            {
                var hawkEyeError = new HawkeyeError();
                switch (param)
                {
                    case "Up":
                        hawkEyeError = LowLevelManualServiceModel.MoveReagentArm(true);
                        break;
                    case "Down":
                        hawkEyeError = LowLevelManualServiceModel.MoveReagentArm(false);
                        break;
                }

                if (hawkEyeError == HawkeyeError.eSuccess)
                {
                    PublishEventAggregator(LanguageResourceHelper.Get("LID_StatusBar_ReagentProbeArmMovedSuccessfully"));
                }
                else
                    ShowMessageBoxInformation(hawkEyeError);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CHANGE_PROBE_UP_DOWN"));
            }
        }

        #endregion

        #region Constructor

        public LowLevelViewModel() : base()
        {
            LowLevelManualService = new LowLevelManualServiceModel();
            Initialize();
        }

        public LowLevelViewModel(LowLevelManualServiceModel lowLevelManualService) : base()
        {
            LowLevelManualService = lowLevelManualService;
            Initialize();
        }

        private void Initialize()
        {
            LowLevelManualService.svc_GetProbePosition();
            LowLevelManualService.svc_GetSyringePumpPosition();
            UpdateLowLevelViewModel();
            IsRepetitionRunning = true;
            IsProbeBtnEnabled = true;
        }

        #endregion

        #region public Method

        public List<KeyValuePair<int, string>> SetGridRowPosition()
        {
            var list = new List<KeyValuePair<int, string>>();
            for (int i = 1; i < 13; i++)
                list.Add(new KeyValuePair<int, string>(i, Global.ConvertToString(i)));
            return list;
        }


        public List<KeyValuePair<int, string>> SetGridColumnPosition()
        {
            var list = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(1, "A"),
                new KeyValuePair<int, string>(2, "B"),
                new KeyValuePair<int, string>(3, "C"),
                new KeyValuePair<int, string>(4, "D"),
                new KeyValuePair<int, string>(5, "E"),
                new KeyValuePair<int, string>(6, "F"),
                new KeyValuePair<int, string>(7, "G"),
                new KeyValuePair<int, string>(8, "H")
            };
            return list;
        }

     
        public List<KeyValuePair<int, string>> SetPosition()
        {
            var list = new List<KeyValuePair<int, string>>();

            for (int i = 1; i < 25; i++)
                list.Add(new KeyValuePair<int, string>(i, Global.ConvertToString(i)));

            return list;
        }

    
        public void UpdateLowLevelViewModel()
        {
            ProbValue = "250";
            LowLevelManualService.LowLevelManualServiceDomain.SelectedValve = LowLevelManualService.svc_GetValvePort();
        }

   
        public void OnInitializeActivate()
        {
            var initializeStatus = LowLevelManualServiceModel.InitializeSampleDeck();

            if (initializeStatus.Equals(HawkeyeError.eSuccess))
            {
                PublishEventAggregator(
                    LanguageResourceHelper.Get("LID_StatusBar_InitializedSampleDeck"));
            }
            else
            {
                ApiHawkeyeMsgHelper.ErrorCommon(initializeStatus);
            }
            IsRepetitionRunning = true;
        }

  
        public void OnSampleAspirate()
        {
            IsRepetitionRunning = true;
            if (!ValidateProbe())
            {
                return;
            }
            var aspirateStatus = LowLevelManualService.svc_AspirateSample(uint.Parse(ProbValue));
            if (aspirateStatus.Equals(HawkeyeError.eSuccess))
            {
                PublishEventAggregator(LanguageResourceHelper.Get("LID_ButtonContent_AspirateSuccessful"));
            }
            else
                ShowMessageBoxInformation(aspirateStatus);
        }

        private bool ValidateProbe()
        {
            int probeValue = 0;
            int.TryParse(ProbValue, out probeValue);

            if (probeValue > MaxProbeVolume || probeValue < MinProbeVolume)
            {
                var msg = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_SyringeMsg"), Global.ConvertToString(MinProbeVolume), 
                    Global.ConvertToString(MaxProbeVolume));
                DialogEventBus.DialogBoxOk(this, msg);
                return false;
            }

            return true;
        }
   
        public void OnSampleDispense()
        {
            IsRepetitionRunning = true;
            if (!ValidateProbe())
                return;
            var dispenseStatus = LowLevelManualService.svc_DispenseSample(uint.Parse(ProbValue));
            if (dispenseStatus.Equals(HawkeyeError.eSuccess))
            {
                PublishEventAggregator(LanguageResourceHelper.Get("LID_ButtonContent_DispenseSuccessful"));
            }
            else
                ShowMessageBoxInformation(dispenseStatus);
        }

   
        public void RepetitionSection(object parameter)
        {
            try
            {
                IsRepetitionRunning = false;
                IsAspirateActive = false;
                IsDispenseActive = false;
                if (parameter == null)
                    return;
                switch (parameter.ToString())
                {
                    case "Aspirate":
                        OnSampleAspirate();
                        AspirateDispenseStatus(LowLevelManualService.ValveStatus);
                        break;
                    case "Dispense":
                        OnSampleDispense();
                        AspirateDispenseStatus(LowLevelManualService.ValveStatus);
                        break;
                    case "Initialize":
                        OnInitializeActivate();
                        AspirateDispenseStatus(LowLevelManualService.ValveStatus);
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_REPETITION_SECTION"));
            }
        }

   
        private void AspirateDispenseStatus(Valve valve)
        {
            switch (valve)
            {
                case Valve.ValveA:
                case Valve.ValveB:
                case Valve.ValveC:
                case Valve.ValveD:
                case Valve.ValveE:
                    IsAspirateActive = true;
                    IsDispenseActive = false;
                    break;
                case Valve.ValveG:
                    IsAspirateActive = true;
                    IsDispenseActive = true;
                    break;
                case Valve.ValveF:
                case Valve.ValveH:
                    IsAspirateActive = false;
                    IsDispenseActive = true;
                    break;
                default:
                    break;
            }
        }

        #endregion

        public void SetDefaultLowLevel()
        {
            SystemStatusLog(Global.ScoutSystemStatus);
            SetValvePosition(char.Parse(LowLevelManualService.LowLevelManualServiceDomain.SelectedValve));
            AspirateDispenseStatus(LowLevelManualService.ValveStatus);
        }

        public void SystemStatusLog(SystemStatusData systemStatus)
        {
            Log.Debug("SystemStatusLog::");
            Log.Debug("System Health : " + systemStatus.health);
            Log.Debug(", is_standalone_mode :  " + systemStatus.is_standalone_mode);
            Log.Debug(", Sample Position Row :  " + systemStatus.sampleStageLocation.row);
            Log.Debug(", Sample Position Column :  " + systemStatus.sampleStageLocation.col);
            Log.Debug(", motor_FLRack1 :  " + systemStatus.motor_FLRack1);
            Log.Debug(", motor_FLRack2 :  " + systemStatus.motor_FLRack2);
            Log.Debug(", motor_Focus :  " + systemStatus.motor_Focus);
            Log.Debug(", motor_Probe :  " + systemStatus.motor_Probe);
            Log.Debug(", motor_Radius :  " + systemStatus.motor_Radius);
            Log.Debug(", motor_Reagent :  " + systemStatus.motor_Reagent);
            Log.Debug(", motor_Theta :  " + systemStatus.motor_Theta);
            Log.Debug(", sensor_carousel_detect :  " + systemStatus.sensor_carousel_detect);
            Log.Debug(", sensor_carousel_tube_detect :  " + systemStatus.sensor_carousel_tube_detect);
            Log.Debug(", sensor_flopticsmotor1_home :  " + systemStatus.sensor_flopticsmotor1_home);
            Log.Debug(", sensor_flopticsmotor2_home :  " + systemStatus.sensor_flopticsmotor2_home);
            Log.Debug(", sensor_focusmotor_home :  " + systemStatus.sensor_focusmotor_home);
            Log.Debug(", sensor_probemotor_home :  " + systemStatus.sensor_probemotor_home);
            Log.Debug(", sensor_radiusmotor_home :  " + systemStatus.sensor_radiusmotor_home);
            Log.Debug(", sensor_reagentmotor_lower :  " + systemStatus.sensor_reagentmotor_lower);
            Log.Debug(", sensor_reagentmotor_upper :  " + systemStatus.sensor_reagentmotor_upper);
            Log.Debug(", sensor_reagent_pack_door_closed :  " + systemStatus.sensor_reagent_pack_door_closed);
            Log.Debug(", sensor_reagent_pack_in_place :  " + systemStatus.sensor_reagent_pack_in_place);
            Log.Debug(", sensor_thetamotor_home :  " + systemStatus.sensor_thetamotor_home);
            Log.Debug(", focus_DefinedFocusPosition :  " + systemStatus.focus_DefinedFocusPosition);
            Log.Debug(", focus_IsFocused :  " + systemStatus.focus_IsFocused);
            Log.Debug(", active_error_count :  " + systemStatus.active_error_count);
            Log.Debug(", brightfieldLedPercentPower :  " + systemStatus.brightfieldLedPercentPower);
            Log.Debug(", system_total_sample_count :  " + systemStatus.system_total_sample_count);
            Log.Debug(", active_error_count :  " + systemStatus.active_error_count);
            Log.Debug(", active_error_codes :  " + systemStatus.active_error_codes);
            Log.Debug(", nightly_clean_cycle :  " + systemStatus.nightly_clean_cycle);
            Log.Debug(", syringePosition :  " + systemStatus.syringePosition);
            Log.Debug(", sensor_reagent_pack_in_place :  " + systemStatus.syringeValvePosition);
            Log.Debug(", sensor_thetamotor_home :  " + systemStatus.system_total_sample_count);
            Log.Debug(", temperature_ControllerBoard :  " + systemStatus.temperature_ControllerBoard);
            Log.Debug(", temperature_CPU :  " + systemStatus.temperature_CPU);
            Log.Debug(", temperature_OpticalCase :  " + systemStatus.temperature_OpticalCase);
            Log.Debug(", voltage_12V :  " + systemStatus.voltage_12V);
            Log.Debug(", voltage_24V :  " + systemStatus.voltage_24V);
            Log.Debug(", voltage_3_3V :  " + systemStatus.voltage_3_3V);
            Log.Debug(", voltage_5V_Circuit :  " + systemStatus.voltage_5V_Circuit);
            Log.Debug(", voltage_5V_Sensor :  " + systemStatus.voltage_5V_Sensor);
            Log.Debug(", voltage_neg_3V :  " + systemStatus.voltage_neg_3V);
        }

    }
}
