// ***********************************************************************
// <copyright file="ucCellTypeBpQcComboBox.xaml.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScoutDomains;
using System.Linq;
using ScoutUtilities.Enums;
using ScoutUtilities;

namespace ScoutUI.Views.ucCommon
{
    /// <summary>
    /// Interaction logic for ucCellTypeBpQcComboBox.xaml
    /// </summary>
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class CellTypeBpQcComboBox
    {
        #region DependencyProperties

        /// <summary>
        /// Gets or sets the type of the combo style.
        /// </summary>
        /// <value>The type of the combo style.</value>
        public UcCellTypeBpQcType ComboStyleType
        {
            get { return (UcCellTypeBpQcType) GetValue(ComboStyleTypeProperty); }
            set
            {
                if (value == UcCellTypeBpQcType.Normal)
                    return;
                SetValue(ComboStyleTypeProperty, value);
            }
        }


        public object CommandParamOne
        {
            get { return (object) GetValue(CommandParamOneProperty); }
            set { SetValue(CommandParamOneProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParamOne.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParamOneProperty =
            DependencyProperty.Register("CommandParamOne", typeof(object), typeof(CellTypeBpQcComboBox));


        public object CommandParamTwo
        {
            get { return (object) GetValue(CommandParamTwoProperty); }
            set { SetValue(CommandParamTwoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParamTwo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParamTwoProperty =
            DependencyProperty.Register("CommandParamTwo", typeof(object), typeof(CellTypeBpQcComboBox));


        public object CommandParamThree
        {
            get { return (object) GetValue(CommandParamThreeProperty); }
            set { SetValue(CommandParamThreeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParamThree.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParamThreeProperty =
            DependencyProperty.Register("CommandParamThree", typeof(object), typeof(CellTypeBpQcComboBox));


        /// <summary>
        /// The theme property
        /// </summary>
        public static readonly DependencyProperty ComboStyleTypeProperty = DependencyProperty.Register(
            "ComboStyleType",
            typeof(UcCellTypeBpQcType),
            typeof(CellTypeBpQcComboBox));

        /// <summary>
        /// Gets or sets the selected command.
        /// </summary>
        /// <value>The selected command.</value>
        public ICommand SelectedCommand
        {
            get { return (ICommand) GetValue(SelectedCommandProperty); }
            set { SetValue(SelectedCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedCommand.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The selected command property
        /// </summary>
        public static readonly DependencyProperty SelectedCommandProperty =
            DependencyProperty.Register("SelectedCommand", typeof(ICommand), typeof(CellTypeBpQcComboBox),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the cell type bp qc collection.
        /// </summary>
        /// <value>The cell type bp qc collection.</value>
        public ObservableCollection<CellTypeQualityControlGroupDomain> CellTypeBpQcCollection
        {
            get { return (ObservableCollection<CellTypeQualityControlGroupDomain>) GetValue(CellTypeBpQcCollectionProperty); }
            set { SetValue(CellTypeBpQcCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The cell type bp qc collection property
        /// </summary>
        public static readonly DependencyProperty CellTypeBpQcCollectionProperty =
            DependencyProperty.Register("CellTypeBpQcCollection", typeof(ObservableCollection<CellTypeQualityControlGroupDomain>),
                typeof(CellTypeBpQcComboBox), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the selected cell type bp qc item.
        /// </summary>
        /// <value>The selected cell type bp qc item.</value>
        public CellTypeQualityControlGroupDomain SelectedCellTypeQualityControlGroupItem
        {
            get { return (CellTypeQualityControlGroupDomain) GetValue(SelectedCellTypeBpQcItemProperty); }
            set { SetValue(SelectedCellTypeBpQcItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedCellTypeQualityControlGroupItem.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The selected cell type bp qc item property
        /// </summary>
        public static readonly DependencyProperty SelectedCellTypeBpQcItemProperty =
            DependencyProperty.Register("SelectedCellTypeQualityControlGroupItem", typeof(CellTypeQualityControlGroupDomain),
                typeof(CellTypeBpQcComboBox), new PropertyMetadata(ValidateSelectedCellTypeBpQcItem));

        private static void ValidateSelectedCellTypeBpQcItem(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uc = d as CellTypeBpQcComboBox;
            if (uc?.CellTypeBpQcCollection == null || uc.SelectedCellTypeQualityControlGroupItem == null)
                return;

            foreach (var item in uc.CellTypeBpQcCollection)
            {
                if (item.CellTypeQualityControlChildItems.Count > 0)
                {
                    foreach (var x in item.CellTypeQualityControlChildItems)
                    {
                        x.HasValue = false;
                        x.IsSelectionActive = false;
                        x.HasValue = false;
                    }

                    if (item.SelectedCtBpQcType == uc.SelectedCellTypeQualityControlGroupItem.SelectedCtBpQcType)
                    {
                        foreach (var x in item.CellTypeQualityControlChildItems.Where(x => x.Name == uc.SelectedCellTypeQualityControlGroupItem.Name))
                        {
                            x.IsSelectionActive = true;
                        }
                    }
                }
            }

        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CellTypeBpQcComboBox"/> class.
        /// </summary>
        public CellTypeBpQcComboBox()
        {
            InitializeComponent();
            SelectedCommand = new RelayCommand(SetSelectedItem);
        }

        /// <summary>
        /// Sets the selected item.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void SetSelectedItem(object param)
        {
            var menuItem = param as MenuItem;
            var cellTypeBpQcDomain = menuItem?.DataContext as CellTypeQualityControlGroupDomain;
            foreach (var item in CellTypeBpQcCollection)
            {
                foreach (var child in item.CellTypeQualityControlChildItems)
                {
                    if (cellTypeBpQcDomain != null && child.Name == cellTypeBpQcDomain.Name)
                    {
                        child.IsSelectionActive = true;
                        SelectedCellTypeQualityControlGroupItem = new CellTypeQualityControlGroupDomain
                        {
                            Name = cellTypeBpQcDomain.Name,
                            KeyName = cellTypeBpQcDomain.KeyName,
                            CellTypeIndex = cellTypeBpQcDomain.CellTypeIndex,
                            AppTypeIndex = cellTypeBpQcDomain.AppTypeIndex,
                            SelectedCtBpQcType = cellTypeBpQcDomain.SelectedCtBpQcType
                        };
                    }
                    else
                        child.IsSelectionActive = false;
                }
            }
        }

        #endregion

        private void CellTypeMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            var ct = SelectedCellTypeQualityControlGroupItem;

            foreach (var item in CellTypeBpQcCollection)
            {
                if (item.CellTypeQualityControlChildItems.Count > 0)
                {
                    foreach (var x in item.CellTypeQualityControlChildItems)
                    {                      
                        if (x.Name == ct?.Name)
                        {
                            x.IsSelectionActive = true;
                        }
                        else
                            x.IsSelectionActive = false;
                    }
                }
            }
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs args)
        {
            if ((args.NewFocus as Window) != null)
            {
                args.Handled = true;
            }
            
            base.OnPreviewLostKeyboardFocus(args);
        }
    }
}