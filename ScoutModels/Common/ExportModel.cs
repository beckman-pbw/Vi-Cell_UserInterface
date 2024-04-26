using ApiProxies.Generic;
using HawkeyeCoreAPI.Facade;
using log4net;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutDomains.Reagent;
using ScoutDomains.RunResult;
using ScoutLanguageResources;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Structs;
using ScoutUtilities.UIConfiguration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Documents;

namespace ScoutModels.Common
{
	public class ExportModel
	{
		protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


		public static string GetExportFullFilePath(string exportPath, string fileName, string ext)
		{
			string tstExt = Path.GetExtension(fileName);
			if (!string.IsNullOrEmpty(ext))
			{
				// add the file extension, if needed
				if (string.IsNullOrEmpty(tstExt) || !tstExt.ToLower().Equals(ext.ToLower()))
					fileName += ext;
			}
			return Path.Combine(exportPath, fileName);
		}

		private static void PublishEventAggregator(string statusBarMessage, MessageType messageType = MessageType.Normal)
		{
			MessageBus.Default.Publish(new SystemMessageDomain
			{
				IsMessageActive = true,
				Message = statusBarMessage,
				MessageType = messageType
			});
		}

		public static void ExportSuccessMessage()
		{
			PublishEventAggregator(LanguageResourceHelper.Get("LID_MSGBOX_ExportSucces"));
		}

