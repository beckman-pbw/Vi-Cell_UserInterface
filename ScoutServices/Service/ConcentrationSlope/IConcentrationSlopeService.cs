using System;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutUtilities.Helper;
using System.Collections.Generic;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ScoutServices.Service.ConcentrationSlope
{
    public interface IConcentrationSlopeService
    {
        IList<LineGraphDomain> GetCalibrationData(CalibrationData totalCells,
            CalibrationData originalConcentration, CalibrationData adjustedConcentration);

        /// <summary>
        /// Gets the double for the AssayValueEnum within "calibration".
        /// </summary>
        /// <param name="calibration"></param>
        /// <returns></returns>
        double ConvertAssayValueToDouble(ICalibrationConcentrationListDomain calibration);

        /// <summary>
        /// Saves the calibration data to HawkeyeCore.dll database.
        /// </summary>
        /// <param name="consumables"></param>
        /// <param name="calType"></param>
        /// <param name="workListUuid"></param>
        /// <param name="slope"></param>
        /// <param name="intercept"></param>
        /// <returns>The backend HawkeyeError code for the save. Returns HawkeyeError.eInvalidArgs if an exception is thrown</returns>
        HawkeyeError SaveCalibration(IList<ICalibrationConcentrationListDomain> consumables,
            calibration_type calType, uuidDLL workListUuid, double slope, double intercept);

        /// <summary>
        /// Creates a CalibrationConcentrationOverTimeDomain from a ICalibrationConcentrationListDomain.
        /// </summary>
        /// <param name="calibration"></param>
        /// <returns></returns>
        CalibrationConcentrationOverTimeDomain GetCalibrationConcentrationOverTimeDomain(
            ICalibrationConcentrationListDomain calibration);

        /// <summary>
        /// Gets all previously run calibrations for the given filter criteria.
        /// </summary>
        /// <returns></returns>
        IList<CalibrationActivityLogDomain> GetAllCalibrations(calibration_type calibrationType,
            DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets all previously run calibrations for the given calibration type.
        /// </summary>
        /// <returns></returns>
        IList<CalibrationActivityLogDomain> GetAllCalibrations(calibration_type calibrationType);

        /// <summary>
        /// Gets the most recent calibration for the given calibrationType.
        /// </summary>
        /// <returns></returns>
        CalibrationActivityLogDomain GetMostRecentCalibration(calibration_type calibrationType);

        /// <summary>
        /// Checks the concentrationList for validity.
        /// </summary>
        /// <param name="concentrationList"></param>
        /// <param name="localizedErrorMessage"></param>
        /// <returns>true if concentrationList is valid. localizedErrorMessage is either empty or contains the error message for what is invalid.</returns>
        bool ValidateConcentrationValues(IList<ICalibrationConcentrationListDomain> concentrationList,
            out string localizedErrorMessage);

        /// <summary>
        /// Gets the 3 concentration sample definitions to use with calibrations.
        /// </summary>
        /// <returns></returns>
        List<ICalibrationConcentrationListDomain> GetStandardConcentrationList();

        /// <summary>
        /// Retrieve's the calibration results for the parameters given.
        /// </summary>
        /// <param name="calibrationType"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<CalibrationActivityLogDomain> RetrieveCalibrationActivityLogRange(calibration_type calibrationType,
            ulong startTime, ulong endTime);

        /// <summary>
        /// Calls the backend ClearCalibrationActivityLog method.
        /// </summary>
        /// <param name="calibrationType"></param>
        /// <param name="endTime"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        HawkeyeError ClearCalibrationActivityLog(calibration_type calibrationType, DateTime endTime, string password, bool clearAllACupData);

        /// <summary>
        /// Goes through the columnHeaders and updates the Average Adjustment values for each of
        /// consumable types.
        /// </summary>
        /// <param name="columnHeaders"></param>
        void UpdateAvgAdjustedValues(IList<DataGridExpanderColumnHeader> columnHeaders);

        /// <summary>
        /// Goes through the columnHeaders and updates the AvgTotCount and AvgOriginal values
        /// for each of consumable types.
        /// </summary>
        /// <param name="columnHeaders"></param>
        void UpdateAverageOriginalValues(IList<DataGridExpanderColumnHeader> columnHeaders);
    }
}