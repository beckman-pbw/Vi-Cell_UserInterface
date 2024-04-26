using System.ComponentModel;

namespace ScoutUtilities.Enums
{
    public enum RecurrenceFrequency : ushort
    {
        // We aren't using all these values for the scheduled exports
        [Description("LID_Label_RecurrenceFreq_Once")] Once = 0,
        //Secondly = 1,
        //Minutely = 2,
        //Hourly = 3,
        [Description("LID_Label_RecurrenceFreq_Daily")] Daily = 4,
        [Description("LID_Label_RecurrenceFreq_Weekly")] Weekly = 5,
        [Description("LID_Label_RecurrenceFreq_Monthly")] Monthly = 6,
        //Yearly = 7
    }
}