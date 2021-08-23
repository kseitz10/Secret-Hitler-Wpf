using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

using SecretHitler.UI.ViewModels;

namespace SecretHitler.UI.Views
{
    /// <summary>
    /// Interaction logic for GameSurfaceView.xaml
    /// </summary>
    public partial class GameSurfaceView : UserControl
    {
        public GameSurfaceView()
        {
            InitializeComponent();

            DataContextChanged += (s, e) =>
            {
                var oldVm = e.OldValue as GameSurfaceViewModel;
                var newVm = e.NewValue as GameSurfaceViewModel;

                if (oldVm != null)
                    oldVm.PropertyChanged -= OnViewModelPropChanged;

                if (newVm != null)
                    newVm.PropertyChanged += OnViewModelPropChanged;
            };
        }

        private void OnViewModelPropChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = DataContext as GameSurfaceViewModel;
            if (vm == null)
                return;

            // TODO No scrolljacking if user manually scrolled.
            if (e.PropertyName == nameof(GameSurfaceViewModel.Messages))
                MessagesTextBox.ScrollToEnd();
        }
    }
}
