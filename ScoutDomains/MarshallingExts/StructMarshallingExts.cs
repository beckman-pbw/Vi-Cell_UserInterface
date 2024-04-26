using log4net;
using ScoutDomains.Analysis;
using ScoutDomains.ClusterDomain;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.RunResult;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CoordinatePair = System.Drawing.Point;

namespace ScoutDomains
{
    public static class StructMarshallingExts
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static CellTypeDomain MarshalToCellTypeDomain(this CellType cellType)
        {
            var cell = new CellTypeDomain();

            cell.CellTypeIndex = cellType.celltype_index;
            cell.CellTypeName = cellType.label.ToSystemString();
            cell.TempCellName = cellType.label.ToSystemString();
            cell.MinimumDiameter = Misc.UpdateDecimalPoint(cellType.minimum_diameter_um);
            cell.MaximumDiameter = Misc.UpdateDecimalPoint(cellType.maximum_diameter_um);
            cell.Images = cellType.max_image_count;
            cell.MinimumCircularity = Math.Round(cellType.minimum_circularity, 2);
            cell.CellSharpness = cellType.sharpness_limit;
            cell.DeclusterDegree = cellType.decluster_setting;
            cell.AspirationCycles = cellType.aspiration_cycles;
            cell.CalculationAdjustmentFactor = cellType.calculation_adjustment_factor;

            return cell;
        }

        public static BasicResultDomain MarshalToBasicResultDomain(this BasicResultAnswers basicResultAnswers)
        {
            var basicResult = new BasicResultDomain()
            {
                ProcessedStatus = basicResultAnswers.eProcessedStatus,
                TotalCumulativeImage = basicResultAnswers.nTotalCumulative_Imgs,
                TotalCells = basicResultAnswers.count_pop_general,
                ViableCells = basicResultAnswers.count_pop_ofinterest,
                ConcentrationML = Misc.UpdateDecimalPoint(basicResultAnswers.concentration_general,TrailingPoint.Four),
                ViableConcentration = Misc.UpdateDecimalPoint(basicResultAnswers.concentration_ofinterest,TrailingPoint.Four),
                Viability = Misc.UpdateDecimalPoint(basicResultAnswers.percent_pop_ofinterest, TrailingPoint.One),
                Size = Misc.UpdateDecimalPoint(basicResultAnswers.avg_diameter_pop),
                ViableSize = Misc.UpdateDecimalPoint(basicResultAnswers.avg_diameter_ofinterest),
                Circularity = Misc.UpdateDecimalPoint(basicResultAnswers.avg_circularity_pop),
                ViableCircularity = Misc.UpdateDecimalPoint(basicResultAnswers.avg_circularity_ofinterest),
                AverageCellsPerImage = basicResultAnswers.average_cells_per_image,
                Bubble = basicResultAnswers.bubble_count,
                ClusterCount = basicResultAnswers.large_cluster_count,
                AvgBackground = basicResultAnswers.average_brightfield_bg_intensity
            };
            return basicResult;
        }

        public static ObservableCollection<ResultSummaryDomain> GetSummaryResultList(this SampleRecord sampleRecord)
        {
            return new ObservableCollection<ResultSummaryDomain>(
                    sampleRecord.result_summaries.MarshalToResultSummaryDomainList(sampleRecord.num_result_summary));
        }

