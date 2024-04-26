// ***********************************************************************
// Assembly         : ScoutUI
// Author           : 20115954
// Created          : 08-18-2017
//
// Last Modified By : 20115954
// Last Modified On : 10-26-2017
// ***********************************************************************
// <copyright file="ucExpanderInDataGrid.xaml.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Ninject.Extensions.Logging.Log4net.Infrastructure;
using ScoutDomains.Common;
using ScoutServices.Service.ConcentrationSlope;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ScoutUI.Views.ucCommon
{
    /// <summary>
    /// Interaction logic for ucExpanderInDataGrid.xaml
    /// </summary>
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class ExpanderInDataGrid
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpanderInDataGrid"/> class.
        /// </summary>
        public ExpanderInDataGrid()
        {
            InitializeComponent();
        }

        #region DependencyProperties

        public double SlopeValue
        {
            get { return (double) GetValue(SlopeValueProperty); }
            set { SetValue(SlopeValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SlopeValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SlopeValueProperty =
            DependencyProperty.Register("SlopeValue", typeof(double), typeof(ExpanderInDataGrid),
                new PropertyMetadata(0.0, OnSlopeChange));

        private static void OnSlopeChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }


        public double InterceptValue
        {
            get { return (double) GetValue(InterceptValueProperty); }
            set { SetValue(InterceptValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InterceptValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InterceptValueProperty =
            DependencyProperty.Register("InterceptValue", typeof(double), typeof(ExpanderInDataGrid),
                new PropertyMetadata(0.0, OnInterceptValueChange));

        private static void OnInterceptValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public bool ExpanderInDataGridEnable
        {
            get { return (bool) GetValue(ExpanderInDataGridEnableProperty); }
            set { SetValue(ExpanderInDataGridEnableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExpanderInDataGridEnable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpanderInDataGridEnableProperty =
            DependencyProperty.Register("ExpanderInDataGridEnable", typeof(bool), typeof(ExpanderInDataGrid),
                new PropertyMetadata(false));


        /// <summary>
        /// Gets or sets a value indicating whether [update adjusted value].
        /// </summary>
        /// <value><c>true</c> if [update adjusted value]; otherwise, <c>false</c>.</value>
        public bool UpdateAdjustedValue
        {
            get { return (bool) GetValue(UpdateAdjustedValueProperty); }
            set { SetValue(UpdateAdjustedValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpdateAdjustedValue.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The update adjusted value property
        /// </summary>
        public static readonly DependencyProperty UpdateAdjustedValueProperty =
            DependencyProperty.Register("UpdateAdjustedValue", typeof(bool), typeof(ExpanderInDataGrid),
                new PropertyMetadata(UpdateAdjustedValueCallBack));

        /// <summary>
        /// Gets or sets a value indicating whether this instance is status completed.
        /// </summary>
        /// <value><c>true</c> if this instance is status completed; otherwise, <c>false</c>.</value>
        public bool IsStatusCompleted
        {
            get { return (bool) GetValue(IsStatusCompletedProperty); }
            set { SetValue(IsStatusCompletedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsStatusCompleted.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The is status completed property
        /// </summary>
        public static readonly DependencyProperty IsStatusCompletedProperty =
            DependencyProperty.Register("IsStatusCompleted", typeof(bool), typeof(ExpanderInDataGrid),
                new PropertyMetadata(StatusCompletedCallBack));

        /// <summary>
        /// The expander column header list property
        /// </summary>
        public static readonly DependencyProperty ExpanderColumnHeaderListProperty = DependencyProperty.Register(
            "ExpanderColumnHeaderList", typeof(ObservableCollection<DataGridExpanderColumnHeader>),
            typeof(ExpanderInDataGrid), new PropertyMetadata(ExpanderColumnHeaderListCallBack));

        /// <summary>
        /// Gets or sets the expander column header list.
        /// </summary>
        /// <value>The expander column header list.</value>
        public ObservableCollection<DataGridExpanderColumnHeader> ExpanderColumnHeaderList
        {
            get
            {
                return (ObservableCollection<DataGridExpanderColumnHeader>) GetValue(ExpanderColumnHeaderListProperty);
            }
            set { SetValue(ExpanderColumnHeaderListProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected sample item.
        /// </summary>
        /// <value>The selected sample item.</value>
        public DataGridExpanderColumnHeader SelectedSampleItem
        {
            get { return (DataGridExpanderColumnHeader) GetValue(SelectedSampleItemProperty); }
            set { SetValue(SelectedSampleItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedSampleItem.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The selected sample item property
        /// </summary>
        public static readonly DependencyProperty SelectedSampleItemProperty =
            DependencyProperty.Register("SelectedSampleItem", typeof(DataGridExpanderColumnHeader),
                typeof(ExpanderInDataGrid), new PropertyMetadata(null));

        #endregion

        #region DependencyPropertyCallBacks

        /// <summary>
        /// Updates the adjusted value call back.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void UpdateAdjustedValueCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ucExpanderInDataGrid = d as ExpanderInDataGrid;
            if (ucExpanderInDataGrid == null)
                return;

            var view =
                (CollectionView) CollectionViewSource.GetDefaultView(ucExpanderInDataGrid.ExpanderColumnHeaderList);

            if (view == null)
                return;
            var groupCouunt = view.GroupDescriptions.Count;
            var AssayValueCount = ucExpanderInDataGrid.ExpanderColumnHeaderList.GroupBy(x => x.AssayValue).Count();
            if (AssayValueCount > groupCouunt)
            {
                view.GroupDescriptions.Clear();
                var groupDescription = new PropertyGroupDescription("AssayValue");
                view?.GroupDescriptions?.Add(groupDescription);
            }

            if (!ucExpanderInDataGrid.ExpanderColumnHeaderList.Count.Equals(1))
                return;
        }

        /// <summary>
        /// Expanders the column header list call back.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void ExpanderColumnHeaderListCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ucExpanderInDataGrid = d as ExpanderInDataGrid;
            if (ucExpanderInDataGrid == null)
                return;
            if (ucExpanderInDataGrid.ExpanderColumnHeaderList == null)
                return;
            if (ucExpanderInDataGrid.ExpanderColumnHeaderList.Count.Equals(0))
                return;
            var view = (CollectionView) CollectionViewSource.GetDefaultView(ucExpanderInDataGrid
                .ExpanderColumnHeaderList);
            var groupDescription = new PropertyGroupDescription("AssayValue");
            view?.GroupDescriptions?.Add(groupDescription);
        }

        /// <summary>
        /// Statuses the completed call back.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void StatusCompletedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ucExpanderInDataGrid = d as ExpanderInDataGrid;
            if (ucExpanderInDataGrid == null)
                return;
            var view =
                (CollectionView) CollectionViewSource.GetDefaultView(ucExpanderInDataGrid.ExpanderColumnHeaderList);
            if (ucExpanderInDataGrid.ExpanderColumnHeaderList == null)
                return;
            if (ucExpanderInDataGrid.ExpanderColumnHeaderList.Any(x => x.Adjusted.Equals(Double.NaN)))
                return;
            ucExpanderInDataGrid.UpdatedAdjustedStatus();
            ucExpanderInDataGrid.SetAvgAdjValue();
            view.GroupDescriptions.Clear();
            var groupDescription = new PropertyGroupDescription("AssayValue");
            view?.GroupDescriptions?.Add(groupDescription);
            if (!ucExpanderInDataGrid.ExpanderColumnHeaderList.Count.Equals(1))
                return;         
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Updateds the adjusted status.
        /// </summary>
        internal void UpdatedAdjustedStatus()
        {
            if (ExpanderColumnHeaderList == null)
                return;

            bool Status = true;
            ExpanderInDataGridEnable = Status;
            IsStatusCompleted = false;
        }
        
        internal void SetAvgAdjValue()
        {
            // This method used to contain the code in the service method below. I put it in the service but
            // I was unsuccessful in my attempt to use Ninject to inject the service into this UIControl.
            // Instead, I just made a normal instance of the service with some dummy parameter and moved on
            // with life.
            var logger = new Log4NetLogger(""); // dummy logger only for the ability to access this service
            var concentrationService = new ConcentrationSlopeService(logger);
            concentrationService.UpdateAvgAdjustedValues(ExpanderColumnHeaderList);
        }

        #endregion
    }
}