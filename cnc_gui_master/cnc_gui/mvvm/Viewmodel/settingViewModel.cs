using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
namespace cnc_gui
{
    internal class settingViewModel : INotifyPropertyChanged
    {
        private DatabaseModel dbModel;
        public settingViewModel()
        {
            dbModel = new DatabaseModel();
            flusher_LevelOptions = new ObservableCollection<string>
            {
                "極少量級", "少量級", "中量級", "多量級", "極多量級"
            };
            excluder_LevelOptions = new ObservableCollection<string>
            {
                "極少量級", "少量級", "中量級", "多量級", "極多量級"
            };
            // 預設選項
            excluder_selectedLevel_text = excluder_LevelOptions.FirstOrDefault();
            flusher_selectedLevel_text = flusher_LevelOptions.FirstOrDefault();
            refreshpage();
        }
        public static string flusher_binary { get; set; } = ""; //底座環沖二元數
        public static string excluder_binary { get; set; } = "";//排屑機二元數
        public void refreshpage()//更新輸入框顯示文字與圖表
        {
            update_ip_port();
            update_plc_control();
            initialization_flusher_time_total_piechart();
            update_flusher_level_set();
            update_excluder_level_set();
            var exclude = dbModel.GetSetting();
            excluder_threshold_text = exclude.Excluder_Period.ToString();
        }
        public void update_ip_port()
        {
            var ipPort = dbModel.GetIp_Port();
            cnc_ip_text = ipPort.Cncip;
            cnc_port_text = ipPort.Cncport;
        }
        public void update_plc_control()
        {
            flusher_address_handl_text = dbModel.GetColumnValueById("Plc_control", "Handl", 1).ToString();
            flusher_address_text = dbModel.GetColumnValueById("Plc_control", "Address", 1).ToString();
            flusher_address_decimal_text = dbModel.GetColumnValueById("Plc_control", "Decimal_num", 1).ToString();
            excluder_address_handl_text = dbModel.GetColumnValueById("Plc_control", "Handl", 2).ToString();
            excluder_address_text = dbModel.GetColumnValueById("Plc_control", "Address", 2).ToString();
            excluder_address_decimal_text = dbModel.GetColumnValueById("Plc_control", "Decimal_num", 2).ToString();
            flusher_address_binary_text = flusher_ConvertToBinaryArray((int)dbModel.GetColumnValueById("Plc_control", "Decimal_num", 1)).ToString();
            excluder_address_binary_text = excluder_ConvertToBinaryArray((int)dbModel.GetColumnValueById("Plc_control", "Decimal_num", 2)).ToString();
            flusher_ConvertToBinaryArray(int.Parse(flusher_address_decimal));
            flusher_address_binary_text = flusher_binary;
            excluder_ConvertToBinaryArray(int.Parse(excluder_address_decimal));
            excluder_address_binary_text = excluder_binary;
        }
        public void initialization_flusher_time_total_piechart()
        {
            var flusher_time_total = dbModel.GetFlusherTimeSetting();
            pie_Flusher_time_total_chart = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "影像處理時間",
                    Values = new ChartValues<double> { flusher_time_total.Total_flusher_time*0.1 }
                },
                new PieSeries
                {
                    Title = "可底沖總時間",
                    Values = new ChartValues<double> { (int)dbModel.GetColumnValueById("Level_set_time","Level_5_time",1) }
                },
                new PieSeries
                {
                    Title = "保留時間",
                    Values = new ChartValues<double> { flusher_time_total.Total_flusher_time - (flusher_time_total.Total_flusher_time * 0.2) - (int)dbModel.GetColumnValueById("Level_set_time", "Level_5_time", 1) }
                }
            };
            image_process_time_text = (flusher_time_total.Total_flusher_time * 0.1).ToString();
            flusher_process_time_text = dbModel.GetColumnValueById("Level_set_time", "Level_5_time", 1).ToString();
            delay_time_text = (flusher_time_total.Total_flusher_time - (flusher_time_total.Total_flusher_time * 0.2) - (int)dbModel.GetColumnValueById("Level_set_time", "Level_5_time", 1)).ToString();
            total_flusher_time_text = flusher_time_total.Total_flusher_time.ToString();
        }
        public void update_flusher_level_set()
        {
            flusher_level_1_max_text = dbModel.GetColumnValueById("Level_set", "Level_1", 1).ToString();
            flusher_level_2_max_text = dbModel.GetColumnValueById("Level_set", "Level_2", 1).ToString();
            flusher_level_3_max_text = dbModel.GetColumnValueById("Level_set", "Level_3", 1).ToString();
            flusher_level_4_max_text = dbModel.GetColumnValueById("Level_set", "Level_4", 1).ToString();
            flusher_level_2_min_text = (Convert.ToInt32(dbModel.GetColumnValueById("Level_set", "Level_1", 1)) + 1).ToString();
            flusher_level_3_min_text = (Convert.ToInt32(dbModel.GetColumnValueById("Level_set", "Level_2", 1)) + 1).ToString();
            flusher_level_4_min_text = (Convert.ToInt32(dbModel.GetColumnValueById("Level_set", "Level_3", 1)) + 1).ToString();
            flusher_level_5_min_text = (Convert.ToInt32(dbModel.GetColumnValueById("Level_set", "Level_4", 1)) + 1).ToString();
            transfer_level_value_to_str(1,"flusher");
        }
        public void update_excluder_level_set()
        {
            excluder_level_1_max_text = dbModel.GetColumnValueById("Level_set", "Level_1", 2).ToString();
            excluder_level_2_max_text = dbModel.GetColumnValueById("Level_set", "Level_2", 2).ToString();
            excluder_level_3_max_text = dbModel.GetColumnValueById("Level_set", "Level_3", 2).ToString();
            excluder_level_4_max_text = dbModel.GetColumnValueById("Level_set", "Level_4", 2).ToString();
            excluder_level_2_min_text = (Convert.ToInt32(dbModel.GetColumnValueById("Level_set", "Level_1", 2)) + 1).ToString();
            excluder_level_3_min_text = (Convert.ToInt32(dbModel.GetColumnValueById("Level_set", "Level_2", 2)) + 1).ToString();
            excluder_level_4_min_text = (Convert.ToInt32(dbModel.GetColumnValueById("Level_set", "Level_3", 2)) + 1).ToString();
            excluder_level_5_min_text = (Convert.ToInt32(dbModel.GetColumnValueById("Level_set", "Level_4", 2)) + 1).ToString();
            transfer_level_value_to_str(2, "excluder");
        }
        public void transfer_level_value_to_str(int id,string type )
        {
            string level_1_to_str,level_2_to_str, level_3_to_str, level_4_to_str, level_5_to_str;
            if (id == 1)
            {
                level_1_to_str = $"0%~{flusher_level_1_max}%";
                level_2_to_str = $"{flusher_level_2_min}%~{flusher_level_2_max}%";
                level_3_to_str = $"{flusher_level_3_min}%~{flusher_level_3_max}%";
                level_4_to_str = $"{flusher_level_4_min}%~{flusher_level_4_max}%";
                level_5_to_str = $"{flusher_level_5_min}%~100%";
            }
            else
            {
                level_1_to_str = $"0%~{excluder_level_1_max}%";
                level_2_to_str = $"{excluder_level_2_min}%~{excluder_level_2_max}%";
                level_3_to_str = $"{excluder_level_3_min}%~{excluder_level_3_max}%";
                level_4_to_str = $"{excluder_level_4_min}%~{excluder_level_4_max}%";
                level_5_to_str = $"{excluder_level_5_min}%~100%";
            }

            // 更新資料庫
            dbModel.UpdateLevelSetToString(id, "Level_1_to_str", level_1_to_str);
            dbModel.UpdateLevelSetToString(id, "Level_2_to_str", level_2_to_str);
            dbModel.UpdateLevelSetToString(id, "Level_3_to_str", level_3_to_str);
            dbModel.UpdateLevelSetToString(id, "Level_4_to_str", level_4_to_str);
            dbModel.UpdateLevelSetToString(id, "Level_5_to_str", level_5_to_str);
        }
        //底沖排屑機設定秒數
        private void flusher_LoadLevelDetails()
        {
            if (flusher_selectedLevel == null) return;

            // 將選項轉換為資料庫對應的 Level 列
            int levelIndex = flusher_LevelOptions.IndexOf(flusher_selectedLevel) + 1; // Level_1 ~ Level_5
            if (levelIndex < 1 || levelIndex > 5) return;

            try
            {
                // 從資料庫讀取描述
                flusher_selectedLevelDescription_text = dbModel.GetColumnValueById("Level_set_to_string", $"Level_{levelIndex}_to_str", 1)?.ToString();

                // 從資料庫讀取時間
                flusher_selectedLevelTime_text = dbModel.GetColumnValueById("Level_set_time", $"Level_{levelIndex}_time", 1)?.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"讀取資料失敗: {ex.Message}");
            }
        }
        public void flusher_UpdateLevelTime(int newTime)
        {
            if (flusher_selectedLevel == null) return;

            int levelIndex = flusher_LevelOptions.IndexOf(flusher_selectedLevel) + 1; // Level_1 ~ Level_5
            if (levelIndex < 1 || levelIndex > 5) return;

            try
            {
                dbModel.UpdateTimeByIdAndColumn(1, $"Level_{levelIndex}_time", newTime); // 更新資料庫
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新資料失敗: {ex.Message}");
            }
        }
        private void excluder_LoadLevelDetails()
        {
            if (excluder_selectedLevel == null) return;
            int levelIndex = excluder_LevelOptions.IndexOf(excluder_selectedLevel) + 1; // Level_1 ~ Level_5
            if (levelIndex < 1 || levelIndex > 5) return;
            try
            {
                excluder_selectedLevelDescription_text = dbModel.GetColumnValueById("Level_set_to_string", $"Level_{levelIndex}_to_str", 2)?.ToString();
                excluder_selectedLevelTime_text = dbModel.GetColumnValueById("Level_set_time", $"Level_{levelIndex}_time", 2)?.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"讀取資料失敗: {ex.Message}");
            }
        }
        public void excluder_UpdateLevelTime(int newTime)
        {
            if (excluder_selectedLevel == null) return;
            int levelIndex = excluder_LevelOptions.IndexOf(excluder_selectedLevel) + 1; // Level_1 ~ Level_5
            if (levelIndex < 1 || levelIndex > 5) return;
            try
            {
                dbModel.UpdateTimeByIdAndColumn(2, $"Level_{levelIndex}_time", newTime); // 更新資料庫
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新資料失敗: {ex.Message}");
            }
        }
        //10進制轉2進制陣列
        int[] flusher_ConvertToBinaryArray(int decimalNumber)
        {
            int[] binaryarr = new int[8];
            for (int i = 7; i >= 0; i--)
            {
                binaryarr[i] = decimalNumber % 2;
                   decimalNumber /= 2;
                flusher_binary = binaryarr[i].ToString() + flusher_binary;
            }
            flusher_binary = string.Join("", binaryarr);
            return binaryarr;
        }
        int[] excluder_ConvertToBinaryArray(int decimalNumber)
        {
            int[] binaryarr = new int[8];
            excluder_address_binary = "";
            for (int i = 7; i >= 0; i--)
            {
                binaryarr[i] = decimalNumber % 2;
                decimalNumber /= 2;

                excluder_address_binary = binaryarr[i].ToString() + excluder_binary;
            }
            excluder_binary = string.Join("", binaryarr);
            return binaryarr;
        }

        //2進制轉10進制
        int flusher_ConvertBinaryArrayToDecimal(int[] binaryArray)
        {
            int decimalValue = 0;
            int length = binaryArray.Length;
            for (int i = 0; i < length; i++)
            {
                decimalValue += binaryArray[i] * (int)Math.Pow(2, binaryArray.Length - 1 - i);
            }
            return decimalValue;
        }
        int excluder_ConvertBinaryArrayToDecimal(int[] binaryArray)
        {
            int decimalValue = 0;
            int length = binaryArray.Length;
            for (int i = 0; i < length; i++)
            {
                decimalValue += binaryArray[i] * (int)Math.Pow(2, binaryArray.Length - 1 - i);
            }
            return decimalValue;
        }
        //ip port
        private string cnc_ip;
        public string cnc_ip_text
        {
            get { return cnc_ip; }
            set { cnc_ip = value; OnPropertyChanged(); }
        }
        public string cnc_port;
        public string cnc_port_text
        {
            get { return cnc_port; }
            set { cnc_port = value; OnPropertyChanged(); }
        }
        //排屑機臨界值
        public string excluder_threshold;
        public string excluder_threshold_text
        {
            get => excluder_threshold;
            set
            {
                excluder_threshold = value;
                OnPropertyChanged();
                if (int.TryParse(value, out int excluderthresholde))
                {
                    try
                    {
                        dbModel.UpdateSetting(excluderthresholde); // 更新資料庫
                    }
                    catch (Exception ex)
                    {
                        // 錯誤處理（例如顯示錯誤訊息）
                        Console.WriteLine($"更新資料失敗: {ex.Message}");
                    }
                }
                else
                {
                    // 若輸入無效，清空或提供回饋
                    Console.WriteLine("請輸入有效的整數");
                }
            }
        }
        //底冲控制
        public string flusher_address_handl;
        public string flusher_address_handl_text
        {
            get => flusher_address_handl;
            set
            {
                if (flusher_address_handl != value)
                {
                    flusher_address_handl = value;
                    OnPropertyChanged();

                    // 嘗試解析為整數並更新資料庫
                    if (int.TryParse(value, out int flusher_handl))
                    {
                        try
                        {                    
                           dbModel.UpdatePlcControl(1,"Handl",flusher_handl); // 更新資料庫
                        }
                        catch (Exception ex)
                        {
                            // 錯誤處理（例如顯示錯誤訊息）
                            Console.WriteLine($"更新資料失敗: {ex.Message}");
                        }
                    }
                    else
                    {
                        // 若輸入無效，清空或提供回饋
                        Console.WriteLine("請輸入有效的整數");
                    }
                }
            }
        }
        public string flusher_address;
        public string flusher_address_text
        {
            get { return flusher_address; }
            set
            {
                if (flusher_address != value)
                {
                    flusher_address = value;
                    OnPropertyChanged();
                    // 嘗試解析為整數並更新資料庫
                    if (int.TryParse(value, out int flusher_address_))
                    {
                        try
                        {
                            dbModel.UpdatePlcControl(1, "Address", flusher_address_); // 更新資料庫
                        }
                        catch (Exception ex)
                        {
                            // 錯誤處理（例如顯示錯誤訊息）
                            Console.WriteLine($"更新資料失敗: {ex.Message}");
                        }
                    }
                    else
                    {
                        // 若輸入無效，清空或提供回饋
                        Console.WriteLine("請輸入有效的整數");
                    }
                }
            }
        }
        public string flusher_address_decimal;
        public string flusher_address_decimal_text
        {
            get { return flusher_address_decimal; }
            set
            {
                if (flusher_address_decimal != value)
                {
                    flusher_address_decimal = value;
                    OnPropertyChanged();
                    // 嘗試解析為整數並更新資料庫
                    if (int.TryParse(value, out int flusher_decimal))
                    {
                        try
                        {
                            dbModel.UpdatePlcControl(1, "Decimal_num", flusher_decimal); // 更新資料庫
                            flusher_ConvertToBinaryArray(flusher_decimal).ToString();
                            flusher_address_binary_text=flusher_binary;
                        }
                        catch (Exception ex)
                        {
                            // 錯誤處理（例如顯示錯誤訊息）
                            Console.WriteLine($"更新資料失敗: {ex.Message}");
                        }
                    }
                    else
                    {
                        // 若輸入無效，清空或提供回饋
                        Console.WriteLine("請輸入有效的整數");
                    }
                }
            }
        }
        public string flusher_address_binary;
        public string flusher_address_binary_text
        {
            get { return flusher_address_binary; }
            set { flusher_address_binary = value; OnPropertyChanged(); }
        }
        //排屑機控制
        public string excluder_address_handl;
        public string excluder_address_handl_text
        {
            get { return excluder_address_handl; }
            set
            {
                excluder_address_handl = value;
                OnPropertyChanged();
                if (int.TryParse(value, out int excluder_handl))
                {
                    try
                    {
                        dbModel.UpdatePlcControl(2, "Handl", excluder_handl); // 更新資料庫
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"更新資料失敗: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("請輸入有效的整數");
                }
            }
        }
        public string excluder_address;
        public string excluder_address_text
        {
            get { return excluder_address; }
            set
            {
                if (excluder_address != value)
                {
                    excluder_address = value;
                    OnPropertyChanged();
                    // 嘗試解析為整數並更新資料庫
                    if (int.TryParse(value, out int excluder_address_))
                    {
                        try
                        {
                            dbModel.UpdatePlcControl(2, "Address", excluder_address_); // 更新資料庫
                        }
                        catch (Exception ex)
                        {
                            // 錯誤處理（例如顯示錯誤訊息）
                            Console.WriteLine($"更新資料失敗: {ex.Message}");
                        }
                    }
                    else
                    {
                        // 若輸入無效，清空或提供回饋
                        Console.WriteLine("請輸入有效的整數");
                    }
                }
            }
        }
        public string excluder_address_decimal;
        public string excluder_address_decimal_text
        {
            get { return excluder_address_decimal; }
            set
            {
                if (excluder_address_decimal != value)
                {
                    excluder_address_decimal = value;
                    OnPropertyChanged();
                    // 嘗試解析為整數並更新資料庫
                    if (int.TryParse(value, out int excluder_decimal))
                    {
                        try
                        {
                            dbModel.UpdatePlcControl(2, "Decimal_num", excluder_decimal); // 更新資料庫
                            excluder_ConvertToBinaryArray(excluder_decimal).ToString();
                            excluder_address_binary_text = excluder_binary;
                        }
                        catch (Exception ex)
                        {
                            // 錯誤處理（例如顯示錯誤訊息）
                            Console.WriteLine($"更新資料失敗: {ex.Message}");
                        }
                    }
                    else
                    {
                        // 若輸入無效，清空或提供回饋
                        Console.WriteLine("請輸入有效的整數");
                    }
                }
            }
        }
        public string excluder_address_binary;
        public string excluder_address_binary_text
        {
            get { return excluder_address_binary; }
            set { excluder_address_binary = value; OnPropertyChanged(); }
        }
        //沖水時間分配
        public string image_process_time;
        public string image_process_time_text
        {
            get { return image_process_time; }
            set { image_process_time = value; OnPropertyChanged(); }
        }
        public string flusher_process_time;
        public string flusher_process_time_text
        {
            get { return flusher_process_time; }
            set { flusher_process_time = value; OnPropertyChanged(); }
        }
        public string delay_time;
        public string delay_time_text
        {
            get { return delay_time; }
            set { delay_time = value; OnPropertyChanged(); }
        }
        public string total_flusher_time;
        public string total_flusher_time_text
        {
            get => total_flusher_time;
            set
            {
                if (total_flusher_time != value)
                {
                    total_flusher_time = value;
                    OnPropertyChanged();

                    // 嘗試解析為整數並更新資料庫
                    if (int.TryParse(value, out int totalTime))
                    {
                        try
                        {
                            dbModel.UpdateFlusherTimeSetting(totalTime); // 更新資料庫
                            initialization_flusher_time_total_piechart(); // 刷新圖表
                        }
                        catch (Exception ex)
                        {
                            // 錯誤處理（例如顯示錯誤訊息）
                            Console.WriteLine($"更新資料失敗: {ex.Message}");
                        }
                    }
                    else
                    {
                        // 若輸入無效，清空或提供回饋
                        Console.WriteLine("請輸入有效的整數");
                    }
                }
            }
        }
        //沖水時間分配圖表
        public SeriesCollection pie_Flusher_time_total;
        public SeriesCollection pie_Flusher_time_total_chart
        {
            get => pie_Flusher_time_total;
            set
            {
                pie_Flusher_time_total = value;
                OnPropertyChanged(); // 通知 WPF 刷新 UI
            }
        }
        //積屑量級定義
        public string flusher_level_1_max;
        public string flusher_level_1_max_text
        {
            get { return flusher_level_1_max; }
            set
            {
                if (flusher_level_1_max != value) // 確保值改變時才觸發
                {
                    flusher_level_1_max = value;
                    OnPropertyChanged();

                    // 更新資料庫
                    if (int.TryParse(value, out int newValue))
                    {
                        try
                        {
                            dbModel.UpdateLevelSetColumn(1, "Level_1", newValue); // 假設 id = 1，更新 Level_1
                            update_flusher_level_set(); // 刷新顯示文字
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"更新資料失敗: {ex.Message}");
                        }
                    }
                }
            }
        }

        public string flusher_level_2_max;
        public string flusher_level_2_max_text
        {
            get { return flusher_level_2_max; }
            set
            {
                if (flusher_level_2_max != value)
                {
                    flusher_level_2_max = value;
                    OnPropertyChanged();
                    if (int.TryParse(value, out int newValue))
                    {
                        try
                        {
                            dbModel.UpdateLevelSetColumn(1, "Level_2", newValue);
                            update_flusher_level_set();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"更新資料失敗: {ex.Message}");
                        }
                    }
                }
            }
        }
        public string flusher_level_3_max;
        public string flusher_level_3_max_text
        {
            get { return flusher_level_3_max; }
            set
            {
                if (flusher_level_3_max != value)
                {
                    flusher_level_3_max = value;
                    OnPropertyChanged();
                    if (int.TryParse(value, out int newValue))
                    {
                        try
                        {
                            dbModel.UpdateLevelSetColumn(1, "Level_3", newValue);
                            update_flusher_level_set();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"更新資料失敗: {ex.Message}");
                        }
                    }
                }
            }
        }
        public string flusher_level_4_max;
        public string flusher_level_4_max_text
        {
            get { return flusher_level_4_max; }
            set
            {
                if (flusher_level_4_max != value)
                {
                    flusher_level_4_max = value;
                    OnPropertyChanged();
                    if (int.TryParse(value, out int newValue))
                    {
                        try
                        {
                            dbModel.UpdateLevelSetColumn(1, "Level_4", newValue);
                            update_flusher_level_set();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"更新資料失敗: {ex.Message}");
                        }
                    }
                }
            }
        }
        public string flusher_level_2_min;
        public string flusher_level_2_min_text
        {
            get { return flusher_level_2_min; }
            set { flusher_level_2_min = value; OnPropertyChanged(); }
        }
        public string flusher_level_3_min;
        public string flusher_level_3_min_text
        {
            get { return flusher_level_3_min; }
            set { flusher_level_3_min = value; OnPropertyChanged(); }
        }
        public string flusher_level_4_min;
        public string flusher_level_4_min_text
        {
            get { return flusher_level_4_min; }
            set { flusher_level_4_min = value; OnPropertyChanged(); }
        }
        public string flusher_level_5_min;
        public string flusher_level_5_min_text
        {
            get { return flusher_level_5_min; }
            set { flusher_level_5_min = value; OnPropertyChanged(); }
        }
        //排屑機臨量級定義
        public string excluder_level_1_max;
        public string excluder_level_1_max_text
        {
            get { return excluder_level_1_max; }
            set
            {
                if (excluder_level_1_max != value)
                {
                    excluder_level_1_max = value;
                    OnPropertyChanged();
                    if (int.TryParse(value, out int newValue))
                    {
                        try
                        {
                            dbModel.UpdateLevelSetColumn(2, "Level_1", newValue);
                            update_excluder_level_set();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"更新資料失敗: {ex.Message}");
                        }
                    }
                }
            }
        }
        public string excluder_level_2_max;
        public string excluder_level_2_max_text
        {
            get { return excluder_level_2_max; }
            set
            {
                if (excluder_level_2_max != value)
                {
                    excluder_level_2_max = value;
                    OnPropertyChanged();
                    if (int.TryParse(value, out int newValue))
                    {
                        try
                        {
                            dbModel.UpdateLevelSetColumn(2, "Level_2", newValue);
                            update_excluder_level_set();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"更新資料失敗: {ex.Message}");
                        }
                    }
                }
            }
        }
        public string excluder_level_3_max;
        public string excluder_level_3_max_text
        {
            get { return excluder_level_3_max; }
            set
            {
                if (excluder_level_3_max != value)
                {
                    excluder_level_3_max = value;
                    OnPropertyChanged();
                    if (int.TryParse(value, out int newValue))
                    {
                        try
                        {
                            dbModel.UpdateLevelSetColumn(2, "Level_3", newValue);
                            update_excluder_level_set();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"更新資料失敗: {ex.Message}");
                        }
                    }
                }
            }
        }
        public string excluder_level_4_max;
        public string excluder_level_4_max_text
        {
            get { return excluder_level_4_max; }
            set
            {
                if (excluder_level_4_max != value)
                {
                    excluder_level_4_max = value;
                    OnPropertyChanged();
                    if (int.TryParse(value, out int newValue))
                    {
                        try
                        {
                            dbModel.UpdateLevelSetColumn(2, "Level_4", newValue);
                            update_flusher_level_set();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"更新資料失敗: {ex.Message}");
                        }
                    }
                }
            }
        }
        public string excluder_level_2_min;
        public string excluder_level_2_min_text
        {
            get { return excluder_level_2_min; }
            set { excluder_level_2_min = value; OnPropertyChanged(); }
        }
        public string excluder_level_3_min;
        public string excluder_level_3_min_text
        {
            get { return excluder_level_3_min; }
            set { excluder_level_3_min = value; OnPropertyChanged(); }
        }
        public string excluder_level_4_min;
        public string excluder_level_4_min_text
        {
            get { return excluder_level_4_min; }
            set { excluder_level_4_min = value; OnPropertyChanged(); }
        }
        public string excluder_level_5_min;
        public string excluder_level_5_min_text
        {
            get { return excluder_level_5_min; }
            set { excluder_level_5_min = value; OnPropertyChanged(); }
        }
        //底沖排屑機開啟時間
        public string flusher_time;
        public string flusher_time_text
        {
            get { return flusher_time; }
            set { flusher_time = value; OnPropertyChanged(); }
        }
        public string excluder_time;
        public string excluder_time_text
        {
            get { return excluder_time; }
            set { excluder_time = value; OnPropertyChanged(); }
        }
        //底沖排屑機設定時間
        public ObservableCollection<string> flusher_LevelOptions { get; set; }

        // 選擇的量級
        private string flusher_selectedLevel;
        public string flusher_selectedLevel_text
        {
            get => flusher_selectedLevel;
            set
            {
                if (flusher_selectedLevel != value)
                {
                    flusher_selectedLevel = value;
                    OnPropertyChanged();
                    flusher_LoadLevelDetails(); // 更新選擇後的描述與時間
                }
            }
        }
        // 選擇量級的描述 (例如 "0%~20%")
        private string flusher_selectedLevelDescription;
        public string flusher_selectedLevelDescription_text
        {
            get => flusher_selectedLevelDescription;
            set { flusher_selectedLevelDescription = value; OnPropertyChanged(); }
        }

        // 選擇量級的時間
        private string flusher_selectedLevelTime;
        public string flusher_selectedLevelTime_text
        {
            get => flusher_selectedLevelTime;
            set
            {
                if (flusher_selectedLevelTime != value)
                {
                    flusher_selectedLevelTime = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<string> excluder_LevelOptions { get; set; }

        private string excluder_selectedLevel;
        public string excluder_selectedLevel_text
        {
            get => excluder_selectedLevel;
            set
            {
                if (excluder_selectedLevel != value)
                {
                    excluder_selectedLevel = value;
                    OnPropertyChanged();
                    excluder_LoadLevelDetails();
                }
            }
        }
        private string excluder_selectedLevelDescription;
        public string excluder_selectedLevelDescription_text
        {
            get => excluder_selectedLevelDescription;
            set { excluder_selectedLevelDescription = value; OnPropertyChanged(); }
        }
        private string excluder_selectedLevelTime;
        public string excluder_selectedLevelTime_text
        {
            get => excluder_selectedLevelTime;
            set
            {
                if (excluder_selectedLevelTime != value)
                {
                    excluder_selectedLevelTime = value;
                    OnPropertyChanged();
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
