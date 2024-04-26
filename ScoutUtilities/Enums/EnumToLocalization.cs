using ScoutLanguageResources;
using System;
using System.ComponentModel;

namespace ScoutUtilities.Enums
{
    public class EnumToLocalization
    {
        public static string GetLocalizedDescription(Enum enumObj)
        {
            var fieldInfo = enumObj.GetType().GetField(enumObj.ToString());
            if (fieldInfo == null)
            {
                return string.Empty;
            }

            object[] attribArray = fieldInfo.GetCustomAttributes(false);

            if (attribArray.Length == 0)
            {
                return enumObj.ToString();
            }

            var attrib = attribArray[0] as DescriptionAttribute;
            return !string.IsNullOrEmpty(attrib?.Description) ? LanguageResourceHelper.Get(attrib.Description) : string.Empty;
        }
    }
}
