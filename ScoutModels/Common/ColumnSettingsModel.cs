using HawkeyeCoreAPI;
using ScoutLanguageResources;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScoutModels.Common
{
    public class ColumnSettingsModel
    {
        public static Dictionary<string, Tuple<string, bool, double>> GetAll(string username)
        {
            var list = new Dictionary<string, Tuple<string, bool, double>>();
            var colSettings = ColumnSettingsApi.GetSampleColumnsApi(username) ?? new List<ColumnSetting>();

            var allColOptions = Enum.GetValues(typeof(ColumnOption)).Cast<ColumnOption>().ToList();
            if (colSettings.Count < allColOptions.Count)
            {
                // The backend is missing some settings. Save the missing ones with their default value:
                var listToSave = new List<ColumnSetting>();
                foreach (var colOption in allColOptions)
                {
                    var colSetting = colSettings.FirstOrDefault(s => s.Column == colOption);
                    if (colSetting.IsEmpty())
                    {
                        listToSave.Add(GetDefaultColumnSetting(colOption));
                    }
                }

                if (SaveColumnSettings(username, listToSave))
                {
                    colSettings = ColumnSettingsApi.GetSampleColumnsApi(username) ?? new List<ColumnSetting>();
                }
            }

            if (colSettings.Any())
            {
                foreach (var colSetting in colSettings)
                {
                    ColumnSettingHelper.GetKeyValuePair(colSetting, out var key, out var tuple);
                    list.Add(key, tuple);
                }
            }
            else
            {
                // get the defaults:
                list.Add(nameof(ColumnOption.SampleStatus), new Tuple<string, bool, double>(LanguageResourceHelper.Get("LID_Label_Status"), true, 105));
                list.Add(nameof(ColumnOption.SamplePosition), new Tuple<string, bool, double>(LanguageResourceHelper.Get("LID_Label_Pos"), true, 70));
                list.Add(nameof(ColumnOption.SampleID), new Tuple<string, bool, double>(LanguageResourceHelper.Get("LID_QMgmtHEADER_SampleId"), true, 125));
                list.Add(nameof(ColumnOption.TotalConcentration), new Tuple<string, bool, double>(LanguageResourceHelper.Get("LID_Result_Concentration_Export"), true, 115));
                list.Add(nameof(ColumnOption.ViableConcentration), new Tuple<string, bool, double>(LanguageResourceHelper.Get("LID_Result_ViaConc_Export"), true, 115));
                list.Add(nameof(ColumnOption.TotalViability), new Tuple<string, bool, double>(LanguageResourceHelper.Get("LID_Result_Viability"), true, 70));
                list.Add(nameof(ColumnOption.TotalCells), new Tuple<string, bool, double>(LanguageResourceHelper.Get("LID_Label_TotalCells"), true, 85));
                list.Add(nameof(ColumnOption.AverageDiameter), new Tuple<string, bool, double>(LanguageResourceHelper.Get("LID_Label_AverageDiam_Abbr"), true, 85));
                list.Add(nameof(ColumnOption.CellTypeQcName), new Tuple<string, bool, double>(LanguageResourceHelper.Get("LID_Label_CellType"), true, 125));
                list.Add(nameof(ColumnOption.Dilution), new Tuple<string, bool, double>(LanguageResourceHelper.Get("LID_Label_Dilution"), true, 85));
                list.Add(nameof(ColumnOption.WashType), new Tuple<string, bool, double>(LanguageResourceHelper.Get("LID_QMgmtHEADER_Wash"), true, 100));
                list.Add(nameof(ColumnOption.SampleTag), new Tuple<string, bool, double>(LanguageResourceHelper.Get("LID_Label_Tag"), true, 120));
            }

            return list;
        }

        public static bool SaveAll(string username, List<Tuple<ColumnOption, bool, double>> itemsToSave)
        {
            return ColumnSettingsApi.SetSampleColumnsApi(username, ColumnSettingHelper.GetColumnSettings(itemsToSave));
        }

        public static bool SaveColumnSettings(string username, List<ColumnSetting> columnSettings)
        {
            if (!columnSettings.Any()) return true;
            return ColumnSettingsApi.SetSampleColumnsApi(username, columnSettings);
        }

        public static bool SaveColumnSetting(string username, ColumnOption column, bool isVisible, double width)
        {
            return ColumnSettingsApi.SetSampleColumnsApi(username, new List<ColumnSetting>
            {
                ColumnSettingHelper.GetColumnSetting(column, isVisible, width)
            });
        }

        public static bool SaveColumnSetting(string username, string columnOptionStr, bool isVisible, double width)
        {
            if (Enum.TryParse(columnOptionStr, out ColumnOption columnOption))
            {
                return SaveColumnSetting(username, columnOption, isVisible, width);
            }

            return false;
        }

        private static ColumnSetting GetDefaultColumnSetting(ColumnOption columnOption)
        {
            switch (columnOption)
            {
                case ColumnOption.SampleStatus: return new ColumnSetting(ColumnOption.SampleStatus, true, 105);
                case ColumnOption.SamplePosition: return new ColumnSetting(ColumnOption.SamplePosition, true, 70);
                case ColumnOption.SampleID: return new ColumnSetting(ColumnOption.SampleID, true, 125);
                case ColumnOption.TotalConcentration: return new ColumnSetting(ColumnOption.TotalConcentration, true, 115);
                case ColumnOption.ViableConcentration: return new ColumnSetting(ColumnOption.ViableConcentration, true, 115);
                case ColumnOption.TotalViability: return new ColumnSetting(ColumnOption.TotalViability, true, 70);
                case ColumnOption.TotalCells: return new ColumnSetting(ColumnOption.TotalCells, true, 85);
                case ColumnOption.AverageDiameter: return new ColumnSetting(ColumnOption.AverageDiameter, true, 85);
                case ColumnOption.CellTypeQcName: return new ColumnSetting(ColumnOption.CellTypeQcName, true, 125);
                case ColumnOption.Dilution: return new ColumnSetting(ColumnOption.Dilution, true, 85);
                case ColumnOption.WashType: return new ColumnSetting(ColumnOption.WashType, true, 100);
                case ColumnOption.SampleTag: return new ColumnSetting(ColumnOption.SampleTag, true, 120);
            }

            return new ColumnSetting();
        }
    }
}