using System.Net;
using System.Windows;
using System.Configuration;
using System.Linq;

namespace SampleHttpServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Server server = new Server();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (!server.IsListening)
            {
                server.Start();
                button.Content = "送受信ソフト起動中";
            }
            else
            {
                server.Stop();
                button.Content = "送受信ソフト起動";
            }
        }
    }
}
