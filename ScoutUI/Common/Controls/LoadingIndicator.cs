using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common.Controls
{
   
    [TemplatePart(Name = "Border", Type = typeof(Border))]
    public class LoadingIndicator : Control
    {
      
        public static readonly DependencyProperty SpeedRatioProperty =
            DependencyProperty.Register("SpeedRatio", typeof(double), typeof(LoadingIndicator), new PropertyMetadata(1d,
                (o, e) =>
                {
                    LoadingIndicator li = (LoadingIndicator) o;
                    if (li.PART_Border == null || !li.IsActive)
                    {
                        return;
                    }
                    var visualStateGroup = VisualStateManager.GetVisualStateGroups(li.PART_Border);
                    if (visualStateGroup == null)
                        return;
                    foreach (VisualStateGroup group in visualStateGroup)
                    {
                        if (group.Name == "ActiveStates")
                        {
                            foreach (VisualState state in group.States)
                            {
                                if (state.Name == "Active")
                                {
                                    state.Storyboard.SetSpeedRatio(li.PART_Border, (double) e.NewValue);
                                }
                            }
                        }
                    }
                }));

  
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(LoadingIndicator), new PropertyMetadata(true,
                (o, e) =>
                {
                    LoadingIndicator li = (LoadingIndicator) o;
                    if (li.PART_Border == null)
                    {
                        return;
                    }
                    if ((bool) e.NewValue == false)
                    {
                        VisualStateManager.GoToElementState(li.PART_Border, "Inactive", false);
                        li.PART_Border.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        VisualStateManager.GoToElementState(li.PART_Border, "Active", false);
                        li.PART_Border.Visibility = Visibility.Visible;
                        var visualStateGroup = VisualStateManager.GetVisualStateGroups(li.PART_Border);
                        if (visualStateGroup == null)
                            return;
                        foreach (VisualStateGroup group in visualStateGroup)
                        {
                            if (group.Name == "ActiveStates")
                            {
                                foreach (VisualState state in group.States)
                                {
                                    if (state.Name == "Active")
                                    {
                                        state.Storyboard.SetSpeedRatio(li.PART_Border, li.SpeedRatio);
                                    }
                                }
                            }
                        }
                    }
                }));

   
        protected Border PART_Border;

        public double SpeedRatio
        {
            get { return (double) GetValue(SpeedRatioProperty); }
            set { SetValue(SpeedRatioProperty, value); }
        }

        public bool IsActive
        {
            get { return (bool) GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PART_Border = (Border) GetTemplateChild("PART_Border");
            if (PART_Border != null)
            {
                VisualStateManager.GoToElementState(PART_Border, (IsActive ? "Active" : "Inactive"), false);
                var visualStateGroup = VisualStateManager.GetVisualStateGroups(PART_Border);
                if (visualStateGroup == null)
                    return;
                foreach (VisualStateGroup group in visualStateGroup)
                {
                    if (group.Name == "ActiveStates")
                    {
                        foreach (VisualState state in group.States)
                        {
                            if (state.Name == "Active")
                            {
                                state.Storyboard.SetSpeedRatio(PART_Border, SpeedRatio);
                            }
                        }
                    }
                }

                PART_Border.Visibility = (IsActive ? Visibility.Visible : Visibility.Collapsed);
            }
        }
    }
}