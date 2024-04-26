using ScoutUtilities.Interfaces;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ScoutUI.Common.Controls
{
    public class DataGridExtended : DataGrid
    {
        private bool _widthChanged;
        private DataGridColumn _changingColumn;
        private readonly DependencyPropertyDescriptor _widthPropertyDescriptor;
        public EventHandler WidthPropertyChangedHandler;

        public DataGridExtended()
        {
            _widthPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
            WidthPropertyChangedHandler = OnWidthPropertyChanged;
            Loaded += ExtendedDataGrid_Loaded;
            Unloaded += ExtendedDataGrid_Unloaded;
        }

        #region Event Handlers

        private void ExtendedDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var column in Columns)
            {
                _widthPropertyDescriptor.AddValueChanged(column, WidthPropertyChangedHandler);
            }
        }

        private void ExtendedDataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var column in Columns)
            {
                _widthPropertyDescriptor.RemoveValueChanged(column, WidthPropertyChangedHandler);
            }
        }

        private void OnWidthPropertyChanged(object sender, EventArgs x)
        {
            _widthChanged = true;
            if (sender is DataGridColumn dgTextColumn)
            {
                _changingColumn = dgTextColumn;
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            CheckAndUpdate();
            base.OnPreviewMouseLeftButtonUp(e);
        }

        protected override void OnPreviewTouchUp(TouchEventArgs e)
        {
            CheckAndUpdate();
            base.OnPreviewTouchUp(e);
        }

        private void CheckAndUpdate()
        {
            if (_widthChanged)
            {
                _widthChanged = false;
                UpdateColumnInfo();
            }
        }

        #endregion

        #region Private Methods

        private void UpdateColumnInfo()
        {
            if (!(_widthPropertyDescriptor.GetValue(_changingColumn) is DataGridLength dgLength))
                return;

            var bindingPropertyName = string.Empty;
            if (_changingColumn is DataGridTextColumnExtended dgTextColumnExt)
            {
                bindingPropertyName = dgTextColumnExt.BindingName;
            }
            else if (_changingColumn is DataGridTemplateColumnExtended dgTemplateColumn)
            {
                bindingPropertyName = dgTemplateColumn.BindingName;
            }
            else if (_changingColumn is DataGridTextColumn dgTextColumn && dgTextColumn.Binding is Binding textBinding)
            {
                bindingPropertyName = textBinding.Path?.Path;
            }
            else if (_changingColumn is DataGridCheckBoxColumn dgCheckBox && dgCheckBox.Binding is Binding cbBinding)
            {
                bindingPropertyName = cbBinding.Path?.Path;
            }

            if (DataContext is IUpdateColumns vmWithUpdateColumns && !string.IsNullOrEmpty(bindingPropertyName))
            {
                vmWithUpdateColumns.UpdateColumns(bindingPropertyName, dgLength.Value);
            }
        }

        #endregion
    }
}