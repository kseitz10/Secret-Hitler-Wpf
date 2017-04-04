using System.Windows;
using SecretHitler.App.ViewModels;

namespace SecretHitler.App.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Shell : Window
    {
        public Shell()
        {
            InitializeComponent();
            DataContext = ShellViewModel.Instance;
        }
    }
}
