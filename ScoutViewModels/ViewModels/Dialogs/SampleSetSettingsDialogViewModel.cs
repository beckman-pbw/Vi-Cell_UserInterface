using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutViewModels.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class SampleSetSettingsDialogViewModel : BaseDialogViewModel
    {
        public SampleSetSettingsDialogViewModel(SampleSetSettingsDialogEventArgs<Dictionary<string, ColumnSettingViewModel>> args, Window parentWindow)
            : base(args, parentWindow)
        {
            DialogTitle = LanguageResourceHelper.Get("LID_Label_SampleSetTableSettings");
            _originalColumns = ColumnSettingViewModel.Clone(args.ColumnSettingViewModelDictionary);
            Columns = ColumnSettingViewModel.Clone(args.ColumnSettingViewModelDictionary);
            foreach (var column in Columns)
            {
                column.Value.SettingChanged += SettingChanged;
            }
        }

        protected override void DisposeManaged()
        {
            foreach (var column in Columns)
            {
                column.Value.SettingChanged -= SettingChanged;
            }
            base.DisposeManaged();
        }


        private void SettingChanged(object sender, PropertyChangedEventArgs e)
        {
            AcceptCommand.RaiseCanExecuteChanged();
        }

        private List<Tuple<ColumnOption, bool, double>> ConvertColumnsForSaving()
        {
            var list = new List<Tuple<ColumnOption, bool, double>>();
            foreach (var item in Columns)
            {
                if (Enum.TryParse(item.Key, out ColumnOption columnOption))
                {
                    list.Add(new Tuple<ColumnOption, bool, double>(columnOption, item.Value.IsVisible, item.Value.ColumnWidth.Value));
                }
            }
            return list;
        }

        #region Properties & Fields

        private readonly Dictionary<string, ColumnSettingViewModel> _originalColumns;

        public Dictionary<string, ColumnSettingViewModel> Columns
        {
            get { return GetProperty<Dictionary<string, ColumnSettingViewModel>>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        public override bool CanAccept()
        {
            return !ColumnSettingViewModel.AreSame(_originalColumns, Columns);
        }

        protected override void OnAccept()
        {
            var settingsChanged = !ColumnSettingViewModel.AreSame(_originalColumns, Columns);
            if (settingsChanged)
            {
                if (!ColumnSettingsModel.SaveAll(LoggedInUser.CurrentUserId, ConvertColumnsForSaving()))
                {
                    Log.Warn($"Failed to save all column settings.");
                    PostToMessageHub(LanguageResourceHelper.Get("LID_Warning_FailedToSaveColumnSetting"), MessageType.Warning);
                }
            }

            Close(settingsChanged);
        }

        #endregion
    }
}