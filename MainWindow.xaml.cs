using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Memory
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Die Methode InitializeComponent wird von der automatisch generierten Datei MainWindow.g.i.cs bereitgestellt,
            // die durch das Kompilieren der XAML-Datei MainWindow.xaml erzeugt wird.
            // Wenn die Datei fehlt oder nicht korrekt eingebunden ist, überprüfen Sie:
            // - Existiert die Datei MainWindow.xaml im Projekt?
            // - Ist das Build Action der XAML-Datei auf "Page" gesetzt?
            // - Ist die Klasse MainWindow als "partial" deklariert?
            // - Stimmen die Namen und Namespaces überein?
            InitializeComponent();
        }

        private void BtnNewGame_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}