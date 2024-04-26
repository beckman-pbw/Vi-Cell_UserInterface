using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;

namespace ScoutUtilities.Enums
{
   
    public enum SampleStatusColor
    {
        [Description("#FFF4ED23")] [XmlEnum("1000")] Defined = 1000,          // Yellow

        [Description("#FFF4ED23")] [XmlEnum("1001")] Selected = 1001,         // Yellow

        [Description("#FF000000")] [XmlEnum("1003")] Empty = 1003,            // Black
        
        [Description("#FFFFF9B3")] [XmlEnum("1004")] HighLighted = 1004,      // Lighter yellow
        
        [Description("#EDE20B")] [XmlEnum("1005")] ConcentrationType1 = 1005, // Darker yellow
        
        [Description("#A349A2")] [XmlEnum("1006")] ConcentrationType2 = 1006, // Purple
        
        [Description("#FFADC9")] [XmlEnum("1007")] ConcentrationType3 = 1007, // Pinkish
        
        [Description("#737373")] [XmlEnum("1008")] Completed = 108,           // Gray
        
        [Description("#737373")] [XmlEnum("1009")] Skip = 1009,               // Gray

        [Description("#ff4d4d")] [XmlEnum("1010")] Error = 1010               // Red
    }

   
    public static class GetEnumDescription
    {
        public static string GetDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi == null)
                return string.Empty;

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[]) fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            return value.ToString();
        }
    }
}
