using System;
using System.Windows;
using System.Linq;

namespace SampleHttpServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Server server = new Server();
        public MainWindow()
        {
            InitializeComponent();
            server.OnLogWrite = Server_OnLogWrite;
        }
        private void Server_OnLogWrite(string text)
        {
            #region listbox追加用
            this.listBox.Dispatcher.BeginInvoke(
                new Action(() => {
                    listBox.Items.Add(text);
                }));
            #endregion
            #region コンソール出力用
            //Console.WriteLine(text);
            #endregion
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
