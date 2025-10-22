using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using static cnc_gui.DatabaseModel;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
namespace cnc_gui
{
    internal class clearViewModel : INotifyPropertyChanged
    {
        private DatabaseModel dbModel;
        public Level_result level_result { get; set; }
        private const int MaxDataPoints = 10;
        private Random _random = new Random();
        private List<DateTime> _firstChartTimestamps = new List<DateTime>();
        private List<DateTime> _secondChartTimestamps = new List<DateTime>();
        public Func<double, string> FirstChartFormatter { get; set; }
        public Func<double, string> SecondChartFormatter { get; set; }

        public clearViewModel()
        {
            dbModel = new DatabaseModel();
            InitializeChart();
            Task.Run(Work);
        }

        public async Task Work()
        {
            while (true)
            {
                LoadData();
                Excluder_level_text = level_result.Excluder_level_result.ToString();
                int latestId = dbModel.GetMaxId("flusher_history");
                excluder_threshold = dbModel.GetSetting().Excluder_Period.ToString();
                var currthreshold = dbModel.GetColumnValueById("flusher_history", "threshold", latestId);
                flusher_leveltolevelthreshold();
                Excluder_level_text = currthreshold?.ToString() ?? "0";
                Excluder_level_bar = excluder_bar;
                UpdateChartsIfNewData();
                await Task.Delay(1000);
            }
        }

        public void LoadData()
        {
            level_result = dbModel.GetLevelResult();
        }

        public void flusher_leveltolevelthreshold()
        {
            int level = dbModel.GetLevelResult().Flusher_level_result;
            if (level == 1) { Excluder_level_time = dbModel.GetColumnValueById("Level_set_time", "Level_1_time", 2).ToString(); }
            else if (level == 2) { Excluder_level_time = dbModel.GetColumnValueById("Level_set_time", "Level_2_time", 2).ToString(); }
            else if (level == 3) { Excluder_level_time = dbModel.GetColumnValueById("Level_set_time", "Level_3_time", 2).ToString(); }
            else if (level == 4) { Excluder_level_time = dbModel.GetColumnValueById("Level_set_time", "Level_4_time", 2).ToString(); }
            else if (level == 5) { Excluder_level_time = dbModel.GetColumnValueById("Level_set_time", "Level_5_time", 2).ToString(); }
        }
        private string excluder_level;
        public string Excluder_level_text
        {
            get { return excluder_level; }
            set { excluder_level = value; OnPropertyChanged(); }
        }

        private string excluder_time;
        public string Excluder_level_time
        {
            get { return excluder_time; }
            set { excluder_time = value; OnPropertyChanged(); }
        }
        private double excluder_bar;
        public double Excluder_level_bar
        {
            get { return excluder_bar; }
            set
            {
                excluder_bar = value;
                switch (dbModel.GetLevelResult().Excluder_level_result)
                {
                    case 1:
                        excluder_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_1", 2));
                        //Excluder_level_time = dbModel.GetColumnValueById("Level_set_time", "Level_1_time", 2).ToString();
                        break;
                    case 2:
                        excluder_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_2", 2));
                        //Excluder_level_time = dbModel.GetColumnValueById("Level_set_time", "Level_2_time", 2).ToString();
                        break;
                    case 3:
                        excluder_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_3", 2));
                        //Excluder_level_time = dbModel.GetColumnValueById("Level_set_time", "Level_3_time", 2).ToString();
                        break;
                    case 4:
                        excluder_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_4", 2));
                        //Excluder_level_time = dbModel.GetColumnValueById("Level_set_time", "Level_4_time", 2).ToString();
                        break;
                    case 5:
                        excluder_bar = 100;
                        //Excluder_level_time = dbModel.GetColumnValueById("Level_set_time", "Level_5_time", 2).ToString();
                        break;
                }
                OnPropertyChanged();
            }
        }

        private void InitializeChart()
        {
            flusher_run_value = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "First Chart",
                    Values = new ChartValues<double> { }
                }
            };

            // 初始化第二個折線圖
            excluder_run_value = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Second Chart",
                    Values = new ChartValues<double> { }
                }
            };

            // 設定 X 軸的標籤格式化器1
            FirstChartFormatter = value =>
            {
                int index = (int)value;

                if (index >= 0 && index < _firstChartTimestamps.Count) // 確保 _firstChartTimestamps 有數據
                {

                    // 返回對應時間的格式化字串
                    return _firstChartTimestamps[index].ToString("HH:mm:ss");
                }

                return string.Empty;
            };
            // 設定 X 軸的標籤格式化器2
            SecondChartFormatter = value =>
            {
                int index = (int)value;

                if (index >= 0 && index < _secondChartTimestamps.Count)
                {
                    return _secondChartTimestamps[index].ToString("HH:mm:ss");
                }

                return string.Empty;
            };
        }
        private int _previousId = -1;

        public void UpdateChartsIfNewData()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                int latestId = dbModel.GetLatestId("flusher_history");
                if (latestId == -1 || latestId == _previousId) return;

                // 儲存本次最新 ID，避免重複更新
                _previousId = latestId;

                // 取得資料庫欄位值
                object resultObj = dbModel.GetColumnValueById("flusher_history", "result", latestId);
                object thresholdObj = dbModel.GetColumnValueById("flusher_history", "threshold", latestId);

                if (resultObj == null || thresholdObj == null ||
                    Convert.IsDBNull(resultObj) || Convert.IsDBNull(thresholdObj))
                    return;

                double resultValue = Convert.ToDouble(resultObj);
                double thresholdValue = Convert.ToDouble(thresholdObj);
                var currentTime = DateTime.Now;

                // 更新第一個圖表（result）
                flusher_run_value[0].Values.Add(resultValue);
                _firstChartTimestamps.Add(currentTime);
                if (flusher_run_value[0].Values.Count > MaxDataPoints)
                {
                    flusher_run_value[0].Values.RemoveAt(0);
                    _firstChartTimestamps.RemoveAt(0);
                }

                // 更新第二個圖表（threshold）
                excluder_run_value[0].Values.Add(thresholdValue);
                _secondChartTimestamps.Add(currentTime);
                if (excluder_run_value[0].Values.Count > MaxDataPoints)
                {
                    excluder_run_value[0].Values.RemoveAt(0);
                    _secondChartTimestamps.RemoveAt(0);
                }

                // 通知 UI 更新
                OnPropertyChanged(nameof(flusher_run_value));
                OnPropertyChanged(nameof(excluder_run_value));
            });
        }



        private SeriesCollection flusher_run_value;
        public SeriesCollection flusher_run_time
        {
            get { return flusher_run_value; }
            set { flusher_run_value = value; OnPropertyChanged(); }
        }
        private string excluder_threshold_value;
        public string excluder_threshold
        {
            get { return excluder_threshold_value; }
            set { excluder_threshold_value = value; OnPropertyChanged(); }
        }
        private SeriesCollection excluder_run_value;
        public SeriesCollection excluder_run_time
        {
            get { return excluder_run_value; }
            set { excluder_run_value = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }


}
