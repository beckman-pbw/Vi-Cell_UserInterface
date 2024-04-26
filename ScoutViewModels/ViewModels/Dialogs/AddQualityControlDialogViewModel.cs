using ApiProxies.Generic;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels;
using ScoutServices.Interfaces;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class AddQualityControlDialogViewModel : BaseDialogViewModel
    {
        public AddQualityControlDialogViewModel(ICellTypeManager cellTypeManager, AddCellTypeEventArgs args, System.Windows.Window parentWindow) 
            : base(new BaseDialogEventArgs(sizeToContent: true), parentWindow)
        {
            _cellTypeManager = cellTypeManager;
            DialogTitle = LanguageResourceHelper.Get("LID_PopUpHeader_AddQualityControl");
            QualityControl = new QualityControlDomain();
            SelectedAssayParameter = AssayParameterList.FirstOrDefault();
        }

        #region Properties & Fields

        private readonly ICellTypeManager _cellTypeManager;
        public string DynamicAssayHeader
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public IEnumerable<assay_parameter> AssayParameterList => Enum.GetValues(typeof(assay_parameter)).Cast<assay_parameter>();

        public assay_parameter SelectedAssayParameter
        {
            get { return GetProperty<assay_parameter>(); }
            set
            {
                SetProperty(value);
                QualityControl.AssayParameter = value; // update the current QC (#5128)
                if (QualityControl?.AssayValue != null)
                {
                    QualityControl.AssayValue = null;
                    NotifyPropertyChanged(nameof(QualityControl));
                }
                SetAssayHeaderValue(value);
            }
        }

        public ObservableCollection<CellTypeDomain> AllCellTypesList
        {
            get
            {
                var item = GetProperty<ObservableCollection<CellTypeDomain>>();
                if (item != null) return item;

                var cts = LoggedInUser.CurrentUser.AssignedCellTypes;
                SetProperty(cts.ToObservableCollection());
                return GetProperty<ObservableCollection<CellTypeDomain>>();
            }
        }

        public CellTypeDomain SelectedCellType
        {
            get { return GetProperty<CellTypeDomain>(); }
            set { SetProperty(value); }
        }

        public QualityControlDomain QualityControl
        {
            get { return GetProperty<QualityControlDomain>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        public override bool CanAccept()
        {
            return AllCellTypesList.Count > 0;
        }

        protected override void OnAccept()
        {
            if (AddQualityControl())
            {
                base.OnAccept();
            }
        }

        #endregion

        #region Private Methods

        private void SetAssayHeaderValue(assay_parameter assayParameter)
        {
            switch (assayParameter)
            {
                case assay_parameter.ap_Concentration:
                    DynamicAssayHeader = LanguageResourceHelper.Get("LID_GraphLabel_AssayConcentrationPerml");
                    break;
                case assay_parameter.ap_PopulationPercentage:
                    DynamicAssayHeader = LanguageResourceHelper.Get("LID_GraphLabel_AssayConcentrationPerUnit");
                    break;
                case assay_parameter.ap_Size:
                    DynamicAssayHeader = LanguageResourceHelper.Get("LID_GraphLabel_AssayConcentrationMicrometerUnit");
                    break;
            }
        }

        private bool AddQualityControl()
        {
            try
            {
                if (QualityControl == null)
                    return false;

                if (QualityControl.AcceptanceLimit == null)
                {
                    QualityControl.AcceptanceLimit = (int?)ApplicationConstants.DefaultAcceptanceValue;
                }

                if (_cellTypeManager.QualityControlValidation(QualityControl, true))
                {
                    if (SelectedCellType == null)
                    {
                        string err = string.Format(LanguageResourceHelper.Get("LID_ERRMSGBOX_CellTypeNameBlank"), QualityControl.QcName);
                        DialogEventBus.DialogBoxOk(this, err);
                        Log.Debug(err);
                        return false;
                    }
                    QualityControl.CellTypeIndex = SelectedCellType.CellTypeIndex;
                    QualityControl.CellTypeName = SelectedCellType.CellTypeName;
                    var msg = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_AddConfirmation"), QualityControl.QcName);
                    if (DialogEventBus.DialogBoxYesNo(this, msg) != true)
                    {
                        return false;
                    }
                    var username = ScoutModels.LoggedInUser.CurrentUserId;
                    return _cellTypeManager.CreateQualityControl(username, "", QualityControl, true);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QC_ADD_QUALITY"));
            }

            return false;
        }

        #endregion
    }
}