        public static ResultSummaryDomain MarshalToResultSummaryDomain(this ResultSummary resultSummary)
        {
            var result = new ResultSummaryDomain()
            {
                UUID = resultSummary.uuid,
                UserId = resultSummary.user_id.ToSystemString(),
                TimeStamp = resultSummary.time_stamp,
                RetrieveDate = DateTimeConversionHelper.FromSecondUnixToDateTime(resultSummary.time_stamp),
                CumulativeResult = resultSummary.cumulative_result.MarshalToBasicResultDomain(),
                CellTypeDomain = resultSummary.cell_type_settings.MarshalToCellTypeDomain(),
                AnalysisDomain = resultSummary.analysis_settings.MarshalToApplicationTypeDomain()
            };
            result.CellTypeDomain.AnalysisDomain = MarshalToApplicationTypeDomain(resultSummary.analysis_settings);
            result.SignatureList = resultSummary.signature_set.MarshalToSignatureInstanceDomainList(
                resultSummary.num_signatures).ToObservableCollection();
            if (result.SignatureList != null && result.SignatureList.Any())
                result.SelectedSignature = result.SignatureList.Last();

            result.QCStatus = resultSummary.qcStatus;

            return result;
        }

        public static List<BlobMeasurementDomain> MarshalToBlobMeasurementDomain(this image_blobs_t blobs)
        {
            var blbList = new List<BlobMeasurementDomain>();
            var nextBlobPtr = blobs.blob_list;
            for (int i = 0; i < blobs.blob_count; i++)
            {
                var blobRaw =
                    (blob_measurements_t) Marshal.PtrToStructure(nextBlobPtr, typeof(blob_measurements_t));

                blbList.Add(new BlobMeasurementDomain()
                {
                    Coordinates = new CoordinatePair(blobRaw.x_coord, blobRaw.y_coord),
                    Measurements = blobRaw.MarshalToMeasurementList()
                });

                nextBlobPtr += Marshal.SizeOf(typeof(blob_measurements_t));
            }

            return blbList;
        }

        public static Dictionary<BlobCharacteristicKeys, double> MarshalToMeasurementList(this blob_measurements_t bm)
        {
            var measureDict = new Dictionary<BlobCharacteristicKeys, double>();
            var nextMeasurementPtr = bm.measurements;
            for (int i = 0; i < bm.num_measurements; i++)
            {
                var m = (measurement_t) Marshal.PtrToStructure(nextMeasurementPtr, typeof(measurement_t));
                measureDict[(BlobCharacteristicKeys) m.characteristic.key] = m.value;
                nextMeasurementPtr += Marshal.SizeOf(typeof(measurement_t));
            }

            return measureDict;
        }

        public static List<LargeClusterDataDomain> MarshalToLargeClusterDataList(this large_cluster_t largeCluster)
        {
            var largeclusterlist = new List<LargeClusterDataDomain>();
            var largeclusterStrList = new List<large_cluster_data_t>();
            for (int i = 0; i < largeCluster.cluster_count; i++)
            {
                largeclusterStrList.Add((large_cluster_data_t) Marshal.PtrToStructure(largeCluster.cluster_list,
                    typeof(large_cluster_data_t)));
                largeCluster.cluster_list =
                    new IntPtr(largeCluster.cluster_list.ToInt64() + Marshal.SizeOf(typeof(large_cluster_data_t)));
            }

            if (largeclusterStrList.Count > 0)
            {
                largeclusterlist.ForEach(cluster =>
                {
                    largeclusterlist.Add(new LargeClusterDataDomain()
                    {
                        bottom_right_x = cluster.bottom_right_x,
                        bottom_right_y = cluster.bottom_right_y,
                        numCell = cluster.numCell,
                        top_left_x = cluster.top_left_x,
                        top_left_y = cluster.top_left_y
                    });
                });
            }

            return largeclusterlist;
        }

        public static ImageDto MarshalToImageDto(this imagewrapper_t imageWrapper)
        {
            var dto = new ImageDto();
            dto.Rows = imageWrapper.rows;
            dto.Cols = imageWrapper.cols;
            dto.Type = imageWrapper.type;
            dto.Step = imageWrapper.step;

            var pixelFormat = getPixelFormat(dto);
            if (!pixelFormat.HasValue || imageWrapper.data == IntPtr.Zero)
            {
                dto.ImageSource = null;
            }
            else
            {
                var bufferSize = Convert.ToInt32(dto.Rows * dto.Step);
                dto.ImageSource = BitmapSource.Create(dto.Cols, dto.Rows, 96, 96, pixelFormat.Value, null, imageWrapper.data, bufferSize, Convert.ToInt32(dto.Step));
                dto.ImageSource.Freeze(); // Call to freeze is required to make it accessible across different thread
            }

            return dto;
        }

