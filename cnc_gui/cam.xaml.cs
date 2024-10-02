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
    /// cam.xaml 的互動邏輯
    /// </summary>
    public partial class cam : Page
    {
        public cam()
        {
            InitializeComponent();
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

        }
    }
}
