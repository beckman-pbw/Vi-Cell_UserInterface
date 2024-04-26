using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScoutViewModels.ViewModels.ExpandedSampleWorkflow
{
    public class SequentialNamingSetViewModel : BaseViewModel, ICloneable
    {
        #region Constructor

        public SequentialNamingSetViewModel(string defaultSampleName)
        {
            TextFirst = true;
            SequentialNamingItems = new SequentialNamingItemViewModel[2];

            var vmText = new SequentialNamingItemViewModel(new SequentialNamingItem(defaultSampleName));
            vmText.ValuesChanged += OnItemViewModelValuesChanged;
            SequentialNamingItems[0] = vmText;

            var vmNum = new SequentialNamingItemViewModel(new SequentialNamingItem(ApplicationConstants.SequentialNamingStartingNumber,
                ApplicationConstants.SequentialNamingNumberOfDigits));
            vmNum.ValuesChanged += OnItemViewModelValuesChanged;
            SequentialNamingItems[1] = vmNum;

            SequentialItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        public SequentialNamingSetViewModel(IList<SequentialNamingItem> items)
        {
            var itemText = items.FirstOrDefault(i => i.SeqNamingType == SequentialNamingType.Text);
            var itemNum = items.FirstOrDefault(i => i.SeqNamingType == SequentialNamingType.Integer);

            TextFirst = items.FirstOrDefault()?.SeqNamingType != SequentialNamingType.Integer;
            SequentialNamingItems = new SequentialNamingItemViewModel[2];

            var vmText = new SequentialNamingItemViewModel(itemText);
            vmText.ValuesChanged += OnItemViewModelValuesChanged;
            
            var vmNum = new SequentialNamingItemViewModel(itemNum);
            vmNum.ValuesChanged += OnItemViewModelValuesChanged;

            if (TextFirst)
            {
                SequentialNamingItems[0] = vmText;
                SequentialNamingItems[1] = vmNum;
            }
            else
            {
                SequentialNamingItems[0] = vmNum;
                SequentialNamingItems[1] = vmText;
            }

            SequentialItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void DisposeManaged()
        {
            foreach (var item in SequentialNamingItems)
            {
                item.ValuesChanged -= OnItemViewModelValuesChanged;
            }
            base.DisposeManaged();
        }
        #endregion

        #region Properties & Fields

        public event EventHandler SequentialItemsChanged;

        public bool TextFirst
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public SequentialNamingItemViewModel[] SequentialNamingItems
        {
            get { return GetProperty<SequentialNamingItemViewModel[]>(); }
            private set { SetProperty(value); }
        }

        #endregion

        #region Public Methods

        public string Next(bool previewNext = false)
        {
            var sb = new StringBuilder();

            foreach (var item in SequentialNamingItems)
            {
                sb.Append(item.Next(previewNext));
            }

            return sb.ToString();
        }

        public string Previous()
        {
            var sb = new StringBuilder();

            foreach (var item in SequentialNamingItems)
            {
                sb.Append(item.Previous());
            }

            return sb.ToString();
        }

        public void DecrementSequenceNumber()
        {
            var item = GetIntegerItem();
            
            if (--item.CurrentSeqNumber < item.StartingDigit)
            {
                item.CurrentSeqNumber = item.StartingDigit ?? 0;
            }
        }

        public void ResetSequenceNumbering()
        {
            foreach (var item in SequentialNamingItems)
            {
                item.ResetDigits();
            }
        }

        public SequentialNamingItemViewModel GetIntegerItem()
        {
            return SequentialNamingItems[TextFirst ? 1 : 0];
        }

        public SequentialNamingItemViewModel GetTextItem()
        {
            return SequentialNamingItems[TextFirst ? 0 : 1];
        }
        
        public int GetCurrentSeqNumber()
        {
            return GetIntegerItem().CurrentSeqNumber;
        }

        public string GetBaseString()
        {
            return GetTextItem()?.BaseTextString ?? string.Empty;
        }

        public void SetBaseString(string newValue)
        {
            if (GetTextItem() != null)
            {
                GetTextItem().BaseTextString = newValue;
            }
        }

        public string GetSampleNameForDisplay(bool useSequencing)
        {
            var intItem = GetIntegerItem();
            var intStr = intItem.CurrentSeqNumber.ToString("D" + intItem.NumberOfDigits);

            return useSequencing
                ? TextFirst
                    ? $"{GetBaseString()}[{intStr}]"
                    : $"[{intStr}]{GetBaseString()}"
                : GetBaseString();
        }

        public List<SequentialNamingItem> GetSequentialNamingItems()
        {
            var list = new List<SequentialNamingItem>();
            foreach(var item in SequentialNamingItems) list.Add(item.GetSequentialNamingItem());
            return list;
        }

        #endregion

        #region Private Methods

        private void OnItemViewModelValuesChanged(object sender, EventArgs args)
        {
            SequentialItemsChanged?.Invoke(sender, args);
        }

        #endregion

        #region Commands

        private RelayCommand _swapCommand;
        public RelayCommand SwapCommand => _swapCommand ?? (_swapCommand = new RelayCommand(Swap));

        public void Swap()
        {
            var textVm = SequentialNamingItems[TextFirst ? 0 : 1];
            var numVm = SequentialNamingItems[TextFirst ? 1 : 0];
            
            TextFirst = !TextFirst;
            SequentialNamingItems = new SequentialNamingItemViewModel[2];
            SequentialNamingItems[TextFirst ? 0 : 1] = textVm;
            SequentialNamingItems[TextFirst ? 1 : 0] = numVm;
            
            SequentialItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public object Clone()
        {
            return new SequentialNamingSetViewModel(SequentialNamingItems.Select(x => x.GetSequentialNamingItem()).ToList());
        }
    }
}