using AutoMapper;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using GrpcServer.EventProcessors;
using GrpcService;
using Ninject;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutServices.Enums;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using ScoutUtilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using GrpcServer.GrpcInterceptor;
using SubstrateType = ScoutUtilities.Enums.SubstrateType;
using Precession = GrpcService.PrecessionEnum;

namespace GrpcServer
{
    public class OpcUaGrpcModule : NinjectModule
    {
        private readonly bool _onlyMapper;

        public OpcUaGrpcModule(bool onlyMapper = false)
        {
            _onlyMapper = onlyMapper;
        }

        public override void Load()
        {
            // For AutoMapper
            var mapperConfiguration = CreateConfiguration();
            Bind<MapperConfiguration>().ToConstant(mapperConfiguration).InSingletonScope();

            // This teaches Ninject how to create AutoMapper instances say if for instance
            // MyResolver has a constructor with a parameter that needs to be injected
            Bind<IMapper>().ToMethod(ctx =>
                new Mapper(mapperConfiguration, type => ctx.Kernel.Get(type)));

            if (_onlyMapper)
            {
                return;
            }

            Bind<GrpcServices.GrpcServicesBase>().To<ScoutOpcUaGrpcService>().InSingletonScope();
            Bind<OpcUaGrpcServer>().ToSelf().InSingletonScope();
            Bind<ScoutInterceptor>().ToSelf().InSingletonScope();
            Bind<LockResultProcessor>().ToSelf().InTransientScope();
            Bind<ViCellStatusResultProcessor>().ToSelf().InTransientScope();
            Bind<ViCellIdentifierResultProcessor>().ToSelf().InTransientScope();
            Bind<SampleProcessingSampleStatusEventProcessor>().ToSelf().InTransientScope();
            Bind<SampleProcessingWorkListCompleteEventProcessor>().ToSelf().InTransientScope();
            Bind<ReagentUseRemainingResultProcessor>().ToSelf().InTransientScope();
            Bind<WasteTubeCapacityResultProcessor>().ToSelf().InTransientScope();
            
            Bind<PrimeReagentsStatusEventProcessor>().ToSelf().InTransientScope();
            Bind<PurgeReagentsStatusEventProcessor>().ToSelf().InTransientScope();
            Bind<CleanFluidicsStatusEventProcessor>().ToSelf().InTransientScope();
            Bind<DecontaminateStatusEventProcessor>().ToSelf().InTransientScope();
            Bind<SoftwareVersionEventProcessor>().ToSelf().InTransientScope();
	        Bind<ErrorStatusEventProcessor>().ToSelf().InTransientScope();

            Bind<GrpcClient>().ToSelf().InSingletonScope();

            Bind<DeleteSampleResultsEventProcessor>().ToSelf().InTransientScope();
            Bind<IOpcUaGrpcFactory>().ToFactory();
        }

        private MapperConfiguration CreateConfiguration()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                // Add all profiles for each gRPC event
                cfg.CreateMap<string, string>().ConvertUsing(s => s ?? string.Empty);

                cfg.CreateMap<GrpcService.ErrorStatusType, ScoutUtilities.Structs.ErrorStatusType>().ReverseMap();

                cfg.CreateMap<byte, ByteString>().ConvertUsing(source =>
                    ByteString.CopyFrom(source));
                cfg.CreateMap<ByteString, byte>().ConvertUsing(source =>
                    source.ToByteArray().FirstOrDefault());
                cfg.CreateMap<string, Guid>().ConvertUsing(s =>
                    string.IsNullOrEmpty(s) ? Guid.Empty : Guid.Parse(s));
                cfg.CreateMap<Guid, string>().ConvertUsing(g =>
                    g == null ? Guid.Empty.ToString() : g.ToString().ToUpper());
                cfg.CreateMap<string, uuidDLL>().ConvertUsing(s =>
                    string.IsNullOrEmpty(s) ? new uuidDLL(Guid.Empty) : new uuidDLL(Guid.Parse(s)));
                cfg.CreateMap<uuidDLL, string>().ConvertUsing(u => 
                    u.IsNullOrEmpty() ? Guid.Empty.ToString() : u.ToString());
                cfg.CreateMap<Guid, uuidDLL>().ConvertUsing(g => new uuidDLL(g));
                cfg.CreateMap<uuidDLL, Guid>().ConvertUsing(u => u.ToGuid());
                cfg.CreateMap<DateTime, Timestamp>().ConvertUsing(d =>
                    d != null ? DateTime.SpecifyKind(d, DateTimeKind.Utc).ToTimestamp() : null);
                cfg.CreateMap<Timestamp, DateTime>().ConvertUsing(t =>
                    t != null ? DateTime.SpecifyKind(t.ToDateTime(), DateTimeKind.Local) : DateTime.MinValue);

