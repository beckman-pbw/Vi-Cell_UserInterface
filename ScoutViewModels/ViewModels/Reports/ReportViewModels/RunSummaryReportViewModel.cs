using Microsoft.Reporting.WinForms;
using ScoutDomains;
using ScoutDomains.Reports.Common;
using ScoutDomains.Reports.RunSummary;
using ScoutDomains.RunResult;
using ScoutLanguageResources;
using ScoutModels.Reports;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using ScoutModels.Common;
using ScoutUtilities.Helper;

namespace ScoutViewModels.ViewModels.Reports
{
    public class RunSummaryReportViewModel : ReportWindowViewModel
    {
        public RunSummaryReportViewModel(string printTitle, string comments, List<ReportPrintOptions> reportPrintOptions, List<ReportPrintOptions> analysisParameters,
            List<ReportPrintOptions> defaultParameters, List<SampleRecordDomain> sampleRecords)
        {
            _runSummaryReportModel = new RunSummaryReportModel();
            _parameterList = new List<ReportPrintOptions>();
            ReportTitle = LanguageResourceHelper.Get("LID_GridLabel_CompletedRunSummary");
            ReportWindowTitle = LanguageResourceHelper.Get("LID_GridLabel_CompletedRunSummary");
            ReportType = ReportType.CompletedRunSummary;

            _printTitle = printTitle;
            _comments = comments;
            _reportPrintOptions = reportPrintOptions;
            _analysisParameters = analysisParameters;
            _defaultParameters = defaultParameters;
            _sampleRecords = sampleRecords;
        }
        
        #region Properties

        private List<ReportPrintOptions> _parameterList;
        private RunSummaryReportModel _runSummaryReportModel;
        private RunSummaryReportTableVisibility _runSummaryReportTableVisibility;

        private string _printTitle;
        private string _comments;
        private List<ReportPrintOptions> _reportPrintOptions;
        private List<ReportPrintOptions> _analysisParameters;
        private List<ReportPrintOptions> _defaultParameters;
        private List<SampleRecordDomain> _sampleRecords;

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
            _runSummaryReportTableVisibility = new RunSummaryReportTableVisibility();
            _runSummaryReportModel.RunSummaryReportDomainInstance.ReportMandatoryHeaderDomainList = new List<ReportMandatoryHeaderDomain>();
            _runSummaryReportModel.RunSummaryReportDomainInstance.RunSummaryReportTableHeaderList = new List<RunSummaryReportTableDomain>();
            _runSummaryReportModel.RunSummaryReportDomainInstance.RunSummaryReportTableVisibilityList = new List<RunSummaryReportTableVisibility>();
            _runSummaryReportModel.RunSummaryReportDomainInstance.RunSummaryReportTableDataList = new List<RunSummaryReportTableDomain>();
            _runSummaryReportModel.RunSummaryReportDomainInstance.RunSummaryTableSizeList = new List<RunSummaryTableSize>();
        }

        protected override void RetrieveData()
        {
            SetHeaderData();
            SetDefaultTableVisibilityToHidden();
            SetTableHeaderAndVisibility();
            CenterTable();
            SetTableData();
            SetTableColumnWidth();
        }

        protected override void LoadReportViewer()
        {
            ReportViewer = new ReportViewer();
            ReportViewer.LocalReport.DisplayName = LanguageResourceHelper.Get("LID_GridLabel_CompletedRunSummary");
            var lang = LanguageEnums.GetLangType(ApplicationLanguage.GetLanguage());
            var reportViewerPath = GetReportViewerPath(lang);
            var runSummaryDomain = _runSummaryReportModel.RunSummaryReportDomainInstance;

            AddReportDataSource("ReportMandatoryHeaderDomainTBL", runSummaryDomain.ReportMandatoryHeaderDomainList);
            AddReportDataSource("RunSummaryReportTableHeaderTBL", runSummaryDomain.RunSummaryReportTableHeaderList);
            AddReportDataSource("RunSummaryReportTableVisibilityTBL", runSummaryDomain.RunSummaryReportTableVisibilityList);
            AddReportDataSource("RunSummaryReportTableDataTBL", runSummaryDomain.RunSummaryReportTableDataList);
            AddReportDataSource("RunSummaryTableSizeListTBL", runSummaryDomain.RunSummaryTableSizeList);

            RefreshAndSetReportContent(reportViewerPath);

            ReportViewer.RefreshReport();
        }

