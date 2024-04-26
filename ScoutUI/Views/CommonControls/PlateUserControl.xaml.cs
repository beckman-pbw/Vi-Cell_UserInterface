using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScoutUtilities.Enums;
using ScoutDomains.Common;

namespace ScoutUI.Views.ucCommon
{
    public partial class PlateUserControl
    {
        public UserControl GetQueueCreationView
        {
            get { return (UserControl) GetValue(GetQueueCreationViewProperty); }
            set { SetValue(GetQueueCreationViewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GetQueueCreationView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GetQueueCreationViewProperty =
            DependencyProperty.Register("GetQueueCreationView", typeof(UserControl), typeof(PlateUserControl), new PropertyMetadata(null));


        public ICommand UpdateGridCarouselStatus
        {
            get { return (ICommand) GetValue(UpdateGridCarouselStatusProperty); }
            set { SetValue(UpdateGridCarouselStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpdateGridCarouselStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpdateGridCarouselStatusProperty =
            DependencyProperty.Register("UpdateGridCarouselStatus", typeof(ICommand), typeof(PlateUserControl), new PropertyMetadata(null));


        public Visibility MotorRegStep2
        {
            get { return (Visibility) GetValue(MotorRegStep2Property); }
            set { SetValue(MotorRegStep2Property, value); }
        }

        // Using a DependencyProperty as the backing store for MotorRegStep2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MotorRegStep2Property =
            DependencyProperty.Register("MotorRegStep2", typeof(Visibility), typeof(PlateUserControl), new PropertyMetadata(Visibility.Collapsed));


        public Visibility MotorRegStep3
        {
            get { return (Visibility) GetValue(MotorRegStep3Property); }
            set { SetValue(MotorRegStep3Property, value); }
        }

        // Using a DependencyProperty as the backing store for MotorRegStep3.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MotorRegStep3Property =
            DependencyProperty.Register("MotorRegStep3", typeof(Visibility), typeof(PlateUserControl), new PropertyMetadata(Visibility.Collapsed));



        public Visibility IsAddRowColumnActive
        {
            get { return (Visibility) GetValue(IsAddRowColumnActiveProperty); }
            set { SetValue(IsAddRowColumnActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAddRowColumnActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAddRowColumnActiveProperty =
            DependencyProperty.Register("IsAddRowColumnActive", typeof(Visibility), typeof(PlateUserControl), new PropertyMetadata(Visibility.Hidden));


        public Visibility IsRunRowColumnActive
        {
            get { return (Visibility) GetValue(IsRunRowColumnActiveProperty); }
            set { SetValue(IsRunRowColumnActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRunRowColumnActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRunRowColumnActiveProperty =
            DependencyProperty.Register("IsRunRowColumnActive", typeof(Visibility), typeof(PlateUserControl), new PropertyMetadata(Visibility.Hidden));


        public bool IsRowWiseColumnWiseRunActive
        {
            get { return (bool) GetValue(IsRowWiseColumnWiseRunActiveProperty); }
            set { SetValue(IsRowWiseColumnWiseRunActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRowWiseColumnWiseRunActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRowWiseColumnWiseRunActiveProperty =
            DependencyProperty.Register("IsRowWiseColumnWiseRunActive", typeof(bool), typeof(PlateUserControl), new PropertyMetadata(false));


        #region DependencyProperty and Property

        public bool IsSelectedSampleClear
        {
            get { return (bool) GetValue(IsSelectedSampleClearProperty); }
            set { SetValue(IsSelectedSampleClearProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelectedSampleClear.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedSampleClearProperty =
            DependencyProperty.Register("IsSelectedSampleClear", typeof(bool), typeof(PlateUserControl),
                new PropertyMetadata(false, (Sender, e) => IsSelectedSampleClearChange(Sender as PlateUserControl)));

        private static void IsSelectedSampleClearChange(PlateUserControl ucCarouselGridUserControl)
        {
            ucCarouselGridUserControl.Position = string.Empty;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [multiple grid selection].
        /// </summary>
        /// <value><c>true</c> if [multiple grid selection]; otherwise, <c>false</c>.</value>
        public bool MultipleGridSelection
        {
            get { return (bool) GetValue(MultipleGridSelectionProperty); }
            set { SetValue(MultipleGridSelectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MultipleGridSelection.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The multiple grid selection property
        /// </summary>
        public static readonly DependencyProperty MultipleGridSelectionProperty =
            DependencyProperty.Register("MultipleGridSelection", typeof(bool), typeof(PlateUserControl), new PropertyMetadata(false));


        //  public bool MultipleGridSelection;

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public string Position
        {
            get { return (string) GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The position property
        /// </summary>
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(string), typeof(PlateUserControl), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value indicating whether this instance is uc carousel grid enable.
        /// </summary>
        /// <value><c>true</c> if this instance is uc carousel grid enable; otherwise, <c>false</c>.</value>
        public bool IsUcCarouselGridEnable
        {
            get { return (bool) GetValue(IsUcCarouselGridEnableProperty); }
            set { SetValue(IsUcCarouselGridEnableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsUcCarouselGridEnable.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The is uc carousel grid enable property
        /// </summary>
        public static readonly DependencyProperty IsUcCarouselGridEnableProperty =
            DependencyProperty.Register("IsUcCarouselGridEnable", typeof(bool), typeof(PlateUserControl), new PropertyMetadata(true));


        /// <summary>
        /// Gets or sets a value indicating whether this instance is row wise column wise.
        /// </summary>
        /// <value><c>true</c> if this instance is row wise column wise; otherwise, <c>false</c>.</value>
        public bool IsRowWiseColumnWise
        {
            get { return (bool) GetValue(IsRowWiseColumnWiseProperty); }
            set { SetValue(IsRowWiseColumnWiseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRowWiseColumnWise.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The is row wise column wise property
        /// </summary>
        public static readonly DependencyProperty IsRowWiseColumnWiseProperty =
            DependencyProperty.Register("IsRowWiseColumnWise", typeof(bool), typeof(PlateUserControl),
                new PropertyMetadata(true, (Sender, e) => IsRowWiseColumnWiseChange(Sender as PlateUserControl)));


        /// <summary>
        /// Gets or sets the selected sample grid.
        /// </summary>
        /// <value>The selected sample grid.</value>
        public SampleDomain SelectedSampleGrid
        {
            get { return (SampleDomain) GetValue(SelectedSampleGridProperty); }
            set { SetValue(SelectedSampleGridProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedSampleGrid.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The selected sample grid property
        /// </summary>
        public static readonly DependencyProperty SelectedSampleGridProperty =
            DependencyProperty.Register("SelectedSampleGrid", typeof(SampleDomain),
                typeof(PlateUserControl), new PropertyMetadata(null));


        /// <summary>
        /// My items source property
        /// </summary>
        public static readonly DependencyProperty MyItemsSourceProperty;

        /// <summary>
        /// Gets or sets my items source.
        /// </summary>
        /// <value>My items source.</value>
        public IList MyItemsSource
        {
            get { return (IList) GetValue(MyItemsSourceProperty); }
            set { SetValue(MyItemsSourceProperty, value); }
        }



        public GridCarouselType GridCarouselType
        {
            get { return (GridCarouselType)GetValue(GridCarouselTypeProperty); }
            set { SetValue(GridCarouselTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GridCarouselType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridCarouselTypeProperty =
            DependencyProperty.Register("GridCarouselType", typeof(GridCarouselType), typeof(PlateUserControl), 
                new PropertyMetadata(GridCarouselType.RunResult,OnChangeOfGridCarouselType));

        #endregion

        #region Command

        /// <summary>
        /// Gets or sets the insert row command.
        /// </summary>
        /// <value>The insert row command.</value>
        public ICommand InsertRowCommand { get; set; }

        #endregion

        #region Constructor

        public PlateUserControl()
        {
            InitializeComponent();
            MultipleGridSelection = true;
            InsertRowCommand = new SelectedSampleData(this);
            UpdateGridCarouselRowColumn(GridCarouselType.RunResult, this);
        }

        static PlateUserControl()
        {
            MyItemsSourceProperty = DependencyProperty.Register("MyItemsSource", typeof(IList), typeof(PlateUserControl));
        }

        #endregion

        #region Method

        private static void OnChangeOfGridCarouselType(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ucGridCarousel = (PlateUserControl)d;
            var gridCarouselType = (GridCarouselType) e.NewValue;
            UpdateGridCarouselRowColumn(gridCarouselType, ucGridCarousel);
        }

        private static void UpdateGridCarouselRowColumn(GridCarouselType gridCarouselType,
            PlateUserControl ucGridCarousel)
        {
            switch (gridCarouselType)
            {
                case GridCarouselType.QueueCreation:
                    ucGridCarousel.GdRow.Visibility = Visibility.Collapsed;
                    ucGridCarousel.GdColumn.Visibility = Visibility.Collapsed;
                    ucGridCarousel.GdQueueCreationRow.Visibility = Visibility.Visible;
                    ucGridCarousel.GdQueueCreationColumn.Visibility = Visibility.Visible;
                    ucGridCarousel.GdQueueCreationRowDirection.Visibility = Visibility.Visible;
                    break;
                case GridCarouselType.RunResult:
                case GridCarouselType.MotorRegistration:
                case GridCarouselType.CreateByRun:
                    ucGridCarousel.GdRow.Visibility = Visibility.Visible;
                    ucGridCarousel.GdColumn.Visibility = Visibility.Visible;
                    ucGridCarousel.GdQueueCreationRow.Visibility = Visibility.Collapsed;
                    ucGridCarousel.GdQueueCreationColumn.Visibility = Visibility.Collapsed;
                    ucGridCarousel.GdQueueCreationRowDirection.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Determines whether [is row wise column wise change] [the specified uc carousel grid user control].
        /// </summary>
        /// <param name="ucCarouselGridUserControl">The uc carousel grid user control.</param>
        /// <param name="oldValue">if set to <c>true</c> [old value].</param>
        /// <param name="newValue">if set to <c>true</c> [new value].</param>
        private static void IsRowWiseColumnWiseChange(PlateUserControl ucCarouselGridUserControl)
        {
            if (ucCarouselGridUserControl?.MyItemsSource?.Count > 0)
            {
                ucCarouselGridUserControl.Position = string.Empty;
                IList<SampleDomain> listSample = ucCarouselGridUserControl.MyItemsSource as IList<SampleDomain>;

                if (listSample != null)
                {
                    var result = listSample.Where(x => x.SampleStatusColor == SampleStatusColor.Selected);
                    int count = 0;
                    foreach (var item in result)
                    {
                        count++;

                        ucCarouselGridUserControl.Position =
                            ucCarouselGridUserControl.Position + item.RowWisePosition;
                        if (count < result.Count())
                        {
                            ucCarouselGridUserControl.Position = ucCarouselGridUserControl.Position + ",";
                        }
                    }
                }
            }
        }

        #endregion
    }

    class SelectedSampleData : ICommand
    {
        /// <summary>
        /// The uc carousel grid user control
        /// </summary>
        private PlateUserControl ucCarouselGridUserControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedSampleData" /> class.
        /// </summary>
        /// <param name="ucCarouselGridUserControl">The uc carousel grid user control.</param>
        public SelectedSampleData(PlateUserControl ucCarouselGridUserControl)
        {
            this.ucCarouselGridUserControl = ucCarouselGridUserControl;
        }

#pragma warning disable CS0067 
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            SampleDomain sendData = parameter as SampleDomain;

            switch (sendData?.SampleStatusColor)
            {
                case SampleStatusColor.Selected:
                    sendData.SampleStatusColor = SampleStatusColor.Empty;
                    break;
                case SampleStatusColor.Empty:
                    sendData.SampleStatusColor = SampleStatusColor.Selected;
                    break;
            }

            ucCarouselGridUserControl.Position = string.Empty;

            IList<SampleDomain> listSample =
                ucCarouselGridUserControl.MyItemsSource as IList<SampleDomain>;

            var result =
                listSample?.Where(
                    x => x.SampleStatusColor == SampleStatusColor.Selected || x.SampleStatusColor == SampleStatusColor.Defined).ToList() ??
                new List<SampleDomain>();

            int count = 0;
            foreach (var item in result)
            {
                count++;
                ucCarouselGridUserControl.Position = ucCarouselGridUserControl.Position + item.RowWisePosition;
                if (count < result.Count)
                {
                    ucCarouselGridUserControl.Position = ucCarouselGridUserControl.Position + ",";
                }
            }
        }
    }
}