                cfg.CreateMap<ConfigStateEnum, ConfigState>().ReverseMap();
                cfg.CreateMap<LockStateEnum, LockResult>().ReverseMap();
                cfg.CreateMap<DeclusterDegreeEnum, eCellDeclusterSetting>().ReverseMap();
                cfg.CreateMap<GrpcService.SampleStatusEnum, ScoutUtilities.Enums.SampleStatus>().ReverseMap();
                cfg.CreateMap<Precession, ScoutUtilities.Enums.Precession>().ReverseMap();

                MapSampleStatusToDeleteStatus(cfg);

                cfg.CreateMap<RepeatedField<string>, List<uuidDLL>>().ConvertUsing(
                    new RepeatedFieldToListTypeConverter<string, uuidDLL>());

                cfg.CreateMap<List<uuidDLL>, RepeatedField<string>>().ConvertUsing(
                    new ListToRepeatedFieldTypeConverter<uuidDLL, string>());

                cfg.CreateMap<GrpcService.SampleRecordDomain, ScoutDomains.SampleRecordDomain>()
                   .ForMember(dest => dest.UUID, opt =>
                       opt.MapFrom(src => src.UuidDll));

                cfg.CreateMap<BitmapSource, ByteString>().ConvertUsing(source =>
                    ByteString.CopyFrom(ConvertBitmapSourceToByteArray(source)));
                cfg.CreateMap<ImageDto, GrpcService.Image>(); // fails if ImageSource is not set and initialized
                cfg.CreateMap<FilterOnEnum, eFilterItem>().ReverseMap();

