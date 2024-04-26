using System.ComponentModel;

namespace ScoutUtilities.Enums
{
    public enum ImageType
    {
        [Description("LID_ImageLabel_Raw")]
        Raw,
        [Description("LID_ImageLabel_Annotated")]
        Annotated,
        [Description("LID_ImageLabel_Binary")]
        Binary
    }
}