using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Focas;
using System.Windows.Threading;
using System.Configuration;

namespace cnc_gui
{
    /// <summary>
    /// cam.xaml 的互動邏輯
    /// </summary>
    public partial class cam : Page
    {

        private setting settingsPage;

        private Random _random = new Random();
        private List<DateTime> _timestamps = new List<DateTime>(); // 
        private const int MaxDataPoints = 10;

        private DispatcherTimer timer;

        public cam()
        {
            settingsPage = new setting();
            settingsPage.LoadConfig();
            settingsPage.SaveConfig();
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

            flusher_run_time = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Sample Data",
                    Values = new ChartValues<double> { }
                }
            };

            Formatter = value =>
            {
                int index = (int)value;
                if (index >= 0 && index < _timestamps.Count)
                {
                    return _timestamps[index].ToString("HH:mm:ss");
                }
                return string.Empty;
            };

            DataContext = this;
            Task.Run(UpdateChart);

            UpdateProgressBars();

            // 初始化並配置 DispatcherTimer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10); // 設置為每 10 秒觸發一次
            timer.Tick += Timer_Tick; // 每次觸發執行的事件
            timer.Start();
            Task.Run(UpdateChart);
        }

        // 此方法用於載入設定並更新 UI
        private void Timer_Tick(object sender, EventArgs e)
        {
            settingsPage.LoadConfig(); // 重新載入配置
            UpdateProgressBars();      // 更新進度條
        }

        private void UpdateProgressBars()
        {
            flusher_level_bar.Value = setting.flusher_level_bar;
            flusher_level_cam.Text = setting.flusher_level_bar.ToString();

        }

        public SeriesCollection flusher_run_time { get; set; }
        public Func<double, string> Formatter { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private async Task UpdateChart()
        {
            while (true)
            {
                await Task.Delay(10000); // 每秒更新一次
                var currentTime = DateTime.Now;
                var newValue = _random.NextDouble() * 10; // 模擬新數據
                Application.Current.Dispatcher.Invoke(() =>
                {
                    flusher_run_time[0].Values.Add(newValue);
                    _timestamps.Add(currentTime);

                    if (flusher_run_time[0].Values.Count > MaxDataPoints)
                    {
                        flusher_run_time[0].Values.RemoveAt(0);
                        _timestamps.RemoveAt(0);
                    }
                });

                OnPropertyChanged(nameof(flusher_run_time));
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
