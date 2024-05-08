using Microsoft.Reporting.WinForms;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutDomains.Reports.Common;
using ScoutDomains.Reports.RunResult;
using ScoutDomains.RunResult;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutModels.Reports;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using AForge.Imaging;
using ApiProxies.Extensions;
using Org.BouncyCastle.Math;
using ScoutModels;

namespace ScoutViewModels.ViewModels.Reports
{
    public class RunResultsReportViewModel : ReportWindowViewModel
    {
        public RunResultsReportViewModel(string printTitle, string comments, UserPermissionLevel userPermissionLevel, SampleRecordDomain sampleRecord,
            bool isAutoExportPDF, IEnumerable<ReportPrintOptions> reportPrintOptions, List<BarGraphDomain> graphList,
            IEnumerable<ReportGraphOptions> reportGraphOptions, string reportImageCaption)
        {
            _runResultsReportModel = new RunResultsReportModel();
            _recordHelper = new ResultRecordHelper();
            _graphTableVisibleRowCount = 0;
            ReportTitle = LanguageResourceHelper.Get("LID_GridLabel_RunResult");
            ReportWindowTitle = LanguageResourceHelper.Get("LID_GridLabel_RunResult");
            ReportType = ReportType.RunResults;

            _printTitle = printTitle;
            _comments = comments;
            _permissionLevel = userPermissionLevel;
            _sampleRecord = sampleRecord;
            _isAutoExportPDF = isAutoExportPDF;
            _reportPrintOptions = reportPrintOptions;
            _graphList = graphList;
            _reportGraphOptions = reportGraphOptions;
            _reportImageCaption = reportImageCaption;
        }

        protected override void DisposeUnmanaged()
        {
            _recordHelper?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties

        const int DefaultRowCount = 6;

        private bool _isDisCardedImageVisible;
        private RunResultsReportModel _runResultsReportModel;
        private ResultRecordHelper _recordHelper;
        private int _graphTableVisibleRowCount; 
        private RunResultsReportGraphTitlesDomain _runResultsReportGraphTitlesDomain;
        private List<BarGraphDomain> _listOfGraphsToBeDisplayed;
        private List<int> _coordinatesCountList;
        private RunResultTableVisibility _runResultTableVisibility;
        private RunResultGraphVisibility _runResultsReportParametersDomain;

        private string _printTitle;
        private string _comments;
        private UserPermissionLevel _permissionLevel;
        private SampleRecordDomain _sampleRecord;
        private bool _isAutoExportPDF;
        private IEnumerable<ReportPrintOptions> _reportPrintOptions;
        private List<BarGraphDomain> _graphList;
        private IEnumerable<ReportGraphOptions> _reportGraphOptions;
        private string _reportImageCaption;

        #endregion

        #region Public Methods

        public void AutoExportExecute(string fullFilePath)
        {
            SaveToPdfAndNotify(fullFilePath);
        }

        #endregion

        #region Override Methods

        public override void LoadReport()
        {
            InitializeLists();
            RetrieveData();
            LoadReportViewer();
            DisposeManaged();
        }

        protected override void RetrieveData()
        {
            SetHeaderData();
            SetSampleDetailTable();
            SetMandatoryData();
            CheckForSelectedPrintOptions();
            _runResultTableVisibility = new RunResultTableVisibility();
            SetTableVisibility();
            FetchGraphData();
        }

        protected override void LoadReportViewer()
        {
            ReportViewer = new ReportViewer();
            ReportViewer.LocalReport.DisplayName = LanguageResourceHelper.Get("LID_GridLabel_RunResult");
            ReportViewer.LocalReport.EnableExternalImages = true;

            var lang = LanguageEnums.GetLangType(ApplicationLanguage.GetLanguage());
            var reportViewerPath = GetReportViewerPath(lang);
            var runResultDomain = _runResultsReportModel.RunResultsReportDomainInstance;

            AddReportDataSource("RunResultsReportGraphAxesDataTBL", runResultDomain.RunResultsReportGraphAxesDataList);
            AddReportDataSource("RunResultsReportGraphDetailsTBL", runResultDomain.RunResultsReportGraphTitlesDomainList);
            AddReportDataSource("RunResultReportResultsTBL", runResultDomain.RunResultAboutReportParameterDomainList);
            AddReportDataSource("RunResultAnalysisParameterTBL", runResultDomain.RunResultAnalysisParameterReportDomainList);
            AddReportDataSource("RunResultsSignatureDataTBL", runResultDomain.RunResultSignatureColumnList);
            AddReportDataSource("RunResultTableHeaderDomainTBL", runResultDomain.RunResultTableHeaderDomainList);
            AddReportDataSource("RunResultsReportHeaderTBL", runResultDomain.ReportMandatoryHeaderDomainList);
            AddReportDataSource("RunResultReportSampleDetailsTBL", runResultDomain.SampleDetailsList);
            AddReportDataSource("RunResultImageDetailsTBL", runResultDomain.ImageDetailsList);
            AddReportDataSource("RunResultTableVisbilityTBL", runResultDomain.RunResultTableVisbilityList);
            AddReportDataSource("RunResultsGraphVisibilityTBL", runResultDomain.RunResultGraphVisibilityList);
            AddReportDataSource("RunResultRegentParametersSampleTBL", runResultDomain.RunResultReagentParameterList);
            AddReportDataSource("RunResultDiscardedImageTBL", runResultDomain.RunResultDiscardedImageList);

            RefreshAndSetReportContent(reportViewerPath);

			var DefaultImageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ApplicationConstants.TargetFolderName);
			var imageName = ApplicationConstants.ImageFilenameForPDF + ApplicationConstants.ImageFileExtension;
			var imagePath = Path.Combine(DefaultImageDirectory, imageName);

			ReportViewer.LocalReport.SetParameters(new ReportParameter("ImagePath", imagePath));

            if (!_isAutoExportPDF)
            {
                ReportViewer.LocalReport.Refresh();
                ReportViewer.RefreshReport();
            }
        }

        protected override void DisposeManaged()
        {
            _runResultsReportGraphTitlesDomain = null;
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList = null;
        }

