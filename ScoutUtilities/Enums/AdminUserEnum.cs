using System;
using System.ComponentModel;

namespace ScoutUtilities.Enums
{
    public enum eNightlyCleanStatus : UInt16
    {
        ncsIdle = 0,
        ncsInProgress,
        ncsUnableToPerform,
        ncsAutomationInProgress,
        ncsAutomationUnableToPerform,
    }

    public enum eSensorStatus : UInt16
    {
        ssStateUnknown = 0,
        ssStateActive,
        ssStateInactive
    }

    public enum assay_parameter : UInt16
    {
        [Description("LID_CheckBox_TotalCellConcentration")] ap_Concentration = 0,
        [Description("LID_Label_Viability")] ap_PopulationPercentage,
        [Description("LID_Label_Size")] ap_Size
    }
}