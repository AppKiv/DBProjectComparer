using DBProjectComparer.ViewModel;
using System.Windows;

namespace DBProjectComparer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var p = new ProjectComparerViewModel();
            DataContext = p;
            InitializeComponent();
        }
    }
}