                MapSamplePosition(cfg);
                MapCellTypeDomainToCellType(cfg);
                MapQualityControlDomainToQualityControl(cfg);
                MapQualityControlToQualityControlDomain(cfg);
                MapSampleRecordDomainToSampleConfig(cfg);
                MapSampleConfigToSampleEswDomain(cfg);
                MapSampleEswDomainToSampleResult(cfg);
                MapSampleEswDomainToSampleConfig(cfg);
                MapCellTypeToCellTypeDomain(cfg);
                MapSampleSetConfigToSampleSetDomain(cfg);
                MapSampleEswDomainToSampleStatusData(cfg);
            });

            return mapperConfig;
        }

        #region Complicated AutoMapper Maps



        private static void MapSampleStatusToDeleteStatus(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<SampleProgressEventArgs, DeleteSampleResultsArgs>()
                .ForMember(dest => dest.DeleteStatus, opt => opt.MapFrom(src => (src.PercentComplete == 100) ? DeleteStatusEnum.DsDone : DeleteStatusEnum.DsDeleting))
                .ForMember(dest => dest.PercentComplete, opt => opt.MapFrom(src => src.PercentComplete));
        }

        private static void MapCellTypeToCellTypeDomain(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<GrpcService.CellType, CellTypeDomain>()
                .ForMember(dest => dest.TempCellName, opt => opt.MapFrom(src => src.CellTypeName))
                .ForMember(dest => dest.MinimumDiameter, opt => opt.MapFrom(src => src.MinDiameter))
                .ForMember(dest => dest.MaximumDiameter, opt => opt.MapFrom(src => src.MaxDiameter))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.NumImages))
                .ForMember(dest => dest.MinimumCircularity, opt => opt.MapFrom(src => src.MinCircularity))
                .ForMember(dest => dest.AspirationCycles, opt => opt.MapFrom(src => src.NumAspirationCycles))
                .ForMember(dest => dest.CalculationAdjustmentFactor, opt => opt.MapFrom(src => src.ConcentrationAdjustmentFactor))
                .AfterMap((src, dest) => dest.AnalysisDomain = new AnalysisDomain())
                .AfterMap((src, dest) => dest.AnalysisDomain.AnalysisParameter.Add(new ScoutDomains.AnalysisParameterDomain { ThresholdValue = src.ViableSpotArea }))
                .AfterMap((src, dest) => dest.AnalysisDomain.AnalysisParameter.Add(new ScoutDomains.AnalysisParameterDomain { ThresholdValue = src.ViableSpotBrightness }))
                .AfterMap((src, dest) => dest.AnalysisDomain.MixingCycle = src.NumMixingCycles);
        }

        private static void MapSamplePosition(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ScoutUtilities.Common.SamplePosition, GrpcService.SamplePosition>()
               .ForMember(dest => dest.Row, opt =>
                   opt.MapFrom(src => !src.IsAutomationCup() 
                        ? src.IsCarousel() 
                            ? ApplicationConstants.CarouselLabel
                            : src.Row.ToString().ToUpper()
                        : ApplicationConstants.ACupLabel))
               .ForMember(dest => dest.Column, opt =>
                   opt.MapFrom(src => (uint) src.Column));

            cfg.CreateMap<GrpcService.SamplePosition, ScoutUtilities.Common.SamplePosition>()
               .ForMember(dest => dest.Row, opt =>
                   opt.MapFrom(src => string.IsNullOrEmpty(src.Row)
                       ? default(char)
                       : char.Parse(src.Row.ToUpper())))
               .ForMember(dest => dest.Column, opt =>
                   opt.MapFrom(src => (byte) src.Column));
        }

        private static void MapCellTypeDomainToCellType(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<CellTypeDomain, GrpcService.CellType>()
                .ForMember(dest => dest.CellTypeName, opt =>
                    opt.MapFrom(src => string.IsNullOrEmpty(src.CellTypeName)
                        ? string.Empty
                        : src.CellTypeName))
                .ForMember(dest => dest.MinDiameter, opt =>
                    opt.MapFrom(src => src.MinimumDiameter ?? 0d))
                .ForMember(dest => dest.MaxDiameter, opt =>
                    opt.MapFrom(src => src.MaximumDiameter ?? 0d))
                .ForMember(dest => dest.MinCircularity, opt =>
                    opt.MapFrom(src => src.MinimumCircularity ?? 0d))
                .ForMember(dest => dest.NumImages, opt =>
                    opt.MapFrom(src => src.Images ?? 0))
                .ForMember(dest => dest.ConcentrationAdjustmentFactor, opt =>
                    opt.MapFrom(src => src.CalculationAdjustmentFactor ?? 0f))
                .ForMember(dest => dest.NumAspirationCycles, opt =>
                    opt.MapFrom(src => src.AspirationCycles))
                .ForMember(dest => dest.ViableSpotArea, opt =>
                    opt.MapFrom(src => src.AnalysisDomain.AnalysisParameter[0].ThresholdValue ?? 0f))
                .ForMember(dest => dest.ViableSpotBrightness, opt =>
                    opt.MapFrom(src => src.AnalysisDomain.AnalysisParameter[1].ThresholdValue ?? 0f))
                .ForMember(dest => dest.NumMixingCycles, opt =>
                    opt.MapFrom(src => src.AnalysisDomain.MixingCycle));
        }

        private static void MapQualityControlDomainToQualityControl(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<QualityControlDomain, QualityControl>()
               .ForMember(dest => dest.AcceptanceLimits, opt =>
                   opt.MapFrom(src => src.AcceptanceLimit ?? 0))
               .ForMember(dest => dest.Comments, opt =>
                   opt.MapFrom(src => src.CommentText))
               .ForMember(dest => dest.LotNumber, opt =>
                   opt.MapFrom(src => src.LotInformation))
               .ForMember(dest => dest.QualityControlName, opt =>
                   opt.MapFrom(src => src.QcName));
        }

        private static void MapQualityControlToQualityControlDomain(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<QualityControl, QualityControlDomain>()
               .ForMember(dest => dest.AcceptanceLimit, opt =>
                   opt.MapFrom(src => src.AcceptanceLimits))
               .ForMember(dest => dest.AcceptanceLimitString, opt =>
                   opt.MapFrom(src => src.AcceptanceLimits.ToString()))
               .ForMember(dest => dest.QcName, opt =>
                   opt.MapFrom(src => src.QualityControlName))
               .ForMember(dest => dest.CommentText, opt =>
                   opt.MapFrom(src => src.Comments))
               .ForMember(dest => dest.AssayValueString, opt =>
                   opt.MapFrom(src => src.AssayValue.ToString()))
               .ForMember(dest => dest.LotInformation, opt =>
                   opt.MapFrom(src => src.LotNumber));
        }

        private static void MapSampleRecordDomainToSampleConfig(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ScoutDomains.SampleRecordDomain, SampleConfig>()
               .ForMember(dest => dest.CellType, opt =>
                   opt.MapFrom(src => src.SelectedResultSummary.CellTypeDomain))
               .ForMember(dest => dest.SamplePosition, opt =>
                   opt.MapFrom(src => src.Position));
        }

        private static void MapSampleEswDomainToSampleResult(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<SampleEswDomain, SampleResult>()
                .ForMember(dest => dest.SampleDataUuid, opt => opt.MapFrom(src => src.SampleDataUuid.ToString()))
                .ForMember(dest => dest.SampleId, opt => opt.MapFrom(src => src.SampleRecord.SampleIdentifier))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Map_sample_completion_statusToSampleStatus(src.SampleRecord.SampleCompletionStatus)))
                .ForMember(dest => dest.CellCount, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CumulativeResult.TotalCells))
                .ForMember(dest => dest.ViableCells, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CumulativeResult.ViableCells))
                .ForMember(dest => dest.TotalCellsPerMilliliter, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CumulativeResult.ConcentrationML))
                .ForMember(dest => dest.ViableCellsPerMilliliter, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CumulativeResult.ViableConcentration))
                .ForMember(dest => dest.ViabilityPercent, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CumulativeResult.Viability))
                .ForMember(dest => dest.AverageDiameter, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CumulativeResult.Size))
                .ForMember(dest => dest.AverageViableDiameter, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CumulativeResult.ViableSize))
                .ForMember(dest => dest.AverageCircularity, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CumulativeResult.Circularity))
                .ForMember(dest => dest.AverageViableCircularity, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CumulativeResult.ViableCircularity))
                .ForMember(dest => dest.AverageCellsPerImage, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CumulativeResult.AverageCellsPerImage))
                .ForMember(dest => dest.AverageBackgroundIntensity, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CumulativeResult.AvgBackground))
                .ForMember(dest => dest.BubbleCount, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CumulativeResult.Bubble))
                .ForMember(dest => dest.ClusterCount, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CumulativeResult.ClusterCount))
                .ForMember(dest => dest.QcStatus, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.QCStatus))
                .ForMember(dest => dest.ImagesForAnalysis, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CumulativeResult.TotalCumulativeImage))
                .ForMember(dest => dest.QualityControlName, opt => opt.MapFrom(src => src.SampleRecord.BpQcName))
                .ForMember(dest => dest.CellType, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CellTypeDomain.CellTypeName))
                .ForMember(dest => dest.MinimumDiameter, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CellTypeDomain.MinimumDiameter))
                .ForMember(dest => dest.MaximumDiameter, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CellTypeDomain.MaximumDiameter))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CellTypeDomain.Images))
                .ForMember(dest => dest.CellSharpness, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CellTypeDomain.CellSharpness))
                .ForMember(dest => dest.MinimumCircularity, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CellTypeDomain.MinimumCircularity))
                .ForMember(dest => dest.DeclusterDegree, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CellTypeDomain.DeclusterDegree))
                .ForMember(dest => dest.AspirationCycles, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CellTypeDomain.AspirationCycles))
                .ForMember(dest => dest.ViableSpotBrightness, opt => opt.MapFrom(
                src => null != src.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain && src.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain.AnalysisParameter.Count >= 2
                    ? src.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain.AnalysisParameter[1].ThresholdValue
                    : 0
                    ))
                .ForMember(dest => dest.ViableSpotArea, opt => opt.MapFrom(
                src => null != src.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain && src.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain.AnalysisParameter.Count >= 1
                    ? src.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain.AnalysisParameter[0].ThresholdValue
                    : 0
                    ))
            .ForMember(dest => dest.MixingCycles, opt => opt.MapFrom(src =>
                null != src.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain ? src.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain.MixingCycle : 0))
                .ForMember(dest => dest.AnalysisDateTime, opt => opt.MapFrom(src =>
                        src.SampleRecord.ResultSummaryList.Any()
                        ? src.SampleRecord.ResultSummaryList.FirstOrDefault().RetrieveDate
                        : DateTime.MinValue
                        ))
                .ForMember(dest => dest.ConcentrationAdjustmentFactor, opt => opt.MapFrom(src => src.SampleRecord.SelectedResultSummary.CellTypeDomain.CalculationAdjustmentFactor))
                .ForMember(dest => dest.ReanalysisDateTime, opt => opt.MapFrom(src =>
                        src.SampleRecord.ResultSummaryList.Count > 1 && !src.SampleRecord.SelectedResultSummary.UUID.IsEmpty()
                        ? !src.SampleRecord.SelectedResultSummary.UUID.Equals(src.SampleRecord.ResultSummaryList.First().UUID)
                            ? src.SampleRecord.SelectedResultSummary.RetrieveDate
                            : DateTime.MinValue
                        : DateTime.MinValue
                    ))
                .ForMember(dest => dest.AnalysisBy, opt => opt.MapFrom(src =>
                        src.SampleRecord.ResultSummaryList.Any()
                       ? src.SampleRecord.ResultSummaryList.FirstOrDefault().UserId
                       : String.Empty
                   ))
                .ForMember(dest => dest.ReanalysisBy, opt => opt.MapFrom(src =>
                        src.SampleRecord.ResultSummaryList.Count > 1 && !src.SampleRecord.SelectedResultSummary.UUID.IsEmpty()
                        ? !src.SampleRecord.SelectedResultSummary.UUID.Equals(src.SampleRecord.ResultSummaryList.First().UUID)
                            ? src.SampleRecord.SelectedResultSummary.UserId
                            : String.Empty
                        : String.Empty
                    ))
                .ForMember(dest => dest.Dilution, opt => opt.MapFrom(src => int.Parse(src.SampleRecord.DilutionName)))
                .ForMember(dest => dest.WashType, opt => opt.MapFrom(src => src.SampleRecord.WashName))
                .ForMember(dest => dest.Tag, opt => opt.MapFrom(src => src.SampleRecord.Tag))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.SamplePosition));
        }

        private static SampleStatus Map_sample_completion_statusToSampleStatus(sample_completion_status completionStatus)
        {
            switch (completionStatus)
            {
                case sample_completion_status.sample_completed: return SampleStatus.Completed;
                case sample_completion_status.sample_errored: return SampleStatus.SkipError;
                case sample_completion_status.sample_not_run: return SampleStatus.NotProcessed;
                case sample_completion_status.sample_skipped: return SampleStatus.SkipManual;
                default: return SampleStatus.NotProcessed;
            }
        }

        private static void MapSampleEswDomainToSampleStatusData(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<SampleEswDomain, SampleStatusData>()
                .ForMember(dest => dest.SampleId, opt => opt.MapFrom(src => src.SampleName))
                .ForMember(dest => dest.AnalysisBy, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.SampleStatus, opt => opt.MapFrom(src => src.SampleStatus))
                .ForMember(dest => dest.SamplePosition, opt => opt.MapFrom(src => src.SamplePosition))
                .ForMember(dest => dest.SampleDataUuid, opt => opt.MapFrom(src => src.SampleDataUuid.ToString()));
        }

        private static void MapgRPCErrorStatusTypeToErrorStatusType(IMapperConfigurationExpression cfg)
        {
	        cfg.CreateMap<ScoutUtilities.Structs.ErrorStatusType, GrpcService.ErrorStatusType>()
		        .ForMember(dest => dest.ErrorCode, opt => opt.MapFrom(src => src.ErrorCode))
		        .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity))
		        .ForMember(dest => dest.System, opt => opt.MapFrom(src => src.System))
		        .ForMember(dest => dest.SubSystem, opt => opt.MapFrom(src => src.SubSystem))
		        .ForMember(dest => dest.Instance, opt => opt.MapFrom(src => src.Instance))
		        .ForMember(dest => dest.FailureMode, opt => opt.MapFrom(src => src.FailureMode)).ReverseMap();
        }

        private static void MapSampleEswDomainToSampleConfig(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<SampleEswDomain, SampleConfig>()
               .ForMember(dest => dest.SampleUuid, opt =>
                   opt.MapFrom(src => src.Uuid.ToString()))
               .ForMember(dest => dest.Tag, opt =>
                   opt.MapFrom(src => string.IsNullOrEmpty(src.SampleTag)
                       ? string.Empty
                       : src.SampleTag))
               .ForMember(dest => dest.SampleName, opt =>
                   opt.MapFrom(src => string.IsNullOrEmpty(src.SampleName)
                       ? string.Empty
                       : src.SampleName))
               .ForMember(dest => dest.CellType, opt =>
                   opt.MapFrom(src =>
                       src.IsQualityControl
                           ? new GrpcService.CellType
                           {
                               CellTypeName = string.Empty,
                           }
                           : new GrpcService.CellType
                           {
                               CellTypeName = //src.CellTypeQcName
                                   string.IsNullOrEmpty(src.CellTypeQcName)
                                       ? string.Empty
                                       : src.CellTypeQcName,
                           }))
               .ForMember(dest => dest.QualityControl, opt =>
                   opt.MapFrom(src =>
                       src.IsQualityControl
                           ? new QualityControl
                           {
                               CellTypeName = string.Empty, // Should not use since it's a QC
                               QualityControlName = //src.CellTypeQcName
                                   string.IsNullOrEmpty(src.CellTypeQcName)
                                       ? string.Empty
                                       : src.CellTypeQcName
                           }
                           : null));
        }

        private static void MapSampleConfigToSampleEswDomain(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<SampleConfig, SampleEswDomain>()
               .ForMember(dest => dest.SamplePosition, opt =>
                   opt.MapFrom(src => src.SamplePosition))
               .ForMember(dest => dest.Uuid, opt =>
                   opt.MapFrom(src => src.SampleUuid))
               .ForMember(dest => dest.SampleTag, opt =>
                   opt.MapFrom(src => src.Tag))

               .ForMember(dest => dest.SubstrateType, opt =>
                   opt.MapFrom(src =>
                       src.SamplePosition == null || string.IsNullOrEmpty(src.SamplePosition.Row)
                           ? SubstrateType.NoType
                           : new ScoutUtilities.Common.SamplePosition(
                               char.Parse(src.SamplePosition.Row.ToUpper()),
                               src.SamplePosition.Column).GetSubstrateType()))

               .ForMember(dest => dest.CellTypeQcName, opt =>
                   opt.MapFrom(src =>
                       src.QualityControl != null && !string.IsNullOrEmpty(src.QualityControl.QualityControlName)
                           ? src.QualityControl.QualityControlName
                           : src.CellType != null 
                               ? src.CellType.CellTypeName
                               : string.Empty))

               .ForMember(dest => dest.IsQualityControl, opt =>
                   opt.MapFrom(src => src.QualityControl != null && !string.IsNullOrEmpty(src.QualityControl.QualityControlName)));
        }

        private static void MapSampleSetConfigToSampleSetDomain(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<SampleSetConfig, SampleSetDomain>()
                .ForMember(dest => dest.Uuid, opt =>
                   opt.MapFrom(src => src.SampleSetUuid))
                .ForMember(dest => dest.PlatePrecession, opt =>
                    opt.MapFrom(src => src.PlatePrecession))
               .ForMember(dest => dest.Samples, opt =>
                   opt.MapFrom(src => src.Samples))
                .AfterMap((src, dest) =>
                {
                    foreach (var sample in dest.Samples)
                    {
                        sample.PlatePrecession = dest.PlatePrecession;
                    }
                })
                .AfterMap((grpc, domain) => domain.Carrier = SubstrateType.Plate96);
        }

        #endregion

        #region Helper Methods

        private byte[] ConvertBitmapSourceToByteArray(BitmapSource bitmapSource)
        {
            var stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel) / 8;
            var pixels = new byte[bitmapSource.PixelHeight * stride];
            bitmapSource.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        #endregion
    }
    
    public class RepeatedFieldToListTypeConverter<TITemSource, TITemDest> 
        : ITypeConverter<RepeatedField<TITemSource>, List<TITemDest>>
    {
        public List<TITemDest> Convert(RepeatedField<TITemSource> source, List<TITemDest> destination, ResolutionContext context)
        {
            destination = destination ?? new List<TITemDest>();
            foreach (var item in source)
            {
                destination.Add(context.Mapper.Map<TITemDest>(item));
            }

            return destination;
        }
    }

    public class ListToRepeatedFieldTypeConverter<TITemSource, TITemDest> 
        : ITypeConverter<List<TITemSource>, RepeatedField<TITemDest>>
    {
        public RepeatedField<TITemDest> Convert(List<TITemSource> source, RepeatedField<TITemDest> destination, ResolutionContext context)
        {
            destination = destination ?? new RepeatedField<TITemDest>();
            foreach (var item in source)
            {
                destination.Add(context.Mapper.Map<TITemDest>(item));
            }
            return destination;
        }
    }
}