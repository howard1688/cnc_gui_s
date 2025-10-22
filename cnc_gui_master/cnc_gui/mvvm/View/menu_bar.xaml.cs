using System.Windows;
using System.Windows.Input;

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
            this.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };


        }
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove(); // 讓視窗可拖動
            }
        }

        // 最小化
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // 最大化 / 還原
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        // 關閉視窗
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
