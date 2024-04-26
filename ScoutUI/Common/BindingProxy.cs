﻿using System.Windows;

namespace ScoutUI.Common
{
    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        public object Data
        {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            nameof(Data), typeof(object), typeof(BindingProxy),
            new UIPropertyMetadata(null));
    }
}