using Microsoft.Reporting.WinForms;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutDomains.Reports.Common;
using ScoutDomains.Reports.QualityControls;
using ScoutLanguageResources;
using ScoutModels.Reports;
using ScoutUtilities.Enums;
using ScoutViewModels.ViewModels.QualityControl;
using System;
using System.Collections.Generic;
using System.Linq;
using ScoutModels.Common;
using ScoutViewModels.Interfaces;

namespace ScoutViewModels.ViewModels.Reports
{
    public class QualityControlsReportViewModel : ReportWindowViewModel
    {
        public QualityControlsReportViewModel(string printTitle, string comments, QualityControlDomain selectedQualityControl, List<LineGraphDomain> graphListForReport, IScoutViewModelFactory viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
            _qualityControlVm = _viewModelFactory.CreateQualityControlViewModel();
            _qualityControlVm.SelectedQualityControl = selectedQualityControl;
            _qualityControlsReportModel = new QualityControlsReportModel();
            ReportTitle = LanguageResourceHelper.Get("LID_GridLabel_QualityControl");
            ReportWindowTitle = LanguageResourceHelper.Get("LID_GridLabel_QualityControl");
            ReportType = ReportType.QualityControl;

            _printTitle = printTitle;
            _comments = comments;
            _selectedQualityControl = selectedQualityControl;
            _graphListForReport = graphListForReport;
        }

        protected override void DisposeUnmanaged()
        {
            _qualityControlVm?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties

        private QualityControlViewModel _qualityControlVm;
        private QualityControlsReportModel _qualityControlsReportModel;
        private LineGraphDomain _lineGraphDomain;

        private string _printTitle;
        private string _comments;
        private QualityControlDomain _selectedQualityControl;
        private List<LineGraphDomain> _graphListForReport;
        private readonly IScoutViewModelFactory _viewModelFactory;

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
            _qualityControlsReportModel.QualityControlsReportDomainInstance.QualityControlReportMandatoryHeaderDomainList = new List<ReportMandatoryHeaderDomain>();
            _qualityControlsReportModel.QualityControlsReportDomainInstance.QualityControlReportTableHeaderList = new List<QualityControlTableHeaderDomain>();
            _qualityControlsReportModel.QualityControlsReportDomainInstance.QualityControlReportNameTableList = new List<ReportTableTemplate>();
            _qualityControlsReportModel.QualityControlsReportDomainInstance.QualityControlReportCellTypeTableList = new List<ReportTableTemplate>();
            _qualityControlsReportModel.QualityControlsReportDomainInstance.QualityControlReportCurrentLotTableList = new List<ReportTableTemplate>();
            _qualityControlsReportModel.QualityControlsReportDomainInstance.QualityControlReportLastCheckTableList = new List<ReportTableTemplate>();
            _qualityControlsReportModel.QualityControlsReportDomainInstance.QualityControlsReportGaphXyAxisList = new List<QualityControlsReportGaphXyAxis>();
            _qualityControlsReportModel.QualityControlsReportDomainInstance.QualityControlsReportGraphTitlesList = new List<QualityControlsReportGraphTitles>();
        }

        protected override void RetrieveData()
        {
            SetHeaderData();
            if (_qualityControlVm.SampleRecordList.Count > 0)
            {
                SetTableHeader();
            }

            SetTableData();
            FetchGraphData();
        }

        protected override void LoadReportViewer()
        {
            ReportViewer = new ReportViewer();

            var lang = LanguageEnums.GetLangType(ApplicationLanguage.GetLanguage());
            var reportViewerPath = GetReportViewerPath(lang);
            var qualityControlDomain = _qualityControlsReportModel.QualityControlsReportDomainInstance;

            AddReportDataSource("QualityControlReportMandatoryHeaderDomainTBL", qualityControlDomain.QualityControlReportMandatoryHeaderDomainList);
            AddReportDataSource("QualityControlReportTableHeaderTBL", qualityControlDomain.QualityControlReportTableHeaderList);
            AddReportDataSource("QualityControlReportNameTBL", qualityControlDomain.QualityControlReportNameTableList);
            AddReportDataSource("QualityControlReportCellTypeTBL", qualityControlDomain.QualityControlReportCellTypeTableList);
            AddReportDataSource("QualityControlReportCurrentLotTBL", qualityControlDomain.QualityControlReportCurrentLotTableList);
            AddReportDataSource("QualityControlReportLastCheckTBL", qualityControlDomain.QualityControlReportLastCheckTableList);
            AddReportDataSource("QualityControlReportGraphTitlesTBL", qualityControlDomain.QualityControlsReportGraphTitlesList);
            AddReportDataSource("QualityControlReportGraphXyAxisTBL", qualityControlDomain.QualityControlsReportGaphXyAxisList);

            RefreshAndSetReportContent(reportViewerPath);
        }

