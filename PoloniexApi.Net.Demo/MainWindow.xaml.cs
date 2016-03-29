using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Jojatekok.PoloniexAPI.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set icon from the assembly
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToImageSource();

            var LendingWindow = new LendingWindow();
            TabItem LendingTab = new TabItem();
            LendingTab.Header = "Lending";
            LendingTab.Content = LendingWindow.Content;
            LendingTab.Background = System.Windows.Media.Brushes.Red;
            TabMain.Items.Add(LendingTab);

            var ExchangeWindow = new ExchangeWindow();
            TabItem ExchangeTab = new TabItem();
            ExchangeTab.Header = "Exchange";
            ExchangeTab.Content = ExchangeWindow.Content;
            ExchangeTab.Background = System.Windows.Media.Brushes.Yellow;
            TabMain.Items.Add(ExchangeTab);

            var AccountWindow = new AccountWindow();
            TabItem AccountTab = new TabItem();
            AccountTab.Header = "Account";
            AccountTab.Content = AccountWindow.Content;
            AccountTab.Background = System.Windows.Media.Brushes.Green;
            TabMain.Items.Add(AccountTab);

        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

    }
}
