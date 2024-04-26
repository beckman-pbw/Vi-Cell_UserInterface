using ScoutUtilities.Enums;
using System;
using System.Windows;
using System.Windows.Input;

namespace ScoutUI.Views.UserControls
{
    /// <summary>
    /// Interaction logic for FineAdjustmentTextBox.xaml
    /// </summary>
    public partial class FineAdjustmentTextBox
    {
        public bool IsBrightFieldActive
        {
            get { return (bool) GetValue(IsBrightFieldActiveProperty); }
            set { SetValue(IsBrightFieldActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsBrightFieldActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsBrightFieldActiveProperty =
            DependencyProperty.Register("IsBrightFieldActive", typeof(bool), typeof(FineAdjustmentTextBox),
                new PropertyMetadata(false));


        public double StartRange
        {
            get { return (double) GetValue(StartRangeProperty); }
            set { SetValue(StartRangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartRange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartRangeProperty =
            DependencyProperty.Register("StartRange", typeof(double), typeof(FineAdjustmentTextBox),
                new PropertyMetadata(0.0));

        public double EndRange
        {
            get { return (double) GetValue(EndRangeProperty); }
            set { SetValue(EndRangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndRange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndRangeProperty =
            DependencyProperty.Register("EndRange", typeof(double), typeof(FineAdjustmentTextBox),
                new PropertyMetadata(100.0));

        public double FineAdjustment
        {
            get { return (double) GetValue(FineAdjustmentProperty); }
            set { SetValue(FineAdjustmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FineAdjustment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FineAdjustmentProperty =
            DependencyProperty.Register("FineAdjustment", typeof(double), typeof(FineAdjustmentTextBox));


        public double Adjustment
        {
            get { return (double) GetValue(AdjustmentProperty); }
            set { SetValue(AdjustmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Adjustment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdjustmentProperty =
            DependencyProperty.Register("Adjustment", typeof(double), typeof(FineAdjustmentTextBox),
                new PropertyMetadata(null));


        public Visibility AdjustmentButtonVisibility
        {
            get { return (Visibility) GetValue(AdjustmentButtonVisibilityProperty); }
            set { SetValue(AdjustmentButtonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AdjustmentButtonVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdjustmentButtonVisibilityProperty =
            DependencyProperty.Register("AdjustmentButtonVisibility", typeof(Visibility), typeof(FineAdjustmentTextBox),
                new PropertyMetadata(null));


        public double AdjustmentValue
        {
            get { return (double) GetValue(AdjustmentValueProperty); }
            set { SetValue(AdjustmentValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AdjustmentValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdjustmentValueProperty =
            DependencyProperty.Register("AdjustmentValue", typeof(double), typeof(FineAdjustmentTextBox));

        public Action<OpticsAction, double> ValueUpdateCallBack
        {
            get { return (Action<OpticsAction, double>) GetValue(ValueUpdateCallBackProperty); }
            set { SetValue(ValueUpdateCallBackProperty, value);}
        }

        public static readonly DependencyProperty ValueUpdateCallBackProperty =
            DependencyProperty.Register("ValueUpdateCallBack", typeof(Action<OpticsAction, double>), typeof(FineAdjustmentTextBox), new PropertyMetadata());

        public OpticsAction OpticsAction
        {
            get { return (OpticsAction)GetValue(OpticsActionProperty); }
            set { SetValue(OpticsActionProperty, value); }
        }

        public static readonly DependencyProperty OpticsActionProperty =
            DependencyProperty.Register("OpticsAction", typeof(OpticsAction), typeof(FineAdjustmentTextBox));

        public FineAdjustmentTextBox()
        {
            InitializeComponent();
            AdjustmentValue = 0;
            Adjustment = 0;
            FineAdjustment = 0;
        }

        private ICommand _adjustmentCommand;

        public ICommand AdjustmentCommand
        {
            get
            {
                if (_adjustmentCommand == null)
                {
                    _adjustmentCommand = new AdjustmentCommand(this);
                }

                return _adjustmentCommand;
            }
            set { _adjustmentCommand = value; }
        }

        public AdjustValue AdjustState
        {
            get { return (AdjustValue) GetValue(AdjustStateProperty); }
            set { SetValue(AdjustStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AdjustState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdjustStateProperty =
            DependencyProperty.Register("AdjustState", typeof(AdjustValue), typeof(FineAdjustmentTextBox),
                new PropertyMetadata(null));
    }

    class AdjustmentCommand : ICommand
    {
        private readonly FineAdjustmentTextBox Model;

        public AdjustmentCommand(FineAdjustmentTextBox model)
        {
            Model = model;
        }

#pragma warning disable CS0067 
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var adjustedValue = 0.0;
            var roundedValue = Math.Round(Model.AdjustmentValue, 2);
            var isValidationSucceed = false;
            switch (parameter.ToString())
            {
                case "Left":
                    Model.AdjustState = AdjustValue.Left;
                    if (roundedValue - Model.FineAdjustment >= Model.StartRange)
                    {
                        adjustedValue = roundedValue - Model.FineAdjustment;
                        isValidationSucceed = true;
                    }
                    break;
                case "Right":
                    Model.AdjustState = AdjustValue.Right;
                    if (roundedValue + Model.FineAdjustment <= Model.EndRange)
                    { 
                        adjustedValue = roundedValue + Model.FineAdjustment;
                        isValidationSucceed = true;
                    }
                    break;
                case "LeftForward":
                    Model.AdjustState = AdjustValue.LeftForward;
                    if (roundedValue - Model.Adjustment >= Model.StartRange)
                    { 
                        adjustedValue = roundedValue - Model.Adjustment;
                        isValidationSucceed = true;
                    }
                    break;
                case "RightForward":
                    Model.AdjustState = AdjustValue.RightForward;
                    if (roundedValue + Model.Adjustment <= Model.EndRange)
                    { 
                        adjustedValue = roundedValue + Model.Adjustment;
                        isValidationSucceed = true;
                    }
                    break;
            }

            if (Model.OpticsAction == OpticsAction.FocusPosition)
            {
                OnvokeValueUpdateCallback(adjustedValue);
                return;
            }

            if (!isValidationSucceed)
                return;

            if (Model.OpticsAction == OpticsAction.BrightField)
                OnvokeValueUpdateCallback(adjustedValue);
            else
                Model.AdjustmentValue = adjustedValue;

            HandleIsEnable(adjustedValue);
        }

        private void OnvokeValueUpdateCallback(double adjustedValue)
        {
            if(Model.ValueUpdateCallBack == null)
                return;

            Model.ValueUpdateCallBack(Model.OpticsAction, adjustedValue);
        }

        private void HandleIsEnable(double adjustedValue)
        {
            Model.btnLeft.IsEnabled = Model.btnLeftForward.IsEnabled = Model.btnRight.IsEnabled =
                Model.btnRightForward.IsEnabled = true;

            adjustedValue = Math.Round(Model.AdjustmentValue, 2);
            if (adjustedValue - Model.FineAdjustment < Model.StartRange)
                Model.btnLeft.IsEnabled = false;

            if (adjustedValue + Model.FineAdjustment > Model.EndRange)
                Model.btnRight.IsEnabled = false;

            if (adjustedValue - Model.Adjustment < Model.StartRange)
                Model.btnLeftForward.IsEnabled = false;

            if (adjustedValue + Model.Adjustment > Model.EndRange)
                Model.btnRightForward.IsEnabled = false;
        }
    }
}