        #endregion

        #region Private Methods

        private string GetReportViewerPath(LanguageType language)
        {
            if (language == LanguageType.eChinese)
                return AppDomain.CurrentDomain.BaseDirectory + @"Reports\QualityControlsReportRdlcViewerZh-CN.rdlc";
            if (language == LanguageType.eJapanese)
                return AppDomain.CurrentDomain.BaseDirectory + @"Reports\QualityControlsReportRdlcViewerJa-JP.rdlc";
            return AppDomain.CurrentDomain.BaseDirectory + @"Reports\QualityControlsReportRdlcViewer.rdlc";
        }

        private void SetTableHeader()
        {

            var qualityControlTableHeaderDomainObj = new QualityControlTableHeaderDomain()
            {
                LotHeader = LanguageResourceHelper.Get("LID_TextBlock_CurrentLot"),
                LastCheckHeader = LanguageResourceHelper.Get("LID_Report_Last_Check")
            };
            _qualityControlsReportModel.QualityControlsReportDomainInstance.QualityControlReportTableHeaderList
                .Add(qualityControlTableHeaderDomainObj);
        }

        private void FetchGraphData()
        {

            if (_graphListForReport == null ||
                _graphListForReport.Count <= 0)
            {
                var qualityControlsReportGraphTitlesObj = new QualityControlsReportGraphTitles()
                {
                    NoDataTitle = LanguageResourceHelper.Get("LID_Report_NoData")
                };
                _qualityControlsReportModel.QualityControlsReportDomainInstance.QualityControlsReportGraphTitlesList
                    .Add(qualityControlsReportGraphTitlesObj);
                return;
            } 
            if (_graphListForReport.FirstOrDefault()?.GraphDetailList ==
                null ||
                _graphListForReport.FirstOrDefault()?.GraphDetailList.Count <
                1)
            {
                var qualityControlsReportGraphTitlesObj = new QualityControlsReportGraphTitles()
                {
                    NoDataTitle = LanguageResourceHelper.Get("LID_Report_NoData")
                };
                _qualityControlsReportModel.QualityControlsReportDomainInstance.QualityControlsReportGraphTitlesList
                    .Add(qualityControlsReportGraphTitlesObj);

                return;
            }
            GetLineGraphData();
            SetGraphHeader();
            SetGraphCoordinates();
        }


        private void GetLineGraphData()
        {
            _lineGraphDomain = new LineGraphDomain();

            string graphName = string.Empty;
            switch (_selectedQualityControl.AssayParameter)
            {
                case assay_parameter.ap_Concentration:
                    graphName = LanguageResourceHelper.Get("LID_CheckBox_TotalCellConcentration");
                    break;
                case assay_parameter.ap_PopulationPercentage:
                    graphName = LanguageResourceHelper.Get("LID_Label_Viability");
                    break;
                case assay_parameter.ap_Size:
                    graphName = LanguageResourceHelper.Get("LID_RunResultsReport_Graph_Diameter_AxisTitle");
                    break;
            }

            if (_graphListForReport != null)
            {
                _lineGraphDomain = _graphListForReport.FirstOrDefault(x => x.GraphName == graphName);
            }
        }

        private void SetGraphHeader()
        {
            if (_lineGraphDomain != null)
            {
                var qualityControlsReportGraphTitlesObj = new QualityControlsReportGraphTitles()
                {
                    HeaderTitle = _lineGraphDomain.GraphName,
                    XAxisTitle = LanguageResourceHelper.Get("LID_Label_Date"),
                    YAxisTitle = _lineGraphDomain.YAxisName
                };
                if (_lineGraphDomain.GraphName ==
                    LanguageResourceHelper.Get("LID_TabItem_Concentration"))
                {
                    qualityControlsReportGraphTitlesObj.HeaderTitle = LanguageResourceHelper.Get("LID_CheckBox_TotalCellConcentration");
                }
                _qualityControlsReportModel.QualityControlsReportDomainInstance.QualityControlsReportGraphTitlesList
                    .Add(qualityControlsReportGraphTitlesObj);
            }
            else
            {
                var qualityControlsReportGraphTitlesObj = new QualityControlsReportGraphTitles()
                {
                    NoDataTitle = LanguageResourceHelper.Get("LID_Report_NoData")
                };
                _qualityControlsReportModel.QualityControlsReportDomainInstance.QualityControlsReportGraphTitlesList
                    .Add(qualityControlsReportGraphTitlesObj);
            }

        }

