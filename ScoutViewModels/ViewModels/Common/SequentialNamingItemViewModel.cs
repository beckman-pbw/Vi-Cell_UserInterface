using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;

namespace ScoutViewModels.ViewModels.Common
{
    public class SequentialNamingItemViewModel : BaseViewModel
    {
        #region Constructor

        public SequentialNamingItemViewModel(SequentialNamingItem item)
        {
            _sequentialNamingItem = item;
            SeqNamingType = item.SeqNamingType;
            BaseTextString = item.BaseTextString;
            StartingDigit = item.StartingDigit;
            NumberOfDigits = item.NumberOfDigits;
            CurrentSeqNumber = StartingDigit ?? 0;
        }

        #endregion

        #region Properties & Fields

        public event EventHandler ValuesChanged;

        private SequentialNamingItem _sequentialNamingItem;

        public SequentialNamingType SeqNamingType
        {
            get { return _sequentialNamingItem.SeqNamingType; }
            set
            {
                _sequentialNamingItem.SeqNamingType = value;
                NotifyPropertyChanged(nameof(SeqNamingType));
            }
        }

        public string BaseTextString
        {
            get { return _sequentialNamingItem.BaseTextString; }
            set
            {
                _sequentialNamingItem.BaseTextString = value;
                NotifyPropertyChanged(nameof(BaseTextString));
                ValuesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int? StartingDigit
        {
            get { return _sequentialNamingItem.StartingDigit; }
            set
            {
                _sequentialNamingItem.StartingDigit = value;
                NotifyPropertyChanged(nameof(StartingDigit));
                ValuesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int? NumberOfDigits
        {
            get { return _sequentialNamingItem.NumberOfDigits; }
            set
            {
                _sequentialNamingItem.NumberOfDigits = value;
                NotifyPropertyChanged(nameof(NumberOfDigits));
                ValuesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int CurrentSeqNumber
        {
            get { return _sequentialNamingItem.CurrentSeqNumber; }
            set
            {
                _sequentialNamingItem.CurrentSeqNumber = value;
                NotifyPropertyChanged(nameof(CurrentSeqNumber));
            }
        }

        #endregion

        #region Public Methods

        public SequentialNamingItem GetSequentialNamingItem()
        {
            return SeqNamingType == SequentialNamingType.Integer
                ? new SequentialNamingItem(StartingDigit, NumberOfDigits, CurrentSeqNumber)
                : new SequentialNamingItem(BaseTextString);
        }

        public string Next(bool previewNext = false)
        {
            return _sequentialNamingItem.Next(previewNext);
        }

        public string Previous()
        {
            return _sequentialNamingItem.Previous();
        }

        public void ResetDigits()
        {
            CurrentSeqNumber = StartingDigit ?? 0;
        }

        #endregion
    }
}