        public static ImageSetDto MarshalToImageSetDto(this imagesetwrapper_t imgSetWrapper)
        {
            var dto = new ImageSetDto();
            dto.BrightfieldImage = imgSetWrapper.brightfield_image.MarshalToImageDto();
            dto.FlourescenceImages = new List<ImageDto>();
            IntPtr imgBuffer = imgSetWrapper.fl_images;
            int imgSize = Marshal.SizeOf(typeof(imagewrapper_t));
            for (int i = 0; i < imgSetWrapper.num_fl_channels; i++)
            {
                dto.FlourescenceImages.Add(imgBuffer.MarshalToImageDto());
                imgBuffer += imgSize;
            }

            return dto;
        }

        public static ImageSetDto MarshalToImageSetDto(this ImageDto image)
        {
	        var dto = new ImageSetDto();
	        dto.BrightfieldImage = image;
	        dto.FlourescenceImages = null;
	        return dto;
        }

		public static SignatureDomain MarshalToSignatureDomain(this DataSignature_t dataSig)
        {
            var signature = new SignatureDomain();
            signature.SignatureIndicator = dataSig.short_text.ToSystemString();
            if (dataSig.long_text != IntPtr.Zero)
            {
                signature.SignatureMeaning = dataSig.long_text.ToSystemString();
            }
            return signature;
        }

        public static SampleImageRecordDomain MarshalToSampleImageRecordDomain(this SampleImageSetRecord sampleImageRecord)
        {
            return new SampleImageRecordDomain()
            {
                UUID = sampleImageRecord.uuid,
                UserId = sampleImageRecord.userId.ToSystemString(),
                TimeStamp = sampleImageRecord.timeStamp,
                SequenceNumber = sampleImageRecord.sequenceNumber,
                BrightFieldId = sampleImageRecord.brightfieldImage,
                ImageSet = new ImageSetDto()
            };
        }

        [HandleProcessCorruptedStateExceptions]
        public static AnalysisDomain MarshalToApplicationTypeDomain(this AnalysisDefinition analysis)
        {
            var size = Marshal.SizeOf(typeof(AnalysisParameter));
            try
            {
                var appDomain = new AnalysisDomain()
                {
                    AnalysisIndex = analysis.Analysis_index,
                    MixingCycle = analysis.mixing_cycles,
                    Label = analysis.label.ToSystemString(),
                    IsAnalysisEnable = false,
                    AnalysisParameter = new List<AnalysisParameterDomain>()
                };
                for (int i = 0; i < analysis.num_analysis_parameters; i++)
                {
                    var analysisParameter = (AnalysisParameter) Marshal.PtrToStructure(analysis.analysis_parameters, typeof(AnalysisParameter));
                    analysis.analysis_parameters += size;

                    appDomain.AnalysisParameter.Add(new AnalysisParameterDomain()
                    {
                        AboveThreshold = analysisParameter.above_threshold,
                        Characteristic = analysisParameter.characteristic,
                        Label = analysisParameter.label.ToSystemString(),
                        ThresholdValue = analysisParameter.threshold_value
                    });
                }

                return appDomain;
            }
            catch (AccessViolationException e)
            {
                Log.Error(e.ToString(), e);
                return new AnalysisDomain();
            }
        }

        private static PixelFormat? getPixelFormat(ImageDto image)
        {
            if (image.Step <= 0 || image.Cols <= 0 || image.Rows <= 0)
                return null;

            var channel = image.Step / image.Cols;
            if (channel == 1)
            {
                return PixelFormats.Gray8;
            }
            return PixelFormats.Bgr24;
        }
    }
}