        protected override void InitializeLists()
        {
            _runResultsReportModel.RunResultsReportDomainInstance = new RunResultsReportDomain();
            _runResultTableVisibility = new RunResultTableVisibility();
            _runResultsReportParametersDomain = new RunResultGraphVisibility();
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultAboutReportParameterDomainList = new List<ReportTableTemplate>();
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultAnalysisParameterReportDomainList = new List<ReportTableTemplate>();
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultSignatureColumnList = new List<ReportThreeColumnDomain>();
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultTableHeaderDomainList = new List<RunResultTableHeaderDomain>();
            _runResultsReportModel.RunResultsReportDomainInstance.ReportMandatoryHeaderDomainList = new List<ReportMandatoryHeaderDomain>();
            _runResultsReportModel.RunResultsReportDomainInstance.SampleDetailsList = new List<ReportTableTemplate>();
            _runResultsReportModel.RunResultsReportDomainInstance.ImageDetailsList = new List<ReportTableTemplate>();
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultTableVisbilityList = new List<RunResultTableVisibility>();
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList = new List<RunResultsReportGraphDataListDomain>();
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportGraphTitlesDomainList = new List<RunResultsReportGraphTitlesDomain>();
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportGraphAxesDataList = new List<RunResultsReportGraphAxesDataDomain>();
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultGraphVisibilityList = new List<RunResultGraphVisibility>();
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultReagentParameterList = new List<RunResultReagentParameterDomain>();
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultDiscardedImageList = new List<RunResultDiscardedImageDomain>();
        }

        #endregion

        #region Private Methods

        private string GetReportViewerPath(LanguageType language)
        {
            if (language == LanguageType.eChinese)
                return AppDomain.CurrentDomain.BaseDirectory + @"Reports\RunResultsReportRdlcViewerZh-CN.rdlc";
            if (language == LanguageType.eJapanese)
                return AppDomain.CurrentDomain.BaseDirectory + @"Reports\RunResultsReportRdlcViewerJa-JP.rdlc";
            return AppDomain.CurrentDomain.BaseDirectory + @"Reports\RunResultsReportRdlcViewer.rdlc";
        }

        private void SaveToPdfAndNotify(string reportName)
        {
            if (ReportWindowModel.SaveToPdf(ReportViewer, reportName, ApplicationConstants.ReportDeviceInfoForPortrait))
            {
                MessageBus.Default.Publish(new SystemMessageDomain
                {
                    IsMessageActive = true,
                    Message = LanguageResourceHelper.Get("LID_MSGBOX_ExportSucces"),
                    MessageType = MessageType.Normal
                });
            }
        }

        private void CheckForSelectedPrintOptions()
        {
            foreach (var printOption in _reportPrintOptions)
            {
                var isAnalysisParameterSelected = printOption.ParameterName == GetResourceKeyName("LID_CheckBox_AnalysisParameters") &&
                                                  printOption.IsParameterChecked;
                if (isAnalysisParameterSelected)
                {
                    var analysisParameterList = ReportWindowModel.SetAnalysisParameter(_sampleRecord);
                    if (analysisParameterList != null)
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultAnalysisParameterReportDomainList = analysisParameterList.ToList();
                }

                var isSignatureSelected = printOption.ParameterName == LanguageResourceHelper.Get("LID_CheckBox_Signatures") &&
                                          printOption.IsParameterChecked;
                if (isSignatureSelected)
                {
                    var signatureList = ReportWindowModel.GetSignaturesForReport(_sampleRecord.SelectedResultSummary?.SignatureList);
                    if (signatureList != null)
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultSignatureColumnList = signatureList.ToList();
                }

                var isImageDetailSelected = printOption.ParameterName == LanguageResourceHelper.Get("LID_Label_FirstAnnotatedImage") &&
                                            printOption.IsParameterChecked;
                if (isImageDetailSelected)
                {
                    SetImageData();
                }

                if (printOption.ParameterName == LanguageResourceHelper.Get("LID_Label_ReagentParameters") &&
                    printOption.IsParameterChecked)
                {
                    SetReagentParameterData();
                }
            }
        }

        private void SetTableVisibility()
        {
            foreach (var printOption in _reportPrintOptions)
            {
                if (printOption.ParameterName ==
                    LanguageResourceHelper.Get("LID_Label_ResultParameters"))
                {
                    _runResultTableVisibility.IsResultTableHidden = !printOption.IsParameterChecked;
                }
                else if (printOption.ParameterName ==
                    LanguageResourceHelper.Get("LID_CheckBox_AnalysisParameters"))
                {
                    _runResultTableVisibility.IsAnalysisTableHidden = !printOption.IsParameterChecked;
                }
                else if (printOption.ParameterName ==
                         LanguageResourceHelper.Get("LID_CheckBox_Signatures"))
                {
                    _runResultTableVisibility.IsSigntaureTableHidden = !printOption.IsParameterChecked;
                }
                else if (printOption.ParameterName ==
                         LanguageResourceHelper.Get("LID_Label_FirstAnnotatedImage"))
                {
                    _runResultTableVisibility.IsImageTableHidden = !printOption.IsParameterChecked;
                }
                else if (printOption.ParameterName ==
                         LanguageResourceHelper.Get("LID_Label_ReagentParameters"))
                {
                    _runResultTableVisibility.IsReagentTableHidden = !printOption.IsParameterChecked;
                }
            }
            _runResultTableVisibility.IsImageDiscardedTableHidden = !_isDisCardedImageVisible;
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultTableVisbilityList.Add(
                _runResultTableVisibility);
        }

        private void SetSampleDetailTable()
        {
            try
            {
                var sampleRecordDomainInstance = _sampleRecord;
                var reportTableTemplateObj =
                    ReportWindowModel.CreateReportTableTemplate("LID_QMgmtHEADER_SampleId",
                        sampleRecordDomainInstance.SampleIdentifier);
                AddReportTableTemplateToSampleDetailList(reportTableTemplateObj);
                if (sampleRecordDomainInstance.SelectedResultSummary != null)
                {
                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                        "LID_Report_SampleDate",
                        ScoutUtilities.Misc.ConvertToCustomLongDateTimeFormat(sampleRecordDomainInstance
                            .SelectedResultSummary.RetrieveDate));
                    AddReportTableTemplateToSampleDetailList(reportTableTemplateObj);
                }

                reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                    "LID_Report_AnalysisVersion",
                    HardwareManager.HardwareSettingsModel.HardwareSettingsDomain.ImageAnalysisSoftwareVersion);
                AddReportTableTemplateToSampleDetailList(reportTableTemplateObj);

