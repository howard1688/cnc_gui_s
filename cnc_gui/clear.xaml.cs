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

namespace cnc_gui
{
    /// <summary>
    /// clear.xaml 的互動邏輯
    /// </summary>
    public partial class clear : Page
    {
        public clear()
        {
            InitializeComponent();
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
    }
}