		public static void ExportFailedMessage()
		{
			Log.Warn(LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONEXPORT"));
			PublishEventAggregator(LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONEXPORT"), MessageType.Warning);
		}

		public static void NoDataMessage()
		{
			DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_MSGBOX_NoDataExport"));
		}

		public static bool ExportToFile<T>(IList<T> list, FileType selectedFileType, string filePath, string fileName)
		{
			if (list == null || !list.Any())
			{
				NoDataMessage();
				return false;
			}

			var success = false;
			var exportPath = Path.Combine(filePath, fileName);
			switch (selectedFileType)
			{
				case FileType.Csv:
					exportPath = exportPath + ".csv";
					success = FileSystem.ConvertGenericListToCsvXlsFile(list.ToList(), exportPath);
					break;
				case FileType.Pdf:
					exportPath = exportPath + ".pdf";
					success = false;
					break;
			}

			if (success) ExportSuccessMessage();
			else ExportFailedMessage();

			return success;
		}

		public static bool ExportConcentrationSlopeDetails(DataSet ds, FileType selectedFileType, string filePath, string fileName)
		{
			if (ds?.Tables == null || ds.Tables.Count <= 0)
			{
				NoDataMessage();
				return false;
			}

			var success = false;
			var exportPath = Path.Combine(filePath, fileName);
			switch (selectedFileType)
			{
				case FileType.Csv:
					if (!exportPath.EndsWith(".csv")) exportPath += ".csv";
					success = FileSystem.ExportConcentrationSlopeToCsv(ds, exportPath);
					break;
				case FileType.Pdf:
					if (!exportPath.EndsWith(".pdf")) exportPath += ".pdf";
					success = false;
					break;
			}

			if (success) ExportSuccessMessage();
			else ExportFailedMessage();

			return success;
		}

		public static void ExportConcentrationSlopeDetails(IList<ICalibrationConcentrationListDomain> calibrationList,
			List<KeyValuePair<string, double>> slope, IList<DataGridExpanderColumnHeader> headerList,
			FileType fileType, string filePath, string fileName)
		{
			try
			{
				var concentrationListDomains = calibrationList.ToList();
				var dataGridExpanderColumnHeaders = headerList.ToList();

				var dataSet = new DataSet();
				dataSet.Tables.Add(CreateCalibrationDt(concentrationListDomains, dataGridExpanderColumnHeaders));
				dataSet.Tables.Add(CreateSlopeDt(slope));
				dataSet.Tables.Add(CreateHeaderDt(dataGridExpanderColumnHeaders));

				ExportConcentrationSlopeDetails(dataSet, fileType, filePath, fileName);
			}
			catch (Exception ex)
			{
				Log.Error($"Failed to export concentration slope details", ex);
			}
		}

		public static DataTable CreateCalibrationDt(List<ICalibrationConcentrationListDomain> calibrationList, List<DataGridExpanderColumnHeader> headerList)
		{
			using (var table = new DataTable())
			{
				table.TableName = "First";
				table.Columns.Add(LanguageResourceHelper.Get("LID_TabItem_Concentration"), typeof(string));
				table.Columns.Add(LanguageResourceHelper.Get("LID_Label_NoOfTubes"), typeof(string));
				table.Columns.Add(LanguageResourceHelper.Get("LID_GraphLabel_AssayConcentration"), typeof(string));
				table.Columns.Add(LanguageResourceHelper.Get("LID_QCHeader_LotNumber"), typeof(string));
				table.Columns.Add(LanguageResourceHelper.Get("LID_GridLabel_ExpirationDate"), typeof(string));
				table.Columns.Add(LanguageResourceHelper.Get("LID_Label_AvgTotalCells"), typeof(string));
				table.Columns.Add(LanguageResourceHelper.Get("LID_Label_AvgOriginalConc"), typeof(string));
				table.Columns.Add(LanguageResourceHelper.Get("LID_Label_AvgAdjustedConc"), typeof(string));
				calibrationList.ForEach(a =>
				{
					var items = headerList.Where(s => s.KnownConcentration.Equals(a.KnownConcentration)).Select(m => m).FirstOrDefault();
					if (items != null)
						table.Rows.Add(a.KnownConcentration, a.NumberOfTubes, GetValues(items.AssayValue), a.Lot,
							Misc.ConvertToString(a.ExpiryDate), items.AvgTotCount, GetValues(items.AvgOriginal), GetValues(items.AvgAdjusted));
				});
				return table;
			}
		}

		private static string GetValues(double sourceValue)
		{
			return Misc.ConvertToPower(sourceValue) + "x10^6";
		}

		public static DataTable CreateSlopeDt(List<KeyValuePair<string, double>> slope)
		{
			using (var table = new DataTable())
			{
				table.TableName = "Second";
				table.Columns.Add("A", typeof(string));
				table.Columns.Add("B", typeof(string));
				slope.ForEach(a => { table.Rows.Add(a.Key, a.Value); });
				return table;
			}
		}

		public static DataTable CreateHeaderDt(List<DataGridExpanderColumnHeader> headerList)
		{
			using (var table = new DataTable())
			{
				table.TableName = "Third";
				table.Columns.Add(LanguageResourceHelper.Get("LID_QMgmtHEADER_Position"), typeof(string));
				table.Columns.Add(LanguageResourceHelper.Get("LID_GraphLabel_AssayConcentration"), typeof(string));
				table.Columns.Add(LanguageResourceHelper.Get("LID_GraphLabel_Totalcells"), typeof(string));
				table.Columns.Add(LanguageResourceHelper.Get("LID_GraphLabel_OriginalConcentrationMl"), typeof(string));
				table.Columns.Add(LanguageResourceHelper.Get("LID_Graph_AdjustConcMl"), typeof(string));
				table.Columns.Add(LanguageResourceHelper.Get("LID_Label_Validate"), typeof(bool));
				headerList.ForEach(a =>
				{
					table.Rows.Add(a.ImageId, GetValues(a.AssayValue), a.TotCount, GetValues(a.Original),
						GetValues(a.Adjusted), a.Validate);
				});
				return table;
			}
		}

		#region Export summary and selected quality control csv report

		public static void ExportQualityControlToCsv(QualityControlDomain qualityControl, List<SampleResultRecordExportDomain> exportResultRecords,
			List<SampleRecordDomain> sampleRecords, string exportPath)
		{
			//Quality control details
			FileSystem.ExportDataTableWithKeysToFile(CreateQualityControlKeysDataTable(qualityControl), exportPath);

			//CellType details 
			FileSystem.ExportDataTableWithKeysToFile(CreateCellTypeKeysDataTable(exportResultRecords.FirstOrDefault(),
				sampleRecords.FirstOrDefault()), exportPath, true);

			//Sample details
			FileSystem.ExportDataTableWithKeysToFile(CreateSampleKeysDataTable(exportResultRecords, sampleRecords), exportPath, true);
		}

		private static DataTable CreateQualityControlKeysDataTable(QualityControlDomain qualityControl)
		{
			using (var table = new DataTable())
			{
				CreateSelectedQualityControlColumnsToExport(table);
				table.Rows.Add(qualityControl.QcName,
					qualityControl.CellTypeName,
					LanguageResourceHelper.Get(GetEnumDescription.GetDescription(qualityControl.AssayParameter)),
					qualityControl.LotInformation,
					qualityControl.AssayValueString,
					qualityControl.AcceptanceLimitString,
					Misc.ConvertToCustomLongDateTimeFormat(qualityControl.ExpirationDate),
					qualityControl.CommentText);
				return table;
			}
		}

		private static void CreateSelectedQualityControlColumnsToExport(DataTable table)
		{
			table.Columns.Add("LID_MSGBOX_EnterQualityControlName", typeof(string));
			table.Columns.Add("LID_UsersLabel_CellType", typeof(string));
			table.Columns.Add("LID_QCHeader_AssayParameter", typeof(string));
			table.Columns.Add("LID_QCHeader_LotNumber", typeof(string));
			table.Columns.Add("LID_GraphLabel_AssayConcentration", typeof(string));
			table.Columns.Add("LID_QCHeader_AcceptanceLimits", typeof(string));
			table.Columns.Add("LID_GridLabel_ExpirationDate", typeof(string));
			table.Columns.Add("LID_Label_Comments", typeof(string));
		}

		private static DataTable CreateCellTypeKeysDataTable(SampleResultRecordExportDomain sampleDetails, SampleRecordDomain sampleRecordDomain)
		{
			using (var table = new DataTable())
			{
				CreateCellTypeTableColumns(table);
				eCellDeclusterSetting isParsed;
				table.Rows.Add(sampleRecordDomain.SelectedResultSummary?.CellTypeDomain?.CellTypeName,
					sampleDetails.MinimumDiameter,
					sampleDetails.MaximumDiameter,
					sampleDetails.Images,
					sampleDetails.CellSharpness,
					sampleDetails.MinimumCircularity,
					Enum.TryParse(sampleDetails.DeclusterDegree, out isParsed)
						? GetEnumDescription.GetDescription((eCellDeclusterSetting)Enum.Parse(typeof(eCellDeclusterSetting), sampleDetails.DeclusterDegree))
						: sampleDetails.DeclusterDegree,
					sampleRecordDomain.SelectedResultSummary?.CellTypeDomain?.AspirationCycles,
					sampleDetails.ViableSpotBrightness,
					sampleDetails.ViableSpotArea,
					sampleRecordDomain.SelectedResultSummary?.AnalysisDomain?.MixingCycle,
					sampleRecordDomain.SelectedResultSummary?.CellTypeDomain?.CalculationAdjustmentFactor);

				return table;
			}
		}

		private static void CreateCellTypeTableColumns(DataTable table)
		{
			table.Columns.Add("LID_UsersLabel_CellType", typeof(string));
			table.Columns.Add("LID_Result_MinimumDiameter", typeof(string));
			table.Columns.Add("LID_Result_MaximumDiameter", typeof(string));
			table.Columns.Add("LID_Label_Images", typeof(string));
			table.Columns.Add("LID_Label_CellSharpness", typeof(string));
			table.Columns.Add("LID_Label_MinimumCircularity", typeof(string));
			table.Columns.Add("LID_Label_DeclusterDegree", typeof(string));
			table.Columns.Add("LID_Label_AspirationCycle", typeof(string));
			table.Columns.Add("LID_Result_SpotBrightness", typeof(string));
			table.Columns.Add("LID_Result_SpotArea", typeof(string));
			table.Columns.Add("LID_Label_MixingCycle", typeof(string));
			table.Columns.Add("LID_Label_AdjustmentFactorPercentage", typeof(string));
		}

		private static DataTable CreateSampleKeysDataTable(List<SampleResultRecordExportDomain> samples, List<SampleRecordDomain> sampleRecordDomainList)
		{
			using (var table = new DataTable())
			{
				CreateSampleTableColumns(table);
				for (var i = 0; i < sampleRecordDomainList.Count; i++)
				{
					var item = sampleRecordDomainList[i].SelectedResultSummary?.CumulativeResult;
					if (item != null)
					{
						var rowStr = "";
						if (sampleRecordDomainList[i].Position.IsCarousel())
						{
							rowStr = ApplicationConstants.CarouselLabel;
						}
						else if (sampleRecordDomainList[i].Position.Is96WellPlate())
						{
							rowStr = sampleRecordDomainList[i].Position.Row.ToString();
						}
						else if (sampleRecordDomainList[i].Position.IsAutomationCup())
						{
							rowStr = ApplicationConstants.ACupLabel;
						}
						table.Rows.Add(samples[i].SampleId,
							samples[i].Dilution, 
							samples[i].Wash,
							samples[i].Tag,
							samples[i].AnalysisDateTime,
							samples[i].ReAnalysisDateTime,
							samples[i].AnalysisBy,
							samples[i].ReAnalysisBy,
							samples[i].TotalImages,
							item.TotalCells,
							item.ViableCells,
							Misc.ConvertToConcPower(item.ConcentrationML),
							Misc.ConvertToConcPower(item.ViableConcentration),
							item.Viability,
							item.Size,
							item.ViableSize,
							item.Circularity,
							item.ViableCircularity,
							item.AverageCellsPerImage,
							item.AvgBackground,
							item.Bubble,
							item.ClusterCount,
							rowStr,
							sampleRecordDomainList[i].Position.Column.ToString());
					}
				}
				return table;
			}
		}

		private static void CreateSampleTableColumns(DataTable table)
		{
			table.Columns.Add("LID_QMgmtHEADER_SampleId", typeof(string));
			table.Columns.Add("LID_Label_Dilution", typeof(string));
			table.Columns.Add("LID_QMgmtHEADER_Wash", typeof(string));
			table.Columns.Add("LID_Label_Tag", typeof(string));
			table.Columns.Add("LID_QCHeader_AnalysisDateTime", typeof(string));
			table.Columns.Add("LID_QCHeader_ReAnalysisDateTime", typeof(string));
			table.Columns.Add("LID_Report_AnalysisBy", typeof(string));
			table.Columns.Add("LID_Report_ReanalysisBy", typeof(string));
			table.Columns.Add("LID_ReportLabel_TotalImages_Analysis", typeof(string));
			table.Columns.Add("LID_Result_Totalcells", typeof(string));
			table.Columns.Add("LID_Result_ViaCount", typeof(string));
			table.Columns.Add("LID_Result_Concentration_Export", typeof(string));
			table.Columns.Add("LID_Result_ViaConc_Export", typeof(string));
			table.Columns.Add("LID_Result_Viability", typeof(string));
			table.Columns.Add("LID_Label_Size", typeof(string));
			table.Columns.Add("LID_GraphLabel_Viablesize", typeof(string));
			table.Columns.Add("LID_Label_AverageCircularity", typeof(string));
			table.Columns.Add("LID_GraphLabel_Viablecircularity", typeof(string));
			table.Columns.Add("LID_Result_AverageCellPerImage", typeof(string));
			table.Columns.Add("LID_GraphLabel_Averagebackground", typeof(string));
			table.Columns.Add("LID_Result_Bubbles", typeof(string));
			table.Columns.Add("LID_Result_CellClusters", typeof(string));
			table.Columns.Add("LID_CSV_Row_Header", typeof(string));
			table.Columns.Add("LID_CSV_Column_Header", typeof(string));
		}

		#endregion

		public static void PromptAndExportReportLogs<T>(IList<T> list, int reportId, string fileName)
		{
			try
			{
				if (list == null || !OpenCsvSaveFileDialog(fileName, out var fullFilePath))
				{
					return;
				}

				ExportReportLogsToCsv(list, reportId, fullFilePath);
				ExportSuccessMessage();
			}
			catch (Exception ex)
			{
				ExportFailedMessage();
				ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONEXPORT"));
			}
		}

		public static void PromptAndExportQualityControlToCsv(QualityControlDomain selectedQualityControl, List<SampleRecordDomain> sampleRecordDomains,
			List<SampleResultRecordExportDomain> resultRecords, string fileName)
		{
			if (selectedQualityControl == null || sampleRecordDomains == null || resultRecords == null ||
				!OpenCsvSaveFileDialog(fileName, out var fullFilePath))
			{
				return;
			}

			ExportQualityControlToCsv(selectedQualityControl, resultRecords.ToList(), sampleRecordDomains, fullFilePath);
			ExportSuccessMessage();
		}

		public static void PromptAndExportSampleToCsv(SampleResultRecordExportDomain samplesToExport, SampleRecordDomain sampleRecordDomainList, string fileName)
		{
			try
			{
				if (!OpenCsvSaveFileDialog(fileName, out var fullFilePath)) return;
				ExportSelectedSampleDetailsToCsv(samplesToExport, sampleRecordDomainList, fullFilePath);
				ExportSuccessMessage();
			}
			catch (Exception e)
			{
				Log.Error($"Failed to prompt and export samples to CSV", e);
				ExportFailedMessage();
				throw;
			}
		}

		public static string PromptAndExportSamplesToCsv(ref List<SampleRecordDomain> sampleRecordDomainList, string fileName)
		{
			try
			{
				// Remove NULL entries
				for (int j = 0; j < sampleRecordDomainList.Count(); j++)
				{
					if (sampleRecordDomainList[j] == null)
					{
						sampleRecordDomainList.RemoveAt(j);
						j--;
					}
				}
				if (sampleRecordDomainList.Count() > 0)
				{
					if (!OpenCsvSaveFileDialog(fileName, out var fullFilePath)) return "";
					return fullFilePath;
				}
			}
			catch (Exception e)
			{
				Log.Error($"Failed to prompt and export samples to CSV", e);
				ExportFailedMessage();
				throw;
			}
			return "";
		}

		#region Export summary and selected sample report

		private static void UpdateCSVFileHeader(string filePath, DataTable dataTable)
		{
			var headerValues = dataTable.Columns.OfType<DataColumn>().Select(column => FileSystem.QuoteValue(LanguageResourceHelper.Get(column.ColumnName)));

			try
			{
				List<string> buf = new List<string>();
				StreamReader sr = new StreamReader(filePath);

				// Read the 1st line from the file, don't need to save the existing header.
				string line = sr.ReadLine();

				// Is the header up to date?
				if (!line.Contains("COLUMN"))
				{
					// Read the rest of the file.
					while ((line = sr.ReadLine()) != null)
					{
						line += "\n";
						buf.Add(line);
					}

					sr.Close();

					// Overwrite the existing file.
					var sw = new StreamWriter(filePath, false, FileSystem.DefaultEncoding);

					// Write the updated header.
					sw.WriteLine(string.Join(",", headerValues));

					foreach (string str in buf)
					{
						sw.Write(str);
					}

					sw.Close();
				}
			}
			catch (Exception e)
			{
				Log.Error($"Failed to update CSV file header {nameof(ExportModel)}", e);
			}
		}

		public static void ExportSamplesToCsv(List<SampleResultRecordExportDomain> samplesToExport,
			List<SampleRecordDomain> sampleRecordDomainList, string exportPath, bool isAppend = false)
		{
			// CSVs can't have multiple headers.
			bool fileExists = File.Exists(exportPath);
			bool includeHeader = !fileExists || !isAppend;

			var dataTable = CreateSampleReportKeysDataTable(samplesToExport, sampleRecordDomainList);
			FileSystem.ExportDataTableWithKeysToFile(dataTable, exportPath, isAppend, includeHeader);

			UpdateCSVFileHeader(exportPath, dataTable);
		}

		private static DataTable CreateSampleReportKeysDataTable(List<SampleResultRecordExportDomain> samples, List<SampleRecordDomain> sampleRecordDomainList)
		{
			using (var table = new DataTable())
			{
				CreateSampleColumnsToExport(table);
				for (var i = 0; i < sampleRecordDomainList.Count; i++)
				{
					if (sampleRecordDomainList[i]?.SelectedResultSummary?.CumulativeResult == null)
						continue;

					var item = sampleRecordDomainList[i].SelectedResultSummary?.CumulativeResult;
					if (item != null)
					{
						var rowStr = "";
						if (sampleRecordDomainList[i].Position.IsCarousel())
						{
							rowStr = ApplicationConstants.CarouselLabel;
						}
						else if (sampleRecordDomainList[i].Position.Is96WellPlate())
						{
							rowStr = sampleRecordDomainList[i].Position.Row.ToString();
						}
						else if (sampleRecordDomainList[i].Position.IsAutomationCup())
						{
							rowStr = ApplicationConstants.ACupLabel;
						}

						eCellDeclusterSetting isParsed;
						QCStatus isQCParsed;
						table.Rows.Add(samples[i].SampleId,
							item.TotalCells,
							item.ViableCells,
							Misc.ConvertToConcPower(item.ConcentrationML, LanguageResourceHelper.CurrentFormatCulture),
							Misc.ConvertToConcPower(item.ViableConcentration, LanguageResourceHelper.CurrentFormatCulture),
							Misc.UpdateTrailingPoint(item.Viability, TrailingPoint.One),
							Misc.ConvertToString(item.Size),        // Average diameter
							Misc.ConvertToString(item.ViableSize),  // Average viable diameter
							Misc.ConvertToString(item.Circularity), // Average circularity
							Misc.ConvertToString(item.ViableCircularity),   // Average viable circularity
							item.AverageCellsPerImage,
							item.AvgBackground,
							item.Bubble,
							item.ClusterCount,
							samples[i].TotalImages,
							samples[i].CellType, samples[i].MinimumDiameter, samples[i].MaximumDiameter, samples[i].Images,
							samples[i].CellSharpness, samples[i].MinimumCircularity,
							Enum.TryParse(samples[i].DeclusterDegree, out isParsed)
								? GetEnumDescription.GetDescription((eCellDeclusterSetting)Enum.Parse(typeof(eCellDeclusterSetting), samples[i].DeclusterDegree))
								: samples[i].DeclusterDegree,
							sampleRecordDomainList[i].SelectedResultSummary?.CellTypeDomain?.AspirationCycles,
							samples[i].ViableSpotBrightness,
							samples[i].ViableSpotArea,
							sampleRecordDomainList[i].SelectedResultSummary?.AnalysisDomain?.MixingCycle,
							samples[i].AnalysisDateTime, samples[i].ReAnalysisDateTime, samples[i].AnalysisBy,
							samples[i].ReAnalysisBy, samples[i].Dilution, samples[i].Wash, samples[i].Tag,
							sampleRecordDomainList[i].SelectedResultSummary?.CellTypeDomain?.CalculationAdjustmentFactor,
							rowStr,
							sampleRecordDomainList[i].Position.Column.ToString(),
							Enum.TryParse(samples[i].QCStatus, out isQCParsed)
								? GetEnumDescription.GetDescription((QCStatus)Enum.Parse(typeof(QCStatus), samples[i].QCStatus))
								: samples[i].QCStatus);
					}
				}
				return table;
			}
		}

		private static void CreateSampleColumnsToExport(DataTable table)
		{
			table.Columns.Add("LID_QMgmtHEADER_SampleId", typeof(string));
			table.Columns.Add("LID_Result_Totalcells", typeof(string));
			table.Columns.Add("LID_Result_ViaCount", typeof(string));
			table.Columns.Add("LID_Result_Concentration_Export", typeof(string));
			table.Columns.Add("LID_Result_ViaConc_Export", typeof(string));
			table.Columns.Add("LID_Result_Viability", typeof(string));
			table.Columns.Add("LID_Label_Size", typeof(string));
			table.Columns.Add("LID_GraphLabel_Viablesize", typeof(string));
			table.Columns.Add("LID_Label_AverageCircularity", typeof(string));
			table.Columns.Add("LID_GraphLabel_Viablecircularity", typeof(string));
			table.Columns.Add("LID_Result_AverageCellPerImage", typeof(string));
			table.Columns.Add("LID_GraphLabel_Averagebackground", typeof(string));
			table.Columns.Add("LID_Result_Bubbles", typeof(string));
			table.Columns.Add("LID_Result_CellClusters", typeof(string));
			table.Columns.Add("LID_ReportLabel_TotalImages_Analysis", typeof(string));
			table.Columns.Add("LID_UsersLabel_CellType", typeof(string));
			table.Columns.Add("LID_Result_MinimumDiameter", typeof(string));
			table.Columns.Add("LID_Result_MaximumDiameter", typeof(string));
			table.Columns.Add("LID_Label_Images", typeof(string));
			table.Columns.Add("LID_Label_CellSharpness", typeof(string));
			table.Columns.Add("LID_Label_MinimumCircularity", typeof(string));
			table.Columns.Add("LID_Label_DeclusterDegree", typeof(string));
			table.Columns.Add("LID_Label_AspirationCycle", typeof(string));
			table.Columns.Add("LID_Result_SpotBrightness", typeof(string));
			table.Columns.Add("LID_Result_SpotArea", typeof(string));
			table.Columns.Add("LID_Label_MixingCycle", typeof(string));
			table.Columns.Add("LID_QCHeader_AnalysisDateTime", typeof(string));
			table.Columns.Add("LID_QCHeader_ReAnalysisDateTime", typeof(string));
			table.Columns.Add("LID_Report_AnalysisBy", typeof(string));
			table.Columns.Add("LID_Report_ReanalysisBy", typeof(string));
			table.Columns.Add("LID_Label_Dilution", typeof(string));
			table.Columns.Add("LID_QMgmtHEADER_Wash", typeof(string));
			table.Columns.Add("LID_Label_Tag", typeof(string));
			table.Columns.Add("LID_Label_AdjustmentFactorPercentage", typeof(string));
			table.Columns.Add("LID_CSV_Row_Header", typeof(string));
			table.Columns.Add("LID_CSV_Column_Header", typeof(string));
			table.Columns.Add("LID_GridLabel_QualityControl", typeof(string));
		}

		#region - Selected sample record export

		private static void CreateSelectedSampleColumnsToExport(DataTable table)
		{
			table.Columns.Add("LID_QMgmtHEADER_SampleId", typeof(string));
			table.Columns.Add("LID_ReportLabel_TotalImages_Analysis", typeof(string));
			table.Columns.Add("LID_UsersLabel_CellType", typeof(string));
			table.Columns.Add("LID_Result_MinimumDiameter", typeof(string));
			table.Columns.Add("LID_Result_MaximumDiameter", typeof(string));
			table.Columns.Add("LID_Label_Images", typeof(string));
			table.Columns.Add("LID_Label_CellSharpness", typeof(string));
			table.Columns.Add("LID_Label_MinimumCircularity", typeof(string));
			table.Columns.Add("LID_Label_DeclusterDegree", typeof(string));
			table.Columns.Add("LID_Label_AspirationCycle", typeof(string));
			table.Columns.Add("LID_Result_SpotBrightness", typeof(string));
			table.Columns.Add("LID_Result_SpotArea", typeof(string));
			table.Columns.Add("LID_Label_MixingCycle", typeof(string));
			table.Columns.Add("LID_QCHeader_AnalysisDateTime", typeof(string));
			table.Columns.Add("LID_QCHeader_ReAnalysisDateTime", typeof(string));
			table.Columns.Add("LID_Report_AnalysisBy", typeof(string));
			table.Columns.Add("LID_Report_ReanalysisBy", typeof(string));
			table.Columns.Add("LID_Label_Dilution", typeof(string));
			table.Columns.Add("LID_QMgmtHEADER_Wash", typeof(string));
			table.Columns.Add("LID_Label_Tag", typeof(string));
			table.Columns.Add("LID_Label_AdjustmentFactorPercentage", typeof(string));
			table.Columns.Add("LID_CSV_Row_Header", typeof(string));
			table.Columns.Add("LID_CSV_Column_Header", typeof(string));
			table.Columns.Add("LID_GridLabel_QualityControl", typeof(string));
		}

		private static DataTable CreateSelectedSampleKeysDataTable(SampleResultRecordExportDomain sample, SampleRecordDomain sampleRecord)
		{
			using (var table = new DataTable())
			{
				var rowStr = "";
				if (sampleRecord.Position.IsCarousel())
				{
					rowStr = ApplicationConstants.CarouselLabel;
				}
				else if (sampleRecord.Position.Is96WellPlate())
				{
					rowStr = sampleRecord.Position.Row.ToString();
				}
				else if (sampleRecord.Position.IsAutomationCup())
				{
					rowStr = ApplicationConstants.ACupLabel;
				}

				CreateSelectedSampleColumnsToExport(table);

				QCStatus isQCParsed;
				table.Rows.Add(sample.SampleId,
					sample.TotalImages,
					sample.CellType,
					sample.MinimumDiameter,
					sample.MaximumDiameter,
					sample.Images,
					sample.CellSharpness,
					sample.MinimumCircularity,
					sample.DeclusterDegree,
					sampleRecord.SelectedResultSummary?.CellTypeDomain?.AspirationCycles,
					sample.ViableSpotBrightness,
					sample.ViableSpotArea,
					sampleRecord.SelectedResultSummary?.AnalysisDomain?.MixingCycle,
					sample.AnalysisDateTime,
					sample.ReAnalysisDateTime,
					sample.AnalysisBy,
					sample.ReAnalysisBy,
					sample.Dilution,
					sample.Wash,
					sample.Tag,
					sampleRecord.SelectedResultSummary?.CellTypeDomain?.CalculationAdjustmentFactor,
					rowStr,
					sampleRecord.Position.Column.ToString(),
					Enum.TryParse(sample.QCStatus, out isQCParsed)
								? GetEnumDescription.GetDescription((QCStatus)Enum.Parse(typeof(QCStatus), sample.QCStatus))
								: sample.QCStatus);
				return table;
			}
		}

		public static void ExportSelectedSampleDetailsToCsv(SampleResultRecordExportDomain exportSample, SampleRecordDomain sample, string exportPath)
		{
			//#Sample Details
			FileSystem.ExportDataTableWithKeysToFile(CreateSelectedSampleKeysDataTable(exportSample, sample), exportPath);

			//#Reagent Details

			if (null == sample?.SelectedResultSummary)
			{
				return;
			}


			if (sample.RegentInfoRecordList != null)
			{
				var reagentList = ApplicationConstants.ServiceUser.Equals(sample.SelectedResultSummary.UserId)
					? sample.RegentInfoRecordList
					: sample.RegentInfoRecordList.Where(reagent => reagent.ReagentName.Equals(ApplicationConstants.ReagentNameForAllUser)).ToList();
				if (reagentList != null && reagentList.Any())
					FileSystem.ExportDataTableWithKeysToFile(CreateReagentKeysDataTable(reagentList), exportPath, true);
			}

			//#Result per image
			var imageResultList = sample.GetResultRecord(sample.SelectedResultSummary.UUID)?.ResultPerImage;
			if (imageResultList != null && imageResultList.Any())
				FileSystem.ExportDataTableWithKeysToFile(CreateImageRecordKeysDataTable(imageResultList, sample), exportPath, true);

			//#Signature
			var signatureList = sample.SelectedResultSummary?.SignatureList;
			if (signatureList != null && signatureList.Any())
				FileSystem.ExportDataTableWithKeysToFile(CreateSignatureKeysDataTable(signatureList), exportPath, true);
		}

		private static void CreateReagentTableColumns(DataTable table)
		{
			table.Columns.Add("LID_Label_PN", typeof(string));
			table.Columns.Add("LID_QCHeader_LotNumber", typeof(string));
			table.Columns.Add("LID_ButtonContent_Reagent", typeof(string));
			table.Columns.Add("LID_GridLabel_ExpirationDate", typeof(string));
			table.Columns.Add("LID_Label_InService_Date", typeof(string));
			table.Columns.Add("LID_Label_EffectiveExpiration", typeof(string));
		}

		private static DataTable CreateReagentKeysDataTable(List<ReagentInfoRecordDomain> reagents)
		{
			using (var dataTable = new DataTable())
			{
				CreateReagentTableColumns(dataTable);
				reagents.ForEach(x =>
				{
					dataTable.Rows.Add(x.PackNumber, x.LotNumber, x.ReagentName,
						Misc.ConvertToCustomLongDateTimeFormat(x.ExpirationDate),
						Misc.ConvertToCustomLongDateTimeFormat(x.InServiceDate),
						Misc.ConvertToCustomLongDateTimeFormat(x.EffectiveExpirationDate));
				});
				return dataTable;
			}
		}

		private static void CreateImageRecordTableColumns(DataTable table)
		{
			table.Columns.Add("LID_ResultHeader_ImageCount", typeof(string));
			table.Columns.Add("LID_Result_Totalcells", typeof(string));
			table.Columns.Add("LID_Result_ViaCount", typeof(string));
			table.Columns.Add("LID_Result_Concentration_Export", typeof(string));
			table.Columns.Add("LID_Result_ViaConc_Export", typeof(string));
			table.Columns.Add("LID_Result_Viability", typeof(string));
			table.Columns.Add("LID_Label_Size", typeof(string));
			table.Columns.Add("LID_GraphLabel_Viablesize", typeof(string));
			table.Columns.Add("LID_Label_AverageCircularity", typeof(string));
			table.Columns.Add("LID_GraphLabel_Viablecircularity", typeof(string));
			table.Columns.Add("LID_Result_AverageCellPerImage", typeof(string));
			table.Columns.Add("LID_GraphLabel_Averagebackground", typeof(string));
			table.Columns.Add("LID_Result_Bubbles", typeof(string));
			table.Columns.Add("LID_Result_CellClusters", typeof(string));
		}

		private static string GetDiscardedImageDetails(E_ERRORCODE errorCode)
		{
			switch (errorCode)
			{
				case E_ERRORCODE.eBubbleImage:
					return errorCode + " - " + LanguageResourceHelper.Get("LID_eBubbleImage");
				case E_ERRORCODE.eInvalidBackgroundIntensity:
					return errorCode + " - " + LanguageResourceHelper.Get("LID_eInvalidBackgroundIntensity");
				default:
					return errorCode + " - " + LanguageResourceHelper.Get("LID_BubbleLabel_DiscardedImages");
			}
		}

		private static DataTable CreateImageRecordKeysDataTable(List<BasicResultDomain> resultList, SampleRecordDomain sampleRecord)
		{
			using (var dataTable = new DataTable())
			{
				CreateImageRecordTableColumns(dataTable);
				for (var i = 0; i < resultList.Count; i++)
				{
					var item = resultList[i];
					if (item.ProcessedStatus != E_ERRORCODE.eSuccess)
					{
						dataTable.Rows.Add(i + 1, GetDiscardedImageDetails(item.ProcessedStatus));
					}
					else
					{
						dataTable.Rows.Add(i + 1,
							item.TotalCells,
							item.ViableCells,
							Misc.ConvertToConcPower(item.ConcentrationML, LanguageResourceHelper.CurrentFormatCulture),
							Misc.ConvertToConcPower(item.ViableConcentration, LanguageResourceHelper.CurrentFormatCulture),
							Misc.UpdateTrailingPoint(item.Viability, TrailingPoint.One), Misc.ConvertToString(item.Size),
							Misc.ConvertToString(item.ViableSize),
							Misc.ConvertToString(item.Circularity),
							Misc.ConvertToString(item.ViableCircularity),
							item.AverageCellsPerImage,
							item.AvgBackground,
							item.Bubble,
							item.ClusterCount);
					}
				}
				dataTable.Rows.Add();
				dataTable.Rows.Add(LanguageResourceHelper.Get("LID_ResultHeader_Total"),
					sampleRecord.SelectedResultSummary.CumulativeResult.TotalCells,
					sampleRecord.SelectedResultSummary.CumulativeResult.ViableCells,
					Misc.ConvertToConcPower(sampleRecord.SelectedResultSummary.CumulativeResult.ConcentrationML, LanguageResourceHelper.CurrentFormatCulture),
					Misc.ConvertToConcPower(sampleRecord.SelectedResultSummary.CumulativeResult.ViableConcentration, LanguageResourceHelper.CurrentFormatCulture),
					Misc.UpdateTrailingPoint(sampleRecord.SelectedResultSummary.CumulativeResult.Viability, TrailingPoint.One),
					Misc.ConvertToString(sampleRecord.SelectedResultSummary.CumulativeResult.Size),
					Misc.ConvertToString(sampleRecord.SelectedResultSummary.CumulativeResult.ViableSize),
					Misc.ConvertToString(sampleRecord.SelectedResultSummary.CumulativeResult.Circularity),
					Misc.ConvertToString(sampleRecord.SelectedResultSummary.CumulativeResult.ViableCircularity),
					sampleRecord.SelectedResultSummary.CumulativeResult.AverageCellsPerImage,
					sampleRecord.SelectedResultSummary.CumulativeResult.AvgBackground,
					sampleRecord.SelectedResultSummary.CumulativeResult.Bubble,
					sampleRecord.SelectedResultSummary.CumulativeResult.ClusterCount);

				return dataTable;
			}
		}

		private static DataTable CreateSignatureKeysDataTable(IList<SignatureInstanceDomain> signatureList)
		{
			using (var dataTable = new DataTable())
			{
				dataTable.Columns.Add("LID_Icon_Signature", typeof(string));
				foreach (var x in signatureList)
				{
					var text = $"{x.Signature?.SignatureIndicator} | {x.SigningUser} | {Misc.ConvertToCustomLongDateTimeFormat(x.SignedDate)}";
					dataTable.Rows.Add(text);
				}

				return dataTable;
			}
		}
		#endregion

		#region - Report logs export

		public static void ExportReportLogsToCsv<T>(IList<T> list, int id, string exportPath)
		{
			DataTable dataTable = null;
			switch (id)
			{
				case 1:
					dataTable = CreateAuditTrailLogKeysDataTable(list as List<AuditLogDomain>);
					break;
				case 2:
					dataTable = CreateSampleActiveLogKeysDataTable(list as List<SampleActivityDomain>);
					break;
				case 3:
					dataTable = CreateIErrorLogKeysDataTable(list as List<ErrorLogDomain>);
					break;
				case 4:
					dataTable = CreateCalibrationErrorLogKeysDataTable(list as List<CalibrationActivityLogDomain>);
					break;
				default:
					break;
			}

			FileSystem.ExportDataTableWithKeysToFile(dataTable, exportPath);
		}

		public static DataTable CreateAuditTrailLogKeysDataTable(List<AuditLogDomain> auditLogList)
		{
			using (var dataTable = new DataTable())
			{
				dataTable.Columns.Add("LID_AdminReportsHeader_DateTime", typeof(string));
				dataTable.Columns.Add("LID_UsersLabel_UserId", typeof(string));
				dataTable.Columns.Add("LID_Label_ReportEventType", typeof(string));
				dataTable.Columns.Add("LID_Label_Description", typeof(string));

				auditLogList.ForEach(x =>
				{
					dataTable.Rows.Add(Misc.ConvertToCustomLongDateTimeFormat(x.Timestamp), x.UserId, x.AuditEventType, x.Message);
				});
				return dataTable;
			}
		}

		private static DataTable CreateSampleActiveLogKeysDataTable(List<SampleActivityDomain> sampleActivityLogs)
		{
			using (var dataTable = new DataTable())
			{
				dataTable.Columns.Add("LID_UsersLabel_UserId", typeof(string));
				dataTable.Columns.Add("LID_AdminReportsHeader_DateTime", typeof(string));
				dataTable.Columns.Add("LID_QMgmtHEADER_SampleId", typeof(string));
				dataTable.Columns.Add("LID_UsersLabel_CellType", typeof(string));
				dataTable.Columns.Add("LID_Label_AppType", typeof(string));
				dataTable.Columns.Add("LID_Label_Status", typeof(string));

				sampleActivityLogs.ForEach(x =>
				{
					var description = GetEnumDescription.GetDescription(x.SampleStatus);
					dataTable.Rows.Add(x.UserId, x.Timestamp, x.SampleLabel, x.CellTypeName, x.AnalysisName,
						!string.IsNullOrEmpty(description)
							? LanguageResourceHelper.Get(description)
							: string.Empty);
				});
				return dataTable;
			}
		}

		public static DataTable CreateIErrorLogKeysDataTable(List<ErrorLogDomain> instrumentLogs)
		{
			using (var dataTable = new DataTable())
			{
				dataTable.Columns.Add("LID_AdminReportsHeader_DateTime", typeof(string));
				dataTable.Columns.Add("LID_UsersLabel_UserId", typeof(string));
				dataTable.Columns.Add("LID_Label_ErrorCode", typeof(string));
				dataTable.Columns.Add("LID_Label_Description", typeof(string));

				instrumentLogs.ForEach(x =>
				{
					dataTable.Rows.Add(Misc.ConvertToCustomLongDateTimeFormat(x.Timestamp), x.UserId, x.ErrorCode, x.Message);
				});
				return dataTable;
			}
		}

		public static DataTable CreateCombinedLogKeysDataTable(List<AuditLogDomain> auditLogs, List<ErrorLogDomain> errorLogs)
		{
			var dataTable = new DataTable();
			dataTable.Columns.Add("LID_AdminReportsHeader_DateTime", typeof(string));
			dataTable.Columns.Add("LID_UsersLabel_UserId", typeof(string));
			dataTable.Columns.Add("LID_Label_ReportEventType", typeof(string));
			dataTable.Columns.Add("LID_Label_ErrorCode", typeof(string));
			dataTable.Columns.Add("LID_Label_ChoosePath", typeof(string)); // Location 
			dataTable.Columns.Add("LID_Label_Description", typeof(string));

			string locationStr = LanguageResourceHelper.Get("LID_API_SystemErrorCode_Instance_Audit");

			auditLogs.ForEach(x =>
			{
				dataTable.Rows.Add(Misc.ConvertToCustomLongDateTimeFormat(x.Timestamp), x.UserId, x.AuditEventType, "-", locationStr, x.Message);
			});


			locationStr = LanguageResourceHelper.Get("LID_API_SystemErrorCode_Instance_Error");
			errorLogs.ForEach(x =>
			{
				dataTable.Rows.Add(Misc.ConvertToCustomLongDateTimeFormat(x.Timestamp), x.UserId, "-", x.ErrorCode, locationStr, x.Message);
			});

			return dataTable;
		}

		public static DataTable CreateLogDataKeysDataTable(List<AuditLogDomain> auditLogs, List<ErrorLogDomain> errorLogs, List<SampleActivityDomain> sampleLogs)
		{
			var dataTable = new DataTable();
			dataTable.Columns.Add("SortDate", typeof(DateTime));
			dataTable.Columns.Add("DateTime", typeof(string));
			dataTable.Columns.Add("DataType", typeof(string));
			dataTable.Columns.Add("UserId", typeof(string));
			dataTable.Columns.Add("EventType", typeof(string));
			dataTable.Columns.Add("ErrorCode", typeof(string));
			dataTable.Columns.Add("SampleId", typeof(string));
			dataTable.Columns.Add("AnalysisType", typeof(string));
			dataTable.Columns.Add("CellType", typeof(string));
			dataTable.Columns.Add("Status", typeof(string));
			dataTable.Columns.Add("Description", typeof(string));

			auditLogs.ForEach(x =>
			{
				dataTable.Rows.Add(
					x.Timestamp,
					Misc.ConvertToCustomLongDateTimeFormat(x.Timestamp),
					"Audit",
					x.UserId,
					x.AuditEventType,
					"-",
					"-",
					"-",
					"-",
					"-",
					x.Message);
			});

			errorLogs.ForEach(x =>
			{
				dataTable.Rows.Add(
					x.Timestamp,
					Misc.ConvertToCustomLongDateTimeFormat(x.Timestamp),
					"Error",
					x.UserId,
					"-",
					x.ErrorCode,
					"-",
					"-",
					"-",
					"-",
					x.Message);
			});

			sampleLogs.ForEach(x =>
			{
				dataTable.Rows.Add(
					x.Timestamp,
					Misc.ConvertToCustomLongDateTimeFormat(x.Timestamp),
					"Sample",
					x.UserId,
					"-",
					"-",
					x.SampleLabel,
					x.AnalysisName,
					x.CellTypeName,
					x.SampleStatus,
					"-");
			});

			dataTable.DefaultView.Sort = "SortDate ASC";
			dataTable = dataTable.DefaultView.ToTable();
			dataTable.Columns.Remove("SortDate");

			return dataTable;
		}

		private static DataTable CreateCalibrationErrorLogKeysDataTable(List<CalibrationActivityLogDomain> calibrationLogs)
		{
			using (var dataTable = new DataTable())
			{
				dataTable.Columns.Add("LID_AdminReportsHeader_DateTime", typeof(string));
				dataTable.Columns.Add("LID_UsersLabel_UserId", typeof(string));
				dataTable.Columns.Add("LID_Label_CalibrationType", typeof(string));
				dataTable.Columns.Add("LID_Label_NoConsum", typeof(string));
				dataTable.Columns.Add("LID_Label_Consumable", typeof(string));
				dataTable.Columns.Add("LID_DataGridHeader_AssayValue", typeof(string));
				dataTable.Columns.Add("LID_QCHeader_LotNumber", typeof(string));
				dataTable.Columns.Add("LID_GridLabel_ExpirationDate", typeof(string));
				dataTable.Columns.Add("LID_Label_Slop", typeof(string));
				dataTable.Columns.Add("LID_Label_Intercept", typeof(string));

				calibrationLogs.ForEach(x =>
				{
					dataTable.Rows.Add(Misc.ConvertToCustomLongDateTimeFormat(x.Date),
						x.UserId, x.CalibrationType, x.NumberOfConsumables, x.Label, x.AssayValue,
						x.LotId, Misc.ConvertToString(x.ExpirationDate), x.Slope, x.Intercept);
				});
				return dataTable;
			}
		}

		#endregion

		#endregion

		public static bool OpenCsvSaveFileDialog(string fileName, out string fullFilePath)
		{
			fullFilePath = string.Empty;

			string filePath = LoggedInUser.CurrentExportPath;
			if (!FileSystem.IsFolderValidForExport(filePath) || !Directory.Exists(filePath))
			{
				filePath = FileSystem.GetDefaultExportPath(LoggedInUser.CurrentUserId);
				if (!FileSystem.EnsureDirectoryExists(filePath))
					filePath = FileSystem.GetDefaultExportPath("");
				LoggedInUser.CurrentExportPath = filePath;
			}

			var args = new SaveFileDialogEventArgs(fileName, filePath, "CSV Files (*.csv)|*.csv", "csv");
			if (DialogEventBus.OpenSaveFileDialog(null, args) != true)
				return false;

			if (!FileSystem.IsFolderValidForExport(args.FullFilePathSelected))
			{
				var invalidPath = LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_PathError");
				var msg = $"{invalidPath}";
				if (FileSystem.IsPathValid(args.FullFilePathSelected))
				{
					string drive = Path.GetPathRoot(args.FullFilePathSelected);
					if (drive.ToUpper().StartsWith("C:"))
						msg += "\n" + LanguageResourceHelper.Get("LID_MSGBOX_ExportPathError");
				}
				DialogEventBus.DialogBoxOk(null, msg);
				return false;
			}

			fullFilePath = args.FullFilePathSelected;
			try
			{
				LoggedInUser.CurrentExportPath = FileSystem.GetDirectoryName(fullFilePath);
			}
			catch (Exception e)
			{
				Log.Error($"Failed to set LoggedInUser.CurrentExportPath", e);
			}
			return true;
		}

		public static List<ExportQueueCreationDomain> GetFrom(List<SampleDomain> sampleList, SubstrateType substrateType,
			bool isRowWiseSort)
		{
			var exportList = new List<ExportQueueCreationDomain>();

			foreach (var item in sampleList)
			{
				exportList.Add(GetFrom(item, substrateType, isRowWiseSort));
			}

			return exportList;
		}

		public static ExportQueueCreationDomain GetFrom(SampleDomain sampleDomain, SubstrateType substrateType, bool isRowWiseSort)
		{
			var exportCreation = new ExportQueueCreationDomain
			{
				StageTypeAsString = SubstrateAsString(substrateType),
				Positions = sampleDomain.SampleRowPosition,
				SampleIDs = sampleDomain.SampleID,
				CellTypes = sampleDomain.SelectedCellTypeQualityControlGroup.Name,
				SelectedQcCtBpQcType = sampleDomain.SelectedCellTypeQualityControlGroup.SelectedCtBpQcType,
				Dilution = sampleDomain.SelectedDilution,
				Wash = sampleDomain.SelectedWash,
				Comment = sampleDomain.Comment,
				NthImage = sampleDomain.NthImage,
				IsExportPdfResultExport = sampleDomain.IsExportPDFSelected,
				IsExportEachSampleActive = sampleDomain.IsExportEachSampleActive,
				IsAppendResultExport = sampleDomain.IsAppendResultExport,
				ExportEachSample = sampleDomain.ExportPathForEachSample,
				AppendResultExport = sampleDomain.AppendResultExport,
				ExportFileName = sampleDomain.ExportFileName,
				RowColWise = substrateType == SubstrateType.Carousel
					? ApplicationConstants.Row
					: isRowWiseSort
						? ApplicationConstants.Row
						: ApplicationConstants.Col
			};

			var allCellTypes = CellTypeFacade.Instance.GetAllCellTypes_BECall();

			switch (sampleDomain.SelectedCellTypeQualityControlGroup.SelectedCtBpQcType)
			{
				case CtBpQcType.CellType:
					exportCreation.CellTypeDetails = ExportDomainHelper.GetCellTypeDetailsForExport(
						sampleDomain.SelectedCellTypeQualityControlGroup.CellTypeIndex, allCellTypes);
					var qc = new ExportQueueCreationQualityControlDomain();
					exportCreation.QualityControlDetails = qc.GetEmptyData();
					break;
				case CtBpQcType.QualityControl:
					exportCreation.QualityControlDetails = ExportDomainHelper.GetQualityControlDetailsForExport(
						sampleDomain.SelectedCellTypeQualityControlGroup.Name);
					if (sampleDomain.SelectedCellTypeQualityControlGroup?.CellTypeIndex > 0)
					{
						exportCreation.CellTypeDetails = ExportDomainHelper.GetCellTypeDetailsForExport(
							sampleDomain.SelectedCellTypeQualityControlGroup.CellTypeIndex, allCellTypes);
					}
					break;
			}

			return exportCreation;
		}

		public static string SubstrateAsString(SubstrateType substrateType)
		{
			switch (substrateType)
			{
				case SubstrateType.Carousel:
					return ApplicationConstants.CarouselName;
				case SubstrateType.Plate96:
					return ApplicationConstants.PlateName;
				case SubstrateType.AutomationCup:
					return ApplicationConstants.AutomationCupName;
				default:
					return string.Empty;
			}
		}
	}
}
