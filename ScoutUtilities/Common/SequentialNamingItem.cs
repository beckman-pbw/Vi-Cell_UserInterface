using System;
using ScoutUtilities.Enums;

namespace ScoutUtilities.Common
{
    public class SequentialNamingItem : ICloneable
    {
        #region Constructor

        public SequentialNamingItem(string baseTextString)
        {
            SeqNamingType = SequentialNamingType.Text;
            BaseTextString = baseTextString;
        }

        public SequentialNamingItem(ushort? startingDigit, ushort? numberOfDigits, ushort currentSeqNumber = 0)
        {
            SeqNamingType = SequentialNamingType.Integer;
            StartingDigit = startingDigit;
            NumberOfDigits = numberOfDigits;
            CurrentSeqNumber = currentSeqNumber;
        }

        #endregion

        #region Properties & Fields

        public SequentialNamingType SeqNamingType { get; set; }
        public string BaseTextString { get; set; }
        public ushort? StartingDigit { get; set; }
        public ushort? NumberOfDigits { get; set; }
        public ushort CurrentSeqNumber { get; set; }

        #endregion

        #region Public Methods

        public string Next(bool previewNext = false)
        {
            if (SeqNamingType == SequentialNamingType.Integer)
            {
                var str = CurrentSeqNumber.ToString("D" + NumberOfDigits);
                if (!previewNext) CurrentSeqNumber++;
                return str;
            }

            if (SeqNamingType == SequentialNamingType.Text)
            {
                return BaseTextString;
            }

            return string.Empty;
        }

        public string Previous()
        {
            if (SeqNamingType == SequentialNamingType.Integer)
            {
                var lastNumber = CurrentSeqNumber - 1;
                if (lastNumber < StartingDigit) lastNumber = StartingDigit.Value;
                var str = lastNumber.ToString("D" + NumberOfDigits);
                return str;
            }

            if (SeqNamingType == SequentialNamingType.Text)
            {
                return BaseTextString;
            }

            return string.Empty;
        }

        public object Clone()
        {
            var clone = (SequentialNamingItem) MemberwiseClone();
            return clone;
        }

        #endregion
    }
}