        private void SetGraphCoordinates()
        {
            if (_lineGraphDomain != null)
            {
                foreach (var keyValuePair in _lineGraphDomain.GraphDetailList)
                {
                    var qualityControlsReportGraphXyAxisObj = new QualityControlsReportGaphXyAxis();
                    qualityControlsReportGraphXyAxisObj.XValue = DateTime.Parse(keyValuePair.Key);
                    qualityControlsReportGraphXyAxisObj.Y1Value = keyValuePair.Value;

                    if (_selectedQualityControl.AcceptanceLimit != null)
                    {
                        if (_selectedQualityControl.AssayValue != null)
                        {
                            const int percent = 100;
                            double? limit = (double)_selectedQualityControl.AssayValue * _selectedQualityControl.AcceptanceLimit / percent;
                            qualityControlsReportGraphXyAxisObj.Y3Value = (double)((double)_selectedQualityControl.AssayValue - limit);
                            qualityControlsReportGraphXyAxisObj.Y4Value = (double)((double)_selectedQualityControl.AssayValue + limit);
                        }
                    }

                    _qualityControlsReportModel.QualityControlsReportDomainInstance
                        .QualityControlsReportGaphXyAxisList.Add(qualityControlsReportGraphXyAxisObj);
                }

                if (_lineGraphDomain.MultiGraphDetailList != null &&
                    _lineGraphDomain.MultiGraphDetailList.Count > 0)
                {
                    var count = _qualityControlsReportModel.QualityControlsReportDomainInstance
                        .QualityControlsReportGaphXyAxisList.Count;
                    for (var index = 0;
                        index < _lineGraphDomain.MultiGraphDetailList.Count;
                        index++)
                    {
                        if (count > index)
                        {
                            _qualityControlsReportModel.QualityControlsReportDomainInstance
                                    .QualityControlsReportGaphXyAxisList[index].Y2Value =
                                _lineGraphDomain.MultiGraphDetailList[index]
                                    .Value;
                        }
                    }
                }
            }
        }


        private void SetTableData()
        {
            if (_selectedQualityControl == null)
                return;
            var selectedQualityControlObj = _selectedQualityControl;
            var reportTableTemplateObj =
                ReportWindowModel.CreateReportTableTemplate("LID_BPHeader_Name",
                    selectedQualityControlObj.QcName);
            _qualityControlsReportModel.QualityControlsReportDomainInstance
                .QualityControlReportNameTableList.Add(reportTableTemplateObj);
            reportTableTemplateObj =
                ReportWindowModel.CreateReportTableTemplate("LID_UsersLabel_CellType",
                    selectedQualityControlObj.CellTypeName);
            AddReportTemplateToCellTypeTableList(reportTableTemplateObj);

            var assayParameter = string.Empty;
            var assayValue = LanguageResourceHelper.Get("LID_GraphLabel_AssayConcentration");
            switch (selectedQualityControlObj.AssayParameter)
            {
                case assay_parameter.ap_Concentration:
                    assayParameter =
                        LanguageResourceHelper.Get("LID_Label_Concentration");
                    assayValue += " " + LanguageResourceHelper.Get("LID_Label_TenthPower");
                    break;
                case assay_parameter.ap_PopulationPercentage:
                    assayParameter =
                        LanguageResourceHelper.Get("LID_Result_Viability");
                    assayValue += " " + LanguageResourceHelper.Get("LID_Label_Percentage_Unit");
                    break;
                case assay_parameter.ap_Size:
                    assayParameter =
                        LanguageResourceHelper.Get("LID_Result_size");
                    assayValue += " " + LanguageResourceHelper.Get("LID_Label_MicroMeter_Unit");
                    break;
            }

            reportTableTemplateObj =
                ReportWindowModel.CreateReportTableTemplate("LID_QCHeader_AssayParameter",
                    assayParameter);
            AddReportTemplateToCellTypeTableList(reportTableTemplateObj);
            
            var acceptanceValue = "+/- " + selectedQualityControlObj.AcceptanceLimit + " %";
            reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                "LID_QCHeader_AcceptanceLimits", acceptanceValue);
            AddReportTemplateToCellTypeTableList(reportTableTemplateObj);
            