        #endregion

        #region Private Methods

        private string GetReportViewerPath(LanguageType language)
        {
            if (language == LanguageType.eChinese)
                return AppDomain.CurrentDomain.BaseDirectory + @"Reports\RunSummaryReportLandscapeRdlcViewerZh-CN.rdlc";
            if (language == LanguageType.eJapanese)
                return AppDomain.CurrentDomain.BaseDirectory + @"Reports\RunSummaryReportLandscapeRdlcViewerJa-JP.rdlc";
            return AppDomain.CurrentDomain.BaseDirectory + @"Reports\RunSummaryReportLandscapeRdlcViewer.rdlc";
        }

        private void SetTableColumnWidth()
        {
            const double pageWidth = 25.9;

            var count = _runSummaryReportModel.RunSummaryReportDomainInstance
                .RunSummaryReportTableVisibilityList[0].GetType().GetProperties()
                .Where(propertyInfo => propertyInfo.PropertyType == typeof(bool))
                .Select(GetBoolValue)
                .Count(value => !value);
            
            var width = ScoutUtilities.Misc.ConvertToString(pageWidth / ((double)count + 2)) + "cm";

            _runSummaryReportModel.RunSummaryReportDomainInstance.RunSummaryTableSizeList.Add(new RunSummaryTableSize
            {
                TableColumnWidth = width
            });

            bool GetBoolValue(PropertyInfo propertyInfo)
            {
                return _runSummaryReportModel.RunSummaryReportDomainInstance.RunSummaryReportTableVisibilityList != null &&
                       (bool)propertyInfo.GetValue(_runSummaryReportModel.RunSummaryReportDomainInstance.RunSummaryReportTableVisibilityList[0], null);
            }
        }

        private void SetDefaultTableVisibilityToHidden()
        {
            _runSummaryReportTableVisibility = new RunSummaryReportTableVisibility();
            foreach (PropertyInfo pi in _runSummaryReportTableVisibility.GetType().GetProperties())
                pi.SetValue(_runSummaryReportTableVisibility, true);
        }

        private void SetTableHeaderAndVisibility()
        {
            var resultParameters = _reportPrintOptions.Where(r => r.IsParameterChecked.Equals(true)).Select(r => r).ToList();
            var analysisParameters = _analysisParameters.Where(r => r.IsParameterChecked.Equals(true)).Select(r => r).ToList();
            resultParameters.AddRange(analysisParameters);
            var tableHeader = new RunSummaryReportTableDomain
            {
                SampleId = LanguageResourceHelper.Get("LID_QMgmtHEADER_SampleId"),
                AnalysisDate = LanguageResourceHelper.Get("LID_QCHeader_AnalysisDate"),
                Concentration = LanguageResourceHelper.Get("LID_ReportLabel_Concentration_WithoutUnit"),
                ViableConcentration = LanguageResourceHelper.Get("LID_Label_ViableConcentrationWithoutUnit"),
                Viability = LanguageResourceHelper.Get("LID_TabItem_Viability"),
                Signature = LanguageResourceHelper.Get("LID_Icon_Signature"),
                ConcentrationUnit = GetUnits(RunSummaryParameterType.Concentration),
                ViableConcentrationUnit = GetUnits(RunSummaryParameterType.ViableConcentration),
                ViabilityUnit = GetUnits(RunSummaryParameterType.Viability)
            };
           
            _parameterList = resultParameters;
            var colNum = 5;
            _runSummaryReportTableVisibility.IsSignatureColumnHidden = true;           
            if(_parameterList.Any(x => x.ParameterName == LanguageResourceHelper.Get("LID_Icon_Signature")))
            {
                _runSummaryReportTableVisibility.IsSignatureColumnHidden = false;
            }
            _parameterList = _parameterList.Where(x => x.ParameterName != LanguageResourceHelper.Get("LID_Icon_Signature")).ToList();
            foreach (var item in _parameterList)
            {
                switch (colNum)
                {
                    // Column 1 - 4 is fixed , mapping configurable columns 5-7
                    case 5:
                        _runSummaryReportTableVisibility.IsColumnFiveHidden = !item.IsParameterChecked;
                        tableHeader.ColumnFive = GetHeader(item.ParameterType, item.ParameterName);
                        tableHeader.UnitFive = GetUnits(item.ParameterType);
                        item.ColumnType = RunSummaryColumnType.ColumnFive;
                        break;
                    case 6:
                        _runSummaryReportTableVisibility.IsColumnSixHidden = !item.IsParameterChecked;
                        tableHeader.ColumnSix = GetHeader(item.ParameterType, item.ParameterName);
                        tableHeader.UnitSix = GetUnits(item.ParameterType);
                        item.ColumnType = RunSummaryColumnType.ColumnSix;
                        break;
                    case 7:
                        _runSummaryReportTableVisibility.IsColumnSevenHidden = !item.IsParameterChecked;
                        tableHeader.ColumnSeven = GetHeader(item.ParameterType, item.ParameterName);
                        tableHeader.UnitSeven = GetUnits(item.ParameterType);
                        item.ColumnType = RunSummaryColumnType.ColumnSeven;
                        break;
                }
                colNum++;
            }
            _runSummaryReportModel.RunSummaryReportDomainInstance.RunSummaryReportTableHeaderList.Add(tableHeader);
            _runSummaryReportModel.RunSummaryReportDomainInstance.RunSummaryReportTableVisibilityList.Add(_runSummaryReportTableVisibility);
        }

