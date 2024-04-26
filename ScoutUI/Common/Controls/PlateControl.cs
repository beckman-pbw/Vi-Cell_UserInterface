using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using System.Collections.Generic;
using System.Windows.Controls;
using ScoutUtilities.Services;

namespace ScoutUI.Common.Controls
{
    public class PlateControl : ItemsControl
    {
        public PlateControl()
        {
            SampleGridRowHeaderButtons = new List<SampleGridHeaderViewModel>();
            for (var i = 0; i < ApplicationConstants.PlateNumRowsCount; i++)
            {
                var str = RowPositionHelper.GetRowChar((RowPosition)i).ToString();
                SampleGridRowHeaderButtons.Add(new SampleGridHeaderViewModel(str, new SolidColorBrushService()));
            }

            SampleGridColumnHeaderButtons = new List<SampleGridHeaderViewModel>();
            for (var j = 0; j < ApplicationConstants.PlateNumColumnsCount; j++)
            {
                var str = (j + 1).ToString();
                SampleGridColumnHeaderButtons.Add(new SampleGridHeaderViewModel(str, new SolidColorBrushService()));
            }
        }

        public List<SampleGridHeaderViewModel> SampleGridRowHeaderButtons { get; set; }
        public List<SampleGridHeaderViewModel> SampleGridColumnHeaderButtons { get; set; }
    }
}