using Microsoft.Reporting.WinForms;
using ScoutDomains.Analysis;
using ScoutDomains.Reports.CellTypes;
using ScoutDomains.Reports.Common;
using ScoutLanguageResources;
using ScoutModels.Reports;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using ScoutModels.Common;
using ScoutUtilities.Helper;

namespace ScoutViewModels.ViewModels.Reports
{
    public class CellTypesReportViewModel : ReportWindowViewModel
    {
        public CellTypesReportViewModel(string selectedUserId, string printTitle, string printComments, List<CellTypeDomain> cellList)
        {
            _selectedUserId = selectedUserId;
            _printTitle = printTitle;
            _printComments = printComments;
            _cellList = cellList;

            _cellTypesReportModel = new CellTypesReportModel();
            ReportTitle = LanguageResourceHelper.Get("LID_GridLabel_CellTypes");
            ReportWindowTitle = LanguageResourceHelper.Get("LID_GridLabel_CellTypes");
            ReportType = ReportType.CellType;
        }

        #region Properties

        private CellTypesReportModel _cellTypesReportModel;
        private List<CellTypeDomain> _cellList;
        private string _selectedUserId;
        private string _printTitle;
        private string _printComments;

        #endregion

        #region Override Methods

        protected override void InitializeLists()
        {
            _cellTypesReportModel.CellTypesReportDomainInstance.ReportMandatoryHeaderDomainList = new List<ReportMandatoryHeaderDomain>();
            _cellTypesReportModel.CellTypesReportDomainInstance.CellTypesReportTableHeaderList = new List<CellTypesReportTableDomain>();
            _cellTypesReportModel.CellTypesReportDomainInstance.CellTypesReportTableDataList = new List<CellTypesReportTableDomain>();
            _cellTypesReportModel.CellTypesReportDomainInstance.CellTypesReportTableColumnWidthList = new List<ReportTableColumnWidth>();
        }

        protected override void RetrieveData()
        {
            SetReportCommonInfo();
            SetHeader();
            SetTableHeader();
            SetTableData();
            SetTableWidth();
        }

        protected override void LoadReportViewer()
        {
            ReportViewer = new ReportViewer();
            ReportViewer.LocalReport.DisplayName = LanguageResourceHelper.Get("LID_GridLabel_CellTypes");
            var cellTypeDomain = _cellTypesReportModel.CellTypesReportDomainInstance;

            AddReportDataSource("CellTypeCommonInfoTBL", cellTypeDomain.CellTypeReportCommonInfoList);
            AddReportDataSource("CellTypeReportHeaderTBL", cellTypeDomain.ReportMandatoryHeaderDomainList);
            AddReportDataSource("CellTypeReportTableHeaderTBL", cellTypeDomain.CellTypesReportTableHeaderList);
            AddReportDataSource("CellTypeReportTableDataTBL", cellTypeDomain.CellTypesReportTableDataList);
            AddReportDataSource("CellTypeReportTableColumnWidthTBL", cellTypeDomain.CellTypesReportTableColumnWidthList);
            AddReportDataSource("AnalysisTypeReportDataTBL", cellTypeDomain.AnalysisTypeList);

            var lang = LanguageEnums.GetLangType(ApplicationLanguage.GetLanguage());
            var reportViewerPath = GetReportViewerPath(lang);
            RefreshAndSetReportContent(reportViewerPath);
        }

        #endregion

        #region Private Methods

        private string GetReportViewerPath(LanguageType language)
        {
            if (language == LanguageType.eChinese)
                return AppDomain.CurrentDomain.BaseDirectory + @"Reports\CellTypesReportLandscapeRdlcViewerZh-CN.rdlc";
            if (language == LanguageType.eJapanese)
                return AppDomain.CurrentDomain.BaseDirectory + @"Reports\CellTypesReportLandscapeRdlcViewerJa-JP.rdlc";
            return AppDomain.CurrentDomain.BaseDirectory + @"Reports\CellTypesReportLandscapeRdlcViewer.rdlc";
        }

        private void SetReportCommonInfo()
        {
            var username = $"{LanguageResourceHelper.Get("LID_UsersLabel_UserId")}   {_selectedUserId}";
            _cellTypesReportModel.CellTypesReportDomainInstance.CellTypeReportCommonInfoList =
                new List<CellTypeReportCommonInfoDomain>
                {
                    new CellTypeReportCommonInfoDomain
                    {
                        UserName = username
                    }
                };
        }

        private void SetHeader()
        {
            var reportMandatoryHeaderDomainObj = ReportWindowModel.SetReportHeaderData(_printTitle, "LID_GridLabel_CellTypes", _printComments);
            _cellTypesReportModel.CellTypesReportDomainInstance.ReportMandatoryHeaderDomainList.Add(reportMandatoryHeaderDomainObj);
        }