                string str;
                string substrateTypeStr = ExportModel.SubstrateAsString(_sampleRecord.SubstrateType);

                if (substrateTypeStr == ApplicationConstants.PlateName)
                {
					str = substrateTypeStr + "-" + _sampleRecord.Position.Row + "-" + _sampleRecord.Position.Column;
                }
				else
                {
	                str = substrateTypeStr + "-" + _sampleRecord.Position.Column;
                }

                reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_QMgmtHEADER_Position", str);
                AddReportTableTemplateToSampleDetailList(reportTableTemplateObj);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        private void AddReportTableTemplateToSampleDetailList(ReportTableTemplate reportTableTemplateObj)
        {
            if (reportTableTemplateObj != null)
            {
                _runResultsReportModel.RunResultsReportDomainInstance.SampleDetailsList.Add(
                    reportTableTemplateObj);
            }
        }

        private void FetchGraphData()
        {
            if (_graphList == null || !_graphList.Any())
            {
                return;
            }

            GetSelectedGraph();
            SetGraphAxisValues();
            GetMaxNumOfCoordinates();
            SetGraphAxisValuesToTheBindedList();
            SetGraphDefaultVisibility();
            SetGraphTableRowVisibility();
        }

        private void GetMaxNumOfCoordinates()
        {
            _coordinatesCountList = new List<int>();
            if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList != null)
                foreach (var runResultXyAxisValue in _runResultsReportModel.RunResultsReportDomainInstance
                    .RunResultsReportXyAxisValueList)
                {
                    _coordinatesCountList.Add(runResultXyAxisValue.RunResultsReportXyAxisValueList.Count);
                }
        }

