using ApiProxies.Generic;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Home.QueueManagement;
using ScoutModels.Service;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutViewModels.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using ScoutModels.Interfaces;

namespace ScoutViewModels.ViewModels.Service
{
    public class LowLevelViewModel : BaseViewModel
    {
        private const int MinProbeVolume = 0;
        private const int MaxProbeVolume = 1000;
        private readonly IInstrumentStatusService _instrumentStatusService;
        private SystemStatusDomain _systemStatusDomain;
        private IDisposable _statusSubscriber;

        public eSensorStatus CarouselDetect => _instrumentStatusService.SystemStatusDom.CarouselDetect;
        public eSensorStatus TubeDetect => _instrumentStatusService.SystemStatusDom.TubeDetect;
        public eSensorStatus ReagentDoor => _instrumentStatusService.SystemStatusDom.ReagentDoor;
        public eSensorStatus ReagentPack => _instrumentStatusService.SystemStatusDom.ReagentPack;
        public eSensorStatus RadiusHome => _instrumentStatusService.SystemStatusDom.RadiusHome;
        public eSensorStatus ThetaHome => _instrumentStatusService.SystemStatusDom.ThetaHome;
        public eSensorStatus ProbeHome => _instrumentStatusService.SystemStatusDom.ProbeHome;
        public eSensorStatus FocusHome => _instrumentStatusService.SystemStatusDom.FocusHome;
        public eSensorStatus ReagentUpper => _instrumentStatusService.SystemStatusDom.ReagentUpper;
        public eSensorStatus ReagentLower => _instrumentStatusService.SystemStatusDom.ReagentLower;
        public int MotorRadiusPosition => _instrumentStatusService.SystemStatusDom.MotorRadiusPosition;
        public int MotorThetaPosition => _instrumentStatusService.SystemStatusDom.MotorThetaPosition;
        public double BrightFieldLED => _instrumentStatusService.SystemStatusDom.BrightFieldLED;
        public int MotorFocusPosition => _instrumentStatusService.SystemStatusDom.MotorFocusPosition;
        public int MotorReagentPosition => _instrumentStatusService.SystemStatusDom.MotorReagentPosition;
        public int SyringePosition => _instrumentStatusService.SystemStatusDom.SyringePosition;
        public double Voltage3_3V => _instrumentStatusService.SystemStatusDom.Voltage3_3V;
        public double Voltage5vSensor => _instrumentStatusService.SystemStatusDom.Voltage5vSensor;
        public double Voltage5vCircuit => _instrumentStatusService.SystemStatusDom.Voltage5vCircuit;
        public double Voltage12v => _instrumentStatusService.SystemStatusDom.Voltage12v;
        public double Voltage24v => _instrumentStatusService.SystemStatusDom.Voltage24v;
        public double TemperatureControlBoard => _instrumentStatusService.SystemStatusDom.TemperatureControlBoard;
        public double TemperatureCPU => _instrumentStatusService.SystemStatusDom.TemperatureCPU;
        public double TemperatureOpticalCase => _instrumentStatusService.SystemStatusDom.TemperatureOpticalCase;
        public string StagePositionString => _instrumentStatusService.SystemStatusDom.StagePositionString;
        public ValvePosition ValvePosition => _instrumentStatusService.SystemStatusDom.ValvePosition;
        public int MotorProbePosition => _instrumentStatusService.SystemStatusDom.MotorProbePosition;

        public LowLevelViewModel(IInstrumentStatusService instrumentStatusService)
        {
            _instrumentStatusService = instrumentStatusService;
            ProbeValue = "250";
            IsRepetitionRunning = true;
            IsProbeBtnEnabled = true;
            SetAspirateDispenseStatus(ValvePosition);
            _statusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe((OnSystemStatusChanged));
        }
        
        protected override void DisposeUnmanaged()
        {
            _statusSubscriber?.Dispose();
            base.DisposeUnmanaged();
        }

        private void OnSystemStatusChanged(SystemStatusDomain systemStatusDomain)
        {
            if (systemStatusDomain == null) return;
            
            _systemStatusDomain = systemStatusDomain;
            NotifyAllPropertiesChanged();
        }

        public bool IsAspirateActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsDispenseActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsRepetitionRunning
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string ProbeValue
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        private List<KeyValuePair<int, string>> _gridRowPositionList;
        public List<KeyValuePair<int, string>> GridRowPositionList
        {
            get
            {
                return _gridRowPositionList ?? (_gridRowPositionList = new List<KeyValuePair<int, string>>(SetGridRowPosition()));
            }
            set { _gridRowPositionList = value; }
        }

        private List<KeyValuePair<int, string>> _gridColumnPositionList;
        public List<KeyValuePair<int, string>> GridColumnPositionList
        {
            get
            {
                return _gridColumnPositionList ?? (_gridColumnPositionList = new List<KeyValuePair<int, string>>(SetGridColumnPosition()));
            }
            set { _gridColumnPositionList = value; }
        }