        private void CenterTable()
        {
            var lang = LanguageEnums.GetLangType(ApplicationLanguage.GetLanguage());
            var reportViewerPath = GetReportViewerPath(lang);

            XmlDocument doc = new XmlDocument();
            doc.Load(reportViewerPath);

            XmlNodeList elemList = doc.GetElementsByTagName("Left");
            for (int i = 0; i < elemList.Count; i++)
            {
                if (elemList[i].ParentNode.Name.Equals("Tablix"))
                {
                    XmlAttributeCollection attributes = elemList[i].ParentNode.Attributes;
                    XmlAttribute attr = (XmlAttribute)attributes.GetNamedItem("Name");
                    if (attr.Value.Equals("Tablix4"))
                    {
                        if (_parameterList.Count == 3 || (_parameterList.Count == 2 && !_runSummaryReportTableVisibility.IsSignatureColumnHidden))
                            elemList[i].InnerXml = "0.31743cm";
                        else if (_parameterList.Count == 2 || (_parameterList.Count == 1 && !_runSummaryReportTableVisibility.IsSignatureColumnHidden))
                            elemList[i].InnerXml = "2cm";
                        else if (_parameterList.Count == 1 || (_parameterList.Count == 0 && !_runSummaryReportTableVisibility.IsSignatureColumnHidden))
                            elemList[i].InnerXml = "3cm";
                        else
                            elemList[i].InnerXml = "5cm";
                        doc.Save(reportViewerPath);
                        break;
                    }
                }
            }
        }

        private string GetUnits(RunSummaryParameterType parameterType)
        {
            switch (parameterType)
            {
                case RunSummaryParameterType.Viability:
                    return LanguageResourceHelper.Get("LID_Label_Percentage_Unit");
                case RunSummaryParameterType.Concentration:
                case RunSummaryParameterType.ViableConcentration:
                    return LanguageResourceHelper.Get("LID_Label_TenthPower");
                case RunSummaryParameterType.Size:
                case RunSummaryParameterType.ViableSize:
                    return LanguageResourceHelper.Get("LID_Label_MicroMeter_Unit");
            }
            return string.Empty;
        }

        private string GetHeader(RunSummaryParameterType parameterType, string name)
        {
            switch (parameterType)
            {
                case RunSummaryParameterType.Viability:
                    return LanguageResourceHelper.Get("LID_TabItem_Viability");
                case RunSummaryParameterType.Concentration:
                    return LanguageResourceHelper.Get("LID_ReportLabel_Concentration_WithoutUnit");
                case RunSummaryParameterType.ViableConcentration:
                    return LanguageResourceHelper.Get("LID_Label_ViableConcentrationWithoutUnit");
                case RunSummaryParameterType.Size:
                    return LanguageResourceHelper.Get("LID_TabItem_Size");
                case RunSummaryParameterType.ViableSize:
                    return LanguageResourceHelper.Get("LID_Label_ViableSize_WithoutUnit");
            }
            return name;
        }
       
