using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static cnc_gui.DatabaseModel;

namespace cnc_gui
{
    internal class energyViewModel : INotifyPropertyChanged
    {
        private DatabaseModel dbModel;
        public Level_result level_result { get; set; }
        private const int MaxDataPoints = 10;
        private Random _random = new Random();
        private List<DateTime> _firstChartTimestamps = new List<DateTime>();
        private List<DateTime> _secondChartTimestamps = new List<DateTime>();
        private List<DateTime> _thirdChartTimestamps = new List<DateTime>();
        public Func<double, string> FirstChartFormatter { get; set; }
        public Func<double, string> SecondChartFormatter { get; set; }
        public Func<double, string> ThirdChartFormatter { get; set; }

        public energyViewModel()
        {
            dbModel = new DatabaseModel();
            InitializeChart();
            
            Task.Run(Work);
        }

        public async Task Work()
        {
            while (true)
            {
                // 每秒取得最新一筆資料
                int latestId = dbModel.GetMaxId("flusher_history");

                var value = dbModel.GetColumnValueById("flusher_history", "total_energy_sum", latestId);
                var kwhObj = dbModel.GetColumnValueById("flusher_history", "total_kwh_sum", latestId);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    total_energy_sum = value?.ToString() ?? "0";

                    if (float.TryParse(kwhObj?.ToString(), out float kwh))
                    {
                        total_energy_wh_sum = ((int)(kwh * 1000)).ToString();
                        total_energy_kwh_sum = kwh.ToString("F2");
                    }
                    else
                    {
                        total_energy_wh_sum = "0";
                        total_energy_kwh_sum = "0";
                    }
                });

                UpdateChartsIfNewData();

                await Task.Delay(1000);
            }
        }

        private void InitializeChart()
        {
            flusher_energy_value = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Flusher Energy",
                    Values = new ChartValues<double> { }
                }
            };

            // 初始化第二個折線圖
            excluder_energy_value = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Excluder Energy",
                    Values = new ChartValues<double> { }
                }
            };

            total_energy_value = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Total Energy",
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
            // 設定 X 軸的標籤格式化器3
            ThirdChartFormatter = value =>
            {
                int index = (int)value;
                if (index >= 0 && index < _thirdChartTimestamps.Count)
                {
                    return _thirdChartTimestamps[index].ToString("HH:mm:ss");
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
                _previousId = latestId;

                // 每次都更新 flusher_energy
                object flusherEnergyObj = dbModel.GetColumnValueById("flusher_history", "flusher_energy_data", latestId);
                if (!Convert.IsDBNull(flusherEnergyObj))
                {
                    double flusherEnergy = Convert.ToDouble(flusherEnergyObj);
                    var now = DateTime.Now;

                    flusher_energy_value[0].Values.Add(flusherEnergy);
                    _firstChartTimestamps.Add(now);

                    if (flusher_energy_value[0].Values.Count > MaxDataPoints)
                    {
                        flusher_energy_value[0].Values.RemoveAt(0);
                        _firstChartTimestamps.RemoveAt(0);
                    }

                    OnPropertyChanged(nameof(flusher_energy_value));
                }

                // 只有 threshold == Excluder_Period 時才更新
                object thresholdObj = dbModel.GetColumnValueById("flusher_history", "threshold", latestId);
                var setting = dbModel.GetSetting();
                int excluderPeriod = setting.Excluder_Period;

                if (!Convert.IsDBNull(thresholdObj) && Convert.ToInt32(thresholdObj) ==0)
                {


                    var now = DateTime.Now;

                    object excluderEnergyObj = dbModel.GetColumnValueById("flusher_history", "excluder_energy_data", latestId);
                    if (!Convert.IsDBNull(excluderEnergyObj))
                    {
                        double excluderEnergy = Convert.ToDouble(excluderEnergyObj);

                        excluder_energy_value[0].Values.Add(excluderEnergy);
                        _secondChartTimestamps.Add(now);

                        if (excluder_energy_value[0].Values.Count > MaxDataPoints)
                        {
                            excluder_energy_value[0].Values.RemoveAt(0);
                            _secondChartTimestamps.RemoveAt(0);
                        }

                        OnPropertyChanged(nameof(excluder_energy_value));
                    }

                    object totalEnergyObj = dbModel.GetColumnValueById("flusher_history", "total_energy_data", latestId);
                    if (!Convert.IsDBNull(totalEnergyObj))
                    {
                        double totalEnergy = Convert.ToDouble(totalEnergyObj);

                        total_energy_value[0].Values.Add(totalEnergy);
                        _thirdChartTimestamps.Add(now);

                        if (total_energy_value[0].Values.Count > MaxDataPoints)
                        {
                            total_energy_value[0].Values.RemoveAt(0);
                            _thirdChartTimestamps.RemoveAt(0);
                        }

                        OnPropertyChanged(nameof(total_energy_value));
                    }


                }
            });

        }


        private SeriesCollection flusher_energy_value;
        public SeriesCollection flusher_energy
        {
            get { return flusher_energy_value; }
            set { flusher_energy_value = value; OnPropertyChanged(); }
        }
        private SeriesCollection excluder_energy_value;
        public SeriesCollection excluder_energy
        {
            get { return excluder_energy_value; }
            set { excluder_energy_value = value; OnPropertyChanged(); }
        }
        private SeriesCollection total_energy_value;
        public SeriesCollection total_energy
        {
            get { return total_energy_value; }
            set { total_energy_value = value; OnPropertyChanged(); }
        }

        private string total_energy_wh_sum_value;
        public string total_energy_wh_sum
        {
            get { return total_energy_wh_sum_value; }
            set
            {
                total_energy_wh_sum_value = value;
                OnPropertyChanged();
            }
        }
        private string total_energy_kwh_sum_value;
        public string total_energy_kwh_sum
        {
            get { return total_energy_kwh_sum_value; }
            set
            {
                total_energy_kwh_sum_value = value;
                OnPropertyChanged();
            }
        }
        private string total_energy_sum_value;
        public string total_energy_sum
        {
            get { return total_energy_sum_value; }
            set { total_energy_sum_value = value; OnPropertyChanged(); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
