using ApiProxies.Generic;
using Microsoft.Reporting.WinForms;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Reports.Common;
using ScoutDomains.Reports.InstrumentStatus;
using ScoutDomains.Reports.RunResult;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Reports;
using ScoutModels.Service;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.UIConfiguration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HawkeyeCoreAPI;
using HawkeyeCoreAPI.Facade;
using ScoutUtilities.Helper;
using ScoutViewModels.Interfaces;
using SystemStatus = ScoutUtilities.Enums.SystemStatus;

namespace ScoutViewModels.ViewModels.Reports
{
    public class InstrumentStatusReportViewModel : ReportWindowViewModel
    {

        public InstrumentStatusReportViewModel(IScoutViewModelFactory viewModelFactory, string printTitle, string comments, SystemStatusDomain systemStatus, HardwareSettingsDomain hardwareSettings,
            List<ReportPrintOptions> reportPrintOptions, List<CalibrationActivityLogDomain> calErrorLogs, List<CalibrationActivityLogDomain> aCupCalErrorLogs, List<UserDomain> userList, 
            IList<ReagentContainerStateDomain> reagentContainers, string analysisType) : base()
        {
            _diskSpaceModel = new DiskSpaceModel();
            _instrumentStatusReportModel = new InstrumentStatusReportModel();
            ReportTitle = LanguageResourceHelper.Get("LID_Label_InstrumentStatusReport");
            ReportWindowTitle = LanguageResourceHelper.Get("LID_Label_InstrumentStatusReport");
            ReportType = ReportType.Instrument;

            _printTitle = printTitle;
            _comments = comments;
            _systemStatus = systemStatus;
            _hardwareSettings = hardwareSettings;
            _reportPrintOptions = reportPrintOptions;
            _calErrorLogs = calErrorLogs;
            _aCupCalErrorLogs = aCupCalErrorLogs;
            _userList = userList;
            _reagentContainers = reagentContainers;
            _analysisType = analysisType;
        }

