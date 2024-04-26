using System.ComponentModel;

namespace ScoutUtilities.Enums
{
    public enum Weekday : ushort
    {
        [Description("LID_Label_Weekday_Sunday")] Sunday = 0,
        [Description("LID_Label_Weekday_Monday")] Monday,
        [Description("LID_Label_Weekday_Tuesday")] Tuesday,
        [Description("LID_Label_Weekday_Wednesday")] Wednesday,
        [Description("LID_Label_Weekday_Thursday")] Thursday,
        [Description("LID_Label_Weekday_Friday")] Friday,
        [Description("LID_Label_Weekday_Saturday")] Saturday
    }
}