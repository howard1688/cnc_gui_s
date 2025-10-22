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
    /// energy.xaml 的互動邏輯
    /// </summary>
    public partial class energy : Page
    {
        private DatabaseModel dbModel;
        energyViewModel viewModel = new energyViewModel();

        public energy()
        {
            dbModel = new DatabaseModel();
            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}
