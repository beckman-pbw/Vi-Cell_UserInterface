using log4net;
using ScoutDomains;
using ScoutDomains.ClusterDomain;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.RunResult;
using ScoutLanguageResources;
using ScoutModels.Review;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using ScoutUtilities.UIConfiguration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ApiProxies.Extensions;
using ScoutDataAccessLayer.DAL;

namespace ScoutModels.Common
{
    public class ResultRecordHelper : Disposable
    {
        private static readonly string DefaultImageDirectory;

        public RunOptionSettingsModel RunOptionModel;

        public ReviewModel ReviewModel;

        static ResultRecordHelper()
        {
            DefaultImageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ApplicationConstants.TargetFolderName);
        }

        public ResultRecordHelper(string username = "")
        {
            if (string.IsNullOrEmpty(username))
                RunOptionModel = new RunOptionSettingsModel(XMLDataAccess.Instance, LoggedInUser.CurrentUserId);
            else
                RunOptionModel = new RunOptionSettingsModel(XMLDataAccess.Instance, username);
            ReviewModel = new ReviewModel();
        }

        protected override void DisposeUnmanaged()
        {
            ReviewModel?.Dispose();
            base.DisposeUnmanaged();
        }

        public static List<SampleResultRecordExportDomain> ExportCompleteRunResult(List<SampleRecordDomain> resultList)
        {
            var exportList = new List<SampleResultRecordExportDomain>();
            foreach(var r in resultList)
            {
                if (r == null) continue;
                var reanalysisBy = string.Empty;
                var reanalysisDateTime = string.Empty;
                
                if (r.ResultSummaryList.Count > 1 && !r.SelectedResultSummary.UUID.IsEmpty())
                {
                    if (!r.SelectedResultSummary.UUID.Equals(r.ResultSummaryList.First().UUID))
                    {
                        reanalysisBy = r.SelectedResultSummary.UserId;
                        reanalysisDateTime = Misc.ConvertToCustomLongDateTimeFormat(r.SelectedResultSummary.RetrieveDate);
                    }
                }

                var sampleResultRecordExportDomain = new SampleResultRecordExportDomain
                {
                    SampleId = r.SampleIdentifier,
                    Dilution = r.DilutionName,
                    Wash = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(r.WashName)),
                    Tag = r.Tag,
                    CellType = "",
                    AnalysisDateTime = r.ResultSummaryList.Any() 
                        ? Misc.ConvertToCustomLongDateTimeFormat(r.ResultSummaryList.FirstOrDefault().RetrieveDate) 
                        : string.Empty,
                    AnalysisBy = r.ResultSummaryList.Any() ? r.ResultSummaryList.FirstOrDefault().UserId : string.Empty,
                    ReAnalysisDateTime = reanalysisDateTime,
                    ReAnalysisBy = reanalysisBy
                };


                if (null != r.SelectedResultSummary)
                {
                    var resultSummary = r.SelectedResultSummary;
                    if (string.IsNullOrEmpty(r.BpQcName))
                    {
                        sampleResultRecordExportDomain.CellType = resultSummary.CellTypeDomain?.CellTypeName;
                    }
                    else
                    {
                        sampleResultRecordExportDomain.CellType = Misc.GetParenthesisQualityControlName(r.BpQcName, resultSummary.CellTypeDomain?.CellTypeName);
                    }

                    sampleResultRecordExportDomain.Images = Misc.ConvertToString(resultSummary.CellTypeDomain?.Images);
                    sampleResultRecordExportDomain.CellSharpness = Misc.ConvertToString(resultSummary.CellTypeDomain?.CellSharpness);
                    sampleResultRecordExportDomain.AnalysisType = resultSummary.AnalysisDomain?.Label;
                    sampleResultRecordExportDomain.TotalCells = Misc.ConvertToString(resultSummary.CumulativeResult?.TotalCells);
                    sampleResultRecordExportDomain.ViableCells = Misc.ConvertToString(resultSummary.CumulativeResult?.ViableCells);
                    sampleResultRecordExportDomain.AvgBckGroundBrightFieldIntensity = Misc.ConvertToString(resultSummary.CumulativeResult?.AvgBackground);
                    sampleResultRecordExportDomain.Bubbles = Misc.ConvertToString(resultSummary.CumulativeResult?.Bubble);
                    sampleResultRecordExportDomain.ClusterCount = Misc.ConvertToString(resultSummary.CumulativeResult?.ClusterCount);
                    sampleResultRecordExportDomain.TotalImages = Misc.ConvertToString(resultSummary.CumulativeResult?.TotalCumulativeImage);
                    if (r.SelectedResultSummary.CellTypeDomain != null)
                    {
                        sampleResultRecordExportDomain.DeclusterDegree = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(resultSummary.CellTypeDomain?.DeclusterDegree));
                    }
                    sampleResultRecordExportDomain.QCStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(resultSummary.QCStatus));
                }

                SetExportData(r, sampleResultRecordExportDomain);
                
                if (r?.SelectedResultSummary?.AnalysisDomain != null && r.SelectedResultSummary.AnalysisDomain.AnalysisParameter.Any())
                {
                    r.SelectedResultSummary.AnalysisDomain.AnalysisParameter.ForEach(ap =>
                    {
                        switch (ap.Label)
                        {
                            case ApplicationConstants.CellSpotArea:
                                if (ap.ThresholdValue != null)
                                    sampleResultRecordExportDomain.ViableSpotArea =
                                        Misc.UpdateTrailingPoint(ap.ThresholdValue.Value, TrailingPoint.One);
                                break;
                            case ApplicationConstants.AvgSpotBrightness:
                                if (ap.ThresholdValue != null)
                                    sampleResultRecordExportDomain.ViableSpotBrightness =
                                        Misc.UpdateTrailingPoint(ap.ThresholdValue.Value, TrailingPoint.One);
                                break;
                        }
                    });
                }
                exportList.Add(sampleResultRecordExportDomain);
            }
            return exportList;
        }

        private static void SetExportData(SampleRecordDomain record, SampleResultRecordExportDomain sampleResultRecordExportDomain)
        {
            if (null == record?.SelectedResultSummary)
            {
                return;
            }

            var resultSummary = record.SelectedResultSummary;
            var cumulativeResult = resultSummary.CumulativeResult;
            if (null == resultSummary.CumulativeResult)
            {
                sampleResultRecordExportDomain.Viability = Misc.UpdateTrailingPoint(cumulativeResult.Viability, TrailingPoint.One);
                sampleResultRecordExportDomain.Concentration = Misc.ConvertToConcPower(cumulativeResult.ConcentrationML);
                sampleResultRecordExportDomain.ViableConcentration = Misc.ConvertToConcPower(cumulativeResult.ViableConcentration);
                sampleResultRecordExportDomain.Size = Misc.UpdateTrailingPoint(cumulativeResult.Size, TrailingPoint.Two);
                sampleResultRecordExportDomain.ViableSize = Misc.UpdateTrailingPoint(cumulativeResult.ViableSize, TrailingPoint.Two);
                sampleResultRecordExportDomain.Circularity = Misc.UpdateTrailingPoint(cumulativeResult.Circularity, TrailingPoint.Two);
                sampleResultRecordExportDomain.ViableCircularity = Misc.UpdateTrailingPoint(cumulativeResult.ViableCircularity, TrailingPoint.Two);
            }

            var cellTypeDomain = resultSummary.CellTypeDomain;
            if (null != cellTypeDomain)
            {
                if (cellTypeDomain?.MinimumCircularity != null)
                    sampleResultRecordExportDomain.MinimumCircularity = Misc.UpdateTrailingPoint(cellTypeDomain.MinimumCircularity.Value, TrailingPoint.Two);
                if (cellTypeDomain?.MinimumDiameter != null)
                    sampleResultRecordExportDomain.MinimumDiameter = Misc.UpdateTrailingPoint(cellTypeDomain.MinimumDiameter.Value, TrailingPoint.Two);
                if (cellTypeDomain?.MaximumDiameter != null)
                    sampleResultRecordExportDomain.MaximumDiameter = Misc.UpdateTrailingPoint(cellTypeDomain.MaximumDiameter.Value, TrailingPoint.Two);
            }
        }

		//NOTE: this code is never called...  keep for possible future use...
        //public List<SampleResultRecordExportDomain> ExportCompleteRunResultForRunningSample(List<SampleRecordDomain> resultList)
        //{
        //    var exportList = new List<SampleResultRecordExportDomain>();
        //    resultList.ForEach(r =>
        //    {
        //        if (r?.SelectedResultSummary == null)
        //            return;
        //        var sampleResultRecordExportDomain = new SampleResultRecordExportDomain
        //        {
        //            SampleId = r.SampleIdentifier,
        //            Dilution = r.DilutionName,
        //            Wash = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(r.WashName)),
        //            CellType = string.IsNullOrEmpty(r.BpQcName)
        //                ? r.SelectedResultSummary.CellTypeDomain?.CellTypeName
        //                : r.BpQcName,
        //            Images = Misc.ConvertToString(r.SelectedResultSummary.CellTypeDomain?.Images),
        //            CellSharpness = Misc.ConvertToString(r.SelectedResultSummary.CellTypeDomain?.CellSharpness),
        //            DeclusterDegree = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(r.SelectedResultSummary.CellTypeDomain?.DeclusterDegree)),
        //            AnalysisType = r.SelectedResultSummary.AppDomain?.ApplicationName,
        //            Tag = r.Tag,
        //            AnalysisDateTime = Misc.ConvertToCustomLongDateTimeFormat(DateTime.Now),
        //            AnalysisBy = r.SelectedResultSummary.UserId,
        //            TotalCells = Misc.ConvertToString(r.SelectedResultSummary.CumulativeResult?.TotalCells),
        //            ViableCells = Misc.ConvertToString(r.SelectedResultSummary.CumulativeResult?.ViableCells),
        //            AvgBckGroundBrightFieldIntensity = Misc.ConvertToString(r.SelectedResultSummary.CumulativeResult?.AvgBackground),
        //            Bubbles = Misc.ConvertToString(r.SelectedResultSummary.CumulativeResult?.Bubble),
        //            ClusterCount = Misc.ConvertToString(r.SelectedResultSummary.CumulativeResult?.ClusterCount),
        //            TotalImages = Misc.ConvertToString(r.SelectedResultSummary.CumulativeResult?.TotalCumulativeImage)
        //        };

        //        SetExportData(r, sampleResultRecordExportDomain);
        //        if (r.SelectedResultSummary.AppDomain != null && r.SelectedResultSummary.AppDomain.AnalysisParameter.Any())
        //        {
        //            r.SelectedResultSummary.AppDomain.AnalysisParameter.ForEach(ap =>
        //            {
        //                switch (ap.Label)
        //                {
        //                    case ApplicationConstants.CellSpotArea:
        //                        if (ap.ThresholdValue != null)
        //                            sampleResultRecordExportDomain.ViableSpotArea =
        //                                Misc.UpdateTrailingPoint(ap.ThresholdValue.Value, TrailingPoint.One);
        //                        break;
        //                    case ApplicationConstants.AvgSpotBrightness:
        //                        if (ap.ThresholdValue != null)
        //                            sampleResultRecordExportDomain.ViableSpotBrightness =
        //                                Misc.UpdateTrailingPoint(ap.ThresholdValue.Value, TrailingPoint.One);
        //                        break;
        //                }
        //            });
        //        }
        //        exportList.Add(sampleResultRecordExportDomain);
        //    });
        //    return exportList;
        //}

        public List<histogrambin_t> RetrieveHistogramForResultRecord(uuidDLL id, bool POI, Characteristic_t measurement, byte bin_count)
        {
            Log.Info("UUID: " + id +
	            "key: " + measurement.key + 
                ", s_key: " + measurement.s_key + 
                ", s_s_Key: " + measurement.s_s_key + 
                ", POI: " + POI + 
                ", bin_count: " + bin_count);
            List<histogrambin_t> HistogramRecord;
            var hawkeyeError = HawkeyeCoreAPI.Result.RetrieveHistogramForResultRecordAPI(id, POI, measurement, bin_count, out HistogramRecord);
			Misc.LogOnHawkeyeError("RetrieveHistogramForResultRecord", hawkeyeError);
            return HistogramRecord;
        }

        public DetailedResultMeasurementsDomain RetrieveDetailedMeasurementsForResultRecord(uuidDLL id)
        {
            DetailedResultMeasurementsDomain clusterRecord;
            var hawkeyeError = HawkeyeCoreAPI.Result.RetrieveDetailedMeasurementsForResultRecordAPI(id, out clusterRecord);
            Misc.LogOnHawkeyeError($"RetrieveDetailedMeasurementsForResultRecordAPI({id})", hawkeyeError);
            return clusterRecord;
        }

        public List<KeyValuePair<int, List<histogrambin_t>>> GetHistogramList(uuidDLL resultId)
        {
            var histogramPairList = new List<KeyValuePair<int, List<histogrambin_t>>>();
            byte bincount_diams = GetDiameterHistogramBinCount(resultId);
            byte bincount_circ = 25;
            var keysPair = new Characteristic_t { s_key = 0, s_s_key = 0 };

            keysPair.key = (ushort)BlobCharacteristicKeys.DiameterInMicrons;
            // The indexes aren't actually used anywhere, so they are arbitrary.
            histogramPairList.Add(new KeyValuePair<int, List<histogrambin_t>>(1,
                RetrieveHistogramForResultRecord(resultId, false, keysPair, bincount_diams)));

            keysPair.key = (ushort)BlobCharacteristicKeys.DiameterInMicrons;
            histogramPairList.Add(new KeyValuePair<int, List<histogrambin_t>>(2,
                RetrieveHistogramForResultRecord(resultId, true, keysPair, bincount_diams)));

            keysPair.key = (ushort)BlobCharacteristicKeys.Circularity;
            histogramPairList.Add(new KeyValuePair<int, List<histogrambin_t>>(3,
                RetrieveHistogramForResultRecord(resultId, false, keysPair, bincount_circ)));

            keysPair.key = (ushort)BlobCharacteristicKeys.Circularity;
            histogramPairList.Add(new KeyValuePair<int, List<histogrambin_t>>(4,
                RetrieveHistogramForResultRecord(resultId, true, keysPair, bincount_circ)));

            return histogramPairList;
        }

        private byte GetDiameterHistogramBinCount(uuidDLL resultId)
        {
            var detailedMeasurements = RetrieveDetailedMeasurementsForResultRecord(resultId);
            var min = Double.PositiveInfinity;
            var max = Double.NegativeInfinity;
            if (detailedMeasurements.BlobsByImage != null
                && detailedMeasurements.BlobsByImage.Any())
            {
                foreach (var blob in detailedMeasurements.BlobsByImage
                    .Where(im => im.BlobList != null
                        && im.BlobList.Any()
                        && im.BlobList.First().Measurements.ContainsKey(BlobCharacteristicKeys.DiameterInMicrons))
                    .SelectMany(im => im.BlobList))     // Flatten lists
                {
                    var diam = blob.Measurements[BlobCharacteristicKeys.DiameterInMicrons];
                    if (diam < min)
                        min = diam;
                    if (diam > max)
                        max = diam;
                }

                if (Double.IsPositiveInfinity(min))
                    // There are no cells. It doesn't matter how many bins we use,
                    // since nothing will be displayed, but rather than force the
                    // backend to deal with the edge-case of 0 bins or rewriting
                    // the logic above, we simply use 6, which is consistent with
                    // older versions of the software.
                    return 6;

                var binWidth = UISettings.DiameterHistogramBinWidth;
                var binCount = (max - min) / binWidth;
                return Convert.ToByte(Math.Min(binCount, 140));
            }
            return 6;
        }

        public static List<KeyValuePair<string, string>> GetShowParameterList(GenericDataDomain genericData, string userName)
        {
            var list = new List<KeyValuePair<string, string>>();
            var runOptionModel = new RunOptionSettingsModel(XMLDataAccess.Instance, userName);
            runOptionModel.GetGenericParameters(userName);
            var genericParameters = runOptionModel.GenericParameters;
            foreach (var param in genericParameters)
            {
                if (!param.IsVisible)
                    continue;
                string dateTime;
                switch (param.ParameterID)
                {
                    case ResultParameter.eCellType:
                        list.Add(new KeyValuePair<string, string>(LanguageResourceHelper.Get("LID_UsersLabel_CellType"), genericData.BpQc));
                        break;
                    case ResultParameter.eDilution:
                        list.Add(new KeyValuePair<string, string>(LanguageResourceHelper.Get("LID_Label_Dilution"), genericData.Dilution));
                        break;
                    case ResultParameter.eWash:
                        list.Add(new KeyValuePair<string, string>(LanguageResourceHelper.Get("LID_QMgmtHEADER_Workflow"),
                            !string.IsNullOrEmpty(genericData.BpQc) ? LanguageResourceHelper.Get(GetEnumDescription.GetDescription(genericData.Wash)) : string.Empty));
                        break;
                    case ResultParameter.eComment:
                        list.Add(new KeyValuePair<string, string>(LanguageResourceHelper.Get("LID_Label_Tag"), genericData.Tag));
                        break;
                    case ResultParameter.eAnalysisDateTime:
                        dateTime = genericData.AnalysisDateTime.HasValue
                            ? Misc.ConvertToCustomLongDateTimeFormat(genericData.AnalysisDateTime.Value)
                            : string.Empty;
                        list.Add(new KeyValuePair<string, string>(LanguageResourceHelper.Get("LID_QCHeader_AnalysisDateTime"), dateTime));
                        break;
                    case ResultParameter.eReanalysisDateTime:
                        dateTime = genericData.ReanalysisDateTime.HasValue
                            ? Misc.ConvertToCustomLongDateTimeFormat(genericData.ReanalysisDateTime.Value)
                            : string.Empty;
                        list.Add(new KeyValuePair<string, string>(LanguageResourceHelper.Get("LID_QCHeader_ReAnalysisDateTime"), dateTime));
                        break;
                    case ResultParameter.eAnalysisBy:
                        list.Add(new KeyValuePair<string, string>(LanguageResourceHelper.Get("LID_Report_AnalysisBy"), genericData.AnalysisBy));
                        break;
                    case ResultParameter.eReanalysisBy:
                        list.Add(new KeyValuePair<string, string>(LanguageResourceHelper.Get("LID_Report_ReanalysisBy"), genericData.ReanalysisBy));
                        break;
                }
            }

            return list;
        }

        public List<KeyValuePair<string, string>> SetListParameter(SampleRecordDomain sample, string userName)
        {
            var genericData = new GenericDataDomain
            {
                AnalysisBy = sample.ResultSummaryList.FirstOrDefault()?.UserId,
                AnalysisDateTime = sample.ResultSummaryList.FirstOrDefault()?.RetrieveDate,
                Tag = sample.Tag,
                Wash = sample.WashName,
                Dilution = sample.DilutionName
            };

            if (sample.ResultSummaryList.Count > 1 && !sample.SelectedResultSummary.UUID.IsEmpty())
            {
                if (!sample.SelectedResultSummary.UUID.Equals(sample.ResultSummaryList.FirstOrDefault()?.UUID))
                {
                    genericData.ReanalysisBy = sample.SelectedResultSummary.UserId;
                    genericData.ReanalysisDateTime = sample.SelectedResultSummary.RetrieveDate;
                }
            }

            if (sample.SelectedResultSummary != null)
            {
	            var name = sample.SelectedResultSummary.CellTypeDomain?.CellTypeName;
	            if (sample.BpQcName != "")
	            {
		            name = Misc.GetParenthesisQualityControlName(sample.BpQcName, name);
	            }

				genericData.BpQc = name;
            }

            return GetShowParameterList(genericData, userName);
        }

        public ResultSummaryDomain OnSelectedSampleRecordForReport(SampleRecordDomain sampleRecord)
        {
            return ReviewModel.RetrieveResultSummary(sampleRecord.SelectedResultSummary.UUID);
        }

        public ResultSummaryDomain OnSelectedSampleRecordForReport(ResultSummaryDomain rseultSummary)
        {
            return ReviewModel.RetrieveResultSummary(rseultSummary.UUID);
        }

        public ResultRecordDomain OnSelectedSampleRecordForReport(uuidDLL uuid)
        {
            return ReviewModel.RetrieveResultRecord(uuid);
        }

     
        public void OnSelectedSampleRecord(SampleRecordDomain sampleRecord)
        {
            if (sampleRecord.ResultSummaryList != null && sampleRecord.ResultSummaryList.Count > 0)
            {
                sampleRecord.SelectedResultSummary = sampleRecord.ResultSummaryList[0];
            }
            SetImageList(sampleRecord);
        }

        public void SetImageList(SampleRecordDomain sampleRecord)
        {
            if (sampleRecord?.ImageSetIds == null) return;

            var sampleImageSetRecordList = ReviewModel.RetrieveSampleImageSetRecordsList(sampleRecord.ImageSetIds);
            if (sampleImageSetRecordList != null && sampleImageSetRecordList.Count > 0)
            {
                SetSampleImageSetRecord(sampleImageSetRecordList, sampleRecord);

                if (sampleRecord.SampleImageList != null && sampleRecord.SampleImageList.Count > 0)
                {
                    sampleRecord.SelectedSampleImageRecord = sampleRecord.SampleImageList.FirstOrDefault();
                }

                if (sampleRecord.ImageIndexList == null)
                    sampleRecord.ImageIndexList = new ObservableCollection<KeyValuePair<int, string>>();

                sampleImageSetRecordList.ForEach(sampleImage =>
                {
                    var key = Convert.ToInt32(sampleImage.SequenceNumber);
                    var value = Misc.ConvertToString(sampleImage.SequenceNumber);
                    var kvp = new KeyValuePair<int, string>(key, value);
                    if (!sampleRecord.ImageIndexList.Contains(kvp))
                    {
                        sampleRecord.ImageIndexList.Add(kvp);
                    }
                });
            }
        }

        public void SetSampleImageSetRecord(List<SampleImageRecordDomain> sampleImageList, SampleRecordDomain sampleRecord)
        {
            try
            {
                var uuid = sampleRecord.SelectedResultSummary?.UUID;
                var resultRecord = uuid.HasValue ? sampleRecord.GetResultRecord(uuid.Value) : null;
                if (resultRecord != null)
                {
                    sampleRecord.SelectedResultSummary.SignatureList = resultRecord.ResultSummary.SignatureList;
                    sampleRecord.SelectedResultSummary.SelectedSignature = resultRecord.ResultSummary.SelectedSignature;
                    var resultPerImageList = resultRecord.ResultPerImage;
                    foreach (var sampleImg in sampleImageList)
                    {
                        sampleImg.ImageID = 1;
                        if (resultPerImageList != null && resultPerImageList.Any())
                        {
                            var index = (int) sampleImg.SequenceNumber - 1;
                            if (index >= resultPerImageList.Count)
                            {
                                Log.Warn($"There's a mismatch in the number of images ({sampleImageList.Count}) " +
                                          $"to the number of result records ({resultPerImageList.Count})." +
                                          $"{Environment.NewLine}sampleRecord: {sampleRecord.ToString()}");
                                break;
                            }
                            
                            sampleImg.ResultPerImage = resultPerImageList[index];
                        }
                    }
                }
                sampleRecord.SampleImageList = new ObservableCollection<SampleImageRecordDomain>(sampleImageList);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to Attach sample result records to sample images", e);
            }
        }

        private string GetImagePath()
        {
	        var imageName = ApplicationConstants.ImageFileName + ApplicationConstants.ImageFileExtension;
	        var filePath = Path.Combine(DefaultImageDirectory, imageName);
	        return filePath;
        }

		private void SaveImageForHistogram(ImageDto source)
		{
			var DefaultImageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ApplicationConstants.TargetFolderName);
			var imageName = ApplicationConstants.ImageFilenameForHistogram + ApplicationConstants.ImageFileExtension;
			var imagePath = Path.Combine(DefaultImageDirectory, imageName);
			ImageDtoExts.SaveImage(source, imagePath);
		}

		public ImageDto GetRawImage(uuidDLL sampleImageId)
        {
            var filePath = GetImagePath();
            var image = ReviewModel.RetrieveImage(sampleImageId, filePath);
            SaveImageForHistogram(image);
            return image;
        }

        private ImageDto GetAnnotatedImage(uuidDLL sampleImageId, uuidDLL resultId)
        {
            var filePath = GetImagePath();
            var image = ReviewModel.RetrieveAnnotatedImage(resultId, sampleImageId, filePath);
            SaveImageForHistogram(image);
            return image;
        }

        private ImageDto GetBlackWhiteImage(uuidDLL sampleImageId)
        {
            var filePath = GetImagePath();
            var image = ReviewModel.RetrieveBWImage(sampleImageId, filePath);
            SaveImageForHistogram(image);
            return image;
        }

        public ImageSetDto GetImage(ImageType selectedImageType, uuidDLL imageId, uuidDLL resultId)
        {
            try
            {
                if (imageId.IsNullOrEmpty())
                {
                    Log.Debug($"GetImage::imageId: {imageId}:{Environment.NewLine}{new System.Diagnostics.StackTrace()}");
                    return new ImageSetDto(); // Must have an image ID to get an image
                }
                if (resultId.IsNullOrEmpty())
                {
                    Log.Debug($"GetImage::resultId: {resultId}:{Environment.NewLine}{new System.Diagnostics.StackTrace()}");
                }
            }
            catch (Exception e)
            {
                Log.Debug($"Exception thrown when checking for null uuidDLLs", e);
            }
            
            var imageSet = new ImageSetDto();
            switch (selectedImageType)
            {
                case ImageType.Raw:
                    imageSet.BrightfieldImage = GetRawImage(imageId);
                    break;
                case ImageType.Annotated:
                    if (!resultId.IsNullOrEmpty()) // Must have a result ID to get an annotated image
                    {
                        imageSet.BrightfieldImage = GetAnnotatedImage(imageId, resultId);
                    }
                    break;
                case ImageType.Binary:
                    imageSet.BrightfieldImage = GetBlackWhiteImage(imageId);
                    break;
            }
            return imageSet;
        }
    }
}