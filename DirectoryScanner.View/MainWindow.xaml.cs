using System.Windows;
using DirectoryScanner.ViewModel;

namespace DirectoryScanner.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DirectoryScannerViewModel viewModel = new DirectoryScannerViewModel();
            DataContext = viewModel;

            InitializeComponent();
        }
    }
}
