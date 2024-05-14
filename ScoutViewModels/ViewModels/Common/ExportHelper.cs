using ApiProxies.Generic;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Settings;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using ScoutViewModels.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;

namespace ScoutViewModels.ViewModels.Common
{
    public class ExportHelper
    {
	    protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void AutoExportPdf(SampleRecordDomain sampleRecord, string exportPath)
        {
            try
            {
                Log.Debug($"AutoExportPdf: <enter>: {sampleRecord.SampleIdentifier}, exportPath: {exportPath}");
                if (!FileSystem.IsFolderValidForExport(exportPath) ||
                    !FileSystem.EnsureDirectoryExists(exportPath))
                {
                    Log.Error($"AutoExportPdf: <exit - bad path>: {sampleRecord.SampleIdentifier}, exportPath: {exportPath}");
                    ExportModel.ExportFailedMessage();
                    return;
                }

				using (var adminViewModel = new ResultsRunResultsViewModel())
                {
                    adminViewModel.RecordHelper.SetImageList(sampleRecord);
                    if (sampleRecord != null)
                    {
                        if (sampleRecord.SampleImageList != null && sampleRecord.SampleImageList.Any())
                        {
                            var sampleImage = sampleRecord.SampleImageList[0];
                            sampleImage.TotalCumulativeImage = Convert.ToInt32(sampleRecord.SelectedResultSummary.CumulativeResult.TotalCumulativeImage);

							// Save for debugging.
                            //Log.Debug("AutoExportPdf: sampleImage.BrightFieldId: " + sampleImage.BrightFieldId + 
                            //          ", sampleRecord.SelectedResultSummary.UUID: " + sampleRecord.SelectedResultSummary.UUID);

							var imageData = adminViewModel.RecordHelper.GetImage(ImageType.Annotated, sampleImage.BrightFieldId, sampleRecord.SelectedResultSummary.UUID);

							sampleRecord.SelectedSampleImageRecord.ImageSet = imageData;
                        }

						adminViewModel.SelectedSampleRecordFromList = sampleRecord;
                        adminViewModel.ReportComments = sampleRecord.Tag;

                        foreach (var param in adminViewModel.ReportPrintOptionsList)
                        {
                            param.IsParameterChecked = true;
                        }

                        using (var recordHelper = new ResultRecordHelper(sampleRecord.UserId))
                        {
	                        // Save for debugging.
	                        //Log.Debug("AutoExportPdf: adminViewModel.SelectedSampleRecordFromList.SelectedResultSummary.UUID: " + adminViewModel.SelectedSampleRecordFromList.SelectedResultSummary.UUID);
                            
	                        var histogramList = recordHelper.GetHistogramList(adminViewModel.SelectedSampleRecordFromList.SelectedResultSummary.UUID);
                            var graphList = new GraphHelper().CreateGraph(sampleRecord, histogramList, true);
                            if (graphList != null)
                            {
                                adminViewModel.GraphListForReport = graphList.ToList();
                            }

                            if (adminViewModel.GraphListForReport != null && adminViewModel.GraphListForReport.Count > 0)
                            {
                                adminViewModel.EnableAndSetGraphOptionsData();
                            }
                        }
                    }

                    adminViewModel.PrintRunResult();

                    var sampleName = adminViewModel.SelectedSampleRecordFromList.SampleIdentifier;
                    var dateTime = ScoutUtilities.Misc.ConvertToFileNameFormat(DateTime.Now);
                    var fileName = $"{sampleName}_{dateTime}";

                    var fullFilePath = ExportModel.GetExportFullFilePath(exportPath, fileName, FileTypeHelper.GetString(FileType.Pdf));

                    Log.Debug($"AutoExportPdf: <exit>: fullFilePath: {fullFilePath}, exportPath: {exportPath}");

					adminViewModel.RunResultsReportViewModel.AutoExportExecute(fullFilePath);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QUEUE_EXPORT_SAMPLES"));
            }
        }

        public static void SortCarouselWellPlateCollectionsWhileRunning(bool isUsingCarousel, bool isPlateOrderByRow, List<SampleDomain> samples)
        {
            if (isUsingCarousel)
            {
                DispatcherHelper.ApplicationExecute(() =>
                {
                    samples = samples.OrderBy(x => x.Tag).Select(x => x).OrderBy(x => x.IsEnabled ? 0 : 1).ToList();
                });
            }
            else
            {
                if (isPlateOrderByRow)
                {
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        samples = samples.OrderBy(x => x.SamplePosition.Row).Select(x => x).OrderBy(x => x.IsEnabled ? 0 : 1).ToList();
                    });
                }
                else
                {
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        samples = samples.OrderBy(x => x.SamplePosition.Column).Select(x => x).OrderBy(x => x.IsEnabled ? 0 : 1).ToList();
                    });
                }
            }
        }

        public static void ExportCompletedRunResult(SampleViewModel svm)
        {
            try
            {
                if(svm.SampleRecord.SelectedResultSummary != null)
                {
                    svm.SampleRecord.SelectedResultSummary.UserId = string.IsNullOrEmpty(svm.SampleRecord.UserId) ? svm.Username : svm.SampleRecord.UserId;
                    svm.SampleRecord.IsSingleExportStatus = true;
                }

                var resultList = new List<SampleRecordDomain> { svm.SampleRecord };

                string exportPath = svm.AdvancedSampleSettings.AppendExportDirectory;
                if (string.IsNullOrEmpty(exportPath))
                {
                    var runOptionsModel = new RunOptionSettingsModel(XMLDataAccess.Instance, svm.Username);
                    exportPath = runOptionsModel.RunOptionsSettings.ExportSampleResultPath;
                }

                if (string.IsNullOrEmpty(exportPath))
                {
                    exportPath = FileSystem.GetDefaultExportPath(svm.Username);
                }

                var fullPath = ExportModel.GetExportFullFilePath(exportPath, svm.AdvancedSampleSettings.AppendExportFileName, FileTypeHelper.GetString(FileType.Csv));

                var summaryOutdir = System.IO.Path.GetDirectoryName(fullPath);
                string ext = Path.GetExtension(fullPath);
                var csvFilename = Path.GetFileName(fullPath);
                if(!string.IsNullOrEmpty(ext) && ext.ToLower().Equals(FileTypeHelper.GetString(FileType.Csv)))
                    csvFilename = Path.GetFileNameWithoutExtension(fullPath);
                if (FileSystem.IsFolderValidForExport(fullPath))
                {
                    ExportManager.EvExportCsvReq.Publish(svm.Username, "", resultList, summaryOutdir,
                        svm.AdvancedSampleSettings.ExportSampleDirectory, csvFilename, "",
                        svm.AdvancedSampleSettings.AppendSampleExport,true,
                        svm.AdvancedSampleSettings.ExportSamples, false);
                }
                else
                {
                    Log.Error("IsFolderValidForExport failed path = " + fullPath);
                }
                ExportModel.ExportSuccessMessage();

            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QUEUE_EXPORT_SAMPLES"));
            }
        }
    }
}