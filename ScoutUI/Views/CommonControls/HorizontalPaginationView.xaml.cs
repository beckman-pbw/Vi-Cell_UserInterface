// ***********************************************************************
// Assembly         : ImageControl
// Author           : 20115954
// Created          : 07-17-2017
//
// Last Modified By : 20115954
// Last Modified On : 07-18-2017
// ***********************************************************************
// <copyright file="ucHorizontalPaginationView.xaml.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ScoutUI.Views.ucCommon
{
    /// <summary>
    /// Interaction logic for ucHorizontalPaginationView.xaml
    /// </summary>
    public partial class HorizontalPaginationView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HorizontalPaginationView"/> class.
        /// </summary>
        public HorizontalPaginationView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the traversal command.
        /// </summary>
        /// <value>The traversal command.</value>
        public ICommand TraversalCommand
        {
            get { return (ICommand) GetValue(TraversalCommandProperty); }
            set { SetValue(TraversalCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TraversalCommand.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The traversal command property
        /// </summary>
        public static readonly DependencyProperty TraversalCommandProperty =
            DependencyProperty.Register("TraversalCommand", typeof(ICommand), typeof(HorizontalPaginationView),
                new PropertyMetadata());


        /// <summary>
        /// Gets or sets the command parameter previous.
        /// </summary>
        /// <value>The command parameter previous.</value>
        public object CommandParamPrevious
        {
            get { return (object) GetValue(CommandParamPreviousProperty); }
            set { SetValue(CommandParamPreviousProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParamPrevious.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The command parameter previous property
        /// </summary>
        public static readonly DependencyProperty CommandParamPreviousProperty =
            DependencyProperty.Register("CommandParamPrevious", typeof(object), typeof(HorizontalPaginationView),
                new PropertyMetadata());

        /// <summary>
        /// Gets or sets the command parameter next.
        /// </summary>
        /// <value>The command parameter next.</value>
        public object CommandParamNext
        {
            get { return (object) GetValue(CommandParamNextProperty); }
            set { SetValue(CommandParamNextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParamNext.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The command parameter next property
        /// </summary>
        public static readonly DependencyProperty CommandParamNextProperty =
            DependencyProperty.Register("CommandParamNext", typeof(object), typeof(HorizontalPaginationView),
                new PropertyMetadata());


        /// <summary>
        /// Gets or sets the total image count.
        /// </summary>
        /// <value>The total image count.</value>
        public int TotalImageCount
        {
            get { return (int) GetValue(TotalImageCountProperty); }
            set { SetValue(TotalImageCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotalImageCount.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The total image count property
        /// </summary>
        public static readonly DependencyProperty TotalImageCountProperty =
            DependencyProperty.Register("TotalImageCount", typeof(int), typeof(HorizontalPaginationView));

        public KeyValuePair<int, string> SelectedImageIndex
        {
            get { return (KeyValuePair<int, string>) GetValue(SelectedImageIndexProperty); }
            set { SetValue(SelectedImageIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedImageIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedImageIndexProperty =
            DependencyProperty.Register("SelectedImageIndex", typeof(KeyValuePair<int, string>),
                typeof(HorizontalPaginationView), new PropertyMetadata(SelectedImageIndexPropertyChangedCallBack));

        private static void SelectedImageIndexPropertyChangedCallBack(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
        }

        //private int value;

        public ObservableCollection<KeyValuePair<int, string>> ImageIndexList
        {
            get { return (ObservableCollection<KeyValuePair<int, string>>) GetValue(ImageIndexListProperty); }
            set { SetValue(ImageIndexListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageIndexList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageIndexListProperty =
            DependencyProperty.Register("ImageIndexList", typeof(ObservableCollection<KeyValuePair<int, string>>),
                typeof(HorizontalPaginationView), new PropertyMetadata(null));
    }
}