        private void SetGraphAxisValuesToTheBindedList()
        {
            var count = _coordinatesCountList.OrderByDescending(x => x).FirstOrDefault();
            for (var i = 0; i <= count; i++)
            {
                var runResultsReportGraphAxesDataDomainInstance = new RunResultsReportGraphAxesDataDomain();

                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    0 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[0]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.FirstGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[0]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.FirstGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[0]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }

                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    1 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[1]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.SecondGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[1]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.SecondGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[1]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }

                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    2 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[2]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.ThirdGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[2]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.ThirdGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[2]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }

                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    3 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[3]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.FourthGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[3]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.FourthGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[3]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }

                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    4 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[4]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.FifthGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[4]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.FifthGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[4]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }

                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    5 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[5]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.SixthGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[5]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.SixthGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[5]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }

                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    6 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[6]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.SeventhGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[6]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.SeventhGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[6]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }

                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    7 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[7]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.EigthGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[7]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.EigthGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[7]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }

                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    8 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[8]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.NinthGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[8]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.NinthGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[8]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }

                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    9 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[9]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.TenthGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[9]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.TenthGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[9]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }

                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    10 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[10]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.EleventhGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[10]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.EleventhGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[10]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }

                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    11 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[11]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.TwelfthGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[11]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.TwelfthGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[11]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }

                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    12 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[12]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.ThirteenthGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[12]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.ThirteenthGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[12]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }
                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    13 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[13]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.FourteenthGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[13]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.FourteenthGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[13]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }
                if (_runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Count >
                    14 && i < _runResultsReportModel.RunResultsReportDomainInstance
                        .RunResultsReportXyAxisValueList[14]
                        .RunResultsReportXyAxisValueList.Count)
                {
                    runResultsReportGraphAxesDataDomainInstance.FifteenthGraphXAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[14]
                            .RunResultsReportXyAxisValueList[i].XValue;
                    runResultsReportGraphAxesDataDomainInstance.FifteenthGraphYAxesData =
                        _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList[14]
                            .RunResultsReportXyAxisValueList[i].YValue;
                }

                _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportGraphAxesDataList.Add(
                    runResultsReportGraphAxesDataDomainInstance);
            }
        }
      
        private void SetGraphAxisValues()
        {
            _runResultsReportGraphTitlesDomain = new RunResultsReportGraphTitlesDomain();
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList = new List<RunResultsReportGraphDataListDomain>();
            var itemIndex = 0;
            foreach (var graphToBeDisplayed in _listOfGraphsToBeDisplayed)
            {
                if (graphToBeDisplayed.PrimaryGraphDetailList != null)
                {
                    var graphName = graphToBeDisplayed.GraphName;
                    switch (graphToBeDisplayed.SelectedGraphType)
                    {
                        case GraphType.TotalCells:
                            itemIndex++;
                            graphName = LanguageResourceHelper.Get("LID_GraphLabel_Nonviablecellcount");
                            graphToBeDisplayed.YAxisName = LanguageResourceHelper.Get("LID_RunResultsReport_Graph_TotalCell_Axis_Title");
                            _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Add(GetRunResultGraphAxisList(graphToBeDisplayed));
                            SetRunResultGraphAxisValues(itemIndex, graphName,graphToBeDisplayed);
                            if (graphToBeDisplayed.MultiGraphDetailList != null)
                            {
                                itemIndex++;
                                graphToBeDisplayed.YAxisName = LanguageResourceHelper.Get("LID_RunResultsReport_Graph_ViableCells_Title");
                                graphName = graphToBeDisplayed.YAxisName;
                                if(graphToBeDisplayed.MultiGraphDetailList.Any())
                                    _runResultsReportModel.RunResultsReportDomainInstance
                                    .RunResultsReportXyAxisValueList.Add(GetRunResultMultiGraphAxisList(graphToBeDisplayed));
                                SetRunResultGraphAxisValues(itemIndex, graphName,graphToBeDisplayed);
                            }
                            break;
                        case GraphType.Concentration:
                            graphName = LanguageResourceHelper.Get("LID_GraphLabel_CellConcentrationML");
                            itemIndex++;
                            graphToBeDisplayed.YAxisName = LanguageResourceHelper.Get("LID_CheckBox_NonviableCellConcentration");
                            _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Add(GetRunResultGraphAxisList(graphToBeDisplayed));
                            SetRunResultGraphAxisValues(itemIndex, graphName,graphToBeDisplayed);

                            if (graphToBeDisplayed.MultiGraphDetailList != null)
                            {
                                itemIndex++;
                                graphToBeDisplayed.YAxisName = LanguageResourceHelper.Get("LID_GraphLabel_ViaConc");
                                graphName = LanguageResourceHelper.Get("LID_RunResultsReport_Graph_ViableCellConcentration_Title");
                                if (graphToBeDisplayed.MultiGraphDetailList.Any())
                                    _runResultsReportModel.RunResultsReportDomainInstance
                                    .RunResultsReportXyAxisValueList.Add(GetRunResultMultiGraphAxisList(graphToBeDisplayed));
                                SetRunResultGraphAxisValues(itemIndex, graphName,graphToBeDisplayed);
                            }
                            break;
                        case GraphType.AverageSize:
                            _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Add(GetRunResultGraphAxisList(graphToBeDisplayed));
                            itemIndex++;
                            SetRunResultGraphAxisValues(itemIndex, graphName,graphToBeDisplayed);

                            if (graphToBeDisplayed.MultiGraphDetailList != null)
                            {
                                itemIndex++;
                                graphName = LanguageResourceHelper.Get("LID_GraphLabel_Viablesize");
                                if (graphToBeDisplayed.MultiGraphDetailList.Any())
                                    _runResultsReportModel.RunResultsReportDomainInstance
                                    .RunResultsReportXyAxisValueList.Add(GetRunResultMultiGraphAxisList(graphToBeDisplayed));
                                SetRunResultGraphAxisValues(itemIndex, graphName,graphToBeDisplayed);
                            }
                            break;
                        case GraphType.AverageCircularity:
                            _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Add(GetRunResultGraphAxisList(graphToBeDisplayed));
                            itemIndex++;
                            SetRunResultGraphAxisValues(itemIndex, graphName,graphToBeDisplayed);

                            if (graphToBeDisplayed.MultiGraphDetailList != null)
                            {
                                itemIndex++;
                                graphName = LanguageResourceHelper.Get("LID_GraphLabel_Viablecircularity");
                                if (graphToBeDisplayed.MultiGraphDetailList.Any())
                                    _runResultsReportModel.RunResultsReportDomainInstance
                                    .RunResultsReportXyAxisValueList.Add(GetRunResultMultiGraphAxisList(graphToBeDisplayed));
                                SetRunResultGraphAxisValues(itemIndex, graphName,graphToBeDisplayed);
                            }
                            break;
                        case GraphType.SizeDistribution:
                        case GraphType.ViableSizeDistribution:
                            itemIndex++;
                            graphToBeDisplayed.XAxisName = LanguageResourceHelper.Get("LID_GraphLabel_Count");
                            graphToBeDisplayed.YAxisName = LanguageResourceHelper.Get("LID_RunResultsReport_Graph_Diameter");

                            _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Add(new RunResultsReportGraphDataListDomain
                            {
                                RunResultsReportXyAxisValueList = graphToBeDisplayed.PrimaryGraphDetailList.Select(
                                    axisPoint => new RunResultsReportXyAxisValueDomain()
                                    {
                                        XValue = Convert.ToDouble(axisPoint.Value),
                                        YValue = Convert.ToDouble(axisPoint.Key)
                                    }).ToList()
                            });
                            SetRunResultGraphAxisValues(itemIndex, graphName, graphToBeDisplayed);
                            break;
                        case GraphType.CircularityDistribution:
                            itemIndex++;
                            graphToBeDisplayed.XAxisName = LanguageResourceHelper.Get("LID_GraphLabel_Count");
                            graphToBeDisplayed.YAxisName = LanguageResourceHelper.Get("LID_Label_AverageCircularity");
                            _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Add(new RunResultsReportGraphDataListDomain
                            {
                                RunResultsReportXyAxisValueList = graphToBeDisplayed.PrimaryGraphDetailList.Select(
                                    axisPoint => new RunResultsReportXyAxisValueDomain()
                                    {
                                        XValue = Convert.ToDouble(axisPoint.Value),
                                        YValue = Convert.ToDouble(axisPoint.Key)
                                    }).ToList()
                            });
                            SetRunResultGraphAxisValues(itemIndex, graphName, graphToBeDisplayed);
                            break;
                        case GraphType.ViableCircularityDistribution:
                            itemIndex++;
                            graphToBeDisplayed.XAxisName = LanguageResourceHelper.Get("LID_GraphLabel_Count");
                            graphToBeDisplayed.YAxisName = LanguageResourceHelper.Get("LID_GraphLabel_Viablecircularity");
                            _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Add(new RunResultsReportGraphDataListDomain
                            {
                                RunResultsReportXyAxisValueList = graphToBeDisplayed.PrimaryGraphDetailList.Select(
                                    axisPoint => new RunResultsReportXyAxisValueDomain()
                                    {
                                        XValue = Convert.ToDouble(axisPoint.Value),
                                        YValue = Convert.ToDouble(axisPoint.Key)
                                    }).ToList()
                            });
                            SetRunResultGraphAxisValues(itemIndex, graphName, graphToBeDisplayed);
                            break;
                        default:
                            itemIndex++;
                            _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportXyAxisValueList.Add(GetRunResultGraphAxisList(graphToBeDisplayed));
                            SetRunResultGraphAxisValues(itemIndex, graphName,graphToBeDisplayed);
                            break;
                    }
                }
            }
            _graphTableVisibleRowCount = itemIndex;
            _runResultsReportModel.RunResultsReportDomainInstance.RunResultsReportGraphTitlesDomainList.Add(
                _runResultsReportGraphTitlesDomain);
        }

        private RunResultsReportGraphDataListDomain GetRunResultGraphAxisList(BarGraphDomain barGraphDomain)
        {
            return new RunResultsReportGraphDataListDomain
            {
                RunResultsReportXyAxisValueList = barGraphDomain.PrimaryGraphDetailList.Select(
                    axisPoint => new RunResultsReportXyAxisValueDomain()
                    {
                        XValue = Convert.ToDouble(axisPoint.Key),
                        YValue = Convert.ToDouble(axisPoint.Value)
                    }).ToList()
            };
        }
        
        private RunResultsReportGraphDataListDomain GetRunResultMultiGraphAxisList(BarGraphDomain barGraphDomain)
        {
            return new RunResultsReportGraphDataListDomain
            {
                RunResultsReportXyAxisValueList = barGraphDomain.MultiGraphDetailList[0]
                    .Select(
                        axisPoint => new RunResultsReportXyAxisValueDomain()
                        {
                            XValue = Convert.ToDouble(axisPoint.Key),
                            YValue = Convert.ToDouble(axisPoint.Value)
                        }).ToList()
            };
        }

        private void SetRunResultGraphAxisValues(int index, string graphName, BarGraphDomain graphToBeDisplayed)
        {
            switch (index)
            {
                case 1:
                    _runResultsReportGraphTitlesDomain.FirstGraphHeaderTitle = graphName;
                    _runResultsReportGraphTitlesDomain.FirstGraphXAxisTitle = graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.FirstGraphYAxisTitle = graphToBeDisplayed.YAxisName;
                    break;
                case 2:
                    _runResultsReportGraphTitlesDomain.SecondGraphHeaderTitle = graphName;
                    _runResultsReportGraphTitlesDomain.SecondGraphXAxisTitle = graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.SecondGraphYAxisTitle = graphToBeDisplayed.YAxisName;
                    break;
                case 3:
                    _runResultsReportGraphTitlesDomain.ThirdGraphHeaderTitle = graphName;
                    _runResultsReportGraphTitlesDomain.ThirdGraphXAxisTitle = graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.ThirdGraphYAxisTitle = graphToBeDisplayed.YAxisName;
                    break;
                case 4:
                    _runResultsReportGraphTitlesDomain.FourthGraphHeaderTitle = graphName;
                    _runResultsReportGraphTitlesDomain.FourthGraphXAxisTitle = graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.FourthGraphYAxisTitle = graphToBeDisplayed.YAxisName;
                    break;
                case 5:
                    _runResultsReportGraphTitlesDomain.FifthGraphHeaderTitle = graphName;
                    _runResultsReportGraphTitlesDomain.FifthGraphXAxisTitle = graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.FifthGraphYAxisTitle = graphToBeDisplayed.YAxisName;
                    break;
                case 6:
                    _runResultsReportGraphTitlesDomain.SixthGraphHeaderTitle = graphName;
                    _runResultsReportGraphTitlesDomain.SixthGraphXAxisTitle = graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.SixthGraphYAxisTitle = graphToBeDisplayed.YAxisName;
                    break;
                case 7:
                    _runResultsReportGraphTitlesDomain.SeventhGraphHeaderTitle = graphName;
                    _runResultsReportGraphTitlesDomain.SeventhGraphXAxisTitle = graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.SeventhGraphYAxisTitle = graphToBeDisplayed.YAxisName;
                    break;
                case 8:
                    _runResultsReportGraphTitlesDomain.EighthGraphHeaderTitle = graphName;
                    _runResultsReportGraphTitlesDomain.EighthGraphXAxisTitle = graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.EighthGraphYAxisTitle = graphToBeDisplayed.YAxisName;
                    break;
                case 9:
                    _runResultsReportGraphTitlesDomain.NinthGraphHeaderTitle = graphName;
                    _runResultsReportGraphTitlesDomain.NinthGraphXAxisTitle = graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.NinthGraphYAxisTitle = graphToBeDisplayed.YAxisName;
                    break;
                case 10:
                    _runResultsReportGraphTitlesDomain.TenthGraphHeaderTitle = graphName;
                    _runResultsReportGraphTitlesDomain.TenthGraphXAxisTitle = graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.TenthGraphYAxisTitle = graphToBeDisplayed.YAxisName;
                    break;
                case 11:
                    _runResultsReportGraphTitlesDomain.EleventhGraphHeaderTitle =
                        graphName;
                    _runResultsReportGraphTitlesDomain.EleventhGraphXAxisTitle =
                        graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.EleventhGraphYAxisTitle =
                        graphToBeDisplayed.YAxisName;
                    break;
                case 12:
                    _runResultsReportGraphTitlesDomain.TwelfthGraphHeaderTitle =
                        graphName;
                    _runResultsReportGraphTitlesDomain.TwelfthGraphXAxisTitle = graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.TwelfthGraphYAxisTitle = graphToBeDisplayed.YAxisName;
                    break;
                case 13:
                    _runResultsReportGraphTitlesDomain.ThirteenthGraphHeaderTitle =
                        graphName;
                    _runResultsReportGraphTitlesDomain.ThirteenthGraphXAxisTitle = graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.ThirteenthGraphYAxisTitle = graphToBeDisplayed.YAxisName;
                    break;
                case 14:
                    _runResultsReportGraphTitlesDomain.FourteenthGraphHeaderTitle =
                        graphName;
                    _runResultsReportGraphTitlesDomain.FourteenthGraphXAxisTitle = graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.FourteenthGraphYAxisTitle = graphToBeDisplayed.YAxisName;
                    break;
                case 15:
                    _runResultsReportGraphTitlesDomain.FifteenthGraphHeaderTitle =
                        graphName;
                    _runResultsReportGraphTitlesDomain.FifteenthGraphXAxisTitle = graphToBeDisplayed.XAxisName;
                    _runResultsReportGraphTitlesDomain.FifteenthGraphYAxisTitle = graphToBeDisplayed.YAxisName;
                    break;
            }
        }

        private void GetSelectedGraph()
        {
            _listOfGraphsToBeDisplayed = new List<BarGraphDomain>();
            if (_reportGraphOptions == null || _graphList == null)
                return;
            
            foreach (var reportGraph in _reportGraphOptions)
            {
                if (reportGraph.IsFirstParameterChecked)
                {
                    _listOfGraphsToBeDisplayed.Add(_graphList.Where(
                        x => x.GraphName == reportGraph.FirstParameterName).Select(x => x).FirstOrDefault());
                }

                if (reportGraph.IsSecondParameterChecked)
                {
                    _listOfGraphsToBeDisplayed.Add(_graphList.Where(
                        x => x.GraphName == reportGraph.SecondParameterName).Select(x => x).FirstOrDefault());
                }
            }
        }

        private void SetGraphDefaultVisibility()
        {
            foreach (PropertyInfo pi in _runResultsReportParametersDomain.GetType().GetProperties())
                pi.SetValue(_runResultsReportParametersDomain, true);
            _runResultsReportParametersDomain.IsGraphSecondBlockHidden = false;
            _runResultsReportParametersDomain.IsGraphFourthBlockHidden = false;
            _runResultsReportParametersDomain.IsGraphSixthBlockHidden = false;
            _runResultsReportParametersDomain.IsGraphEightBlockHidden = false;
            _runResultsReportParametersDomain.IsGraphTenthBlockHidden = false;
            _runResultsReportParametersDomain.IsGraphTwelfthBlockHidden = false;
            _runResultsReportParametersDomain.IsGraphFourteenthBlockHidden = false;
        }

        private void SetHeaderData()
        {
            try
            {
                var reportMandatoryHeaderDomainObj = ReportWindowModel.SetReportHeaderData(_printTitle, "LID_FrameLabel_RunResultReport", _comments);

                _runResultsReportModel.RunResultsReportDomainInstance.ReportMandatoryHeaderDomainList.Add(
                    reportMandatoryHeaderDomainObj);

                var runResultTableHeaderDomainObj = new RunResultTableHeaderDomain()
                {
                    ReportResultsHeader = LanguageResourceHelper.Get("LID_TabItem_Results"),
                    ReportResultsAnalysisParametersHeader = LanguageResourceHelper.Get("LID_CheckBox_AnalysisParameters"),
                    ReportResultsSignaturesHeader = LanguageResourceHelper.Get("LID_CheckBox_Signatures")
                };
                _runResultsReportModel.RunResultsReportDomainInstance.RunResultTableHeaderDomainList.Add(
                    runResultTableHeaderDomainObj);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        private void SetImageData()
        {
            try
            {
                var sampleRecordDomainInstance = _sampleRecord;

                var reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                    "LID_QMgmtHEADER_SampleId",
                    sampleRecordDomainInstance.SampleIdentifier);
                AddReportTableTemplateToImageDetailsList(reportTableTemplateObj);
                _recordHelper.SetImageList(sampleRecordDomainInstance);
                var sampleImage = sampleRecordDomainInstance.SampleImageList[0];
                sampleImage.TotalCumulativeImage = Convert.ToInt32(sampleRecordDomainInstance.SelectedResultSummary.CumulativeResult.TotalCumulativeImage);

                var imageData = _recordHelper.GetImage(ImageType.Annotated, sampleImage.BrightFieldId, sampleRecordDomainInstance.SelectedResultSummary.UUID);

				var DefaultImageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ApplicationConstants.TargetFolderName);
				var imageName = ApplicationConstants.ImageFilenameForPDF + ApplicationConstants.ImageFileExtension;
				var imagePath = Path.Combine(DefaultImageDirectory, imageName);
				ImageDtoExts.SaveImage(imageData.BrightfieldImage, imagePath);

				var width = imageData.BrightfieldImage.ImageSource.Width;
				var height = imageData.BrightfieldImage.ImageSource.Height;
				var imageSize = width + " X " + height;

                reportTableTemplateObj =
                    ReportWindowModel.CreateReportTableTemplate("LID_Report_ImageSize", imageSize);
                AddReportTableTemplateToImageDetailsList(reportTableTemplateObj);
                reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_Label_Caption", _reportImageCaption);

                AddReportTableTemplateToImageDetailsList(reportTableTemplateObj);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
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
                        parameterNames.Add(GetResourceKeyName("LID_Label_PN"));
                        break;
                    case 2:
                        parameterNames.Add(GetResourceKeyName("LID_QCHeader_LotNumber"));
                        break;
                    case 3:
                        parameterNames.Add(GetResourceKeyName("LID_ButtonContent_Reagent"));
                        break;
                    case 4:
                        parameterNames.Add(GetResourceKeyName("LID_GridLabel_ExpirationDate"));
                        break;
                    case 5:
                        parameterNames.Add(GetResourceKeyName("LID_Label_InService_Date"));
                        break;
                    case 6:
                        parameterNames.Add(GetResourceKeyName("LID_Label_EffectiveExpiration"));
                        break;
                }
            }

            return parameterNames;
        }

        private string GetPropertyValue(IReadOnlyList<string> value, int index)
        {
            return value.Any() ? value[index] : string.Empty;
        }

        private void SetReagentParameterData()
        {
            if (_sampleRecord.RegentInfoRecordList == null)
            {
                Log.Debug($"_sampleRecord.RegentInfoRecordList is null");
                return;
            }

            var reagentInfoRecords = _sampleRecord.RegentInfoRecordList;
            reagentInfoRecords = _permissionLevel.Equals(UserPermissionLevel.eService)
                ? reagentInfoRecords
                : reagentInfoRecords.Where(reagent => reagent.ReagentName.Equals(ApplicationConstants.ReagentNameForAllUser)).ToList();
            var parameterNames = GetReagentHeaders();
            var partNumbers = reagentInfoRecords.Select(x => x.PackNumber).ToList();
            var lotNumbers = reagentInfoRecords.Select(x => ScoutUtilities.Misc.ConvertToString(x.LotNumber)).ToList();
            var reagentNames = reagentInfoRecords.Select(x => x.ReagentName).ToList();
            var expirationDates = reagentInfoRecords.Select(x => ScoutUtilities.Misc.ConvertToString(x.ExpirationDate)).ToList();
            var serviceDates = reagentInfoRecords.Select(x => ScoutUtilities.Misc.ConvertToString(x.InServiceDate)).ToList();
            var effectiveExpirationDates = reagentInfoRecords.Select(x => ScoutUtilities.Misc.ConvertToString(x.EffectiveExpirationDate)).ToList();

            var parList = new List<List<string>>();
            for (var i = 0; i < reagentInfoRecords.Count; i++)
            {
                var newList = new List<string>
                {
                    GetPropertyValue(partNumbers, i),
                    GetPropertyValue(lotNumbers, i),
                    GetPropertyValue(reagentNames, i),
                    GetPropertyValue(expirationDates, i),
                    GetPropertyValue(serviceDates, i),
                    GetPropertyValue(effectiveExpirationDates, i)
                };
                parList.Add(newList);
            }

            if (parList.Any())
            {
                // Iterate the values based on default row count
                for (var i = 0; i < DefaultRowCount; i++)
                {
                    // Set the value for ColumnOne, if the parameter list count is one  
                    var parameter = new RunResultReagentParameterDomain
                    {
                        Header = LanguageResourceHelper.Get("LID_Label_ReagentParameters"),
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

                    _runResultsReportModel.RunResultsReportDomainInstance.RunResultReagentParameterList.Add(
                        parameter);
                }
            }

        }

        private void AddReportTableTemplateToImageDetailsList(ReportTableTemplate reportTableTemplateObj)
        {
            if (reportTableTemplateObj != null)
            {
                _runResultsReportModel.RunResultsReportDomainInstance.ImageDetailsList.Add(
                    reportTableTemplateObj);
            }
        }

        private void SetMandatoryData()
        {
            try
            {
                if (_sampleRecord.SelectedResultSummary?.CumulativeResult != null)
                {
                    var cumulativeResultObj = _sampleRecord.SelectedResultSummary.CumulativeResult;

                    var reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
	                    "LID_GraphLabel_Totalcells", 
                        ScoutUtilities.Misc.ConvertToString(cumulativeResultObj.TotalCells));
                    AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);

                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                        "LID_RunResultsReport_Graph_ViableCells_Title", 
                        ScoutUtilities.Misc.ConvertToString(cumulativeResultObj.ViableCells));
                    AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);

                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                        "LID_CheckBox_TotalCellConcentration", 
                        ScoutUtilities.Misc.ConvertToConcPower(cumulativeResultObj.ConcentrationML));
                   AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);

                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                        "LID_Result_ViaConc", 
                        ScoutUtilities.Misc.ConvertToConcPower(cumulativeResultObj.ViableConcentration));
                    AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);

                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                        "LID_Label_Viability", 
                        ScoutUtilities.Misc.UpdateTrailingPoint(cumulativeResultObj.Viability,
                            TrailingPoint.One));
                    AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);

                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
	                    "LID_Label_Size",
                        ScoutUtilities.Misc.UpdateTrailingPoint(cumulativeResultObj.Size, TrailingPoint.Two));
                    AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);

                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                        "LID_GraphLabel_Viablesize",
                        ScoutUtilities.Misc.UpdateTrailingPoint(cumulativeResultObj.ViableSize,
                            TrailingPoint.Two));
                    AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);

                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                        "LID_Label_Circularity",
                        ScoutUtilities.Misc.UpdateTrailingPoint(cumulativeResultObj.Circularity, TrailingPoint.Two));
                    AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);

                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                        "LID_GraphLabel_Viablecircularity",
                        ScoutUtilities.Misc.UpdateTrailingPoint(cumulativeResultObj.ViableCircularity, TrailingPoint.Two));
                    AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);

                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                        "LID_Label_AverageCellPerImage",
                        ScoutUtilities.Misc.ConvertToString(cumulativeResultObj.AverageCellsPerImage));
                    AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);

                    reportTableTemplateObj =
                        ReportWindowModel.CreateReportTableTemplate(
	                        "LID_GraphLabel_Averagebackground",
                            ScoutUtilities.Misc.ConvertToString(cumulativeResultObj.AvgBackground));
                    AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);

                    reportTableTemplateObj =
                        ReportWindowModel.CreateReportTableTemplate(
	                        "LID_Label_bubbles",
                            ScoutUtilities.Misc.ConvertToString(cumulativeResultObj.Bubble));
                    AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);
                    reportTableTemplateObj =
                        ReportWindowModel.CreateReportTableTemplate(
	                        "LID_GraphLabel_CellClusters",
                            ScoutUtilities.Misc.ConvertToString(cumulativeResultObj.ClusterCount));
                    AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);
                    var considered = _sampleRecord.SelectedResultSummary.CumulativeResult.TotalCumulativeImage;

                    var sampleDomain = _sampleRecord;
                    ResultRecordDomain recordDomain = null;
                    if (sampleDomain?.SelectedResultSummary?.UUID != null)
                    {
                        recordDomain = _recordHelper.OnSelectedSampleRecordForReport(sampleDomain.SelectedResultSummary.UUID);
                    }
                    var discardedCount =
                        recordDomain?.ResultPerImage?.Count(x =>
                            x.ProcessedStatus != E_ERRORCODE.eSuccess);

                    reportTableTemplateObj =
                        ReportWindowModel.CreateReportTableTemplate(
	                        "LID_ReportLabel_TotalImages_Analysis",
                            ScoutUtilities.Misc.ConvertToString(considered));
                    AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);

                    if (discardedCount < 1)
                    {
                        return;
                    }
                    reportTableTemplateObj =
                        ReportWindowModel.CreateReportTableTemplate(
	                        "LID_ReportLabel_Images_Discarded",
                            ScoutUtilities.Misc.ConvertToString(discardedCount));
                    AddReportTableTemplateToAboutReportParameterList(reportTableTemplateObj);
                    SetDiscardedImageDetails();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        private void SetDiscardedImageDetails()
        {
            var bubbleCount = 0;
            var invalidBgIntensityCount = 0;
            var imgPrcErrorCount = 0;
            _isDisCardedImageVisible = true;
            var sampleDomain = _sampleRecord;
            ResultRecordDomain recordDomain = null;
            if (sampleDomain?.SelectedResultSummary?.UUID != null)
            {
                recordDomain = _recordHelper.OnSelectedSampleRecordForReport(sampleDomain.SelectedResultSummary.UUID);
            }
            recordDomain?.ResultPerImage?.ForEach(resultDomain =>
            {
                switch (resultDomain.ProcessedStatus)
                    {
                        case E_ERRORCODE.eBubbleImage:
                            bubbleCount++;
                            break;
                        case E_ERRORCODE.eInvalidBackgroundIntensity:
                            invalidBgIntensityCount++;
                            break;
                        case E_ERRORCODE.eInvalidInputPath:
                        case E_ERRORCODE.eValueOutOfRange:
                        case E_ERRORCODE.eZeroInput:
                        case E_ERRORCODE.eNullCellsData:
                        case E_ERRORCODE.eInvalidImage:
                        case E_ERRORCODE.eResultNotAvailable:
                        case E_ERRORCODE.eFileWriteError:
                        case E_ERRORCODE.eZeroOutputData:
                        case E_ERRORCODE.eParameterIsNegative:
                        case E_ERRORCODE.eInvalidParameter:
                        case E_ERRORCODE.eFLChannelsMissing:
                        case E_ERRORCODE.eInvalidCharacteristics:
                        case E_ERRORCODE.eInvalidAlgorithmMode:
                        case E_ERRORCODE.eMoreThanOneFLImageSupplied:
                        case E_ERRORCODE.eTransformationMatrixMissing:
                        case E_ERRORCODE.eFailure:
                            imgPrcErrorCount++;
                            break;
                    }
                });

            var header = LanguageResourceHelper.Get("LID_BubbleLabel_DiscardedImages");
            RunResultDiscardedImageDomain runResultDiscardedImageDomain;
            if (bubbleCount >= 1)
            {
                 runResultDiscardedImageDomain = CreateDiscardedImageTableTemplate(header,
                    LanguageResourceHelper.Get("LID_eBubbleImage"),
                    ScoutUtilities.Misc.ConvertToString(bubbleCount));
                AddDiscardedImageDetails(runResultDiscardedImageDomain);
            }

            if (invalidBgIntensityCount >= 1)
            {
                runResultDiscardedImageDomain = CreateDiscardedImageTableTemplate(header,
                    LanguageResourceHelper.Get("LID_eInvalidBackgroundIntensity"),
                    ScoutUtilities.Misc.ConvertToString(invalidBgIntensityCount));
                AddDiscardedImageDetails(runResultDiscardedImageDomain);
            }

            if (imgPrcErrorCount < 1)
            {
                return;
            }
            runResultDiscardedImageDomain = CreateDiscardedImageTableTemplate(header,
                LanguageResourceHelper.Get("LID_ImageProcessingError"),
                ScoutUtilities.Misc.ConvertToString(imgPrcErrorCount));
            AddDiscardedImageDetails(runResultDiscardedImageDomain);

        }
        
        private void AddReportTableTemplateToAboutReportParameterList(ReportTableTemplate reportTableTemplateObj)
        {
            if (reportTableTemplateObj != null)
            {
                _runResultsReportModel.RunResultsReportDomainInstance.RunResultAboutReportParameterDomainList
                    .Add(reportTableTemplateObj);
            }
        }

        private void AddDiscardedImageDetails(RunResultDiscardedImageDomain discardedImageDomain)
        {
            if (discardedImageDomain != null)
            {
                _runResultsReportModel.RunResultsReportDomainInstance.RunResultDiscardedImageList
                    .Add(discardedImageDomain);
            }
        }

        private RunResultDiscardedImageDomain CreateDiscardedImageTableTemplate(string header, string paramName, string paramValue)
        {
            var runResultDiscardedImageDomain = new RunResultDiscardedImageDomain
            {
                Header = header, ParameterName = paramName, ParameterValue = paramValue
            };
            return runResultDiscardedImageDomain;
        }

        private void SetGraphTableRowVisibility()
        {
            if (_listOfGraphsToBeDisplayed != null)
            {
                switch (((double)_graphTableVisibleRowCount / 2.0).ToString(CultureInfo.InvariantCulture))
                {
                    case "0.5":
                        FirstRowVisible();
                        _runResultsReportParametersDomain
                            .IsGraphSecondBlockHidden = true;
                        break;
                    case "1":
                        FirstRowVisible();
                        break;
                    case "1.5":
                        SecondRowVisible();
                        _runResultsReportParametersDomain
                            .IsGraphFourthBlockHidden = true;
                        break;
                    case "2":
                        SecondRowVisible();
                        break;
                    case "2.5":
                        ThirdRowVisible();
                        _runResultsReportParametersDomain
                            .IsGraphSixthBlockHidden = true;
                        break;
                    case "3":
                        ThirdRowVisible();
                        break;
                    case "3.5":
                        FourthRowVisible();
                        _runResultsReportParametersDomain
                            .IsGraphEightBlockHidden = true;
                        break;
                    case "4":
                        FourthRowVisible();
                        break;
                    case "4.5":
                        FifthRowVisible();
                        _runResultsReportParametersDomain
                            .IsGraphTenthBlockHidden = true;
                        break;
                    case "5":
                        FifthRowVisible();
                        break;
                    case "5.5":
                        SixthRowVisible();
                        _runResultsReportParametersDomain
                            .IsGraphTwelfthBlockHidden = true;
                        break;
                    case "6":
                        SixthRowVisible();
                        break;
                    case "6.5":
                    case "7":
                        SeventhRowVisible();
                        break;
                    case "7.5":
                    case "8":
                        EighthRowVisible();
                        break;
                }

                _runResultsReportModel?.RunResultsReportDomainInstance?.RunResultGraphVisibilityList.Add(
                    _runResultsReportParametersDomain);
            }
           
        }

        private void EighthRowVisible()
        {
            SeventhRowVisible();
            _runResultsReportParametersDomain.IsGraphEigthRowHidden = false;
        }

        private void SeventhRowVisible()
        {
            SixthRowVisible();
            _runResultsReportParametersDomain.IsGraphSeventhRowHidden = false;
        }

        private void SixthRowVisible()
        {
            FifthRowVisible();
            _runResultsReportParametersDomain.IsGraphSixthRowHidden = false;
        }

        private void FifthRowVisible()
        {
            FourthRowVisible();
            _runResultsReportParametersDomain.IsGraphFifthRowHidden = false;
        }

        private void FourthRowVisible()
        {
            ThirdRowVisible();
            _runResultsReportParametersDomain.IsGraphFourthRowHidden = false;
        }

        private void ThirdRowVisible()
        {
            SecondRowVisible();
            _runResultsReportParametersDomain.IsGraphThirdRowHidden = false;
        }

        private void SecondRowVisible()
        {
            FirstRowVisible();
            _runResultsReportParametersDomain.IsGraphSecondRowHidden = false;
        }

        private void FirstRowVisible()
        {
            _runResultsReportParametersDomain.IsGraphFirstRowHidden = false;
        }

        #endregion
    }
}