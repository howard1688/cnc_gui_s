using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static cnc_gui.DatabaseModel;
namespace cnc_gui
{
    internal class camViewModel : INotifyPropertyChanged
    {
        private DatabaseModel dbModel;
        public Level_result level_result { get; set; }
        private const int MaxDataPoints = 10;
        public Func<double, string> Formatter { get; set; }
        private Random _random = new Random();
        private List<DateTime> _timestamps = new List<DateTime>();

        public camViewModel()
        {
            dbModel = new DatabaseModel();
            InitializeChart();
            Task.Run(Work);
        }

        public async Task Work()
        {
            while (true)
            {
                //如果這邊會卡住不更新 是圖片路徑沒改造成
                LoadData();
                Flusher_level_text = level_result.Flusher_level_result.ToString();
                Flusher_level_time = flusher_time;
                Flusher_level_bar = flusher_bar;
                UpdateImage(@"E:\專題ppt合輯\專題\cnc_gui_master\method_AI\origin\origin.jpg", bitmap => Source_img_cam = bitmap);
                UpdateImage(@"E:\專題ppt合輯\專題\cnc_gui_master\method_AI\resized_r1\resized_r1.jpg", bitmap => r1_img_cam = bitmap);
                UpdateImage(@"E:\專題ppt合輯\專題\cnc_gui_master\method_AI\resized_r2\resized_r2.jpg", bitmap => r2_img_cam = bitmap);
                updateflusherchart();
                
                await Task.Delay(1000);
            }
        }

        public void LoadData()
        {
            level_result = dbModel.GetLevelResult();
        }

        private string flusher_level;
        public string Flusher_level_text
        {
            get { return flusher_level; }
            set { flusher_level = value; OnPropertyChanged(); }
        }

        private string flusher_time;
        public string Flusher_level_time
        {
            get { return flusher_time; }
            set { flusher_time = value; OnPropertyChanged(); }
        }
        private double flusher_bar;
        public double Flusher_level_bar
        {
            get { return flusher_bar; }
            set
            {
                flusher_bar = value;
                switch (flusher_level)
                {
                    case "1":
                        flusher_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_1", 1));
                        flusher_time = dbModel.GetColumnValueById("Level_set_time", "Level_1_time", 1).ToString();
                        break;
                    case "2":
                        flusher_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_2", 1));
                        flusher_time = dbModel.GetColumnValueById("Level_set_time", "Level_2_time", 1).ToString();
                        break;
                    case "3":
                        flusher_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_3", 1));
                        flusher_time = dbModel.GetColumnValueById("Level_set_time", "Level_3_time", 1).ToString();
                        break;
                    case "4":
                        flusher_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_4", 1));
                        flusher_time = dbModel.GetColumnValueById("Level_set_time", "Level_4_time", 1).ToString();
                        break;
                    case "5":
                        flusher_bar = 100;
                        flusher_time = dbModel.GetColumnValueById("Level_set_time", "Level_5_time", 1).ToString();
                        break;
                }
                OnPropertyChanged();
            }
        }
       
        public void UpdateImage(string imagePath, Action<BitmapImage> updateAction)
        {
            try
            {
                // 強制刷新圖片，避免因快取而導致圖片不更新
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // 避免快取
                bitmap.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache; // 忽略圖片快取
                bitmap.EndInit();
                bitmap.Freeze(); // 確保圖片可以跨執行緒使用

                updateAction(bitmap);
            }
            catch (Exception ex)
            {
                string defaultImagePath = @"E:\專題ppt合輯\專題\cnc_gui_master\cnc_gui\icon\no_img.png"; // 替換為預設圖片路徑
                var defaultBitmap = new BitmapImage();
                defaultBitmap.BeginInit();
                defaultBitmap.CacheOption = BitmapCacheOption.OnLoad;
                defaultBitmap.UriSource = new Uri(defaultImagePath, UriKind.RelativeOrAbsolute);
                defaultBitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                defaultBitmap.EndInit();
                defaultBitmap.Freeze();

                // 顯示預設圖片
                updateAction(defaultBitmap);
                //image_3 = defaultBitmap;
            }
        }
        int lastUpdatedId = -1; // 放在 class 中作為紀錄上次的 ID

        public void updateflusherchart()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                int latestId = dbModel.GetLatestId("flusher_history");

                // 如果 ID 沒變，代表資料沒更新，就不做事
                if (latestId == lastUpdatedId) return;

                // 否則抓最新資料
                object resultobj = dbModel.GetColumnValueById("flusher_history", "result", latestId);
                int flusherenergy = resultobj == null || Convert.IsDBNull(resultobj) ? 0 : (int)resultobj;

                // 更新圖表
                flusher_run_value[0].Values.Add((double)flusherenergy);
                _timestamps.Add(DateTime.Now); // 記錄時間軸

                // 控制圖表資料數量
                if (flusher_run_value[0].Values.Count > MaxDataPoints)
                {
                    flusher_run_value[0].Values.RemoveAt(0);
                    _timestamps.RemoveAt(0);
                }

                OnPropertyChanged(nameof(flusher_run_value)); // 通知 UI

                lastUpdatedId = latestId; // 更新追蹤用 ID
            });
        }

        //圖表
        private void InitializeChart()
        {
            flusher_run_value = new SeriesCollection
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

        }



        private BitmapImage imageSource;

        public BitmapImage Source_img_cam
        {
            get { return imageSource; }
            set
            {
                imageSource = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage imageSourcer1;

        public BitmapImage r1_img_cam
        {
            get { return imageSourcer1; }
            set
            {
                imageSourcer1 = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage imageSourcer2;

        public BitmapImage r2_img_cam
        {
            get { return imageSourcer2; }
            set
            {
                imageSourcer2 = value;
                OnPropertyChanged();
            }
        }
        private SeriesCollection flusher_run_value;
        public SeriesCollection flusherenergy
        {
            get { return flusher_run_value; }
            set { flusher_run_value = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
