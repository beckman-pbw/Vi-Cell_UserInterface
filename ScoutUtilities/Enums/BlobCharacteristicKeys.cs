
namespace ScoutUtilities.Enums
{
    public enum BlobCharacteristicKeys
    {
        IsCell = 1, 
        IsPOI = 2, /*!< Flag to check if the cell is population of interest */
        IsBubble = 3, 
        IsDeclustered = 4, 
        IsIrregular = 5, 
        Area = 6, 
        DiameterInPixels = 7, 
        DiameterInMicrons = 8, 
        Circularity = 9, 
        Sharpness = 10, 
        Perimeter = 11, 
        Eccentricity = 12, 
        AspectRatio = 13, 
        MinorAxis = 14, 
        MajorAxis = 15, 
        Volume = 16, 
        Roundness = 17, 
        Intrl_MinEnclosedArea_InPixels = 18, /*!< Minimum circle enclosed area of the blob*/
        Elimination = 19, 
        CellSpotArea = 20, 
        AvgSpotBrightness = 21, 
        RawCellSpotBrightness = 22, 
        FLAvgIntensity = 25, 
        FLCellArea = 26, 
        FLSubPeakCount = 27,
        FLPeakAvgIntensity = 28, 
        FLPeakArea = 29,
        FLPeakLoc_X = 30, 
        FLPeakLoc_Y = 31, 
        FLSubPeakAvgIntensity = 32, 
        FLSubPeakPixelCount = 33, 
        FLSubPeakArea = 34, 
        FLSubPeakLoc_X = 35, 
        FLSubPeakLoc_Y = 36, 
        FLPeakDistance = 40, 
        CFG_FLPeakPercentage = 101, /*!<FL Configuration parameter- FL peak percentage*/
        CFG_FLScalableROI = 102 /*!<FL Configuration parameter- FL Scalable ROI*/
    }

}