        private void SetTableHeader()
        {
            _cellTypesReportModel.CellTypesReportDomainInstance.CellTypesReportTableHeaderList =
                new List<CellTypesReportTableDomain>()
                {
                    new CellTypesReportTableDomain()
                    {
                        CellType = LanguageResourceHelper.Get("LID_Icon_CellType"),
                        MinDia = LanguageResourceHelper.Get("LID_Label_MinDiameter") + "|" + LanguageResourceHelper.Get("LID_Label_MicroMeter_Unit"),
                        MaxDia = LanguageResourceHelper.Get("LID_Label_MaxDiameter") + "|" + LanguageResourceHelper.Get("LID_Label_MicroMeter_Unit"),
                        Images = LanguageResourceHelper.Get("LID_Label_Images"),
                        CellSharp = LanguageResourceHelper.Get("LID_Label_CellSharpness"),
                        MinCircularity = LanguageResourceHelper.Get("LID_Label_MinimumCircularity"),
                        DeclusterDegree = LanguageResourceHelper.Get("LID_Label_DeclusterDegree"),
                        AspirationCycles = LanguageResourceHelper.Get("LID_Label_AspirationCycle"),
                        AppTypes = LanguageResourceHelper.Get("LID_Report_AnalysisType"),
                        SpotBrightness = LanguageResourceHelper.Get("LID_Label_SpotBrightness") + "|" + LanguageResourceHelper.Get("LID_Label_Percentage_Unit"),
                        SpotArea = LanguageResourceHelper.Get("LID_Label_SpotArea")+ "|" + LanguageResourceHelper.Get("LID_Label_Percentage_Unit"),
                        Comments = LanguageResourceHelper.Get("LID_Label_Comments"),
                        MixingCycles = LanguageResourceHelper.Get("LID_Label_MixingCycle"),
                        CalculationAdjustmentFactor = LanguageResourceHelper.Get("LID_Label_AdjustmentFactor") + " " + LanguageResourceHelper.Get("LID_Label_Percentage_Unit")
                    }
                };
        }

        private void SetTableData()
        {
            _cellTypesReportModel.CellTypesReportDomainInstance.CellTypesReportTableDataList = new List<CellTypesReportTableDomain>();
            _cellTypesReportModel.CellTypesReportDomainInstance.AnalysisTypeList = new List<AnalysisTypeReportDomain>();

            if (_cellList == null) return;

            foreach (var item in _cellList)
            {
                if (item.IsCellSelected)
                {

                    var cellTypesReportTableDomain = new CellTypesReportTableDomain { CellType = item.CellTypeName };

                    if (item.MinimumDiameter != null)
                        cellTypesReportTableDomain.MinDia = ScoutUtilities.Misc.UpdateTrailingPoint(item.MinimumDiameter.Value, TrailingPoint.Two);
                    if (item.MaximumDiameter != null)
                        cellTypesReportTableDomain.MaxDia = ScoutUtilities.Misc.UpdateTrailingPoint(item.MaximumDiameter.Value, TrailingPoint.Two);

                    var analysesTypes = new AnalysisTypeReportDomain
                    {
                        CellType = item.CellTypeName
                    };

                    if (item.Images != null)
                    {
                        cellTypesReportTableDomain.Images = ScoutUtilities.Misc.ConvertToString(item.Images.Value);
                    }

                    if (item.CellSharpness != null)
                        cellTypesReportTableDomain.CellSharp = ScoutUtilities.Misc.UpdateTrailingPoint(item.CellSharpness.Value, TrailingPoint.One);
                    if (item.MinimumCircularity != null)
                        cellTypesReportTableDomain.MinCircularity = ScoutUtilities.Misc.UpdateTrailingPoint(item.MinimumCircularity.Value, TrailingPoint.Two);

                    cellTypesReportTableDomain.DeclusterDegree = GetResourceKeyName(GetEnumDescription.GetDescription(item.DeclusterDegree));
                    cellTypesReportTableDomain.AspirationCycles = ScoutUtilities.Misc.ConvertToString(item.AspirationCycles);

                    if (item.CalculationAdjustmentFactor != null)
                        cellTypesReportTableDomain.CalculationAdjustmentFactor =
                            ScoutUtilities.Misc.UpdateTrailingPoint(item.CalculationAdjustmentFactor.Value,
                                TrailingPoint.One);
                    if (item.AnalysisDomain?.AnalysisParameter != null)
                    {
                        analysesTypes.AnalysisType = item.AnalysisDomain.Label;
                        cellTypesReportTableDomain.MixingCycles = ScoutUtilities.Misc.ConvertToString(item.AnalysisDomain.MixingCycle);
                        cellTypesReportTableDomain.AppTypes = item.AnalysisDomain.Label;

                        foreach (var ap in item.AnalysisDomain.AnalysisParameter)
                        {
                            switch (ap.Label)
                            {
                                case ApplicationConstants.CellSpotArea:
                                    if (ap.ThresholdValue != null)
                                    {
                                        cellTypesReportTableDomain.SpotArea = ScoutUtilities.Misc.UpdateTrailingPoint(ap.ThresholdValue.Value, TrailingPoint.One);
                                        analysesTypes.ViableSportArea = ScoutUtilities.Misc.ConvertToString(ap.ThresholdValue.Value);
                                    }

                                    break;
                                case ApplicationConstants.AvgSpotBrightness:
                                    if (ap.ThresholdValue != null)
                                    {
                                        cellTypesReportTableDomain.SpotBrightness = ScoutUtilities.Misc.UpdateTrailingPoint(ap.ThresholdValue.Value, TrailingPoint.One);
                                        analysesTypes.ViableSportBrightness = ScoutUtilities.Misc.ConvertToString(ap.ThresholdValue.Value);
                                    }

                                    break;
                            }
                        }
                    }

                    _cellTypesReportModel.CellTypesReportDomainInstance.CellTypesReportTableDataList.Add(cellTypesReportTableDomain);
                    _cellTypesReportModel.CellTypesReportDomainInstance.AnalysisTypeList.Add(analysesTypes);
                }
            }
        }

        private void SetTableWidth()
        {
            var columnWidth = "2.3cm";
            var reportTableColumnWidthObj = new ReportTableColumnWidth {TableColumnWidth = columnWidth};
            _cellTypesReportModel.CellTypesReportDomainInstance.CellTypesReportTableColumnWidthList.Add(reportTableColumnWidthObj);
        }

        #endregion
    }
}
