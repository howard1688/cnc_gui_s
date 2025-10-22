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
using System.Windows;
using System.IO;
using static cnc_gui.DatabaseModel;
using System.Threading;
using OpenCvSharp;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Org.BouncyCastle.Asn1.Cms;
using System.Timers;
using System.Data;
using HandyControl.Controls;
using System.Web.UI.WebControls;
using Org.BouncyCastle.Asn1.X509;
using System.Collections.ObjectModel;
namespace cnc_gui
{
    public delegate void UpdateImageCallback(BitmapSource image);
    internal class homeViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private DatabaseModel dbModel;
        private List<DateTime> _timestamps = new List<DateTime>();
        public Level_result level_result { get; set; }
        public flusher_excluder_onoff flusher_excluder_onoff { get; set; }
        public Level_Set level_set { get; set; }
        public Func<double, string> Formatter { get; set; }
        private const int MaxDataPoints = 5;
        private BitmapImage image_3;
        public VideoCapture _capture;

        public ObservableCollection<his_data> dataList { get; set; }
        public ObservableCollection<his_data> DataList
        {
            get { return dataList; }
            set
            {
                dataList = value;
                OnPropertyChanged(nameof(dataList));
            }
        }
        public static bool foo;
        
        public homeViewModel()
        {
            dbModel = new DatabaseModel();
            DataList = new ObservableCollection<his_data>();

            // 設定一個 Timer，每 5 秒從資料庫讀取最新數據


            LoadhistoryDataFromDatabase(); // 先載入一次
            InitializeCameras();
            //Initializehistory();
            Task.Run(Work);
        }

        public async Task Work()
        {
            while (true)
            {
                LoadData();
                LoadhistoryDataFromDatabase();
                Spindle_load_text = core.spindle.ToString();
                Spinlde_load_bar = core.spindle;
                update_connect_light(core.connect_r4);
                update_flusher_state();
                update_excluder_state();
                Flusher_level_text = level_result.Flusher_level_result.ToString();
                Flusher_level_bar = flusher_bar;
                Excluder_level_text = level_result.Excluder_level_result.ToString();
                Excluder_level_bar = excluder_bar;
                var exclude = dbModel.GetSetting();
                Excluder_threshold_value = exclude.Excluder_Period.ToString();
                UpdateImage(@"E:\專題ppt合輯\專題\cnc_gui_master\method_AI\resized_r1\resized_r1.jpg", bitmap => r1_img_home = bitmap);
                UpdateImage(@"E:\專題ppt合輯\專題\cnc_gui_master\method_AI\resized_r2\resized_r2.jpg", bitmap => r2_img_home = bitmap);
                await Task.Delay(1000);

            }
        }
        public void StartPreview()
        {
            foo = true;
        }
        public void StopPreview()
        {
            foo = false;
        }
        public void Initializehistory()
        {
            var history_data = new List<his_data>
            {
                new his_data{ his_id_value=(int)dbModel.GetColumnValueById("flusher_history","id",1),his_timestamp_value=(TimeSpan)dbModel.GetColumnValueById("flusher_history","open_timestamp",1),his_result_value=(int)dbModel.GetColumnValueById("flusher_history","result",1),his_threshold_value=(int)dbModel.GetColumnValueById("flusher_history","threshold",1)}
            };
        }
        public void LoadhistoryDataFromDatabase()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                // 這裡假設 `dbmodel.GetColumnValueById` 是你的方法
                int latestId = dbModel.GetLatestId("flusher_history"); // 取得最新的 ID
                if (latestId == -1) return; // 沒有新數據

                object timestampobj = dbModel.GetColumnValueById("flusher_history", "open_timestamp", latestId);
                object resultobj = dbModel.GetColumnValueById("flusher_history", "result", latestId);
                object thresholdobj = dbModel.GetColumnValueById("flusher_history", "threshold", latestId);
                TimeSpan timestamp = Convert.IsDBNull(timestampobj) ? TimeSpan.Zero : (TimeSpan)timestampobj;
                int result = Convert.IsDBNull(resultobj) ? 0 : (int)(resultobj);
                int threshold = Convert.IsDBNull(thresholdobj) ? 0 : (int)(thresholdobj);
                his_data newData = new his_data
                {
                    his_id_value = latestId,
                    his_timestamp_value = timestamp,
                    his_result_value = result,
                    his_threshold_value = threshold
                };