        private List<KeyValuePair<int, string>> _positionList;
        public List<KeyValuePair<int, string>> PositionList
        {
            get { return _positionList ?? (_positionList = new List<KeyValuePair<int, string>>(SetPosition())); }
            set { _positionList = value; }
        }

        private KeyValuePair<int, string> _selectedGridRowPosition;
        public KeyValuePair<int, string> SelectedGridRowPosition
        {
            get { return _selectedGridRowPosition; }
            set
            {
                _selectedGridRowPosition = value;
                NotifyPropertyChanged(nameof(SelectedGridRowPosition));
            }
        }

        private KeyValuePair<int, string> _selectedColumnPosition;
        public KeyValuePair<int, string> SelectedColumnPosition
        {
            get { return _selectedColumnPosition; }
            set
            {
                _selectedColumnPosition = value;
                NotifyPropertyChanged(nameof(SelectedColumnPosition));
            }
        }

        private KeyValuePair<int, string> _selectedPosition;
        public KeyValuePair<int, string> SelectedPosition
        {
            get { return _selectedPosition; }
            set
            {
                _selectedPosition = value;
                NotifyPropertyChanged(nameof(SelectedPosition));
            }
        }

        public bool IsProbeBtnEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        private ICommand _probeUpDownCommand;
        public ICommand ProbUpDownCommand => _probeUpDownCommand ?? (_probeUpDownCommand = new RelayCommand(OnChangeProbeUpDown));

        private ICommand _repetitionSectionCommand;
        public ICommand RepetitionSectionCommand => _repetitionSectionCommand ?? (_repetitionSectionCommand = new RelayCommand(RepetitionSection, null));

