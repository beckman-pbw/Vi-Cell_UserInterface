using ScoutUtilities.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ScoutUI.Views.ucCommon
{
    public class LineDrawer : Disposable
    {
        public Line FirstLine { get; set; }

        public bool CanFirstLineShow { get; set; }
        public bool CanSecondLineShow { get; set; }

        public LineDrawer()
        {
            FirstLine = new Line();
            CanFirstLineShow = false;
            CanSecondLineShow = false;
        }

        #region Disposable Support


        protected override void DisposeManaged()
        {
            FirstLine = null;
        }

        #endregion

        public void ResetLines()
        {
            FirstLine.X1 = 0;
            FirstLine.X2 = 0;
            FirstLine.Y1 = 0;
            FirstLine.Y2 = 0;
        }

        public void ShowLines(Panel parentControl, Line line)
        {
            parentControl.Children.Add(line);
        }

        public void UnShowLines(Panel parentControl, Line line)
        {
            if (parentControl.Children.Contains(line))
            {
                parentControl.Children.Remove(line);
            }
        }

        public Line DrawLine(Control userControl, Panel parentControl,
            System.Windows.Controls.DataVisualization.Charting.ColumnSeries chartSeries, Line line, bool IsFirstLine,
            double x, out bool canLineDisplayed)
        {
            canLineDisplayed = false;

            if (parentControl.Children.Contains(line))
            {
                parentControl.Children.Remove(line);
                line = new Line();
            }

            line = ConfigureLinePosition(userControl, parentControl, chartSeries, line, x);
            if (IsFirstLine)
                FirstLine = line;       
            line = ConfigureLineStyle(line);
            canLineDisplayed = true;
            return line;
        }

        public Line ConfigureLinePosition(Control control, Panel parentControl,
            System.Windows.Controls.DataVisualization.Charting.ColumnSeries chartSeries, Line line, double x)
        {
            GeneralTransform gt = chartSeries.TransformToVisual(parentControl);
            Point p = gt.Transform(new Point(0, 0));
            line.X1 = x;
            line.Y1 = p.Y;
            line.X2 = line.X1;
            line.Y2 = p.Y + chartSeries.ActualHeight;
            return line;
        }

        public Line ConfigureLineStyle(Line line)
        {
            line.Stroke = new SolidColorBrush(Colors.Blue);
            line.StrokeThickness = 3;
            line.StrokeStartLineCap = PenLineCap.Flat;
            line.StrokeEndLineCap = PenLineCap.Flat;
            line.StrokeDashCap = PenLineCap.Flat;
            return line;
        }
    }
}