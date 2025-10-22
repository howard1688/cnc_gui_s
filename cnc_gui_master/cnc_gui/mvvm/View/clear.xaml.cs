using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static Focas;

namespace cnc_gui
{
    /// <summary>
    /// clear.xaml 的互動邏輯
    /// </summary>
    public partial class clear : Page
    {
        private DatabaseModel dbModel;
        clearViewModel viewModel = new clearViewModel();
        private DispatcherTimer timer;

        public clear()
        {
            dbModel = new DatabaseModel();
            this.DataContext = viewModel;
            InitializeComponent();
           //excluder_lv1_str.Text = (string)dbModel.GetColumnValueById("Level_set_to_string", "Level_1_to_str", 2);
            //excluder_lv2_str.Text = (string)dbModel.GetColumnValueById("Level_set_to_string", "Level_2_to_str", 2);
            //excluder_lv3_str.Text = (string)dbModel.GetColumnValueById("Level_set_to_string", "Level_3_to_str", 2);
            //excluder_lv4_str.Text = (string)dbModel.GetColumnValueById("Level_set_to_string", "Level_4_to_str", 2);
            //excluder_lv5_str.Text = (string)dbModel.GetColumnValueById("Level_set_to_string", "Level_5_to_str", 2);

            excluder_lv1_time.Text = dbModel.GetColumnValueById("Level_set_time", "Level_1_time", 2).ToString();
            excluder_lv2_time.Text = dbModel.GetColumnValueById("Level_set_time", "Level_2_time", 2).ToString();
            excluder_lv3_time.Text = dbModel.GetColumnValueById("Level_set_time", "Level_3_time", 2).ToString();
            excluder_lv4_time.Text = dbModel.GetColumnValueById("Level_set_time", "Level_4_time", 2).ToString();
            excluder_lv5_time.Text = dbModel.GetColumnValueById("Level_set_time", "Level_5_time", 2).ToString();
        }

    }
}
