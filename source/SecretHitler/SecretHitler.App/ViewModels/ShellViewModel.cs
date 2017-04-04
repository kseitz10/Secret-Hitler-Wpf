using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace SecretHitler.App.ViewModels
{
    /// <summary>
    /// Viewmodel for the shell window.
    /// </summary>
    public class ShellViewModel : ViewModelBase
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        private ShellViewModel()
        {
            UpdateMainRegion(new HomeViewModel());
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
