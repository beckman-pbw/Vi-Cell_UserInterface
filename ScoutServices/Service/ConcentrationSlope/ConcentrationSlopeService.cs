using Ninject.Extensions.Logging;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutLanguageResources;
using ScoutModels.Service;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScoutServices.Service.ConcentrationSlope
{
    public class ConcentrationSlopeService : IConcentrationSlopeService
    {
        private readonly ILogger _logger;

        public ConcentrationSlopeService(ILogger logger)
        {
            _logger = logger;
        }

        public IList<LineGraphDomain> GetCalibrationData(CalibrationData totalCells,
            CalibrationData originalConcentration, CalibrationData adjustedConcentration)
        {
            var graphViewList = new List<LineGraphDomain>
            {
                new LineGraphDomain(), new LineGraphDomain(), new LineGraphDomain(), new LineGraphDomain()
            };

            for (var index = 0; index < graphViewList.Count; index++)
            {
                var g = graphViewList[index];
                g.XAxisName = LanguageResourceHelper.Get("LID_GraphLabel_AssayConcentration");
                g.IsExpandableView = false;
                switch (index)
                {
                    case 0:
                        g.GraphDetailList = new ObservableCollection<KeyValuePair<dynamic, double>>();
                        foreach (KeyValuePair<double, double> item in totalCells.Data)
                        {
                            g.GraphDetailList.Add(
                                new KeyValuePair<dynamic, double>(item.Key / Math.Pow(10, 6), item.Value));
                        }

                        g.PrimaryTrendPoints = GetTrendPair(totalCells);

                        SetTrendLabel(g, totalCells, 3);
                        g.GraphName = LanguageResourceHelper.Get("LID_GraphLabel_Totalcells");
                        g.YAxisName = LanguageResourceHelper.Get("LID_GraphLabel_Totalcells");
                        break;
                    case 1:
                        SetPrimaryTrend(originalConcentration, g);
                        SetTrendLabel(g, originalConcentration, 3);
                        g.GraphName = LanguageResourceHelper.Get("LID_GraphLabel_OriginalConcentration");
                        g.YAxisName = LanguageResourceHelper.Get("LID_CheckBox_TotalCellConcentration");
                        break;
                    case 2:
                        SetPrimaryTrend(adjustedConcentration, g);
                        SetTrendLabel(g, adjustedConcentration, 3);
                        g.GraphName = LanguageResourceHelper.Get("LID_Graph_AdjustConc");
                        g.YAxisName = LanguageResourceHelper.Get("LID_CheckBox_TotalCellConcentration");
                        break;
                    case 3:
                        g.GraphName = LanguageResourceHelper.Get("LID_Graph_Original");
                        g.YAxisName = LanguageResourceHelper.Get("LID_CheckBox_TotalCellConcentration");
                        g.IsMultiAxisEnable = true;
                        g.PrimaryLegendName = LanguageResourceHelper.Get("LID_Graph_OrigConc");
                        g.PrimaryTrendLegendName = LanguageResourceHelper.Get("LID_Graph_LinerOrigConc");
                        g.SecondaryLegendName = LanguageResourceHelper.Get("LID_Graph_AdjustConc");
                        g.SecondaryTrendLegendName = LanguageResourceHelper.Get("LID_Graph_LinerAdjustConc");

                        //Primary Axis
                        SetPrimaryTrend(originalConcentration, g);
                        SetTrendLabel(g, originalConcentration, 4);

                        //Secondary Axis
                        SetMultiTrend(adjustedConcentration, g);
                        SetTrendLabel(g, adjustedConcentration, 4);

                        break;
                }
            }

            return graphViewList;
        }

        public double ConvertAssayValueToDouble(ICalibrationConcentrationListDomain calibrationConcentration)
        {
            var assayValue = calibrationConcentration.AssayValue;
            if (calibrationConcentration.AssayValueType == AssayValueEnum.M10)
            {
                if (assayValue == 0)
                {
                    assayValue = 0;
                }
                else if (assayValue >= 2)
                {
                    assayValue *= Math.Pow(10, 6);
                }
                else
                {
                    assayValue *= Math.Pow(10, 7);
                }
            }
            else
            {
                assayValue *= Math.Pow(10, 6);
            }

            return assayValue;
        }

        public HawkeyeError SaveCalibration(IList<ICalibrationConcentrationListDomain> consumables,
            calibration_type calType, uuidDLL workListUuid, double slope, double intercept)
        {
            try
            {
                var calOverTimeList = new List<CalibrationConcentrationOverTimeDomain>();
                foreach (var consumable in consumables)
                {
                    var item = GetCalibrationConcentrationOverTimeDomain(consumable);
                    calOverTimeList.Add(item);
                }

                var numImages = (uint)100;
                var numConsumables = (ushort)calOverTimeList.Count;
                var result = CalibrationModel.SetConcentrationCalibration(calType,
                    slope, intercept, numImages, workListUuid, numConsumables, calOverTimeList);
                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to save A-Cup calibration data. Setting HawkeyeError to 'eInvalidArgs'");
                return HawkeyeError.eInvalidArgs;
            }
        }

        public CalibrationConcentrationOverTimeDomain GetCalibrationConcentrationOverTimeDomain(ICalibrationConcentrationListDomain calibration)
        {
            var cal = new CalibrationConcentrationOverTimeDomain();

            cal.AssayValue = ConvertAssayValueToDouble(calibration);
            cal.ConsumableLabel = calibration.KnownConcentration;
            cal.ConsumableLotId = calibration.Lot;
            cal.Date = calibration.ExpiryDate;

            return cal;
        }

        public IList<CalibrationActivityLogDomain> GetAllCalibrations(calibration_type calibrationType,
            DateTime startTime, DateTime endTime)
        {
            var start = DateTimeConversionHelper.DateTimeToUnixSecondRounded(startTime);
            var end = DateTimeConversionHelper.DateTimeToUnixSecondRounded(endTime);
            CalibrationModel.RetrieveCalibrationActivityLogRange(calibrationType, start, end,
                out var calibrationErrorLog);
            return calibrationErrorLog;
        }

        public IList<CalibrationActivityLogDomain> GetAllCalibrations(calibration_type calibrationType)
        {
            var list = CalibrationModel.RetrieveCalibrationActivityLog(calibrationType);
            return list;
        }

        public CalibrationActivityLogDomain GetMostRecentCalibration(calibration_type calType)
        {
            var list = GetAllCalibrations(calType)
                .OrderByDescending(c => c.Date);
            return list.FirstOrDefault() ?? new CalibrationActivityLogDomain();
        }

        public bool ValidateConcentrationValues(IList<ICalibrationConcentrationListDomain> concentrationList,
            out string localizedErrorMessage)
        {
            foreach (var item in concentrationList)
            {
                var assayValue = item.AssayValue;
                if (assayValue <= 0.0)
                {
                    localizedErrorMessage = LanguageResourceHelper.Get("LID_ERRMSGBOX_AssayValueFieldBlank");
                    return false;
                }

                if (string.IsNullOrEmpty(item.Lot))
                {
                    localizedErrorMessage = LanguageResourceHelper.Get("LID_ERRMSGBOX_LotNumberFieldBlank");
                    return false;
                }

                if (item.ExpiryDate.Date < DateTime.Now.Date)
                {
                    localizedErrorMessage = LanguageResourceHelper.Get("LID_MSGBOX_Calibration_ExpirationDate");
                    return false;
                }

                switch (item.AssayValueType)
                {
                    case AssayValueEnum.M2:
                        if (!item.IsCorrectAssayValue)
                        {
                            localizedErrorMessage = string.Format(LanguageResourceHelper
                                .Get("LID_ERRMSGBOX_TheAssayValueIsNotWithinRange"), Misc.ConvertToString(2));
                            return false;
                        }
                        break;

                    case AssayValueEnum.M4:
                        if (!item.IsCorrectAssayValue)
                        {
                            localizedErrorMessage = string.Format(LanguageResourceHelper
                                .Get("LID_ERRMSGBOX_TheAssayValueIsNotWithinRange"), Misc.ConvertToString(4));
                            return false;
                        }
                        break;

                    case AssayValueEnum.M10:
                        if (!item.IsCorrectAssayValue)
                        {
                            localizedErrorMessage = string.Format(LanguageResourceHelper.Get("LID_ERRMSGBOX_TheAssayValueIsNotWithinRange"), Misc.ConvertToString(10));
                            return false;
                        }
                        break;
                }
            }

            localizedErrorMessage = string.Empty;
            return true;
        }

        public List<ICalibrationConcentrationListDomain> GetStandardConcentrationList()
        {
            var list = CalibrationModel.GetStandardConcentrationList();
            return list;
        }

        public List<CalibrationActivityLogDomain> RetrieveCalibrationActivityLogRange(calibration_type calibrationType,
            ulong startTime, ulong endTime)
        {
            CalibrationModel.RetrieveCalibrationActivityLogRange(calibrationType, startTime, endTime, 
                out var calibrationErrorLog);

            return calibrationErrorLog ?? new List<CalibrationActivityLogDomain>();
        }

        public HawkeyeError ClearCalibrationActivityLog(calibration_type calibrationType, DateTime endTime, string password, bool clearAllACupData)
        {
            var endTimeLong = DateTimeConversionHelper.DateTimeToEndOfDayUnixSecondRounded(endTime);
            var status = CalibrationModel.ClearCalibrationActivityLog(calibrationType, endTimeLong, password, clearAllACupData);
            return status;
        }

        public void UpdateAvgAdjustedValues(IList<DataGridExpanderColumnHeader> columnHeaders)
        {
            if (columnHeaders == null)
                return;
            
            var itemsAverageValues =
                from assay in columnHeaders
                group assay by assay.AssayValue
                into assayGroup
                select new
                {
                    Assayvalue = assayGroup.Key,
                    AverageAdjusted = assayGroup.Average(x => x.Adjusted),
                    AverageAssayValue = assayGroup.Average(x => x.AssayValue)
                };

            var assayValueGroupings = columnHeaders.GroupBy(x => x.AssayValue).ToList();
            foreach (var obj in itemsAverageValues)
            {
                foreach (var assayValueGroup in assayValueGroupings)
                {
                    foreach (var columnHeader in assayValueGroup)
                    {
                        if (!(Math.Abs(assayValueGroup.Key - obj.Assayvalue) <= 0))
                            continue;
                        columnHeader.AvgAdjusted = Misc.UpdateDecimalPoint(obj.AverageAdjusted);
                    }
                }
            }
        }

        public void UpdateAverageOriginalValues(IList<DataGridExpanderColumnHeader> columnHeaders)
        {
            var itemsAverageValues =
                from assay in columnHeaders
                group assay by assay.AssayValue
                into assayGroup
                select new
                {
                    Assayvalue = assayGroup.Key,
                    AverageOriginal = assayGroup.Average(x => x.Original),
                    AverageTotCount = assayGroup.Average(x => x.TotCount),
                    AverageAssayValue = assayGroup.Average(x => x.AssayValue)
                };

            var assayValueGroupings = columnHeaders.GroupBy(x => x.AssayValue).ToList();

            foreach (var obj in itemsAverageValues)
            {
                foreach (var assayValueGroup in assayValueGroupings)
                {
                    foreach (var columnHeader in assayValueGroup)
                    {
                        if (!(Math.Abs(assayValueGroup.Key - obj.Assayvalue) <= 0))
                            continue;
                        columnHeader.AvgTotCount = Misc.UpdateDecimalPoint(obj.AverageTotCount, null);
                        columnHeader.AvgOriginal = Misc.UpdateDecimalPoint(obj.AverageOriginal);
                    }
                }
            }
        }

        #region Private Helper Methods

        private string GetStraightLineEquation(double slope)
        {
            var localSlope = double.IsNaN(slope) ? 0 : slope;
            var equation = "y = " + decimal.Round((decimal)localSlope, 4) + "x";
            return equation;
        }

        private ObservableCollection<KeyValuePair<dynamic, double>> GetTrendPair(CalibrationData data)
        {
            var trendPair = new ObservableCollection<KeyValuePair<dynamic, double>>();

            trendPair.Add(new KeyValuePair<dynamic, double>(data.Intercept / Math.Pow(10, 6), 0.0));
            var largestPair = trendPair.Max(item => item.Key);

            // TODO PC3527-3219 validate the adjustments here. The trend line should look reasonable.
            trendPair.Add(new KeyValuePair<dynamic, double>(
                largestPair / Math.Pow(10, 6),
                // *divide* the assay value by the slope to determine the expected cell count
                largestPair / data.Slope));
            return trendPair;
        }

        private void SetTrendLabel(LineGraphDomain graph, CalibrationData value, int decimalPoint)
        {
            decimal decR2 = double.IsNaN(value.R2) ? 0 : decimal.Round((decimal)value.R2, decimalPoint);
            graph.PrimaryTrendLabel = GetStraightLineEquation(value.Slope) + "\n" +
                                      LanguageResourceHelper.Get("LID_GridLabel_R2") +
                                      "\u00b2" + " = " + decR2;
        }

        private void SetPrimaryTrend(CalibrationData data, LineGraphDomain graph)
        {
            graph.GraphDetailList = new ObservableCollection<KeyValuePair<dynamic, double>>();
            foreach (var pair in data.Data)
            {
                var k = new KeyValuePair<dynamic, double>(pair.Key / Math.Pow(10, 6), GetConcentrationPoint(pair.Value));
                graph.GraphDetailList.Add(k);
            }

            graph.PrimaryTrendPoints = GetTrendPair(data);

        }

        private double GetConcentrationPoint(double graphValue)
        {
            return Misc.UpdateDecimalPoint(Misc.ConvertToPowerSix(graphValue));
        }

        private void SetMultiTrend(CalibrationData data, LineGraphDomain graph)
        {
            graph.MultiGraphDetailList = new ObservableCollection<KeyValuePair<dynamic, double>>();
            foreach (var item in data.Data)
            {
                var k = new KeyValuePair<dynamic, double>(item.Key / Math.Pow(10, 6), GetConcentrationPoint(item.Value));
                graph.MultiGraphDetailList.Add(k);
            }

            graph.SecondaryTrendPoints = GetTrendPair(data);
        }

        #endregion
    }
}