                // 避免重複插入相同數據
                if (!DataList.Any(d => d.his_id_value == newData.his_id_value))
                {
                    DataList.Add(newData);

                    // 只保留最新 20 筆數據
                    if (DataList.Count > 20)
                    {
                        DataList.RemoveAt(0);
                    }
                }
            });
        }
      
        public void InitializeCameras()
        {
            try
            {
                _capture = new VideoCapture(0);
                _capture.Set(VideoCaptureProperties.AutoFocus, 0);
                _capture.Set(VideoCaptureProperties.FrameWidth, 400);
                _capture.Set(VideoCaptureProperties.FrameHeight, 300);
                _capture.Set(VideoCaptureProperties.Focus, 49);
                foo = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"攝影機初始化失敗: {ex.Message}");
            }
        }
        private BitmapSource _sourceImgHome;
        public BitmapSource SourceImgHome
        {
            get => _sourceImgHome;
            set
            {
                if (_sourceImgHome != value)
                {
                    _sourceImgHome = value;
                    OnPropertyChanged();
                }
            }
        }
        //即時畫面
        public async void StartCameraPreviewAsync()
        {
            while (true)
            {
                await Task.Run(() =>
                {
                    while (foo)
                    {
                        Mat sharedMat = new Mat();
                        try
                        {
                            lock (_capture)
                            {
                                _capture.Read(sharedMat);
                            }

                            if (sharedMat.Empty())
                            {
                                // 如果捕捉到的影像是空的，跳過這次迭代
                                //MessageBox.Show("請檢查攝影機連接狀態");
                                continue;
                                                  
                            }

                            byte[] imageBytes;
                            if (Cv2.ImEncode(".jpg", sharedMat, out imageBytes))
                            {
                                var bitmapImage = new BitmapImage();
                                using (var stream = new MemoryStream(imageBytes))
                                {
                                    bitmapImage.BeginInit();
                                    bitmapImage.StreamSource = stream;
                                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmapImage.EndInit();
                                    bitmapImage.Freeze();
                                }
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    SourceImgHome = bitmapImage;
                                });
                            }
                        }
                        finally
                        {
                            sharedMat.Dispose();
                        }
                    }
                });

                await Task.Delay(33);
            }
        }
        /*
        public void UpdateDataAndRefreshView(int id, TimeSpan newTime, double newMagnitude, double newThreshold)
        {
            // 更新資料庫中的資料
            dbModel.UpdateDataById("flusher_history", id, newTime, newMagnitude, newThreshold);

            // 重新加載最新 20 筆資料
            DataList = dbModel.GetLatest20Data("flusher_history");

            // 更新 UI
            OnPropertyChanged(nameof(DataList));  // 假設有 INotifyPropertyChanged 實作
        }
        */
        //拍照
        public void CaptureImage(string filePath)
        {
            StopPreview();
            lock (_capture)
            {
                try
                {
                    _capture.Set(VideoCaptureProperties.AutoFocus, 0);
                    _capture.Set(VideoCaptureProperties.FrameWidth, 4000);
                    _capture.Set(VideoCaptureProperties.FrameHeight, 3000);
                    _capture.Set(VideoCaptureProperties.Focus, 49);

                    using (Mat highResFrame = new Mat())
                    {
                        try
                        {
                            _capture.Read(highResFrame);
                            highResFrame.SaveImage(filePath);
                            Console.WriteLine("影像已儲存至 " + filePath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"拍照時發生錯誤: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"拍照時發生錯誤: {ex.Message}");
                }
                finally
                {
                    _capture.Set(VideoCaptureProperties.AutoFocus, 0);
                    _capture.Set(VideoCaptureProperties.FrameWidth, 400);
                    _capture.Set(VideoCaptureProperties.FrameHeight, 300);
                    _capture.Set(VideoCaptureProperties.Focus, 49);
                    Task.Delay(100);
                    StartPreview();
                }
            }
        }
        public void UpdateOnUIThread(Action updateAction)
        {
            try
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    updateAction();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(updateAction);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新 UI 失敗: {ex.Message}");
            }
        }
        public void Dispose()
        {
        }
        public void LoadData()
        {
            level_result = dbModel.GetLevelResult();
            flusher_excluder_onoff = dbModel.Getflusher_excluder_onoff();     
        }
        //更新圖片
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

        //主軸負載 
        private string spindle;
        public string Spindle_load_text
        {
            get { return spindle; }
            set { spindle = value; OnPropertyChanged(); }
        }

        private double spindle_bar;
        public double Spinlde_load_bar
        {
            get { return spindle_bar; }
            set { spindle_bar = value; OnPropertyChanged(); }
        }
        //底座環沖
        private string flusher_level;
        public string Flusher_level_text
        {
            get { return flusher_level; }
            set { flusher_level = value; OnPropertyChanged(); }
        }
        //底座環沖換算數值
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
                        break;
                    case "2":
                        flusher_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_2", 1));
                        break;
                    case "3":
                        flusher_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_3", 1));
                        break;
                    case "4":
                        flusher_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_4", 1));
                        break;
                    case "5":
                        flusher_bar = 100;
                        break;
                }
                OnPropertyChanged();
            }
        }
        //排屑機
        private string excluder_level;
        public string Excluder_level_text
        {
            get { return excluder_level; }
            set { excluder_level = value; OnPropertyChanged(); }
        }

        private double excluder_bar;
        public double Excluder_level_bar
        {
            get { return excluder_bar; }
            set
            {
                excluder_bar = value;
                switch (excluder_level)
                {
                    case "1":
                        excluder_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_1", 2));
                        break;
                    case "2":
                        excluder_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_2", 2));
                        break;
                    case "3":
                        excluder_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_3", 2));
                        break;
                    case "4":
                        excluder_bar = Convert.ToDouble((int)dbModel.GetColumnValueById("Level_set", "Level_4", 2));
                        break;
                    case "5":
                        excluder_bar = 100;
                        break;
                }
                OnPropertyChanged();
            }
        }

        private Brush _flusher_state_light;
        public Brush flusher_state_light
        {
            get => _flusher_state_light;
            set
            {
                if (_flusher_state_light != value)
                {
                    _flusher_state_light = value;
                    OnPropertyChanged(nameof(flusher_state_light));
                }
            }
        }

        private Brush _excluder_state;
        public Brush excluder_state_light
        {
            get => _excluder_state;
            set
            {
                if (_excluder_state != value)
                {
                    _excluder_state = value;
                    OnPropertyChanged(nameof(excluder_state_light));
                }
            }
        }

        public void update_connect_light(short R)
        {
            if (R == 0)
            {
                connect_light = "連接成功";
            }
            else
            {
                connect_light = "連接失敗";
            }
        }
        private static readonly Brush GreenBrush = (Brush)new BrushConverter().ConvertFromString("#4ab494");
        private static readonly Brush RedBrush = (Brush)new BrushConverter().ConvertFromString("#da7d9b");

        public void update_flusher_state()
        {
            flusher_state_light = (flusher_excluder_onoff.flusher_state == 1) ? GreenBrush : RedBrush;
        }

        public void update_excluder_state()
        {
            excluder_state_light = (flusher_excluder_onoff.excluder_state == 1) ? GreenBrush : RedBrush;
        }

        private SeriesCollection energy_run_time;
        public SeriesCollection energy_run_time_value
        {
            get { return energy_run_time; }
            set { energy_run_time = value; OnPropertyChanged(); }
        }
        private BitmapImage imageSourcer1;

        public BitmapImage r1_img_home
        {
            get { return imageSourcer1; }
            set
            {
                imageSourcer1 = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage imageSourcer2;

        public BitmapImage r2_img_home
        {
            get { return imageSourcer2; }
            set
            {
                imageSourcer2 = value;
                OnPropertyChanged();
            }
        }

        private string _connect_light;

        public string connect_light
        {
            get { return _connect_light; }
            set
            {
                _connect_light = value;
                OnPropertyChanged("connect_light");
            }
        }
        private string excluder_threshold;
        public string Excluder_threshold_value
        {
            get { return excluder_threshold; }
            set { excluder_threshold = value; OnPropertyChanged(); }
        }
        public class his_data
        {

            public int his_id_value
            {get;set;}
            
            public TimeSpan his_timestamp_value { get; set; }
            public int his_result_value { get; set; }
            public int his_threshold_value { get; set; }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
