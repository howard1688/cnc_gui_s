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
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;

namespace cnc_gui
{
    /// <summary>
    /// menu_bar.xaml 的互動邏輯
    /// </summary>
    public partial class menu_bar : Window
    {
        public menu_bar()
        {
            InitializeComponent();
            Main_menu_bar.Content = new home();

        }

        private void Button_Click_home(object sender, RoutedEventArgs e)
        {
            Main_menu_bar.Content = new home();
        }

        private void Button_Click_cam(object sender, RoutedEventArgs e)
        {
            Main_menu_bar.Content = new cam();
        }

        private void Button_Click_energy(object sender, RoutedEventArgs e)
        {
            Main_menu_bar.Content = new energy();
        }

        private void Button_Click_setting(object sender, RoutedEventArgs e)
        {
            Main_menu_bar.Content = new setting();
        }

        private void Button_Click_clear(object sender, RoutedEventArgs e)
        {
            Main_menu_bar.Content = new clear();
        }

    }
}
