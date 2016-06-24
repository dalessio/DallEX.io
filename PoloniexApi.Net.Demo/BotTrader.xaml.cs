using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DallEX.io.View
{
    /// <summary>
    /// Interaction logic for BotTrader.xaml
    /// </summary>
    public partial class BotTrader : Page
    {
        private static readonly MainWindow MainWindow = (MainWindow)Application.Current.MainWindow;

        public BotTrader()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.SendBaloonMessage("Bot Trader", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.None);
        }

    }
}
