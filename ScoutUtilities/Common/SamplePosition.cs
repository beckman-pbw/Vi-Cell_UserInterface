using ScoutUtilities.Enums;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace ScoutUtilities.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SamplePosition : IEquatable<SamplePosition>
    {
        [DefaultValue(ApplicationConstants.SamplePositionRowChar_Carousel)]
        public char Row { get; set; }

        [DefaultValue(1)]
        public byte Column { get; set; }

        public SamplePosition(char row, int column)
        {
            Row = row;
            Column = (byte) column;
        }

        public SamplePosition(char row, uint column)
        {
            Row = row;
            Column = (byte)column;
        }

        public SamplePosition(char row, byte column)
        {
            Row = row;
            Column = column;
        }

        public SamplePosition(RowPosition rowPosition, byte column)
        {
            Row = RowPositionHelper.GetRowChar(rowPosition);
            Column = column;
        }

        public bool IsCarousel()
        {
            return IsValidForCarousel(Row, Column);
        }

        public bool IsAutomationCup()
        {
            return IsValidForAutomationCup(Row, Column);
        }

        public bool Is96WellPlate()
        {
            return IsValidForPlate96(Row, Column);
        }

        public SubstrateType GetSubstrateType()
        {
            if (IsAutomationCup()) return SubstrateType.AutomationCup;
            if (IsCarousel()) return SubstrateType.Carousel;
            if (IsValidForPlate96(Row, Column)) return SubstrateType.Plate96;
            return SubstrateType.NoType;
        }

        public bool IsValid()
        {
            return IsValid(Row, Column);
        }

        private static bool IsValidForCarousel(char row, uint col)
        {
            return row == ApplicationConstants.SamplePositionRowChar_Carousel && 
                   col >= 1 && 
                   col <= ApplicationConstants.NumOfCarouselPositions;
        }

        private static bool IsValidForPlate96(char row, uint col)
        {
            return row >= 'A' && row <= 'H' && col >= 1 && col <= 12;
        }

        private static bool IsValidForAutomationCup(char row, uint col)
        {
            return row == ApplicationConstants.SamplePositionRowChar_AutomationCup && col == 1;
        }

        private static bool IsValid(char row, uint col)
        {
            return IsValidForCarousel(row, col) || IsValidForPlate96(row, col) || IsValidForAutomationCup(row, col);
        }

        public static SamplePosition GetAutomationCupSamplePosition()
        {
            return new SamplePosition(ApplicationConstants.SamplePositionRowChar_AutomationCup, 1);
        }

        public static SubstrateType GetSubstrateType(char row, byte column)
        {
            if (IsValidForAutomationCup(row, column)) return SubstrateType.AutomationCup;
            if (IsValidForCarousel(row, column)) return SubstrateType.Carousel;
            if (IsValidForPlate96(row, column)) return SubstrateType.Plate96;
            return SubstrateType.Plate96;
        }

        public static SamplePosition Parse(string strPosition)
        {
            if (string.IsNullOrEmpty(strPosition)) throw new NullReferenceException(nameof(strPosition));

            if (int.TryParse(strPosition, out var i))
            {
                // str is ONLY a number, not a letter or combo -- assume using carousel
                return new SamplePosition(ApplicationConstants.SamplePositionRowChar_Carousel, i);
            }

            var rowChar = strPosition.Substring(0, 1).ToCharArray().FirstOrDefault();
            var colStr = strPosition.Substring(1);
            return new SamplePosition(rowChar, int.Parse(colStr));
        }

        public override string ToString()
        {
            var colStr = Column.ToString("D2");
            var rowStr = IsCarousel() ? string.Empty : Row.ToString();
            return $"{rowStr}{colStr}";
        }

        public bool Equals(SamplePosition other)
        {
            return Row == other.Row && Column == other.Column;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SamplePosition) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Row.GetHashCode() * 397) ^ Column.GetHashCode();
            }
        }
    }
}