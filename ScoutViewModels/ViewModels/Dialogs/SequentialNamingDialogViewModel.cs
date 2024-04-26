using ScoutLanguageResources;
using ScoutUtilities.CustomEventArgs;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using System;
using System.Windows;
using System.Windows.Media;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class SequentialNamingDialogViewModel : BaseDialogViewModel
    {
        #region Constructor

        public SequentialNamingDialogViewModel(SequentialNamingEventArgs args, Window parentWindow) 
            : base(args, parentWindow)
        {
            DialogTitle = LanguageResourceHelper.Get("LID_Header_SeqSampleNaming");
            DialogIconDrawBrush = (DrawingBrush) Application.Current.Resources["ListBrush"];

            SequentialNamingSet = new SequentialNamingSetViewModel(args.SeqNamingItems);
            SequentialNamingSet.SequentialItemsChanged += OnSequentialSetValuesChanged;
            UseSequencing = args.UseSequencing;
        }

        protected override void DisposeUnmanaged()
        {
            if (SequentialNamingSet != null)
            {
                SequentialNamingSet.SequentialItemsChanged -= OnSequentialSetValuesChanged;
                SequentialNamingSet.Dispose();
            }
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        public SequentialNamingSetViewModel SequentialNamingSet
        {
            get { return GetProperty<SequentialNamingSetViewModel>(); }
            private set { SetProperty(value); }
        }

        public string PreviewText
        {
            get { return GetProperty<string>(); }
            private set { SetProperty(value); }
        }

        public bool UseSequencing
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                UpdatePreviewText();
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Private Methods

        private void UpdatePreviewText()
        {
            if (UseSequencing)
            {
                SequentialNamingSet.ResetSequenceNumbering();
                var str = $"{SequentialNamingSet.Next()}, {SequentialNamingSet.Next()}, {SequentialNamingSet.Next()}, ...";
                PreviewText = str;
            }
            else
            {
                var str = SequentialNamingSet.GetBaseString();
                PreviewText = $"{str}, {str}, {str}, ...";
            }
        }

        #endregion

        #region Event Handlers

        private void OnSequentialSetValuesChanged(object sender, EventArgs e)
        {
            UpdatePreviewText();
            
            NotifyPropertyChanged(nameof(PreviewText));
            NotifyPropertyChanged(nameof(UseSequencing));
            NotifyPropertyChanged(nameof(SequentialNamingSet));

            AcceptCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region Commands

        public override bool CanAccept()
        {
            var intItem = SequentialNamingSet.GetIntegerItem();
            return !UseSequencing ||
                   (UseSequencing &&
                    !string.IsNullOrEmpty(SequentialNamingSet.GetBaseString()) &&
                    intItem.StartingDigit.HasValue &&
                    intItem.NumberOfDigits.HasValue &&
                    intItem.StartingDigit >= 0 &&
                    intItem.NumberOfDigits > 0);
        }

        #endregion
    }
}