using ScoutLanguageResources;

namespace ScoutUtilities.Enums
{
    public enum ColumnOption
    {
        SampleStatus,
        SamplePosition,
        SampleID, // Note: This is the 'name' not the UUID. We display Sample ID as the heading but it's the 'name' 
        TotalConcentration,
        ViableConcentration,
        TotalViability,
        TotalCells,
        AverageDiameter,
        CellTypeQcName,
        Dilution,
        WashType,
        SampleTag
    }


    public class ColumnOptionHelper
    {
        public static string GetLocalizedColumnOptionString(ColumnOption colOption)
        {
            switch (colOption)
            {
                case ColumnOption.SampleStatus:
                    return LanguageResourceHelper.Get("LID_Label_Status");

                case ColumnOption.SamplePosition:
                    return LanguageResourceHelper.Get("LID_Label_Pos");

                case ColumnOption.SampleID:
                    return LanguageResourceHelper.Get("LID_QMgmtHEADER_SampleId");

                case ColumnOption.TotalConcentration:
                    return LanguageResourceHelper.Get("LID_Result_Concentration_Export");

                case ColumnOption.ViableConcentration:
                    return LanguageResourceHelper.Get("LID_Result_ViaConc_Export");

                case ColumnOption.TotalViability:
                    return LanguageResourceHelper.Get("LID_Result_Viability");

                case ColumnOption.TotalCells:
                    return LanguageResourceHelper.Get("LID_Label_TotalCells");

                case ColumnOption.AverageDiameter:
                    return LanguageResourceHelper.Get("LID_Label_AverageDiam_Abbr");

                case ColumnOption.CellTypeQcName:
                    return LanguageResourceHelper.Get("LID_Label_CellType");

                case ColumnOption.Dilution:
                    return LanguageResourceHelper.Get("LID_Label_Dilution");

                case ColumnOption.WashType:
                    return LanguageResourceHelper.Get("LID_QMgmtHEADER_Workflow");

                case ColumnOption.SampleTag:
                    return LanguageResourceHelper.Get("LID_Label_Tag");

                default:
                    return "[UNKNOWN]";

            }
        }

        public static ColumnOption GetColumnOption(string colOptionLocalStr)
        {
            if (colOptionLocalStr == LanguageResourceHelper.Get("LID_Label_Status"))
                return ColumnOption.SampleStatus;
            
            if (colOptionLocalStr == LanguageResourceHelper.Get("LID_Label_Pos"))
                return ColumnOption.SamplePosition;

            if (colOptionLocalStr == LanguageResourceHelper.Get("LID_QMgmtHEADER_SampleId"))
                return ColumnOption.SampleID;

            if (colOptionLocalStr == LanguageResourceHelper.Get("LID_Result_Concentration_Export"))
                return ColumnOption.TotalConcentration;

            if (colOptionLocalStr == LanguageResourceHelper.Get("LID_Result_ViaConc_Export"))
                return ColumnOption.ViableConcentration;

            if (colOptionLocalStr == LanguageResourceHelper.Get("LID_Result_Viability"))
                return ColumnOption.TotalViability;

            if (colOptionLocalStr == LanguageResourceHelper.Get("LID_Label_TotalCells"))
                return ColumnOption.TotalCells;

            if (colOptionLocalStr == LanguageResourceHelper.Get("LID_Label_AverageDiam_Abbr"))
                return ColumnOption.AverageDiameter;

            if (colOptionLocalStr == LanguageResourceHelper.Get("LID_Label_CellType"))
                return ColumnOption.CellTypeQcName;

            if (colOptionLocalStr == LanguageResourceHelper.Get("LID_Label_Dilution"))
                return ColumnOption.Dilution;

            if (colOptionLocalStr == LanguageResourceHelper.Get("LID_QMgmtHEADER_Workflow"))
                return ColumnOption.WashType;

            if (colOptionLocalStr == LanguageResourceHelper.Get("LID_Label_Tag"))
                return ColumnOption.SampleTag;

            return ColumnOption.SampleID;
        }
    }
}