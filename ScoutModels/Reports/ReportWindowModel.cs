using ApiProxies.Generic;
using log4net;
using Microsoft.Reporting.WinForms;
using ScoutDomains;
using ScoutDomains.Reports.Common;
using ScoutDomains.RunResult;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutModels.Settings;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ScoutUtilities;

namespace ScoutModels.Reports
{
    public class ReportWindowModel
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool OpenPdfSaveFileDialog(ref string fileName)
        {
            try
            {
                var defaultFilter = "PDF Files (*.pdf)|*.pdf";
                var initialDirectory = LoggedInUser.CurrentExportPath;
                if (!string.IsNullOrEmpty(fileName)) 
                    initialDirectory = FileSystem.GetDirectoryName(initialDirectory);
                if (!Directory.Exists(initialDirectory))
                    initialDirectory = FileSystem.GetDefaultExportPath(LoggedInUser.CurrentUserId);
                if (!Directory.Exists(initialDirectory))
                    initialDirectory = FileSystem.GetDefaultExportPath("");

                var args = new SaveFileDialogEventArgs(fileName, initialDirectory, defaultFilter,
                    FileTypeHelper.GetString(FileType.Pdf, false));
                if (DialogEventBus.OpenSaveFileDialog(null, args) != true) return false;

                fileName = args.FullFilePathSelected;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get export path for '{fileName}'.", ex);
                return false;
            }
        }

        public static bool SaveToPdf(ReportViewer viewer, string savePath, string deviceInfo)
        {
            try
            {
                var bytes = viewer.LocalReport.Render("PDF", deviceInfo);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                Log.Debug($"SaveToPdf: savePath: {savePath}");

				return true;
            }
            catch (UnauthorizedAccessException unauthorizedAccessException)
            {
                ExceptionHelper.HandleExceptions(unauthorizedAccessException, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_File_Unauthorized"));
            }
            catch (IOException ioException)
            {
                ExceptionHelper.HandleExceptions(ioException, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_File_ERROR"));
            }
            catch (Exception e)
            {
                Log.Error($"Failed to write pdf to '{savePath}'", e);
            }

            return false;
        }

        public static List<ReportThreeColumnDomain> GetSignaturesForReport(IList<SignatureInstanceDomain> signatures)
        {
            if (signatures == null) return null;

            try
            {
                var list = new List<ReportThreeColumnDomain>();
                foreach (var selectedSignature in signatures)
                {
                    var columnDomain = new ReportThreeColumnDomain
                    {
                        FirstColumnData = selectedSignature.Signature.SignatureIndicator,
                        SecondColumnData = selectedSignature.SigningUser,
                        ThirdColumnData = ScoutUtilities.Misc.ConvertToCustomLongDateTimeFormat(selectedSignature.SignedDate)
                    };

                    list.Add(columnDomain);
                }

                return list;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get convert {nameof(SignatureInstanceDomain)}s to {nameof(ReportThreeColumnDomain)}s", ex);
                return null;
            }
        }

        public static ReportTableTemplate CreateReportTableTemplate(string parameterName, string parameterValue) // only used here and in other reports
        {
            if (string.IsNullOrEmpty(parameterName) && string.IsNullOrEmpty(parameterValue))
            {
                return null;
            }

            return new ReportTableTemplate
            {
                ParameterName = LanguageResourceHelper.Get(parameterName) ?? string.Empty,
                ParameterValue = parameterValue
            };
        }

        public static ReportMandatoryHeaderDomain SetReportHeaderData(string title, string subTitleKey, string commentData)
        {
            var reportMandatoryHeaderDomainObj = new ReportMandatoryHeaderDomain()
            {
                ReportTitle = title,
                ReportSubTitle = LanguageResourceHelper.Get(subTitleKey),
                CurrentDateTime = ScoutUtilities.Misc.ConvertToCustomLongDateTimeFormat(DateTime.Now),
                CommentHeader = LanguageResourceHelper.Get("LID_Label_Comments"),
                CommentData = commentData
            };

            var hardwareSettingsModel = new HardwareSettingsModel();
            hardwareSettingsModel.GetVersionInformation();
            var hardwareSettingsDomain = hardwareSettingsModel.HardwareSettingsDomain;
            if (hardwareSettingsDomain != null)
            {
                const string colon = ":";
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(LanguageResourceHelper.Get("LID_CheckBox_DeviceSerialNumber"));
                stringBuilder.Append("".PadLeft(3, ' '));
                stringBuilder.Append(colon);
                stringBuilder.Append("".PadLeft(3, ' '));
                stringBuilder.Append(hardwareSettingsDomain.SerialNumber);
                reportMandatoryHeaderDomainObj.DeviceSerialData = stringBuilder.ToString();
            }

            return reportMandatoryHeaderDomainObj;
        }

        public static List<ReportTableTemplate> SetAnalysisParameter(SampleRecordDomain sampleRecordDomain)
        {
            if (sampleRecordDomain == null) return null;

            try
            {
                ReportTableTemplate reportTableTemplateObj;
                var reportTableTemplates = new List<ReportTableTemplate>();

                if (sampleRecordDomain.SelectedResultSummary?.CellTypeDomain != null)
                {
	                var cellTypeDomainInstance = sampleRecordDomain.SelectedResultSummary.CellTypeDomain;
	                var name = cellTypeDomainInstance.CellTypeName;

					if (sampleRecordDomain.BpQcName != "")
					{
						name = Misc.GetParenthesisQualityControlName(sampleRecordDomain.BpQcName, name);
					}

					reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_UsersLabel_CellType", name);
                    AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);

                    if (cellTypeDomainInstance.MinimumDiameter != null)
                    {
                        var minimumParameterNames = new[] { "LID_Label_MinDiameter", "LID_Label_MicroMeter_Unit" };
                        reportTableTemplateObj = CreateMultiHeaderReportTableTemplate(minimumParameterNames,
                            ScoutUtilities.Misc.UpdateTrailingPoint(cellTypeDomainInstance.MinimumDiameter.Value,
                                TrailingPoint.Two));

                    }

                    AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);
                    var maximumParameterNames = new[] { "LID_Label_MaxDiameter", "LID_Label_MicroMeter_Unit" };

                    if (cellTypeDomainInstance.MaximumDiameter != null)
                    {
                        reportTableTemplateObj = CreateMultiHeaderReportTableTemplate(maximumParameterNames,
                            ScoutUtilities.Misc.UpdateTrailingPoint(cellTypeDomainInstance.MaximumDiameter.Value,
                                TrailingPoint.Two));
                    }

                    AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);
                }

                if (sampleRecordDomain.SelectedResultSummary?.CellTypeDomain != null)
                {
                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_Label_Images",
                        sampleRecordDomain.SelectedResultSummary.CellTypeDomain.Images.ToString());
                    AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);

