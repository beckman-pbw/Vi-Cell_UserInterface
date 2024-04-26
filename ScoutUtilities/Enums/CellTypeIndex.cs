using System;

namespace ScoutUtilities.Enums
{
    public enum CellTypeIndex : uint
    {
        BciDefault = 0,
        Mammalian = 1,
        Insect = 2,
        Yeast = 3,
        BciViabBeads = 4,
        BciConcBeads = 5,
        BciL10Beads = 6,
    }

    public class CellTypeIndexHelper
    {
        public static string GetCellTypeName(uint index)
        {
            return GetCellTypeName((CellTypeIndex) Enum.Parse(typeof(CellTypeIndex), index.ToString()));
        }

        public static string GetCellTypeName(CellTypeIndex index)
        {
            switch (index)
            {
                case CellTypeIndex.BciDefault: return "BCI Default";
                case CellTypeIndex.Mammalian: return "Mammalian";
                case CellTypeIndex.Insect: return "Insect";
                case CellTypeIndex.Yeast: return "Yeast";
                case CellTypeIndex.BciViabBeads: return "BCI Viab Beads";
                case CellTypeIndex.BciConcBeads: return "BCI Conc Beads";
                case CellTypeIndex.BciL10Beads: return "BCI L10 Beads";
                default: return string.Empty;
            }
        }
    }
}