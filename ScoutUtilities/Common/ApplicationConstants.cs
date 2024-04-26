namespace ScoutUtilities.Common
{   
    public static class ApplicationConstants
    {
        public const string BasePathToExportedZip = "\\Instrument\\Export";

        // use a directory name that a customer is unlikely to choose
        public const string ExportTempDir = "\\Instrument\\Export\\xport_temp";
        public const string ExportTempFileName = "Temp_Export";

        public const string MouseOver = "MouseOver";

        public const string PreviewDown = "MouseLeftButtonDown";
        
        public const int StatusBarMaxWidth = 1200;
        
        public const int StatusBarMinWidth = 250;
        
        public const string ImageFileName = "ViCell_BLU";
        
        public const string ImageFileExtension = ".png";

        public const string ImageFilenameForPDF = "ImageForPDF";

        public const string ImageFilenameForHistogram = "ImageForHistogram";

        public const string DefaultSettings = "DefaultSettings";

        public const int NumOfCarouselPositions = 24;
        
        public const int CarouselInactivePositions = 22;

        public const int CarouselBlockWellsLeft = 4;

        public const int CarouselBlockWellsRight = 5;

/*        public const string SilentAdmin = "Vi-CELL";*/
        public const string SilentAdmin = "Cydem";
        
        public const string ServiceUser = "bci_service";
        
        public const string ConcSlope2M = "ConcSlope_2M";
        
        public const string ConcSlope4M = "ConcSlope_4M";
        
        public const string ConcSlope10M = "ConcSlope_10M";

        public const string ReagentNameForAllUser = "Trypan Blue";

        public const string FinalImageStr = "FinalImage";

        public const int Con2MStartRange = 1600000;

        public const int Con2MEndRange = 2400000;

        public const int Con4MStartRange = 3200000;
        
        public const int Con4MEndRange = 4800000;

        public const int Con10MStartRange = 8000000;
        
        public const int Con10MEndRange = 12000000;
     
        public const uint ProbePositionValue = 30000;

        public const int CarouselSampleCount = 24;
        public const int AutomationCupSampleCount = 1;
        public const int PlateSampleCount = 96;
        public const int PlateNumColumnsCount = 12;
        public const int PlateNumRowsCount = 8;
        public const int StartingIndexOfCarousel = 1;

        public const int IndexPoint = 1;

        public const double UpperDiameterLimit = 60.00;

        public const double LowerDiameterLimit = 1.00;

        public const double LowerSharpLimit = 0.000;

        public const double UpperSharpLimit = 100.000;

        public const double LowerCircularityLimit = 0.000;

        public const double UpperCircularityLimit = 1.000;

        public const int LowerAspirationCyclesLimit = 1;

        public const int UpperAspirationCyclesLimit = 10;

        public const double LowerCellTypeSpotBrightnessLimit = 0.00;

        public const double UpperCellTypeSpotBrightnessLimit = 95.00;

        public const double LowerCellTypeSpotAreaLimit = 0.00;

        public const double UpperCellTypeSpotAreaLimit = 95.00;

        public const int LowerMixingCyclesLimit = 1;

        public const int UpperMixingCyclesLimit = 10;

        public const double DefaultAcceptanceValue = 10;

        public const double LowerConcentrationAssayLimit = 1.00;

        public const double UpperConcentrationAssayLimit = 22.00;

        public const double LowerViabilityAssayLimit = 1.0;

        public const double UpperViabilityAssayLimit = 100.0;

        public const float LowerCellTypeAdjustmentFactorValue = -20.0f;
        public const float UpperCellTypeAdjustmentFactorValue = 20.0f;
        public const float DefaultAdjustmentFactorValue = 0.0f;

        public const double LowerConcentrationAssayValueLimit = 0.0;

        public const double UpperConcentrationAssayValueLimit = 99999.0;

        public const int ConcentrationPower = 6;

        public const int CellTypeNameLimit = 50;

        public const int QualityControlLotLimit = 20;

        public const int QualityControlCommentLimit = 100;

        public const int MinimumQualityControlAcceptanceLimit = 1;

        public const int MaximumQualityControlAcceptanceLimit = 100;

        public const int MinimumInactivityTimeoutMins = 1;

        public const int MaximumInactivityTimeoutMins = 60;

        public const int MinimumPasswordExpirationDays = 1;

        public const int MaximumPasswordExpirationDays = 60;

        public const int MinimumNumberOfNthImages = 0;

        public const int MaximumNumberOfNthImages = 99;

        public const int MinimumCelltypeImageCount = 10;

        public const int MaximumCelltypeImageCount = 100;

        public const uint MinimumDilutionFactor = 1;
        public const uint MaximumDilutionFactor = 9999;

        public const int FromYear = 2010;

        public const string CellSpotArea = "Cell Spot Area";

        public const string AvgSpotBrightness = "Average Spot Brightness";
      
        public const string ReportDeviceInfoForLandScape = "<DeviceInfo>" +
                                               "  <OutputFormat>PDF</OutputFormat>" +
                                               "  <PageWidth>29.7cm</PageWidth>" +
                                               "  <PageHeight>21cm</PageHeight>" +
                                               "  <MarginTop>0.5cm</MarginTop>" +
                                               "  <MarginLeft>2cm</MarginLeft>" +
                                               "  <MarginRight>2cm</MarginRight>" +
                                               "  <MarginBottom>0.5cm</MarginBottom>" +
                                               "</DeviceInfo>";

        public const string ReportDeviceInfoForPortrait = "<DeviceInfo>" +
                                               "  <OutputFormat>PDF</OutputFormat>" +
                                               "  <PageWidth>22cm</PageWidth>" +
                                               "  <PageHeight>29.7cm</PageHeight>" +
                                               "  <MarginTop>0.5cm</MarginTop>" +
                                               "  <MarginLeft>2cm</MarginLeft>" +
                                               "  <MarginRight>2cm</MarginRight>" +
                                               "  <MarginBottom>0.5cm</MarginBottom>" +
                                               "</DeviceInfo>";

        public const string TempCellTypeCharacter = "#";

        public const string SelectedSampleCellIndication = "*";

        public const int DelayTimer = 2000;

        public const int SetFocusTimer = 90;

        public const int DustRefTimer = 30;

        public const int ConcentrationStartRange = 50000;

        public const int ConcentrationEndRange = 15000000;

        public const string ForceRestartKey = "SystemRestart";

        public const int MinimumPasswordLength = 10;

        public const int IncorrectPasswordEnteredMax = 5;

        public const string ImageViewRightClickMenuHistogram = "Histogram";
        public const string ImageViewRightClickMenuImageActualSize = "ImageActualSize";
        public const string ImageViewRightClickMenuImageFitSize = "ImageFitSize";

        public const string NumericOne = "1";
        public const string NumericTwo = "2";
        public const string NumericThree = "3";
        public const string NumericFour = "4";
        public const string NumericFive = "5";

        public const int WindowWidth = 1300;
        public const int WindowHeight = 850;

        public const string SummaryExportFileNameAppendant = "Summary_";
        public const string TargetFolderName = "Target";

        public const string CarouselName = "Carousel";
        public const string PlateName = "96Well";
        public const string AutomationCupName = "AutomationCup";
        public const string Row = "ROW";
        public const string Col = "COL";

        public const int SequentialNamingStartingNumber = 1;
        public const int SequentialNamingNumberOfDigits = 3;

        public const int OrphanSampleSetIndex = 0;
        public const int FirstNonOrphanSampleSetIndex = 1;

        public const int AnalysisIndex = 0;
        public const int SaveEveryNthImage = 1;
        public const int NumberOfTubesInConcentration = 18;

        public const string DllName = "HawkeyeCore.dll";
        public const string Justification = "Use HawkeyeError";

        public const int DefaultFilterFromDaysToSubtract = -7;
        public const int DefaultStorageFilterFromDaysToSubtract = -90;

        public const string FactoryAdminUserId = "factory_admin";

        public const string DefaultDbName = "ViCellDB";
        public const string DefaultDbIpAddress = "127.0.0.1";
        public const uint DefaultDbPort = 5432;

        // the entire slide show should last ~ 17.5 seconds for 100 images
        public const int SlideShowImageTimeMs = 175;

        public const char SamplePositionRowChar_Carousel = 'Z';
        public const char SamplePositionRowChar_AutomationCup = 'Y';

        /// <summary>
        ///Encryption key for queue export/import
        /// </summary>
        public const string ExportEncryptKey = "1234567891234567";

        public const string KnownConcentration2M = "2 M";

        public const string KnownConcentration4M = "4 M";

        public const string KnownConcentration10M = "10 M";

        public const double AssayValue2M = 2.00;

        public const double AssayValue4M = 4.00;

        public const double AssayValue10M = 10.00;

        public const int NumberOfTubes2M = 10;

        public const int NumberOfTubes4M = 5;

        public const int NumberOfTubes10M = 3;

        public const int StartPosition2M = 0;

        public const int StartPosition4M = 11;

        public const int StartPosition10M = 15;

        public const int EndPosition2M = 10;

        public const int EndPosition4M = 15;

        public const int EndPosition10M = 18;

		// Used to fill in AnalysisParameterDomain fields that are not passed by OPC-UA.
		public const string CellSpotAreaText = "Cell Spot Area";
		public const ushort CellSpotAreaKey = 20;
		public const string AvgSpotBrightnessText = "Average Spot Brightness";
		public const ushort AvgSpotBrightnessKey = 21;

		public const string CarouselLabel = "Carousel";
		public const string ACupLabel = "A-Cup";
    }
}