        protected override void DisposeUnmanaged()
        {
            _opticsManualServiceModel?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties

        const int DefaultRowCount = 6;

        private InstrumentStatusReportTableHeaderDomain _instrumentStatusReportTableHeaderDomain;
        private InstrumentStatusReportModel _instrumentStatusReportModel;
        private DiskSpaceModel _diskSpaceModel;
        private OpticsManualServiceModel _opticsManualServiceModel;
        private InstrumentStatusTableVisibility _instrumentStatusTableVisibility;

        private string _printTitle;
        private string _comments;
        private string _analysisType;
        private SystemStatusDomain _systemStatus;
        private HardwareSettingsDomain _hardwareSettings;
        private List<ReportPrintOptions> _reportPrintOptions;
        private List<CalibrationActivityLogDomain> _calErrorLogs;
        private List<CalibrationActivityLogDomain> _aCupCalErrorLogs;
        private List<UserDomain> _userList;
        private IList<ReagentContainerStateDomain> _reagentContainers;

        #endregion

        #region Override Methods

        public override void LoadReport()
        {
            InitializeLists();
            RetrieveData();
            LoadReportViewer();
        }

        protected override void InitializeLists()
        {
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportMandatoryHeaderDomainList = new List<ReportMandatoryHeaderDomain>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportTableHeaderDomainList = new List<InstrumentStatusReportTableHeaderDomain>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportTableVisibilityList = new List<InstrumentStatusTableVisibility>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportAboutFirstTableList = new List<ReportTableTemplate>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportAboutSecondTableList = new List<ReportTableTemplate>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReagentParameterList = new List<RunResultReagentParameterDomain>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportUsersTableList = new List<InstrumentStatusReportTableColumn>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportCellTypesTableList = new List<InstrumentStatusReportTableColumn>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportAppTypesTableList = new List<ReportTableTemplate>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportConcDataDomainList = new List<InstrumentStatusReportCalibrationDomainTable>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportACupConcDataDomainList = new List<InstrumentStatusReportCalibrationDomainTable>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportConcSlopeTableList = new List<ReportTableTemplate>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportACupConcSlopeTableList = new List<ReportTableTemplate>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportSensorStatusStateList = new List<ReportTableTemplate>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportMotorStatusStateList = new List<ReportTableTemplate>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportStatusStateList = new List<ReportTableTemplate>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportVoltageList = new List<ReportTableTemplate>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportTemperatureList = new List<ReportTableTemplate>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportLowerLevelList = new List<ReportTableTemplate>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportStorageList = new List<ReportTableTemplate>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportSystemErrorList = new List<InstrumentStatusReportErrorDomain>();
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportSensorStatusList = new List<InstrumentStatusReportSensorStatusDomain>();
//TODO:... next line is commented out in CHM.
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportRemainingTubesList = new List<ReportTableTemplate>();
        }

        protected override void RetrieveData()
        {
            SetMandatoryTableData();
            SetMandatoryHeaderData();
            SetTableHeader();
            SetDefaultVisibility();
            SetTableDataAndVisibility();
        }

        protected override void LoadReportViewer()
        {
            ReportViewer = new ReportViewer();
            var lang = LanguageEnums.GetLangType(ApplicationLanguage.GetLanguage());
            var reportViewerPath = GetReportViewerPath(lang);

            var instrumentDomain = _instrumentStatusReportModel.InstrumentStatusReportDomain;
            AddReportDataSource("ReportMandatoryHeaderTBL", instrumentDomain.InstrumentStatusReportMandatoryHeaderDomainList);
            AddReportDataSource("InstrumentStatusMandatoryFirstTableDataTBL", instrumentDomain.InstrumentStatusReportAboutFirstTableList);
            AddReportDataSource("InstrumentStatusMandatorySecondTableDataTBL", instrumentDomain.InstrumentStatusReportAboutSecondTableList);
            AddReportDataSource("InstrumentStatusReagentsTableDataTBL", instrumentDomain.InstrumentStatusReagentParameterList);
            AddReportDataSource("InstrumentStatusUsersTableDataTBL", instrumentDomain.InstrumentStatusReportUsersTableList);
            AddReportDataSource("InstrumentStatusCellTypesTableDataTBL", instrumentDomain.InstrumentStatusReportCellTypesTableList);
            AddReportDataSource("InstrumentStatusAppTypesTableDataTBL", instrumentDomain.InstrumentStatusReportAppTypesTableList);
            AddReportDataSource("InstrumentStatusReportTableHeaderDomainTBL", instrumentDomain.InstrumentStatusReportTableHeaderDomainList);
            AddReportDataSource("InstrumentStatusReportConcDataDomainListTBL", instrumentDomain.InstrumentStatusReportConcDataDomainList);
            AddReportDataSource("InstrumentStatusReportACupConcDataDomainListTBL", instrumentDomain.InstrumentStatusReportACupConcDataDomainList);
            AddReportDataSource("InstrumentStatusReportTableVisbilityTBL", instrumentDomain.InstrumentStatusReportTableVisibilityList);
            AddReportDataSource("InstrumentStatusReportConcSlopeTableListTBL", instrumentDomain.InstrumentStatusReportConcSlopeTableList);
            AddReportDataSource("InstrumentStatusReportACupConcSlopeTableListTBL", instrumentDomain.InstrumentStatusReportACupConcSlopeTableList);
            AddReportDataSource("InstrumentStatusReportSensorStatusStateListTBL", instrumentDomain.InstrumentStatusReportSensorStatusStateList);
            AddReportDataSource("InstrumentStatusReportMotorStatusStateListTBL", instrumentDomain.InstrumentStatusReportMotorStatusStateList);
            AddReportDataSource("InstrumentStatusReportStatusStateListTBL", instrumentDomain.InstrumentStatusReportStatusStateList);
            AddReportDataSource("InstrumentStatusReportVoltageListTBL", instrumentDomain.InstrumentStatusReportVoltageList);
            AddReportDataSource("InstrumentStatusReportTemperatureListTBL", instrumentDomain.InstrumentStatusReportTemperatureList);
            AddReportDataSource("InstrumentStatusReportLowerLevelListTBL", instrumentDomain.InstrumentStatusReportLowerLevelList);
            AddReportDataSource("InstrumentStatusReportStorageListTBL", instrumentDomain.InstrumentStatusReportStorageList);
            AddReportDataSource("InstrumentStatusSystemErrorTBL", instrumentDomain.InstrumentStatusReportSystemErrorList);
            AddReportDataSource("InstrumentSensorStatusInfoTBL", instrumentDomain.InstrumentStatusReportSensorStatusList);
//TODO:... next line is commented out in CHM.
            AddReportDataSource("InstrumentStatusReportRemainingTubesListTBL", instrumentDomain.InstrumentStatusReportRemainingTubesList);

            RefreshAndSetReportContent(reportViewerPath);
        }

        #endregion

        #region Private Methods

        private string GetReportViewerPath(LanguageType language)
        {
            if (language == LanguageType.eChinese)
                return AppDomain.CurrentDomain.BaseDirectory + @"Reports\InstrumentStatusReportRdlcViewerZh-CN.rdlc";
            if (language == LanguageType.eJapanese)
                return AppDomain.CurrentDomain.BaseDirectory + @"Reports\InstrumentStatusReportRdlcViewerJa-JP.rdlc";
            return AppDomain.CurrentDomain.BaseDirectory + @"Reports\InstrumentStatusReportRdlcViewer.rdlc";
        }

        private void SetDefaultVisibility()
        {
            _instrumentStatusTableVisibility = new InstrumentStatusTableVisibility();
            foreach (PropertyInfo pi in _instrumentStatusTableVisibility.GetType().GetProperties())
                pi.SetValue(_instrumentStatusTableVisibility, true);
        }

        private void SetTableDataAndVisibility()
        {
            const bool isParameterVisible = false;
            _reportPrintOptions.ForEach(printOption => 
            {
                if (printOption.ParameterName == LanguageResourceHelper.Get("LID_ResultHeader_Reagents"))
                {
                    SetReagentParameterData();
                }
                else if (printOption.ParameterName == LanguageResourceHelper.Get("LID_TabItem_Users"))
                {
                    _instrumentStatusTableVisibility.UsersVisiblity = isParameterVisible;
                    _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportTableHeaderDomainList[0].UsersHeader =
                        LanguageResourceHelper.Get("LID_TabItem_Users");
                    SetUsersTableData();
                }
                else if (printOption.ParameterName == LanguageResourceHelper.Get("LID_GridLabel_CellTypes"))
                {
                    SetCellTypesTableData();
                }
                else if (printOption.ParameterName == LanguageResourceHelper.Get("LID_Label_AppType"))
                {
                    SetAppTypesTableData();
                }
                else if (printOption.ParameterName == LanguageResourceHelper.Get("LID_TabItem_CalibrationControl"))
                {
                    SetCalibrationTableData();
                }
                else if (printOption.ParameterName == LanguageResourceHelper.Get("LID_TabItem_ACupConcSlope"))
                {
                    _instrumentStatusTableVisibility.ACupCalibrationVisibility = !printOption.IsParameterChecked;
                    SetACupCalibrationTableData();
                }
                else if (printOption.ParameterName == LanguageResourceHelper.Get("LID_FrameLabel_Storage"))
                {
                    SetStorageTableData();
                }
                else if (printOption.ParameterName == LanguageResourceHelper.Get("LID_TabItem_LowLevel"))
                {                  
                    SetLowLevelsControlTableData();
                    SetSensorStatusInfo();
                }
            });

            SetSystemErrorList();

            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportTableVisibilityList.Add(_instrumentStatusTableVisibility);
        }

        private string GetSensorStateValue(string value)
        {
            switch (value)
            {
                case "ssStateUnknown":
                    return "Unknown";
                case "ssStateActive":
                    return "Active";
                case "ssStateInactive":
                    return "InActive";
            }

            return null;
        }

        private void SetSensorStatusInfo()
        {
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportSensorStatusList.Add(
                new InstrumentStatusReportSensorStatusDomain
                {
                    Active = LanguageResourceHelper.Get("LID_Report_Label_Active"),
                    InActive = LanguageResourceHelper.Get("LID_Report_Label_InActive"),
                    UnKnown = LanguageResourceHelper.Get("LID_Report_Label_Unknown")
                });
        }

        private void SetLowLevelsControlTableData()
        {
            var systemStatusDomain = _systemStatus;
            var reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_CameraLamp",
                ScoutUtilities.Misc.UpdateTrailingPoint(systemStatusDomain.BrightFieldLED, TrailingPoint.Two));
            AddReportTableTemplateToLowerLevelList(reportTableTemplate);
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_CurFocus",
                ScoutUtilities.Misc.ConvertToString(systemStatusDomain.DefinedFocusPosition));
            AddReportTableTemplateToLowerLevelList(reportTableTemplate);
            if (_opticsManualServiceModel == null)
            {
                _opticsManualServiceModel = new OpticsManualServiceModel();
            }
                
