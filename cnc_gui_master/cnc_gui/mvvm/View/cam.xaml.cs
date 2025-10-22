using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Configuration;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using static cnc_gui.DatabaseModel;

namespace cnc_gui
{
    /// <summary>
    /// cam.xaml 的互動邏輯
    /// </summary>
    public partial class cam : Page
    {
        private DatabaseModel dbModel;
        camViewModel viewModel = new camViewModel();
        public cam()
        {
            dbModel = new DatabaseModel();
            viewModel = new camViewModel();
            this.DataContext = viewModel;
            InitializeComponent();

            //flusher_lv1_str.Text = (string)dbModel.GetColumnValueById("Level_set_to_string", "Level_1_to_str", 1);
            //flusher_lv2_str.Text = (string)dbModel.GetColumnValueById("Level_set_to_string", "Level_2_to_str", 1);
            //flusher_lv3_str.Text = (string)dbModel.GetColumnValueById("Level_set_to_string", "Level_3_to_str", 1);
           //flusher_lv4_str.Text = (string)dbModel.GetColumnValueById("Level_set_to_string", "Level_4_to_str", 1);
            //flusher_lv5_str.Text = (string)dbModel.GetColumnValueById("Level_set_to_string", "Level_5_to_str", 1);

            flusher_lv1_time.Text = dbModel.GetColumnValueById("Level_set_time", "Level_1_time", 1).ToString();
            flusher_lv2_time.Text = dbModel.GetColumnValueById("Level_set_time", "Level_2_time", 1).ToString();
            flusher_lv3_time.Text = dbModel.GetColumnValueById("Level_set_time", "Level_3_time", 1).ToString();
            flusher_lv4_time.Text = dbModel.GetColumnValueById("Level_set_time", "Level_4_time", 1).ToString();
            flusher_lv5_time.Text = dbModel.GetColumnValueById("Level_set_time", "Level_5_time", 1).ToString();

        }
    }
}