            reportTableTemplateObj =
                ReportWindowModel.CreateReportTableTemplate("LID_Label_Comments",
                    selectedQualityControlObj.CommentText);
            AddReportTemplateToCellTypeTableList(reportTableTemplateObj);
            
            var sampleRecordList = _qualityControlVm.SampleRecordList;
            reportTableTemplateObj =
                ReportWindowModel.CreateReportTableTemplate("LID_Report_NoOfRuns", ScoutUtilities.Misc.ConvertToString(sampleRecordList.Count));
            AddReportTemplateToCellTypeTableList(reportTableTemplateObj);

            reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                "LID_QCHeader_LotNumber", selectedQualityControlObj.LotInformation);
            AddReportTemplateToCurrentLotTableList(reportTableTemplateObj);

            reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                "LID_GridLabel_ExpirationDate",
                ScoutUtilities.Misc.ConvertToString(selectedQualityControlObj.ExpirationDate));
            AddReportTemplateToCurrentLotTableList(reportTableTemplateObj);

            var assayValueData = string.Empty;
            if (selectedQualityControlObj.AssayValue != null)
            {
                switch (_selectedQualityControl.AssayParameter)
                {
                    case assay_parameter.ap_Concentration:
                        assayValueData = ScoutUtilities.Misc.UpdateTrailingPoint((double)selectedQualityControlObj.AssayValue,
                            TrailingPoint.Two);
                        break;
                    case assay_parameter.ap_PopulationPercentage:
                        assayValueData = ScoutUtilities.Misc.UpdateTrailingPoint((double)selectedQualityControlObj.AssayValue,
                            TrailingPoint.One);
                        break;
                    case assay_parameter.ap_Size:
                        assayValueData = ScoutUtilities.Misc.UpdateTrailingPoint((double)selectedQualityControlObj.AssayValue,
                            TrailingPoint.Two);
                        break;
                }
            }

            var reportTableTemplateObject = new ReportTableTemplate()
            {
                ParameterName = assayValue,
                ParameterValue = assayValueData
            };
            AddReportTemplateToCurrentLotTableList(reportTableTemplateObject);

            // Upper and lower limit 
            var upperLimitLabel = string.Empty;
            var lowerLimitLabel = string.Empty;
            switch (selectedQualityControlObj.AssayParameter)
            {
                case assay_parameter.ap_Concentration:
                    upperLimitLabel = LanguageResourceHelper.Get("LID_Label_UpperLimitConc_QCReport");
                    lowerLimitLabel = LanguageResourceHelper.Get("LID_Label_LowerLimitConc_QCReport");
                    break;
                case assay_parameter.ap_PopulationPercentage:
                    upperLimitLabel = LanguageResourceHelper.Get("LID_Label_UpperLimitPopulation_QCReport");
                    lowerLimitLabel = LanguageResourceHelper.Get("LID_Label_LowerLimitPopulation_QCReport");
                    break;
                case assay_parameter.ap_Size:
                    upperLimitLabel = LanguageResourceHelper.Get("LID_Label_UpperLimitSize_QCReport");
                    lowerLimitLabel = LanguageResourceHelper.Get("LID_Label_LowerLimitSize_QCReport");
                    break;
            }
            var upperLimitValue = string.Empty;
            var lowerLimitValue = string.Empty;
            if (selectedQualityControlObj.AcceptanceLimit != null)
            {
                if (selectedQualityControlObj.AssayValue != null)
                {
                    const int percent = 100;
                    double? limit =
                        (double)selectedQualityControlObj.AssayValue * selectedQualityControlObj.AcceptanceLimit / percent;
                    switch (selectedQualityControlObj.AssayParameter)
                    {
                        case assay_parameter.ap_Concentration:
                        case assay_parameter.ap_Size:
                            lowerLimitValue = ScoutUtilities.Misc.UpdateTrailingPoint((double)((double)selectedQualityControlObj.AssayValue - limit), TrailingPoint.Two);
                            upperLimitValue = ScoutUtilities.Misc.UpdateTrailingPoint((double)((double)selectedQualityControlObj.AssayValue + limit), TrailingPoint.Two);
                            break;

                        case assay_parameter.ap_PopulationPercentage:
                            lowerLimitValue = ScoutUtilities.Misc.UpdateTrailingPoint((double)((double)selectedQualityControlObj.AssayValue - limit), TrailingPoint.One);
                            upperLimitValue = ScoutUtilities.Misc.UpdateTrailingPoint((double)((double)selectedQualityControlObj.AssayValue + limit), TrailingPoint.One);
                            break;
                    }
                }
            }
            var reportTableTemplateObjectUpperLimit = new ReportTableTemplate()
            {
                ParameterName = upperLimitLabel,
                ParameterValue = upperLimitValue
            };
            AddReportTemplateToCurrentLotTableList(reportTableTemplateObjectUpperLimit);
            var reportTableTemplateObjectLowerLimit = new ReportTableTemplate()
            {
                ParameterName = lowerLimitLabel,
                ParameterValue = lowerLimitValue
            };
            AddReportTemplateToCurrentLotTableList(reportTableTemplateObjectLowerLimit);

            if (sampleRecordList.Count <= 0)
                return;
            var lastSampleData = sampleRecordList.LastOrDefault();
            if (lastSampleData != null)
            {
                reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                    "LID_Label_Date",
                    ScoutUtilities.Misc.ConvertToCustomLongDateTimeFormat(lastSampleData.SelectedResultSummary.RetrieveDate));
                AddReportTemplateToLastCheckTableList(reportTableTemplateObj);
            }

            var resultRecordDomain = _qualityControlVm.RecordHelper.OnSelectedSampleRecordForReport(sampleRecordList.LastOrDefault());
            if (resultRecordDomain == null)
                return;
            string parameterValue;
            switch (_selectedQualityControl.AssayParameter)
            {

                case assay_parameter.ap_Concentration:
                    parameterValue = ScoutUtilities.Misc.ConvertToConcPower(resultRecordDomain.CumulativeResult.ConcentrationML,
                        LanguageResourceHelper.CurrentFormatCulture);
                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                        "LID_Result_Concentration", parameterValue);
                    break;
                case assay_parameter.ap_PopulationPercentage:
                    parameterValue = ScoutUtilities.Misc.UpdateTrailingPoint(resultRecordDomain.CumulativeResult.Viability,
                        TrailingPoint.One);
                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                        "LID_Result_Viability", parameterValue);
                    break;
                case assay_parameter.ap_Size:
                    parameterValue =
                        ScoutUtilities.Misc.UpdateTrailingPoint(resultRecordDomain.CumulativeResult.Size, TrailingPoint.Two);
                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate(
                        "LID_RunResultsReport_Graph_Diameter_AxisTitle", parameterValue);
                    break;
            }
            AddReportTemplateToLastCheckTableList(reportTableTemplateObj);
        }

        private void AddReportTemplateToCellTypeTableList(ReportTableTemplate reportTableTemplateObj)
        {
            if (reportTableTemplateObj != null)
            {
                _qualityControlsReportModel.QualityControlsReportDomainInstance
                    .QualityControlReportCellTypeTableList.Add(reportTableTemplateObj);
            }
        }

        private void AddReportTemplateToCurrentLotTableList(ReportTableTemplate reportTableTemplateObj)
        {
            if (reportTableTemplateObj != null)
            {
                _qualityControlsReportModel.QualityControlsReportDomainInstance
                    .QualityControlReportCurrentLotTableList.Add(reportTableTemplateObj);
            }
        }

        private void AddReportTemplateToLastCheckTableList(ReportTableTemplate reportTableTemplateObj)
        {
            if (reportTableTemplateObj != null)
            {
                _qualityControlsReportModel.QualityControlsReportDomainInstance
                    .QualityControlReportLastCheckTableList.Add(reportTableTemplateObj);
            }
        }

        private void SetHeaderData()
        {
            var reportMandatoryHeaderDomainInstance =
                ReportWindowModel.SetReportHeaderData(_printTitle, "LID_GridLabel_QualityControl", _comments);
            _qualityControlsReportModel.QualityControlsReportDomainInstance
                .QualityControlReportMandatoryHeaderDomainList.Add(reportMandatoryHeaderDomainInstance);
        }

        #endregion
    }
}