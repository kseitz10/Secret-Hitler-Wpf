using System;
using System.Collections.Generic;
using System.Linq;

using GalaSoft.MvvmLight;

using SecretHitler.UI.Interfaces;
using SecretHitler.UI.ViewModels;

namespace SecretHitler.App.ViewModels
{
    /// <summary>
    /// Viewmodel for the shell window.
    /// </summary>
    public class ShellViewModel : ViewModelBase, IShell
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        private ShellViewModel()
        {
            UpdateMainRegion(new HomeViewModel(Properties.Settings.Default, this));
        }

        #region Properties

        public static ShellViewModel Instance = new ShellViewModel();

        private ViewModelBase _mainRegion;

        /// <summary>
        /// Content for the shell.
        /// </summary>
        public ViewModelBase MainRegion => _mainRegion;

        #endregion

        #region Methods

        public void UpdateMainRegion(ViewModelBase newViewModel)
        {
            _mainRegion = newViewModel;
            RaisePropertyChanged(nameof(MainRegion));
        }

        #endregion
    }
}
