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
using LiveCharts;
using LiveCharts.Wpf;


namespace cnc_gui
{
    /// <summary>
    /// home.xaml 的互動邏輯
    /// </summary>
    public partial class home : Page
    {
        private setting settingsPage;
        public home()
        {
            settingsPage = new setting();
            settingsPage.LoadConfig();
            InitializeComponent();
            
            // 在初始化時，將 setting.xaml 中的靜態變數 CncIp 的值顯示在 TextBlock
            home_ip.Text = setting.Cncip;
            home_port.Text = setting.Cncport;
            flusher_lv1_str.Text = setting.flusherlevel_st[0];      
            flusher_lv2_str.Text = setting.flusherlevel_st[1];
            flusher_lv3_str.Text = setting.flusherlevel_st[2];      
            flusher_lv4_str.Text = setting.flusherlevel_st[3];
            flusher_lv5_str.Text = setting.flusherlevel_st[4];

            flusher_lv1_time.Text = setting.flusher_time[0];
            flusher_lv2_time.Text = setting.flusher_time[1];
            flusher_lv3_time.Text = setting.flusher_time[2];
            flusher_lv4_time.Text = setting.flusher_time[3];
            flusher_lv5_time.Text = setting.flusher_time[4];

            excluder_lv1_str.Text = setting.excluderlevel_st[0];
            excluder_lv2_str.Text = setting.excluderlevel_st[1];
            excluder_lv3_str.Text = setting.excluderlevel_st[2];
            excluder_lv4_str.Text = setting.excluderlevel_st[3];
            excluder_lv5_str.Text = setting.excluderlevel_st[4];

            excluder_lv1_time.Text = setting.excluder_time[0];
            excluder_lv2_time.Text = setting.excluder_time[1];
            excluder_lv3_time.Text = setting.excluder_time[2];
            excluder_lv4_time.Text = setting.excluder_time[3];
            excluder_lv5_time.Text = setting.excluder_time[4];
        }

        private void program_start_Checked(object sender, RoutedEventArgs e)
        {

            MessageBox.Show("程式已啟動");
                
            
        }
        private void program_stop_Checked(object sender, RoutedEventArgs e)
        {

            MessageBox.Show("程式已停止");


        }
    }
}