            var flowCellDepth = _opticsManualServiceModel.svc_GetFlowCellDepthSettingInMillimeters();

            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_FLowCellDepth",
                ScoutUtilities.Misc.ConvertToString(flowCellDepth));
            AddReportTableTemplateToLowerLevelList(reportTableTemplate);

//TODO:... next lines are commented out in CHM.
            var sensorValue = GetSensorStateValue(systemStatusDomain.CarouselDetect.ToString());
            if (sensorValue != null)
            {
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_Carousel", sensorValue);
                AddReportTableTemplateToSensorStatusStateList(reportTableTemplate);
            }

            sensorValue = GetSensorStateValue(systemStatusDomain.TubeDetect.ToString());
            if (sensorValue != null)
            {
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_Tube", sensorValue);
                AddReportTableTemplateToSensorStatusStateList(reportTableTemplate);
            }

            sensorValue = GetSensorStateValue(systemStatusDomain.ReagentDoor.ToString());
            if (sensorValue != null)
            {
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_ReagentDoor", sensorValue);
                AddReportTableTemplateToSensorStatusStateList(reportTableTemplate);
            }

            sensorValue = GetSensorStateValue(systemStatusDomain.ReagentPack.ToString());
            if (sensorValue != null)
            {
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_ReagentPack", sensorValue);
                AddReportTableTemplateToSensorStatusStateList(reportTableTemplate);
            }

            sensorValue = GetSensorStateValue(systemStatusDomain.RadiusHome.ToString());
            if (sensorValue != null)
            {
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_RadiusHome", sensorValue);
                AddReportTableTemplateToMotorStatusStateList(reportTableTemplate);
            }

            sensorValue = GetSensorStateValue(systemStatusDomain.ThetaHome.ToString());
            if (sensorValue != null)
            {
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_ThetaHome", sensorValue);
                AddReportTableTemplateToMotorStatusStateList(reportTableTemplate);
            }

            sensorValue = GetSensorStateValue(systemStatusDomain.ProbeHome.ToString());
            if (sensorValue != null)
            {
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_ProbeHome", sensorValue);
                AddReportTableTemplateToMotorStatusStateList(reportTableTemplate);
            }
//TODO:... previous lines are commented out in CHM.

            sensorValue = GetSensorStateValue(systemStatusDomain.FocusHome.ToString());
            if (sensorValue != null)
            {
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_FocusHome", sensorValue);
                AddReportTableTemplateToMotorStatusStateList(reportTableTemplate);
            }

//TODO:... next lines are commented out in CHM.
            sensorValue = GetSensorStateValue(systemStatusDomain.ReagentUpper.ToString());
            if (sensorValue != null)
            {
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_ReagentUpper", sensorValue);
                AddReportTableTemplateToMotorStatusStateList(reportTableTemplate);
            }

            sensorValue = GetSensorStateValue(systemStatusDomain.ReagentLower.ToString());
            if (sensorValue != null)
            {
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_ReagentLower", sensorValue);
                AddReportTableTemplateToMotorStatusStateList(reportTableTemplate);
            }
           
            // set carousel position
            string positionValue ="--";
            switch (systemStatusDomain.CarouselDetect)
            {
                case eSensorStatus.ssStateActive:
                    if (systemStatusDomain.SamplePosition.IsValid())
                        positionValue = ScoutUtilities.Misc.ConvertToString(systemStatusDomain.SamplePosition.Column);
                    break;
                case eSensorStatus.ssStateInactive:
                    if (systemStatusDomain.SamplePosition.IsValid())
                        positionValue = systemStatusDomain.SamplePosition.Row.ToString() + systemStatusDomain.SamplePosition.Column;
                    break;
            }
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Report_carousel", positionValue);
            AddReportTableTemplateToStatusStateList(reportTableTemplate);
//TODO:... previous lines are commented out in CHM.

            var valve = LowLevelModel.svc_GetValvePort(); 
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_ButtonContent_Valve", valve);
            AddReportTableTemplateToStatusStateList(reportTableTemplate);
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate(
                "LID_ButtonContent_Syringe", ScoutUtilities.Misc.ConvertToString(systemStatusDomain.SyringePosition));
            AddReportTableTemplateToStatusStateList(reportTableTemplate);
           

//TODO:... next lines are commented out in CHM.
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_Probe",
                systemStatusDomain.MotorProbePosition.ToString());
            AddReportTableTemplateToStatusStateList(reportTableTemplate);
//TODO:... previous lines are commented out in CHM.

            const string volt = " V";
//TODO:... next lines are commented out in CHM.
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_Motortheta_WithoutColon",
                ScoutUtilities.Misc.ConvertToString(systemStatusDomain.MotorThetaPosition));
            AddReportTableTemplateToStatusStateList(reportTableTemplate);
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_MotorRadius_WithoutColon",
                ScoutUtilities.Misc.ConvertToString(systemStatusDomain.MotorRadiusPosition));
            AddReportTableTemplateToStatusStateList(reportTableTemplate);
