// ***********************************************************************
// <copyright file="CompleteSampleViewModel.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace ScoutViewModels.ViewModels.Common
{
    /// <summary>
    /// Class CompleteSampleViewModel.
    /// </summary>
    /// <seealso cref="ScoutViewModels.ViewModels.ViewModelBase.BaseViewModel" />
    public class CompleteSampleViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the admin view model.
        /// </summary>
        /// <value>The admin view model.</value>
        private SettingsViewModel SettingsViewModel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteSampleViewModel" /> class.
        /// </summary>
        /// <param name="settingsViewModel">The admin view model.</param>
        public CompleteSampleViewModel(SettingsViewModel settingsViewModel)
        {
            SettingsViewModel = settingsViewModel;
        }
    }
}