        private void SetTableData()
        {
            if (!_parameterList.Any())
            {
                var defaultParameters = _defaultParameters.ToList();
                _parameterList.AddRange(defaultParameters);
            }

            foreach (var sampleData in _sampleRecords)
            {
                if (sampleData.ResultSummaryList == null)
                    continue;
                for (int index = 0; index < sampleData.ResultSummaryList.Count; index++)
                {
                    var runSummaryReportTableDomainObject = new RunSummaryReportTableDomain();
                    _parameterList.ForEach(p =>
                    {
                        runSummaryReportTableDomainObject.SampleId = sampleData.SampleIdentifier;
                        runSummaryReportTableDomainObject.Signature =
                            sampleData.ResultSummaryList[index]?.SignatureList?
                                .FirstOrDefault()?.Signature.SignatureIndicator ??
                            string.Empty;
                        runSummaryReportTableDomainObject.AnalysisTimeStamp = GetPropertyValue(sampleData,
                            RunSummaryParameterType.AnalysisTimeStamp, index);
                        runSummaryReportTableDomainObject.AnalysisDate = GetPropertyValue(sampleData,
                            RunSummaryParameterType.AnalysisDate, index);
                        runSummaryReportTableDomainObject.Concentration = GetPropertyValue(sampleData,
                            RunSummaryParameterType.Concentration, index);
                        runSummaryReportTableDomainObject.ViableConcentration = GetPropertyValue(sampleData,
                            RunSummaryParameterType.ViableConcentration, index);
                        runSummaryReportTableDomainObject.Viability =
                            GetPropertyValue(sampleData, RunSummaryParameterType.Viability, index);
                        switch (p.ColumnType)
                        {
                            case RunSummaryColumnType.ColumnFive:
                                runSummaryReportTableDomainObject.ColumnFive =
                                    GetPropertyValue(sampleData, p.ParameterType, index);
                                runSummaryReportTableDomainObject.ColumnFiveAlignment = (int) p.AlignmentType;
                                break;
                            case RunSummaryColumnType.ColumnSix:
                                runSummaryReportTableDomainObject.ColumnSix =
                                    GetPropertyValue(sampleData, p.ParameterType, index);
                                runSummaryReportTableDomainObject.ColumnSixAlignment = (int) p.AlignmentType;
                                break;
                            case RunSummaryColumnType.ColumnSeven:
                                runSummaryReportTableDomainObject.ColumnSeven =
                                    GetPropertyValue(sampleData, p.ParameterType, index);
                                runSummaryReportTableDomainObject.ColumnSevenAlignment = (int) p.AlignmentType;
                                break;
                        }
                    });
                    _runSummaryReportModel.RunSummaryReportDomainInstance.RunSummaryReportTableDataList.Add(
                        runSummaryReportTableDomainObject);
                }
            }
        }