                    var cellTypeDomainInstance = sampleRecordDomain.SelectedResultSummary.CellTypeDomain;
                    if (cellTypeDomainInstance.CellSharpness != null)
                    {
                        reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_Label_CellSharpness",
                            ScoutUtilities.Misc.UpdateTrailingPoint(cellTypeDomainInstance.CellSharpness.Value,
                                TrailingPoint.One));
                    }
					AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);

                    if (cellTypeDomainInstance.MinimumCircularity != null)
                    {
                        reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_Label_MinCircularity",
                            ScoutUtilities.Misc.UpdateTrailingPoint(cellTypeDomainInstance.MinimumCircularity.Value,
                                TrailingPoint.Two));
                    }
                    AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);

                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_Label_DeclusterDegree",
                        LanguageResourceHelper.Get(GetEnumDescription.GetDescription(cellTypeDomainInstance.DeclusterDegree)));
                    AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);

					if (cellTypeDomainInstance.CellSharpness != null)
					{
						reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_Label_AdjustmentFactorPercentage",
							ScoutUtilities.Misc.UpdateTrailingPoint(cellTypeDomainInstance.CalculationAdjustmentFactor.Value,
								TrailingPoint.Two));
					}
					AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);
				}

				if (sampleRecordDomain.SelectedResultSummary?.AnalysisDomain != null)
                {
                    var appTypeInstance = sampleRecordDomain.SelectedResultSummary.AnalysisDomain;
                    reportTableTemplateObj = new ReportTableTemplate();
                    foreach (var ap in appTypeInstance.AnalysisParameter)
                    {
                        switch (ap.Label)
                        {
                            case ApplicationConstants.CellSpotArea:
                                var sAParameterNames = new[] { "LID_Label_SpotArea", "LID_Label_Percentage_Unit" };
                                if (ap.ThresholdValue != null)
                                {
                                    reportTableTemplateObj = CreateMultiHeaderReportTableTemplate(sAParameterNames,
                                        ScoutUtilities.Misc.UpdateTrailingPoint(ap.ThresholdValue.Value,
                                            TrailingPoint.One));
                                }
                                AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);
                                break;

                            case ApplicationConstants.AvgSpotBrightness:
                                var sBParameterNames = new[] { "LID_Label_SpotBrightness", "LID_Label_Percentage_Unit" };
                                if (ap.ThresholdValue != null)
                                {
                                    reportTableTemplateObj = CreateMultiHeaderReportTableTemplate(sBParameterNames,
                                        ScoutUtilities.Misc.UpdateTrailingPoint(ap.ThresholdValue.Value,
                                            TrailingPoint.One));
                                }
                                AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);
                                break;
                        }
                    }
                }

                reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_Label_Dilution", sampleRecordDomain.DilutionName);
                AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);

                var washNameKey = GetEnumDescription.GetDescription(sampleRecordDomain.WashName);
                reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_QMgmtHEADER_Wash", LanguageResourceHelper.Get(washNameKey));
                AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);

                reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_Label_Comments", sampleRecordDomain.Tag);
                AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);

                if (sampleRecordDomain?.ResultSummaryList != null && sampleRecordDomain.ResultSummaryList.Any())
                {
                    var resultSummaryList = sampleRecordDomain.ResultSummaryList;

                    var reAnalysisBy = string.Empty;
                    var reAnalysisDateTime = string.Empty;

                    if (resultSummaryList.Count > 1 && sampleRecordDomain.SelectedResultSummary != null &&
                        !sampleRecordDomain.SelectedResultSummary.UUID.IsEmpty() &&
                        !sampleRecordDomain.SelectedResultSummary.UUID.Equals(resultSummaryList.First().UUID))
                    {
                        reAnalysisBy = sampleRecordDomain.SelectedResultSummary.UserId;
                        reAnalysisDateTime = ScoutUtilities.Misc.ConvertToCustomLongDateTimeFormat(sampleRecordDomain.SelectedResultSummary.RetrieveDate);
                    }

                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_QCHeader_AnalysisDateTime",
                        ScoutUtilities.Misc.ConvertToCustomLongDateTimeFormat(resultSummaryList[0].RetrieveDate));
                    AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);

                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_QCHeader_ReAnalysisDateTime", reAnalysisDateTime);
                    AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);

                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_Report_AnalysisBy", resultSummaryList[0].UserId);
                    AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);

                    reportTableTemplateObj = ReportWindowModel.CreateReportTableTemplate("LID_Report_ReanalysisBy", reAnalysisBy);
                    AddReportTableTemplateToAnalysisParameterList(reportTableTemplateObj, reportTableTemplates);
                }

                return reportTableTemplates;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return null;
            }
        }

        #region Private Methods

        private static ReportTableTemplate CreateMultiHeaderReportTableTemplate(string[] parameterNames, string parameterValue)
        {
            var parameterName = new StringBuilder();
            foreach (var name in parameterNames)
            {
                parameterName.Append(LanguageResourceHelper.Get(name));
            }

            if (string.IsNullOrEmpty(parameterName.ToString()) && string.IsNullOrEmpty(parameterValue))
            {
                return null;
            }

            return new ReportTableTemplate()
            {
                ParameterName = parameterName.ToString(),
                ParameterValue = parameterValue
            };
        }

        private static void AddReportTableTemplateToAnalysisParameterList(ReportTableTemplate reportTableTemplate, List<ReportTableTemplate> templates)
        {
            if (reportTableTemplate != null)
            {
                templates?.Add(reportTableTemplate);
            }
        }

        #endregion
    }
}