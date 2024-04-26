using ScoutDomains.Analysis;
using ScoutDomains.ClusterDomain;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutDomains.Reagent;
using ScoutDomains.RunResult;
using ScoutLanguageResources;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ScoutDomains
{
    public static class IntptrMarshallingExts
    {
        private static List<CellTypeDomain> _availableCellTypes;
        /// <summary>
        /// This is a hack: This list should only be used privately and should only be set/changed by
        /// CellTypeFacade. The creation of the SampleEswDomain objects requires a list of cell types
        /// and rather than hammering the backend with a new GetCellTypes request every sample image and status callback,
        /// we will use this property. This property should NOT be used by anything other than IntptrMarshallingExts.cs.
        /// If you need a list of cell types, you should be using CellTypeFacade.
        /// </summary>
        public static List<CellTypeDomain> AvailableCellTypes
        {
            private get { return _availableCellTypes ?? (_availableCellTypes = new List<CellTypeDomain>()); }
            set { _availableCellTypes = value; }
        }

        private static List<TTo> MarshalToList<TFrom, TTo>(this IntPtr listPtr, uint count, Func<IntPtr, TTo> marshaller)
        {
            var ptrIterator = listPtr;
            var list = new List<TTo>();
            for (int i = 0; i < count; i++)
            {
                list.Add(marshaller(ptrIterator));
                ptrIterator += Marshal.SizeOf<TFrom>();
            }
            return list;
        }

        public static ImageDto MarshalToImageDto(this IntPtr imgwrapperPtr)
        {
            if (imgwrapperPtr == IntPtr.Zero)
            {
                return null;
            }

            var imageWrapper = (imagewrapper_t) Marshal.PtrToStructure(imgwrapperPtr, typeof(imagewrapper_t));

            var dto = imageWrapper.MarshalToImageDto();
            return dto;
        }


        public static List<LiveImageDataDomain> MarshalToBgUniformity(this IntPtr imgPtr)
        {
            if (imgPtr == IntPtr.Zero)
                return new List<LiveImageDataDomain>();
            var imageWrapper = (imagewrapper_t)Marshal.PtrToStructure(imgPtr, typeof(imagewrapper_t));

            var unmanagedData = new BackgroundUniformityCriteriaModel.UnmanagedImageData()
            {
                data = (UIntPtr)(long)imageWrapper.data,
                width = imageWrapper.cols,
                height = imageWrapper.rows,
                type = imageWrapper.type,
                step = imageWrapper.step
            };
            var model = new BackgroundUniformityCriteriaModel.ImageTestsModel();
            if (!model.loadDataAndExecute(unmanagedData))
                return new List<LiveImageDataDomain>();
            BackgroundUniformityCriteriaModel.ResultDataComplete result;
            model.getResult(out result);
            model.Dispose();
            return GetResultData(result);
        }

        private static List<LiveImageDataDomain> GetResultData(BackgroundUniformityCriteriaModel.ResultDataComplete result)
        {
            if (result?.resultList == null || result.resultList.Length < 1)
                return new List<LiveImageDataDomain>();
            var liveImageDataList = new List<LiveImageDataDomain>();

            liveImageDataList.Add(new LiveImageDataDomain()
            {
                TestName = LanguageResourceHelper.Get("LID_Label_Min_Max_Variation"),
                TestResult = Misc.UpdateTrailingPoint(result.resultList[0].value, ScoutUtilities.Enums.TrailingPoint.Two)
            });
            liveImageDataList.Add(new LiveImageDataDomain()
            {
                TestName = LanguageResourceHelper.Get("LID_Label_Image_Coefficient_Variance"),
                TestResult = Misc.UpdateTrailingPoint(result.resultList[1].value, ScoutUtilities.Enums.TrailingPoint.Two)
            });
            liveImageDataList.Add(new LiveImageDataDomain()
            {
                TestName = LanguageResourceHelper.Get("LID_Label_Image_Intensity_Mean"),
                TestResult = Misc.UpdateTrailingPoint(result.resultList[2].value, ScoutUtilities.Enums.TrailingPoint.Two)
            });
            return liveImageDataList;
        }

        public static List<ImageDto> MarshalToImageDtoList(this IntPtr imgwrapperArrayPtr, int imageCount)
        {
            var results = new List<ImageDto>();
            if (imgwrapperArrayPtr != IntPtr.Zero)
            {
                var iterator = imgwrapperArrayPtr;
                var imgSize = Marshal.SizeOf(typeof(imagewrapper_t));
                for (var i = 0; i < imageCount; i++)
                {
                    results.Add(iterator.MarshalToImageDto());
                    iterator += imgSize;
                }
            }

            return results;
        }

        public static ImageSetDto MarshalToImageSetDto(this IntPtr imgSetPtr)
        {
            if (imgSetPtr == IntPtr.Zero)
            {
                return null;
            }

            var imageset = (imagesetwrapper_t) Marshal.PtrToStructure(imgSetPtr, typeof(imagesetwrapper_t));
            var dto = imageset.MarshalToImageSetDto();
            return dto;
        }

        public static ImageRecordDomain MarshalToImageRecordDomain(this IntPtr imgRecordPtr)
        {
            var rec = (ImageRecord)Marshal.PtrToStructure(imgRecordPtr, typeof(ImageRecord));
            return new ImageRecordDomain()
            {
                UUID = rec.uuid,
                userId = rec.user_id.ToSystemString(),
                TimeStamp = rec.time_stamp
            };
        }

        public static List<ImageRecordDomain> MarshalToImageRecordDomains(this IntPtr imgRecordPtr, uint count)
        {
            return imgRecordPtr.MarshalToList<ImageRecord, ImageRecordDomain>(count, MarshalToImageRecordDomain);
        }

        public static SampleImageRecordDomain MarshalToSampleImageRecordDomain(this IntPtr imgSetRecordPtr)
        {
            var sampleImageSetRec = (SampleImageSetRecord)Marshal.PtrToStructure(imgSetRecordPtr,
                typeof(SampleImageSetRecord));
            return sampleImageSetRec.MarshalToSampleImageRecordDomain();
        }

        public static WorkQueueItemDto MarshalToWorkQueueItemDto(this IntPtr wqiPtr)
        {
            var wqiDto = new WorkQueueItemDto();
            var wqi = (WorkQueueItem) Marshal.PtrToStructure(wqiPtr, typeof(WorkQueueItem));
            wqiDto.AnalysisIndices = wqi.analysisIndices;
            wqiDto.BpQcName = wqi.bp_qc_name.ToSystemString();
            wqiDto.CelltypeIndex = wqi.celltypeIndex;
            wqiDto.Comment = wqi.comment.ToSystemString();
            wqiDto.DilutionFactor = wqi.dilutionFactor;
            wqiDto.Label = wqi.label.ToSystemString();
            wqiDto.Location = wqi.location;
            wqiDto.NumAnalyses = wqi.numAnalyses;
            wqiDto.PostWash = wqi.postWash;
            wqiDto.SaveEveryNthImage = wqi.saveEveryNthImage;
            wqiDto.Status = wqi.status;

            return wqiDto;
        }

        public static List<WorkQueueRecordDomain> MarshalToWorkQueueRecordDomains(
            this IntPtr ptrWorkQueue, uint count)
        {
            return ptrWorkQueue.MarshalToList<WorkQueueRecord, WorkQueueRecordDomain>(
                count, ptr => MarshalToWorkQueueRecordDomain(ptr));
        }

        public static WorkQueueRecordDomain MarshalToWorkQueueRecordDomain(this IntPtr wqrPtr)
        {
            var workQueueRecord = (WorkQueueRecord)Marshal.PtrToStructure(wqrPtr, typeof(WorkQueueRecord));
            return new WorkQueueRecordDomain()
            {
                UUID = workQueueRecord.uuid,
                UserId = workQueueRecord.userId.ToSystemString(),
                TimeStamp = (int) workQueueRecord.timeStamp,
                SampleDateTime = DateTimeConversionHelper.FromSecondUnixToDateTime(workQueueRecord.timeStamp),
                NumSampleRecords = workQueueRecord.numSampleRecords,
                SampleRecordIDs = workQueueRecord.sampleRecords.MarshalToUuidArray(workQueueRecord.numSampleRecords)

            };
        }

        public static List<SampleRecordDomain> MarshalToSampleRecordDomains(this IntPtr srListPtr, uint count, ResultRecordRetriever retrieveResultRecords)
        {
            return srListPtr.MarshalToList<SampleRecord, SampleRecordDomain>(count, srPtr => MarshalToSampleRecordDomain(srPtr, retrieveResultRecords));
        }

        public static SampleRecordDomain MarshalToSampleRecordDomain(this IntPtr srPtr, ResultRecordRetriever retrieveResultRecords)
        {
            var sampleRecord = (SampleRecord)Marshal.PtrToStructure(srPtr, typeof(SampleRecord));
            var srd = new SampleRecordDomain
            {
                UUID = sampleRecord.uuid,
                UserId = sampleRecord.user_id.ToSystemString(),
                TimeStamp = sampleRecord.time_stamp,
                WashName = sampleRecord.wash,
                DilutionName = sampleRecord.dilution_factor.ToString(),
                Tag = sampleRecord.comment.ToSystemString(),
                BpQcName = sampleRecord.bp_qc_identifier.ToSystemString(),
                SampleIdentifier = sampleRecord.sample_identifier.ToSystemString(),
                RetrieveDate = DateTimeConversionHelper.FromSecondUnixToDateTime(sampleRecord.time_stamp),
                NumImageSets = sampleRecord.num_image_sets,
                ImageSetIds = sampleRecord.image_sets.MarshalToUuidArray(sampleRecord.num_image_sets),
                NumOfResultRecord = sampleRecord.num_result_summary,
                ResultSummaryList = sampleRecord.GetSummaryResultList(),
                RegentInfoRecordList = ConvertToReagentInfoDomain(sampleRecord.reagent_info_records, sampleRecord.num_reagent_records),
                ResultRecordsRetriever = retrieveResultRecords,
                SampleCompletionStatus = sampleRecord.sam_comp_status,
                IsSampleCompleted = true,
                Position = sampleRecord.position,
                SubstrateType = sampleRecord.position.GetSubstrateType()
            };

            return srd;
        }

        private static List<ReagentInfoRecord> ConvertToReagentInfoStructure(IntPtr reagentPtr, int reagentCount)
        {
            var regentInfoRecordStr = new List<ReagentInfoRecord>();
            if (reagentPtr == IntPtr.Zero || reagentCount <= 0)
                return regentInfoRecordStr;
            var reagentPointer = reagentPtr;
            for (var i = 0; i < reagentCount; i++)
            {
                regentInfoRecordStr.Add((ReagentInfoRecord)Marshal.PtrToStructure(reagentPointer, typeof(ReagentInfoRecord)));
                reagentPointer = new IntPtr(reagentPointer.ToInt64() + Marshal.SizeOf(typeof(ReagentInfoRecord)));
            }

            return regentInfoRecordStr;
        }

        private static List<ReagentInfoRecordDomain> ConvertToReagentInfoDomain(IntPtr reagentPtr, int reagentCount)
        {
            var regentInfoRecordStructure = ConvertToReagentInfoStructure(reagentPtr, reagentCount);
            var regentInfoRecordInfoList = new List<ReagentInfoRecordDomain>();
            if (regentInfoRecordStructure.Any())
            {
                regentInfoRecordStructure.ForEach(reagent =>
                {
                    regentInfoRecordInfoList.Add(GetReagentDomain(reagent));
                });
            }
            return regentInfoRecordInfoList;
        }

        private static ReagentInfoRecordDomain GetReagentDomain(ReagentInfoRecord reagentStr)
        {
            var reagentInfo = new ReagentInfoRecordDomain()
            {
                PackNumber = reagentStr.pack_number.ToSystemString(),
                LotNumber = (int)reagentStr.lot_number,
                ReagentName = reagentStr.reagent_label.ToSystemString(),
                ExpirationDate = DateTimeConversionHelper.FromDaysUnixToDateTime(reagentStr.expiration_date),
                InServiceDate = DateTimeConversionHelper.FromDaysUnixToDateTime(reagentStr.in_service_date),
                EffectiveExpirationDate = DateTimeConversionHelper.FromDaysUnixToDateTime(reagentStr.effective_expiration_date)
            };
            return reagentInfo;
        }


        public static List<SampleImageRecordDomain> MarshalToSampleImageRecordDomains(this IntPtr sirListPtr, uint count)
        {
            return sirListPtr.MarshalToList<SampleImageSetRecord, SampleImageRecordDomain>(count, MarshalToSampleImageRecordDomain);
        }

        public static DetailedResultMeasurementsDomain MarshalToDetailedResultMeasurementDomain(this IntPtr drmPtr)
        {
            var drm = (DetailedResultMeasurements) Marshal.PtrToStructure(drmPtr, typeof(DetailedResultMeasurements));
            var num_image_sets = drm.num_image_sets;
            return new DetailedResultMeasurementsDomain()
            {
                uuid = drm.uuid,
                BlobsByImage = drm.blobs_by_image
                    .MarshalToList<image_blobs_t, ImageBlobsDomain>(num_image_sets, MarshalToImageBlobsDomain),
                LargeClustersByImage = drm.large_clusters_by_image
                    .MarshalToList<large_cluster_t, LargeClusterDomain>(num_image_sets, MarshalToLargeClusterDomain)
            };
        }

        public static ImageBlobsDomain MarshalToImageBlobsDomain(this IntPtr blobsPtr)
        {
            var blob = (image_blobs_t) Marshal.PtrToStructure(blobsPtr, typeof(image_blobs_t));
            return new ImageBlobsDomain()
            {
                ImageSequenceNumber = blob.image_set_number,
                BlobList = blob.MarshalToBlobMeasurementDomain()
            };
        }

        public static LargeClusterDomain MarshalToLargeClusterDomain(this IntPtr lcPtr)
        {
            var largeCluster = (large_cluster_t) Marshal.PtrToStructure(lcPtr, typeof(large_cluster_t));
            return new LargeClusterDomain()
            {
                ImageSequenceNumber = largeCluster.image_set_number,
                LargeClusterDataList = largeCluster.MarshalToLargeClusterDataList()
            };
        }

        public static uuidDLL[] MarshalToUuidArray(this IntPtr uuidPtr, uint numEntries)
        {
            var records = new uuidDLL[numEntries];
            var iterPtr = uuidPtr;
            var size = Marshal.SizeOf(typeof(uuidDLL));
            for (var i = 0; i < numEntries; i++)
            {
                records[i] = (uuidDLL) Marshal.PtrToStructure(iterPtr, typeof(uuidDLL));
                iterPtr += size;
            }

            return records;
        }

        public static List<ResultRecordDomain> MarshalToResultRecordDomains(this IntPtr rrPtr, uint count)
        {
            return rrPtr.MarshalToList<ResultRecord, ResultRecordDomain>(count, MarshalToResultRecordDomain);
        }

        public static AutofocusResultsDto MarshalToAutofocusResultsDto(this IntPtr resultsPtr)
        {
            var result = new AutofocusResultsDto();
            if (resultsPtr == IntPtr.Zero)
            {
                return result;
            }

            var autoFocusResults = (AutofocusResults) Marshal.PtrToStructure(resultsPtr, typeof(AutofocusResults));
            result.IsFocusSuccessful = autoFocusResults.focus_successful;
            result.NumFocusDatapoints = autoFocusResults.nFocusDatapoints;
            result.BestAutofocusPosition = autoFocusResults.bestfocus_af_position;
            result.BestFocusOffsetMicrons = autoFocusResults.offset_from_bestfocus_um;
            result.FinalAutofocusPosition = autoFocusResults.final_af_position;
            result.Dataset = autoFocusResults.dataset.MarshalToAutofocusDatapoints(result.NumFocusDatapoints);
            result.BestAutofocusImage = autoFocusResults.bestfocus_af_image.MarshalToImageDto();

            return result;
        }

        public static List<AutofocusDatapoint> MarshalToAutofocusDatapoints(this IntPtr datasetPtr, uint count)
        {
            var dataPointList = new List<AutofocusDatapoint>();
            var autoFocusStr = new AutofocusDatapoint();
            var structSize = Marshal.SizeOf(autoFocusStr);
            for (int i = 0; i < count; i++)
            {
                dataPointList.Add((AutofocusDatapoint)Marshal.PtrToStructure(datasetPtr + i * structSize,
                    typeof(AutofocusDatapoint)));
            }

            return dataPointList;
        }

        public static ResultSummaryDomain MarshalToResultSummaryDomain(this IntPtr iterator)
        {
            var resultSummary = (ResultSummary) Marshal.PtrToStructure(iterator, typeof(ResultSummary));
            return resultSummary.MarshalToResultSummaryDomain();
        }

        public static List<ResultSummaryDomain> MarshalToResultSummaryDomainList(
            this IntPtr resultSummariesPtr, uint length)
        {
            var summaryList = new List<ResultSummaryDomain>();
            var size = Marshal.SizeOf(typeof(ResultSummary));
            var iterator = resultSummariesPtr;
            for (int i = 0; i < length; i++)
            {
                summaryList.Add(iterator.MarshalToResultSummaryDomain());
                iterator += size;
            }

            return summaryList;
        }

        public static ResultRecordDomain MarshalToResultRecordDomain(this IntPtr resultRecordPtr)
        {
            var resultRecord = (ResultRecord) Marshal.PtrToStructure(resultRecordPtr, typeof(ResultRecord));
            var result = new ResultRecordDomain()
            {
                ResultSummary = resultRecord.summary_info.MarshalToResultSummaryDomain(),
                ResultPerImage = new List<BasicResultDomain>()
            };
            var basicResultAnswers =
                resultRecord.per_image_result.MarshalToBasicResultAnswerList(resultRecord.num_image_results);
            basicResultAnswers.ForEach(bra => result.ResultPerImage.Add(bra.MarshalToBasicResultDomain()));

            return result;
        }

        public static List<BasicResultAnswers> MarshalToBasicResultAnswerList(this IntPtr resultAnswersPtr, int length)
        {
            var resultAnswers = new List<BasicResultAnswers>();
            var size = Marshal.SizeOf(typeof(BasicResultAnswers));
            var iterator = resultAnswersPtr;
            for (int i = 0; i < length; i++)
            {
                resultAnswers.Add((BasicResultAnswers) Marshal.PtrToStructure(iterator, typeof(BasicResultAnswers)));
                iterator += size;
            }

            return resultAnswers;
        }

        public static List<SignatureInstanceDomain> MarshalToSignatureInstanceDomainList(this IntPtr signatureSetPtr,
            int setLength)
        {
            var list = new List<SignatureInstanceDomain>();
            var size = Marshal.SizeOf(typeof(DataSignatureInstance_t));
            var iterator = signatureSetPtr;
            for (int i = 0; i < setLength; i++)
            {
                var dataSigInstance =
                    (DataSignatureInstance_t) Marshal.PtrToStructure(iterator, typeof(DataSignatureInstance_t));
                iterator += size;

                var sigDomain = new SignatureInstanceDomain()
                {
                    Signature = dataSigInstance.signature.MarshalToSignatureDomain(),
                    SigningUser = dataSigInstance.signing_user.ToSystemString(),
                    SignedDate = DateTimeConversionHelper.FromSecondUnixToDateTime(dataSigInstance.timestamp)
                };

                list.Add(sigDomain);
            }

            return list;
        }

        public static List<histogrambin_t> MarshalToHistogrambins(this IntPtr histbinPtr, uint count)
        {
            var histbinList = new List<histogrambin_t>();
            var ptrIterator = histbinPtr;
            for (int i = 0; i < count; i++)
            {
                histbinList.Add(
                    (histogrambin_t) Marshal.PtrToStructure(ptrIterator, typeof(histogrambin_t)));
                ptrIterator += Marshal.SizeOf(typeof(histogrambin_t));
            }

            return histbinList;
        }

        public static List<SampleEswDomain> MarshalToSampleEswDomainList(this IntPtr sampleDefListPtr, uint count)
        {
            var sampleList = new List<SampleEswDomain>();

            var iterator = sampleDefListPtr;
            for (var i = 0; i < count; i++)
            {
                var sampleDef = (SampleDefinition) Marshal.PtrToStructure(iterator, typeof(SampleDefinition));
                sampleList.Add(GetSampleEswDomain(sampleDef));
                iterator += Marshal.SizeOf(typeof(SampleDefinition));
            }

            return sampleList;
        }

        public static SampleEswDomain MarshalToSampleEswDomain(this IntPtr sampleDefPtr)
        {
            var sampleDef = Marshal.PtrToStructure<SampleDefinition>(sampleDefPtr);
            return GetSampleEswDomain(sampleDef);
        }

        private static SampleEswDomain GetSampleEswDomain(SampleDefinition sampleDef)
        {
            var sample = new SampleEswDomain();

            sample.Uuid = sampleDef.sampleDefUuid;
            sample.SampleDataUuid = sampleDef.sampleDataUuid;
            sample.Dilution = sampleDef.parameters.DilutionFactor;
            sample.Index = sampleDef.index;
            sample.SampleSetIndex = sampleDef.sampleSetIndex;
            sample.SamplePosition = sampleDef.position;
            sample.SampleStatus = sampleDef.status;
            sample.SampleTag = sampleDef.parameters.Tag.ToSystemString();
            sample.TimeStamp = sampleDef.timestamp;
            sample.Username = sampleDef.username.ToSystemString();
            sample.WashType = sampleDef.parameters.WashType;
            sample.CellTypeIndex = sampleDef.parameters.CellTypeIndex;
            sample.SampleName = sampleDef.parameters.Label.ToSystemString();
            sample.SampleSetUid = sampleDef.sampleSetUuid;
            sample.SaveEveryNthImage = sampleDef.parameters.SaveEveryNthImage;

            var qcName = sampleDef.parameters.bp_qc_name.ToSystemString();
            var cellTypeName = AvailableCellTypes
                .FirstOrDefault(ct => ct.CellTypeIndex.Equals(sampleDef.parameters.CellTypeIndex))?.CellTypeName;
            sample.CellTypeQcName = string.IsNullOrEmpty(qcName)
                ? cellTypeName
                : Misc.GetParenthesisQualityControlName(qcName, cellTypeName);

            sample.SubstrateType = sampleDef.position.GetSubstrateType();

            return sample;
        }

        public static List<SampleSetDomain> MarshalToSampleSetDomainList(this IntPtr sampleSetListPtr, uint listCount)
        {
            var sampleList = new List<SampleSetDomain>();

            var setIter = sampleSetListPtr;
            for (var i = 0; i < listCount; i++)
            {
                var sampleSet = (SampleSet) Marshal.PtrToStructure(setIter, typeof(SampleSet));
                sampleList.Add(GetSampleSetDomain(sampleSet, sampleSet.numSamples));
                setIter += Marshal.SizeOf(typeof(SampleSet));
            }

            return sampleList;
        }

        public static SampleSetDomain MarshalToSampleSetDomain(this IntPtr sampleSetPtr)
        {
            var sampleSetStruct = Marshal.PtrToStructure<SampleSet>(sampleSetPtr);
            return GetSampleSetDomain(sampleSetStruct, sampleSetStruct.numSamples);
        }

        private static SampleSetDomain GetSampleSetDomain(SampleSet sampleSetStruct, uint numSamples)
        {
            var sampleSet = new SampleSetDomain();

            sampleSet.Uuid = sampleSetStruct.uuid;
            sampleSet.Index = sampleSetStruct.index;
            sampleSet.Carrier = sampleSetStruct.carrier;
            sampleSet.SampleSetName = sampleSetStruct.name.ToSystemString();
            sampleSet.SampleSetStatus = sampleSetStruct.setStatus;
            sampleSet.Timestamp = sampleSetStruct.timestamp;
            sampleSet.Username = sampleSetStruct.username.ToSystemString();
            sampleSet.PlatePrecession = sampleSetStruct.platePrecession;

            sampleSet.Samples = sampleSetStruct.samples.MarshalToSampleEswDomainList(numSamples);

            return sampleSet;
        }

        public static IntPtr ToIntPtr<TStruct>(List<TStruct> items) where TStruct : struct
        {
            var structSize = Marshal.SizeOf(typeof(TStruct));
            var arrayPtr = Marshal.AllocHGlobal(structSize * items.Count);
            var iterator = arrayPtr;

            foreach (var item in items)
            {
                Marshal.StructureToPtr(item, iterator, false);
                iterator += structSize;
            }

            return arrayPtr;
        }
    }
}