        private ICommand _carouselPositionUpdateCommand;
        public ICommand CarouselPositionCommand => _carouselPositionUpdateCommand ?? (_carouselPositionUpdateCommand = new RelayCommand(
               CarouselPositionUpdate =>
               {
                   var column = SelectedColumnPosition;
                   var row = SelectedGridRowPosition;
                   switch (_instrumentStatusService.SystemStatusDom.CarouselDetect)
                   {
                       case eSensorStatus.ssStateUnknown:
                           string str = LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CAROUSEL_POSITION_COMMAND");
                           PostToMessageHub(str, MessageType.System);
                           Log.Warn("Carousel sensor state unknown :" + str);
                           break;

                       case eSensorStatus.ssStateActive:
                           try
                           {
                               if (SelectedPosition.Value != null)
                               {
                                   var hawkeyeError = LowLevelModel.svc_SetSampleWellPosition('Z', uint.Parse(SelectedPosition.Value));
                                   if (hawkeyeError.Equals(HawkeyeError
                                       .eSuccess))
                                   {
                                       var samplePosition = QueueManagementModel.GetSampleWellPosition();
                                       var result = PositionList.Find(x => x.Key == samplePosition.Column);
                                       SelectedPosition = result;
                                   }
                                   else
                                       ApiHawkeyeMsgHelper.ErrorCommon(hawkeyeError);
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
                                   var hawkeyeError = LowLevelModel.svc_SetSampleWellPosition(char.Parse(column.Value), uint.Parse(row.Value));
                                   if (hawkeyeError.Equals(HawkeyeError.eSuccess))
                                   {
                                       var samplePosition = QueueManagementModel.GetSampleWellPosition();
                                       var result = GridRowPositionList.Find(x => x.Key == samplePosition.Column);
                                       column = result;
                                       result = GridColumnPositionList.Find(x => x.Value.Contains(ScoutUtilities.Misc.ConvertToString(samplePosition.Row)));
                                       row = result;
                                   }
                                   else
                                       ApiHawkeyeMsgHelper.ErrorCommon(hawkeyeError);
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


        private ICommand _valveSelectionCommand;
        public ICommand ValveSelectionCommand => _valveSelectionCommand ?? (_valveSelectionCommand = new RelayCommand(param =>
        {
            ValvePosition valvePosition = (ValvePosition) param;
            SetValvePort(valvePosition);
        }));

        public void SetValvePort(ValvePosition valvePosition)
        {
            var hawkeyeError = LowLevelModel.SetValvePort(ValvePositionMap.ValvePositionToChar(valvePosition));
            if (hawkeyeError.Equals(HawkeyeError.eSuccess))
            {
                _instrumentStatusService.SystemStatusDom.ValvePosition = valvePosition;
                NotifyPropertyChanged(nameof(ValvePosition));
                PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_ValvePositionChanged"));
                SetAspirateDispenseStatus(valvePosition);
            }
            else
                ApiHawkeyeMsgHelper.ErrorCommon(hawkeyeError);
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
                        hawkeyeError = await Task.Run(() => LowLevelModel.svc_MoveProbe(true));
                        break;
                    case "Bottom":
                        hawkeyeError = await Task.Run(() => LowLevelModel.svc_MoveProbe(false));
                        break;
                    case "StepUp":
                        hawkeyeError = await Task.Run(() => LowLevelModel.svc_SetProbePosition(true, ApplicationConstants.ProbePositionValue));
                        break;
                    case "StepDown":
                        hawkeyeError = await Task.Run(() => LowLevelModel.svc_SetProbePosition(false, ApplicationConstants.ProbePositionValue));
                        break;
                    default:
                        break;
                }
                if (hawkeyeError.Equals(HawkeyeError.eSuccess))
                {
                    await Task.Run(() => LowLevelModel.svc_GetProbePosition());
                    PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_MovingProbeSuccessful"));
                }
                else
                    ApiHawkeyeMsgHelper.ErrorCommon(hawkeyeError);
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

      
        private ICommand _reagentArmMoveCommand;
        public ICommand ReagentArmMoveCommand => _reagentArmMoveCommand ??
            (_reagentArmMoveCommand = new RelayCommand(param => ReagentArmMovement(param.ToString())));

        private void ReagentArmMovement(string param)
        {
            try
            {
                var hawkeyeError = new HawkeyeError();
                switch (param)
                {
                    case "Up":
                        hawkeyeError = LowLevelModel.MoveReagentArm(true);
                        break;
                    case "Down":
                        hawkeyeError = LowLevelModel.MoveReagentArm(false);
                        break;
                }

                if (hawkeyeError == HawkeyeError.eSuccess)
                {
                    PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_ReagentProbeArmMovedSuccessfully"));
                }
                else
                    ApiHawkeyeMsgHelper.ErrorCommon(hawkeyeError);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CHANGE_PROBE_UP_DOWN"));
            }
        }

        public List<KeyValuePair<int, string>> SetGridRowPosition()
        {
            var list = new List<KeyValuePair<int, string>>();
            for (int i = 1; i < 13; i++)
                list.Add(new KeyValuePair<int, string>(i, ScoutUtilities.Misc.ConvertToString(i)));
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
                list.Add(new KeyValuePair<int, string>(i, ScoutUtilities.Misc.ConvertToString(i)));

            return list;
        }

   
        public void OnInitializeActivate()
        {
            var initializeStatus = LowLevelModel.InitializeSampleDeck();

            if (initializeStatus.Equals(HawkeyeError.eSuccess))
            {
                PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_InitializedSampleDeck"));
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
            var hawkeyeError = LowLevelModel.svc_AspirateSample(uint.Parse(ProbeValue));
            if (hawkeyeError.Equals(HawkeyeError.eSuccess))
            {
                PostToMessageHub(LanguageResourceHelper.Get("LID_ButtonContent_AspirateSuccessful"));
            }
            else
                ApiHawkeyeMsgHelper.ErrorCommon(hawkeyeError);
        }


        private bool ValidateProbe()
        {
            int probeValue = 0;
            int.TryParse(ProbeValue, out probeValue);

            if (probeValue > MaxProbeVolume || probeValue < MinProbeVolume)
            {
                var msg = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_SyringeMsg"), ScoutUtilities.Misc.ConvertToString(MinProbeVolume),
                    ScoutUtilities.Misc.ConvertToString(MaxProbeVolume));
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
            var hawkeyeError = LowLevelModel.svc_DispenseSample(uint.Parse(ProbeValue));
            if (hawkeyeError.Equals(HawkeyeError.eSuccess))
            {
                PostToMessageHub(LanguageResourceHelper.Get("LID_ButtonContent_DispenseSuccessful"));
            }
            else
                ApiHawkeyeMsgHelper.ErrorCommon(hawkeyeError);
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
                    case "Initialize":
                        OnInitializeActivate();
                        break;
                    case "Aspirate":
                        OnSampleAspirate();
                        break;
                    case "Dispense":
                        OnSampleDispense();
                        break;
                }

                SetAspirateDispenseStatus(_instrumentStatusService.SystemStatusDom.ValvePosition);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_REPETITION_SECTION"));
            }
        }

   
        private void SetAspirateDispenseStatus(ValvePosition valve)
        {
            switch (valve)
            {
                case ValvePosition.ValveA:
                case ValvePosition.ValveB:
                case ValvePosition.ValveC:
                case ValvePosition.ValveD:
                case ValvePosition.ValveE:
                    IsAspirateActive = true;
                    IsDispenseActive = false;
                    break;
                case ValvePosition.ValveG:
                    IsAspirateActive = true;
                    IsDispenseActive = true;
                    break;
                case ValvePosition.ValveF:
                case ValvePosition.ValveH:
                    IsAspirateActive = false;
                    IsDispenseActive = true;
                    break;
                default:
                    break;
            }
        }
    }
}