        private string GetPropertyValue(SampleRecordDomain sample, RunSummaryParameterType parameterType, int recordIndex)
        {
            ResultSummaryDomain cellTypeDomain = sample.ResultSummaryList[recordIndex];     // No validation: preconditions are guaranteed by code above
            switch (parameterType)
            {
                case RunSummaryParameterType.TotalCells:
                    return ScoutUtilities.Misc.ConvertToString(cellTypeDomain.CumulativeResult?.TotalCells);
                case RunSummaryParameterType.ViableCells:
                    return ScoutUtilities.Misc.ConvertToString(cellTypeDomain.CumulativeResult?.ViableCells);
                case RunSummaryParameterType.Concentration:
                    if (cellTypeDomain.CumulativeResult != null)
                    {
                        return ScoutUtilities.Misc.ConvertToConcPower(cellTypeDomain.CumulativeResult.ConcentrationML);
                    }
                    return string.Empty;
                case RunSummaryParameterType.ViableConcentration:
                    if (cellTypeDomain.CumulativeResult != null)
                    {
                        return ScoutUtilities.Misc.ConvertToConcPower(cellTypeDomain.CumulativeResult.ViableConcentration);
                    }
                    return string.Empty;
                case RunSummaryParameterType.Viability:
                    if (cellTypeDomain.CumulativeResult != null)
                    {
                        return ScoutUtilities.Misc.UpdateTrailingPoint(cellTypeDomain.CumulativeResult.Viability, TrailingPoint.One);
                    }
                    return null;
                case RunSummaryParameterType.Size:
                    if (cellTypeDomain.CumulativeResult != null)
                    {
                        return ScoutUtilities.Misc.UpdateTrailingPoint(cellTypeDomain.CumulativeResult.Size, TrailingPoint.Two);
                    }
                    return string.Empty;
                case RunSummaryParameterType.ViableSize:
                    if (cellTypeDomain.CumulativeResult != null)
                    {
                        return ScoutUtilities.Misc.UpdateTrailingPoint(cellTypeDomain.CumulativeResult.ViableSize, TrailingPoint.Two);
                    }
                    return string.Empty;
                case RunSummaryParameterType.Circularity:
                    if (cellTypeDomain.CumulativeResult != null)
                    {
                        return ScoutUtilities.Misc.UpdateTrailingPoint(cellTypeDomain.CumulativeResult.Circularity, TrailingPoint.Two);
                    }
                    return string.Empty;
                case RunSummaryParameterType.ViableCircularity:
                    if (cellTypeDomain.CumulativeResult != null)
                    {
                        return ScoutUtilities.Misc.UpdateTrailingPoint(cellTypeDomain.CumulativeResult.ViableCircularity, TrailingPoint.Two);
                    }
                    return string.Empty;
                case RunSummaryParameterType.AverageCellsPerImage:
                    return ScoutUtilities.Misc.ConvertToString(cellTypeDomain.CumulativeResult?.AverageCellsPerImage);
                case RunSummaryParameterType.AverageBrightField:
                    return ScoutUtilities.Misc.ConvertToString(cellTypeDomain.CumulativeResult?.AvgBackground);
                case RunSummaryParameterType.Bubbles:
                    return ScoutUtilities.Misc.ConvertToString(cellTypeDomain.CumulativeResult?.Bubble);
                case RunSummaryParameterType.ClusterCount:
                    return ScoutUtilities.Misc.ConvertToString(cellTypeDomain.CumulativeResult?.ClusterCount);
                case RunSummaryParameterType.CellType:
                    {
                        var name = cellTypeDomain.CellTypeDomain?.CellTypeName;

                        if (string.IsNullOrEmpty(name))
                            return LanguageResourceHelper.Get("LID_Label_CellTypeRemoved");

                        if (!string.IsNullOrEmpty(sample.BpQcName))
                            return sample.BpQcName + $" ({name})";

                        return name;
                    }
                case RunSummaryParameterType.Dilution:
                    return sample.DilutionName;
                case RunSummaryParameterType.Tag:
                    return sample.Tag;
                case RunSummaryParameterType.AnalysisDate:
                    return GetDate(sample.ResultSummaryList.First());
                case RunSummaryParameterType.AnalysisTimeStamp:
                    return sample.ResultSummaryList.First().TimeStamp.ToString();
                case RunSummaryParameterType.ReAnalysisDate:
                    if (sample.ResultSummaryList?.Count > 1 && recordIndex > 0)
                    {
                        return GetDate(sample.ResultSummaryList[recordIndex]);
                    }
                    return string.Empty;
                case RunSummaryParameterType.AnalysisBy:
                    return sample.ResultSummaryList.First().UserId;
                case RunSummaryParameterType.ReAnalysisBy:
                    if (sample.ResultSummaryList?.Count > 1 && recordIndex > 0)
                    {
                        return sample.ResultSummaryList[recordIndex].UserId;
                    }
                    return string.Empty;
            }
            return string.Empty;
        }

        private string GetDate(ResultSummaryDomain resultSummary)
        {
            return resultSummary != null ? ScoutUtilities.Misc.ConvertToCustomLongDateTimeFormat(resultSummary.RetrieveDate) : string.Empty;
        }

        private void SetHeaderData()
        {
            var reportMandatoryHeaderDomainObj = ReportWindowModel.SetReportHeaderData(_printTitle, "LID_GridLabel_CompletedRunSummary", _comments);
            _runSummaryReportModel.RunSummaryReportDomainInstance.ReportMandatoryHeaderDomainList.Add(reportMandatoryHeaderDomainObj);
        }

        #endregion
    }
}