//TODO:... previous lines are commented out in CHM.

            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_MotorFocus",
                ScoutUtilities.Misc.ConvertToString(systemStatusDomain.MotorFocusPosition));
            AddReportTableTemplateToStatusStateList(reportTableTemplate);
//TODO:... next lines are commented out in CHM.
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_MotorReagent",
                ScoutUtilities.Misc.ConvertToString(systemStatusDomain.MotorReagentPosition));
            AddReportTableTemplateToStatusStateList(reportTableTemplate);
//TODO:... previous lines are commented out in CHM.

            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Report_Pos33Volt",
               ScoutUtilities.Misc.UpdateTrailingPoint(systemStatusDomain.Voltage3_3V, TrailingPoint.Two) + volt);
            AddReportTableTemplateToVoltageList(reportTableTemplate);
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Report_Pos5VoltSensor",
                ScoutUtilities.Misc.UpdateTrailingPoint(systemStatusDomain.Voltage5vSensor, TrailingPoint.Two) + volt);
            AddReportTableTemplateToVoltageList(reportTableTemplate);
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Report_Pos5VoltCircuit",
                ScoutUtilities.Misc.UpdateTrailingPoint(systemStatusDomain.Voltage5vCircuit, TrailingPoint.Two) + volt);
            AddReportTableTemplateToVoltageList(reportTableTemplate);
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Report_Pos12Volt",
                ScoutUtilities.Misc.UpdateTrailingPoint(systemStatusDomain.Voltage12v, TrailingPoint.Two) + volt);
            AddReportTableTemplateToVoltageList(reportTableTemplate);
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Report_Pos24Volt",
                ScoutUtilities.Misc.UpdateTrailingPoint(systemStatusDomain.Voltage24v, TrailingPoint.Two) + volt);
            AddReportTableTemplateToVoltageList(reportTableTemplate);

            const string degreeCelsius = "°C";
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Report_Control_Board",
                ScoutUtilities.Misc.UpdateTrailingPoint(systemStatusDomain.TemperatureControlBoard, TrailingPoint.One)+ degreeCelsius);
            AddReportTableTemplateToTemperatureList(reportTableTemplate);
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_CPU",
                ScoutUtilities.Misc.UpdateTrailingPoint(systemStatusDomain.TemperatureCPU, TrailingPoint.One) + degreeCelsius);
            AddReportTableTemplateToTemperatureList(reportTableTemplate);
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_Opticalcase",
                ScoutUtilities.Misc.UpdateTrailingPoint(systemStatusDomain.TemperatureOpticalCase, TrailingPoint.One) + degreeCelsius);
            AddReportTableTemplateToTemperatureList(reportTableTemplate);
        }

        private void SetStorageTableData()
        {
            try
            {
                _diskSpaceModel.CalculateDiskSpace();

                _instrumentStatusReportModel.InstrumentStatusReportDomain
                    .InstrumentStatusReportTableHeaderDomainList[0].StorageSubHeader = 
                    string.Format(LanguageResourceHelper.Get("LID_Label_Space"),
                        ScoutUtilities.Misc.ConvertBytesToSize(_diskSpaceModel.TotalFreeSpace),
                        ScoutUtilities.Misc.ConvertBytesToSize(_diskSpaceModel.TotalDiskSpace));

                var reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_CheckBox_Other",
                     ScoutUtilities.Misc.ConvertBytesToSize(_diskSpaceModel.SizeOfOther));
                  
                AddReportTableTemplateToStorageList(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_CheckBox_Data",
                     ScoutUtilities.Misc.ConvertBytesToSize(_diskSpaceModel.SizeOfData));
                   
                AddReportTableTemplateToStorageList(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_CheckBox_Export",
                    ScoutUtilities.Misc.ConvertBytesToSize(_diskSpaceModel.SizeOfExport));
                
                AddReportTableTemplateToStorageList(reportTableTemplate);
            }
            catch (UnauthorizedAccessException unauthorizedAccessException)
            {
                ExceptionHelper.HandleExceptions(unauthorizedAccessException,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_DISKSPACE_PERMISSION_ERROR"));
            }
            catch (DirectoryNotFoundException directoryNotFoundException)
            {
                ExceptionHelper.HandleExceptions(directoryNotFoundException,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_EXPORT_BROWSE_FILE"));
            }
            catch (IOException ioException)
            {
                ExceptionHelper.HandleExceptions(ioException,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_FILE_ERROR"));
            }
            catch (Exception exception)
            {
                ExceptionHelper.HandleExceptions(exception,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_UNABLETOCALCULATEDISKSPACE"));
            }
        }

        private void AddReportTableTemplateToStorageList(ReportTableTemplate reportTableTemplate)
        {
            if (reportTableTemplate != null)
            {
                _instrumentStatusReportModel.InstrumentStatusReportDomain
                    .InstrumentStatusReportStorageList.Add(reportTableTemplate);
            }
        }

        private void AddReportTableTemplateToLowerLevelList(ReportTableTemplate reportTableTemplate)
        {
            if (reportTableTemplate != null)
            {
                _instrumentStatusReportModel.InstrumentStatusReportDomain
                    .InstrumentStatusReportLowerLevelList.Add(reportTableTemplate);
            }
        }

        private void AddReportTableTemplateToSensorStatusStateList(ReportTableTemplate reportTableTemplate)
        {
            if (reportTableTemplate != null)
            {
                _instrumentStatusReportModel.InstrumentStatusReportDomain
                    .InstrumentStatusReportSensorStatusStateList.Add(reportTableTemplate);
            }
        }

        private void AddReportTableTemplateToMotorStatusStateList(ReportTableTemplate reportTableTemplate)
        {
            if (reportTableTemplate != null)
            {
                _instrumentStatusReportModel.InstrumentStatusReportDomain
                    .InstrumentStatusReportMotorStatusStateList.Add(reportTableTemplate);
            }
        }

        private void AddReportTableTemplateToStatusStateList(ReportTableTemplate reportTableTemplate)
        {
            if (reportTableTemplate != null)
            {
                _instrumentStatusReportModel.InstrumentStatusReportDomain
                    .InstrumentStatusReportStatusStateList.Add(reportTableTemplate);
            }
        }

        private void AddReportTableTemplateToVoltageList(ReportTableTemplate reportTableTemplate)
        {
            if (reportTableTemplate != null)
            {
                _instrumentStatusReportModel.InstrumentStatusReportDomain
                    .InstrumentStatusReportVoltageList.Add(reportTableTemplate);
            }
        }

        private void AddReportTableTemplateToTemperatureList(ReportTableTemplate reportTableTemplate)
        {
            if (reportTableTemplate != null)
            {
                _instrumentStatusReportModel.InstrumentStatusReportDomain
                    .InstrumentStatusReportTemperatureList.Add(reportTableTemplate);
            }
        }

        private void AddReportTableTemplateToConcSlopeList(ReportTableTemplate reportTableTemplate)
        {
            if (reportTableTemplate != null)
            {
                _instrumentStatusReportModel.InstrumentStatusReportDomain
                    .InstrumentStatusReportConcSlopeTableList.Add(reportTableTemplate);
            }
        }

        private void AddReportTableTemplateToACupConcSlopeList(ReportTableTemplate reportTableTemplate)
        {
            if (reportTableTemplate != null)
            {
                _instrumentStatusReportModel.InstrumentStatusReportDomain
                    .InstrumentStatusReportACupConcSlopeTableList.Add(reportTableTemplate);
            }
        }

        private void SetCalibrationTableData()
        {
            if (_calErrorLogs != null && _calErrorLogs.Count > 0)
            {
                var concData = _calErrorLogs.OrderByDescending(t => t.Date).First();
                if (concData.Consumable != null && concData.Consumable.Count > 0)
                {
                    foreach (var conc in concData.Consumable)
                    {
                        var instrumentStatusReportCalibrationDataDomain = new InstrumentStatusReportCalibrationDomainTable
                            {
                                AssayValue = ScoutUtilities.Misc.ConvertToPower(conc.AssayValue),
                                ExpirationDate = ScoutUtilities.Misc.ConvertToString(conc.ExpirationDate),
                                LotNumber = (conc.LotId == "n/a" ? " - " : conc.LotId)
                            };
                        _instrumentStatusReportModel.InstrumentStatusReportDomain
                            .InstrumentStatusReportConcDataDomainList
                            .Add(instrumentStatusReportCalibrationDataDomain);
                    }

                    for (var index = 0;
                        index < _instrumentStatusReportModel.InstrumentStatusReportDomain
                            .InstrumentStatusReportConcDataDomainList.Count;
                        index++)
                    {
                        switch (index)
                        {
                            case 0:
                                _instrumentStatusReportModel.InstrumentStatusReportDomain
                                        .InstrumentStatusReportConcDataDomainList[0].LabelName =
                                    LanguageResourceHelper.Get("LID_Report_2M");
                                break;
                            case 1:
                                _instrumentStatusReportModel.InstrumentStatusReportDomain
                                        .InstrumentStatusReportConcDataDomainList[1].LabelName =
                                    LanguageResourceHelper.Get("LID_Report_4M");
                                break;
                            case 2:
                                _instrumentStatusReportModel.InstrumentStatusReportDomain
                                        .InstrumentStatusReportConcDataDomainList[2].LabelName =
                                    LanguageResourceHelper.Get("LID_Report_10M");
                                break;
                        }
                    }
                }

                const int defaultIntercept = 0;
                var reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_AdminReportsHeader_UserName",
                    concData.UserId);
                 var slope = Math.Round(concData.Slope, 4);
                AddReportTableTemplateToConcSlopeList(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_GraphLabel_Slope",
                    ScoutUtilities.Misc.ConvertToString(slope));
                AddReportTableTemplateToConcSlopeList(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Report_Intercept",
                    ScoutUtilities.Misc.ConvertToString(defaultIntercept));
                AddReportTableTemplateToConcSlopeList(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate(
                    "LID_Label_LastCalibrationDate_WithoutColon",
                    ScoutUtilities.Misc.ConvertToString(concData.Date));
                AddReportTableTemplateToConcSlopeList(reportTableTemplate);
            }
        }

        private void SetACupCalibrationTableData()
        {
          
            if (_aCupCalErrorLogs != null && _aCupCalErrorLogs.Count > 0)
            {
                var aCupConcData = _aCupCalErrorLogs.OrderByDescending(t => t.Date).First();
                if (aCupConcData.Consumable != null && aCupConcData.Consumable.Count > 0)
                {
                    foreach (var conc in aCupConcData.Consumable)
                    {
                        var instrumentStatusReportACupCalibrationDataDomain = new InstrumentStatusReportCalibrationDomainTable
                        {
                            AssayValue = ScoutUtilities.Misc.ConvertToPower(conc.AssayValue),
                            ExpirationDate = ScoutUtilities.Misc.ConvertToString(conc.ExpirationDate),
                            LotNumber = (conc.LotId == "n/a" ? " - " : conc.LotId)
                        };
                        _instrumentStatusReportModel.InstrumentStatusReportDomain
                            .InstrumentStatusReportACupConcDataDomainList
                            .Add(instrumentStatusReportACupCalibrationDataDomain);
                    }

                    for (var index = 0;
                        index < _instrumentStatusReportModel.InstrumentStatusReportDomain
                            .InstrumentStatusReportACupConcDataDomainList.Count;
                        index++)
                    {
                        switch (index)
                        {
                            case 0:
                                _instrumentStatusReportModel.InstrumentStatusReportDomain
                                        .InstrumentStatusReportACupConcDataDomainList[0].LabelName =
                                    LanguageResourceHelper.Get("LID_Report_2M");
                                break;
                            case 1:
                                _instrumentStatusReportModel.InstrumentStatusReportDomain
                                        .InstrumentStatusReportACupConcDataDomainList[1].LabelName =
                                    LanguageResourceHelper.Get("LID_Report_4M");
                                break;
                            case 2:
                                _instrumentStatusReportModel.InstrumentStatusReportDomain
                                        .InstrumentStatusReportACupConcDataDomainList[2].LabelName =
                                    LanguageResourceHelper.Get("LID_Report_10M");
                                break;
                        }
                    }
                }

                const int defaultIntercept = 0;
                var reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_AdminReportsHeader_UserName",
                    aCupConcData.UserId);
                var slope = Math.Round(aCupConcData.Slope, 4);
                AddReportTableTemplateToACupConcSlopeList(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_GraphLabel_Slope",
                    ScoutUtilities.Misc.ConvertToString(slope));
                AddReportTableTemplateToACupConcSlopeList(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Report_Intercept",
                    ScoutUtilities.Misc.ConvertToString(defaultIntercept));
                AddReportTableTemplateToACupConcSlopeList(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate(
                    "LID_Label_LastACupCalibrationDate_WithoutColon",
                    ScoutUtilities.Misc.ConvertToString(aCupConcData.Date));
                AddReportTableTemplateToACupConcSlopeList(reportTableTemplate);
            }
        }

        private void SetAppTypesTableData()
        {
            const int analysisCount = 1;
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportAppTypesTableList.Add(
                new ReportTableTemplate
                {
                    ParameterName = analysisCount + LanguageResourceHelper.Get("LID_Label_Dot_Append") + " " + _analysisType
                });
        }

        private void SetCellTypesTableData()
        {
            try
            {
                var i = 1;
                var cellTypeCount = 1;

                var cellTypeInstance = new InstrumentStatusReportTableColumn();
                var cellList = LoggedInUser.CurrentUser?.AssignedCellTypes;

                foreach (var cell in cellList)
                {
                    switch (i)
                    {
                        case 1:
                            cellTypeInstance.ColumnOne = cellTypeCount + LanguageResourceHelper.Get("LID_Label_Dot_Append")+" " + cell.CellTypeName;
                            i++;
                            cellTypeCount++;
                            break;
                        case 2:
                            cellTypeInstance.ColumnTwo = cellTypeCount + LanguageResourceHelper.Get("LID_Label_Dot_Append") + " " + cell.CellTypeName;
                            i++;
                            cellTypeCount++;
                            break;
                        case 3:
                            cellTypeInstance.ColumnThree = cellTypeCount + LanguageResourceHelper.Get("LID_Label_Dot_Append") + " " + cell.CellTypeName;
                            i++;
                            cellTypeCount++;
                            break;
                        case 4:
                            cellTypeInstance.ColumnFour = cellTypeCount + LanguageResourceHelper.Get("LID_Label_Dot_Append") + " " + cell.CellTypeName;
                            i++;
                            cellTypeCount++;
                            break;
                    }
                    if (i == 5)
                    {
                        i = 1;
                        _instrumentStatusReportModel.InstrumentStatusReportDomain
                  .InstrumentStatusReportCellTypesTableList.Add(cellTypeInstance);
                        cellTypeInstance = new InstrumentStatusReportTableColumn();
                    }
                }
                _instrumentStatusReportModel.InstrumentStatusReportDomain
                    .InstrumentStatusReportCellTypesTableList.Add(cellTypeInstance);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONCREATECELLTYPE"));
            }
        }

        private void SetUsersTableData()
        {
            try
            {
                var i = 1;
                var userCount = 1;

                var userTypeInstance = new InstrumentStatusReportTableColumn();
                foreach (var user in _userList)
                {
                    switch (i)
                    {
                        case 1:
                            userTypeInstance.ColumnOne = userCount+ LanguageResourceHelper.Get("LID_Label_Dot_Append") + " " + user.UserID;
                            i++;
                            userCount++;
                            break;
                        case 2:
                            userTypeInstance.ColumnTwo = userCount + LanguageResourceHelper.Get("LID_Label_Dot_Append") + " " + user.UserID;
                            i++;
                            userCount++;
                            break;
                        case 3:
                            userTypeInstance.ColumnThree = userCount + LanguageResourceHelper.Get("LID_Label_Dot_Append") + " " + user.UserID;
                            i++;
                            userCount++;
                            break;
                        case 4:
                            userTypeInstance.ColumnFour = userCount + LanguageResourceHelper.Get("LID_Label_Dot_Append") + " " + user.UserID;
                            i++;
                            userCount++;
                            break;
                    }

                    if (i == 5)
                    {
                        i = 1;
                        _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportUsersTableList.Add(userTypeInstance);
                        userTypeInstance = new InstrumentStatusReportTableColumn();
                    }

                }
                _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportUsersTableList.Add(userTypeInstance);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_USER_SELECTION_ERROR"));
            }
        }
  
        private void SetReagentParameterData()
        {
            var reagentInfoRecords = _reagentContainers;
            const int defaultColumnCount = 1;
            List<string> containers;
            int columnCount;
            if (reagentInfoRecords == null || 
                reagentInfoRecords.FirstOrDefault()?.Status == ReagentContainerStatus.eNotDetected)
            {
                return;
            }

            if (!LoggedInUser.CurrentUserId.Equals(ApplicationConstants.ServiceUser))
            {

                containers = reagentInfoRecords.FirstOrDefault()?.ReagentNames
                    ?.Where(r => r.Key != null && r.Key.ToString().Equals(ApplicationConstants.ReagentNameForAllUser))
                    .Select(r => r.Key.ToString()).ToList();
                if (containers?.Count <= 0)
                    return;
                columnCount = defaultColumnCount;
            }
            else
            {
                containers = reagentInfoRecords.FirstOrDefault()?.ReagentNames?.Select(r => r.Key.ToString()).ToList();
                if (containers?.Count <= 0)
                    return;
                columnCount = reagentInfoRecords.Select(x => x.ReagentNames.Count).FirstOrDefault();
            }

            var parameterNames = GetReagentHeaders();
            var reagentContainerAll = reagentInfoRecords.FirstOrDefault()?.ReagentNames?.ToList();
            var parList = new List<List<string>>();
            var reagentList = new List<ReagentContainerStateDomain>();
            if (reagentContainerAll != null)
            {
                foreach (var reagentContainer in reagentContainerAll)
                {
                    reagentList.Add(new ReagentContainerStateDomain()
                    {
                        ContainerName = reagentContainer.Key,
                        PartNumber = reagentContainer.Value.Select(x => x.PartNumber?.ToString()).FirstOrDefault(),
                        LotInformation = reagentContainer.Value.Select(x => x.LotInformation?.ToString()).FirstOrDefault(),
                        ExpiryDate = reagentContainer.Value.Select(x => x.ExpiryDate).FirstOrDefault(),
                        EventsPossible = reagentContainer.Value.Select(x => x.EventsPossible).FirstOrDefault(),
                        EventsRemaining = Convert.ToUInt16(reagentContainer.Value.Select(x => x.EventsRemaining).FirstOrDefault())
                    });
                }
            }
            
            for (var i = 0; i < columnCount; i++)
            {
                if (!LoggedInUser.CurrentUserId.Equals(ApplicationConstants.ServiceUser))
                {
                    var trypanList = reagentList?.Where(x => x.ContainerName.ToString().Equals(ApplicationConstants.ReagentNameForAllUser)).ToList();
                    var first = trypanList.FirstOrDefault();
                    if (first != null)
                    {
                        var listAllUser = new List<string>
                        {
                            first.ContainerName,
                            first.PartNumber.ToString(),
                            first.LotInformation.ToString(),
                            ScoutUtilities.Misc.ConvertToString(first.ExpiryDate),
                            ScoutUtilities.Misc.ConvertToString(first.EventsPossible),
                            ScoutUtilities.Misc.ConvertToString(first.EventsRemaining)
                        };
                        parList.Add(listAllUser);
                    }
                }
                else
                {
                    var listServiceUser = new List<string>
                    {
                        LoggedInUser.CurrentUserId.Equals(ApplicationConstants.ServiceUser)
                            ? GetPropertyValue(containers, i)
                            : GetPropertyValue(containers, containers.Count - 1),
                        reagentList[i]?.PartNumber.ToString(),
                        reagentList[i]?.LotInformation.ToString(),
                        ScoutUtilities.Misc.ConvertToString(reagentList[i]?.ExpiryDate),
                        ScoutUtilities.Misc.ConvertToString(reagentList[i]?.EventsPossible),
                        ScoutUtilities.Misc.ConvertToString(reagentList[i]?.EventsRemaining)
                    };
                    parList.Add(listServiceUser);
                }                
            }

            if (!parList.Any())
            {
                return;
            }

            // Iterate the values based on default row count
            for (var i = 0; i < DefaultRowCount; i++)
            {
                // Set the value for ColumnOne, if the parameter list count is one  
                var parameter = new RunResultReagentParameterDomain
                {
                    Name = parameterNames[i],
                    ColumnOne = parList[0][i]
                };

                switch (parList.Count)
                {
                    // Set the value for ColumnTwo ,ColumnThree and ColumnFour and followed by ColumnOne,  if the parameter list count is four
                    case 4:
                        parameter.ColumnTwo = parList[1][i];
                        parameter.ColumnThree = parList[2][i];
                        parameter.ColumnFour = parList[3][i];
                        break;
                    // Set the value for ColumnTwo and ColumnThree and followed by ColumnOne, if the parameter list count is three
                    case 3:
                        parameter.ColumnTwo = parList[1][i];
                        parameter.ColumnThree = parList[2][i];
                        break;
                    // Set the value for ColumnTwo and followed by ColumnOne, if the parameter list count is two
                    case 2:
                        parameter.ColumnTwo = parList[1][i];
                        break;
                }

                _instrumentStatusReportModel?.InstrumentStatusReportDomain?
                    .InstrumentStatusReagentParameterList?.Add(parameter);
            }

            var reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Report_RemainingCapacity",
               ScoutUtilities.Misc.ConvertToString(_systemStatus.SampleTubeDisposalRemainingCapacity));
            _instrumentStatusReportModel?.InstrumentStatusReportDomain?.InstrumentStatusReportRemainingTubesList?.Add(reportTableTemplate);
        }

        private List<string> GetReagentHeaders()
        {
            var parameterNames = new List<string>();
            // Setting default header  
            for (var headerId = 1; headerId <= DefaultRowCount; headerId++)
            {
                switch (headerId)
                {
                    case 1:
                        parameterNames.Add(GetResourceKeyName("LID_ResultHeader_Container"));
                        break;
                    case 2:
                        parameterNames.Add(GetResourceKeyName("LID_Label_PN"));
                        break;
                    case 3:
                        parameterNames.Add(GetResourceKeyName("LID_QCHeader_LotNumber"));
                        break;
                    case 4:
                        parameterNames.Add(GetResourceKeyName("LID_Label_EffectiveExpiration"));
                        break;                  
                    case 5:
                        parameterNames.Add(GetResourceKeyName("LID_Report_UsesTotal"));
                        break;
                    case 6:
                        parameterNames.Add(GetResourceKeyName("LID_Report_UsesRemaining"));
                        break;
                }
            }

            return parameterNames;
        }

        private string GetPropertyValue(IReadOnlyList<string> value, int index)
        {
            return value.Any() ? value[index] : string.Empty;
        }

        private void AddReportTableTemplateToAboutReportFirstTable(ReportTableTemplate reportTableTemplate)
        {
            if (reportTableTemplate != null)
            {
                _instrumentStatusReportModel.InstrumentStatusReportDomain
                    .InstrumentStatusReportAboutFirstTableList.Add(reportTableTemplate);
            }
        }

        private void AddReportTableTemplateToAboutReportSecondTable(ReportTableTemplate reportTableTemplate)
        {
            if (reportTableTemplate != null)
            {
                _instrumentStatusReportModel.InstrumentStatusReportDomain
                    .InstrumentStatusReportAboutSecondTableList.Add(reportTableTemplate);
            }
        }

        private void SetMandatoryTableData()
        {
            ReportTableTemplate reportTableTemplate;
            if (_hardwareSettings != null)
            {
                var hardwareSettingsDataInstance = _hardwareSettings;

                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_CheckBox_DeviceSerialNumber", hardwareSettingsDataInstance.SerialNumber);
                AddReportTableTemplateToAboutReportSecondTable(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_Softwareversion", UISettings.SoftwareVersion);
                AddReportTableTemplateToAboutReportFirstTable(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_Backendversion", hardwareSettingsDataInstance.HawkeyeCoreVersion);
                AddReportTableTemplateToAboutReportFirstTable(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_FirmwareVersion", hardwareSettingsDataInstance.FirmwareVersion);
                AddReportTableTemplateToAboutReportFirstTable(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_UIVersion", hardwareSettingsDataInstance.UIVersion);
                AddReportTableTemplateToAboutReportFirstTable(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_Imageanalysissoftwareversion", hardwareSettingsDataInstance.ImageAnalysisSoftwareVersion);
                AddReportTableTemplateToAboutReportFirstTable(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_SyringePumpFirmwareVersion", hardwareSettingsDataInstance.SyringePumpFWVersion);
                AddReportTableTemplateToAboutReportFirstTable(reportTableTemplate);
                reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Label_CameraFirmwareVersion_WithoutColon", hardwareSettingsDataInstance.CameraFirmwareVersion);
                AddReportTableTemplateToAboutReportFirstTable(reportTableTemplate);
            }

            string health;
            switch (_systemStatus.SystemStatus)
            {
                case SystemStatus.Faulted:
                    health = LanguageResourceHelper.Get("LID_StatusLabel_Fault");
                    break;
                default:
                    health = LanguageResourceHelper.Get("LID_ButtonContent_OK");
                    break;
            }

            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Report_NoOfSampleLife",
                ScoutUtilities.Misc.ConvertToString(_systemStatus.SystemTotalSampleCount));
            AddReportTableTemplateToAboutReportSecondTable(reportTableTemplate);
            reportTableTemplate = ReportWindowModel.CreateReportTableTemplate("LID_Report_InstrumentHealth",
                health);
            AddReportTableTemplateToAboutReportSecondTable(reportTableTemplate);
        }

        private void SetSystemErrorList()
        {
            var systemErrorDomainList = _systemStatus.SystemErrorDomainList;
            if (systemErrorDomainList.Count == 0)
            {
                return;
            }

            _instrumentStatusTableVisibility = new InstrumentStatusTableVisibility
            {
                SystemErrorVisibility = false
            };

            systemErrorDomainList.ForEach(error =>
            {
//TODO: do we need to do this???   
                var instrumentStatusReportErrorDomain = new InstrumentStatusReportErrorDomain
                {
                    SeverityKey = error.SeverityKey,
                    SeverityDisplayValue = error.SeverityDisplayValue,
                    System = error.System,
                    Instance = error.Instance,
                    Description = error.FailureMode,
                    Subsystem = error.SubSystem
                };
                
                _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportSystemErrorList.Add(instrumentStatusReportErrorDomain);
            });
        }

        private void SetTableHeader()
        {
            _instrumentStatusReportTableHeaderDomain = new InstrumentStatusReportTableHeaderDomain()
            {
                ReagentsHeader = LanguageResourceHelper.Get("LID_ResultHeader_Reagents"),
                CellTypesHeader = LanguageResourceHelper.Get("LID_GridLabel_CellTypes"),
                AppTypes = LanguageResourceHelper.Get("LID_Report_AnalysisType"),
                CalibrationHeader = LanguageResourceHelper.Get("LID_TabItem_CalibrationControl"),
                ACupCalibrationHeader = LanguageResourceHelper.Get("LID_TabItem_ACupConcSlope"),
                AssayValueHeader = LanguageResourceHelper.Get("LID_DataGridHeader_AssayValue"),
                LotNumberHeader = LanguageResourceHelper.Get("LID_QCHeader_LotNumber"),
                ExpirationDateHeader = LanguageResourceHelper.Get("LID_Label_ExpirationDate"),
                ConcTableHeader = LanguageResourceHelper.Get("LID_TabItem_Concentration"),
                AboutHeader = LanguageResourceHelper.Get("LID_Lable_AboutInstument"),
                SeverityHeader = LanguageResourceHelper.Get("LID_DataGrid_Header"),
                SystemHeader = LanguageResourceHelper.Get("LID_DataGrid_System"),
                SubsystemHeader = LanguageResourceHelper.Get("LID_DataGrid_SubSystem"),
                InstanceHeader = LanguageResourceHelper.Get("LID_DataGrid_Instance"),
                DescriptionHeader = LanguageResourceHelper.Get("LID_Label_Description"),
                SystemErrorHeader = LanguageResourceHelper.Get("LID_Label_SystemError"),
                StatusInformationHeader = LanguageResourceHelper.Get("LID_Report_StatusInfo_Header"),
                LowLevelsControlsHeader = LanguageResourceHelper.Get("LID_FrameLabel_LowLevelControls"),
                SensorStatusStateHeader = LanguageResourceHelper.Get("LID_Label_SensorStatusState"),
                MotorStatusStateHeader = LanguageResourceHelper.Get("LID_Label_MotorStatusState"),
                StatusStateHeader = LanguageResourceHelper.Get("LID_Report_StatusState"),
                VoltageStateHeader = LanguageResourceHelper.Get("LID_Label_Voltage"),
                TempStateHeader = LanguageResourceHelper.Get("LID_Label_Temperature"),
                OpticsHeader = LanguageResourceHelper.Get("LID_TabItem_Optics"),
                StorageHeader = LanguageResourceHelper.Get("LID_FrameLabel_Storage")
            };

            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportTableHeaderDomainList.Add(_instrumentStatusReportTableHeaderDomain);
        }

        private void SetMandatoryHeaderData()
        {
            var reportMandatoryHeaderDomain = ReportWindowModel.SetReportHeaderData(_printTitle, "LID_Label_InstrumentStatusReport", _comments);
            _instrumentStatusReportModel.InstrumentStatusReportDomain.InstrumentStatusReportMandatoryHeaderDomainList.Add(reportMandatoryHeaderDomain);
        }

        #endregion
    }
}
