using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ColumnSetting
    {
        public ColumnOption Column;
        public uint Width;
        public byte IsVisible; // boolean

        public ColumnSetting(ColumnOption columnOption, bool isVisible, uint width)
        {
            Column = columnOption;
            Width = width;
            IsVisible = Misc.BoolToByte(isVisible);
        }

        public bool IsEmpty()
        {
            return Column == default && Width == default && IsVisible == default(byte);
        }
    }

    public class ColumnSettingHelper
    {
        public static List<ColumnSetting> GetColumnSettings(List<Tuple<ColumnOption, bool, double>> tuples)
        {
            var list = new List<ColumnSetting>();

            foreach (var tuple in tuples)
            {
                list.Add(GetColumnSetting(tuple.Item1, tuple.Item2, tuple.Item3));
            }

            return list;
        }

        public static ColumnSetting GetColumnSetting(ColumnOption colOption, bool isVisible, double width)
        {
            return new ColumnSetting
            {
                Column = colOption,
                Width = (uint) Math.Round(width),
                IsVisible = Misc.BoolToByte(isVisible)
            };
        }

        public static void GetKeyValuePair(ColumnSetting colSetting, out string key, out Tuple<string, bool, double> tuple)
        {
            key = colSetting.Column.ToString();
            tuple = new Tuple<string, bool, double>(ColumnOptionHelper.GetLocalizedColumnOptionString(colSetting.Column),
                Misc.ByteToBool(colSetting.IsVisible), colSetting.Width);
        }
    }
}