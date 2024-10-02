using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace cnc_gui
{
    /// <summary>
    /// setting.xaml 的互動邏輯
    /// </summary>
    public partial class setting : Page
    {
        //json檔
        public class Config
        {
            public string Cncip { get; set; }
            public string Cncport { get; set; }
            public string[] FlusherLevels { get; set; }
            public string[] FlusherLevelsl { get; set; }
            public string[] FlusherlevelSt { get; set; }
            public string[] FlusherTime { get; set; }
            public string[] ExcluderLevels { get; set; }
            public string[] ExcluderLevelsl { get; set; }
            public string[] ExcluderlevelSt { get; set; }
            public string[] ExcluderTime { get; set; }
        }

        private string configFilePath = "config.json"; // 存放設定的檔案路徑
        private Config config; // 儲存設定的物件


        public static string Cncip { get; private set; } = "192.168.1.300"; // 預設 IP 地址
        public static string Cncport { get; private set; } = "8192";
        public string flusherPercentage { get; set; } = "選擇";
        public string excluderPercentage { get; set; } = "選擇";

        public static string[] flusherLevels { get; private set; } = { "20", "40", "60", "80" };
        public static string[] flusherLevelsl { get; private set; } = { "21", "41", "61", "81" };
        public static string[] flusherlevel_st = { "0%~20%", "21%~40%", "41%~60%", "61%~80%", "81%~100%", "", "" };
        public static string[] flusher_time = { "0", "10", "20", "30", "40" };

        public static string[] excluderLevels { get; private set; } = { "20", "40", "60", "80" };
        public static string[] excluderLevelsl { get; private set; } = { "21", "41", "61", "81" };
        public static string[] excluderlevel_st = { "0%~20%", "21%~40%", "41%~60%", "61%~80%", "81%~100%", "", "" };
        public static string[] excluder_time = { "0", "10", "20", "30", "40" };

        //警告並跳出messagebox系統
        public string messageboxtext = "test";
        public string caption = "警告";
        MessageBoxButton button = MessageBoxButton.YesNoCancel;
        MessageBoxImage icon = MessageBoxImage.Warning;
        MessageBoxResult result;



        public void LoadConfig()
        {
            // 如果設定檔不存在，就建立一個預設的 Config
            if (!File.Exists(configFilePath))
            {
                config = new Config
                {
                    Cncip = "192.168.1.300",
                    Cncport = "8192",
                    FlusherLevels = new[] { "20", "40", "60", "80" },
                    FlusherLevelsl = new[] { "21", "41", "61", "81" },
                    FlusherlevelSt = new[] { "0%~20%", "21%~40%", "41%~60%", "61%~80%", "81%~100%", "", "" },
                    FlusherTime = new[] { "0", "10", "20", "30", "40" },
                    ExcluderLevels = new[] { "20", "40", "60", "80" },
                    ExcluderLevelsl = new[] { "21", "41", "61", "81" },
                    ExcluderlevelSt = new[] { "0%~20%", "21%~40%", "41%~60%", "61%~80%", "81%~100%", "", "" },
                    ExcluderTime = new[] { "0", "10", "20", "30", "40" }
                };

                SaveConfig(); // 預設儲存設定檔
            }
            else
            {
                // 讀取已存在的設定檔
                string json = File.ReadAllText(configFilePath);
                config = JsonConvert.DeserializeObject<Config>(json);

                flusherLevels = config.FlusherLevels;
                flusherLevelsl = config.FlusherLevelsl;
                flusherlevel_st = config.FlusherlevelSt;
                flusher_time = config.FlusherTime;

                excluderLevels = config.ExcluderLevels;
                excluderLevelsl = config.ExcluderLevelsl;
                excluderlevel_st = config.ExcluderlevelSt;
                excluder_time = config.ExcluderTime;
            }
        }
        //%樹轉換成string
        public static void GenerateLevels()
        {
            // 第一個範圍從 0% 開始
            flusherlevel_st[0] = "0%~" + flusherLevels[0] + "%";
            excluderlevel_st[0] = "0%~" + excluderLevels[0] + "%";

            // 生成 flusherlevel_st
            for (int i = 0; i < flusherLevels.Length - 1; i++)
            {
                flusherlevel_st[i + 1] = flusherLevelsl[i] + "%~" + flusherLevels[i + 1] + "%";
            }
            flusherlevel_st[4] = flusherLevelsl[3] + "%~100%";  // 最後一個範圍

            // 生成 excluderlevel_st
            for (int i = 0; i < excluderLevels.Length - 1; i++)
            {
                excluderlevel_st[i + 1] = excluderLevelsl[i] + "%~" + excluderLevels[i + 1] + "%";
            }
            excluderlevel_st[4] = excluderLevelsl[3] + "%~100%";  // 最後一個範圍
        }
        public setting()
        {
            InitializeComponent();
            LoadConfig();
            // 初始化時，將靜態變數值顯示在 TextBox 上
            setting_ip.Text = config.Cncip;
            setting_port.Text = config.Cncport;

            flusher_lev1_r.Text = flusherLevels[0];
            flusher_lev2_r.Text = flusherLevels[1];
            flusher_lev3_r.Text = flusherLevels[2];
            flusher_lev4_r.Text = flusherLevels[3];

            flusher_lev2_l.Text = flusherLevelsl[0];
            flusher_lev3_l.Text = flusherLevelsl[1];
            flusher_lev4_l.Text = flusherLevelsl[2];
            flusher_lev5_l.Text = flusherLevelsl[3];

            excluder_lev1_r.Text = excluderLevels[0];
            excluder_lev2_r.Text = excluderLevels[1];
            excluder_lev3_r.Text = excluderLevels[2];
            excluder_lev4_r.Text = excluderLevels[3];

            excluder_lev2_l.Text = excluderLevelsl[0];
            excluder_lev3_l.Text = excluderLevelsl[1];
            excluder_lev4_l.Text = excluderLevelsl[2];
            excluder_lev5_l.Text = excluderLevelsl[3];

            // 註冊 TextChanged 事件來監控文本變化
            setting_ip.TextChanged += Setting_ip_TextChanged;
            setting_port.TextChanged += Setting_port_TextChanged;

            flusher_lev1_r.TextChanged += flusher_lev1_r_TextChanged;
            flusher_lev2_r.TextChanged += flusher_lev2_r_TextChanged;
            flusher_lev3_r.TextChanged += flusher_lev3_r_TextChanged;
            flusher_lev4_r.TextChanged += flusher_lev4_r_TextChanged;

            excluder_lev1_r.TextChanged += excluder_lev1_r_TextChanged;
            excluder_lev2_r.TextChanged += excluder_lev2_r_TextChanged;
            excluder_lev3_r.TextChanged += excluder_lev3_r_TextChanged;
            excluder_lev4_r.TextChanged += excluder_lev4_r_TextChanged;
            DataContext = this;
        }

        //saveconfig
        private void SaveConfig()
        {
            config.ExcluderLevels = excluderLevels; // 更新配置中的 excluderLevels
            config.ExcluderLevelsl = excluderLevelsl;
            config.ExcluderlevelSt = excluderlevel_st;
            config.ExcluderTime = excluder_time;

            config.FlusherLevels = flusherLevels;
            config.FlusherLevelsl = flusherLevelsl;
            config.FlusherlevelSt = flusherlevel_st;
            config.FlusherTime = flusher_time;

            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configFilePath, json);
        }



        //ip 更新
        private void Setting_ip_TextChanged(object sender, TextChangedEventArgs e)
        {
            Cncip = setting_ip.Text.ToString();
            config.Cncip = setting_ip.Text.ToString();
            SaveConfig(); // 儲存到 JSON
        }
        //port 更新
        private void Setting_port_TextChanged(object sender, TextChangedEventArgs e)
        {
            Cncport = setting_port.Text.ToString();
            config.Cncport = setting_port.Text.ToString();
            SaveConfig(); // 儲存到 JSON
        }
        //選單欄狀態更新
        private void flusherButton_Checked(object sender, RoutedEventArgs e)
        {
            LoadConfig();
            if (sender is RadioButton radioButton)
            {
                flusherPercentage = radioButton.Tag.ToString();
                DataContext = null;
                DataContext = this;
            }
            RadioButton selectedRadioButton = sender as RadioButton;
            if (selectedRadioButton != null)
            {
                // 取得選擇的範圍
                string selectedRange = selectedRadioButton.Tag.ToString();
                int index = Array.IndexOf(flusherlevel_st, selectedRange);

                if (index >= 0 && index < flusher_time.Length)
                {
                    // 根據選擇的範圍更新 TextBox
                    flusher_time_st.Text = flusher_time[index];
                    SaveConfig();
                }
            }
        }
        private void excluderButton_Checked(object sender, RoutedEventArgs e)
        {
            LoadConfig();

            if (sender is RadioButton radioButton)
            {
                excluderPercentage = radioButton.Tag.ToString();
                DataContext = null;
                DataContext = this;
            }
            RadioButton selectedRadioButton = sender as RadioButton;
            if (selectedRadioButton != null)
            {
                // 取得選擇的範圍
                string selectedRange = selectedRadioButton.Tag.ToString();
                int index = Array.IndexOf(excluderlevel_st, selectedRange);

                if (index >= 0 && index < excluder_time.Length)
                {
                    // 根據選擇的範圍更新 TextBox
                    excluder_time_st.Text = excluder_time[index];
                    SaveConfig();
                }
            }

        }

        private RadioButton FindCheckedRadioButtonForFlusher()
        {
            RadioButton[] radioButtons = { exp_flusher_time_lev1, exp_flusher_time_lev2, exp_flusher_time_lev3, exp_flusher_time_lev4, exp_flusher_time_lev5 };
            return radioButtons.FirstOrDefault(rb => rb.IsChecked == true);
        }
        private RadioButton FindCheckedRadioButtonForExcluder()
        {
            RadioButton[] radioButtons = { exp_excluder_time_lev1, exp_excluder_time_lev2, exp_excluder_time_lev3, exp_excluder_time_lev4, exp_excluder_time_lev5 };
            return radioButtons.FirstOrDefault(rb => rb.IsChecked == true);

        }
        //選單欄數值更新
        private void flusher_time_st_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string newTime = textBox.Text;

                // 更新 flusher_time 中的值
                RadioButton selectedRadioButton = FindCheckedRadioButtonForFlusher();
                if (selectedRadioButton != null)
                {
                    int index = Array.IndexOf(flusherlevel_st, selectedRadioButton.Tag.ToString());
                    if (index >= 0 && index < flusher_time.Length)
                    {
                        flusher_time[index] = newTime;
                        SaveConfig();
                    }
                }
            }
        }
        private void excluder_time_st_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string newTime = textBox.Text;

                // 更新 excluder_time 中的值
                RadioButton selectedRadioButton = FindCheckedRadioButtonForExcluder();
                if (selectedRadioButton != null)
                {
                    int index = Array.IndexOf(excluderlevel_st, selectedRadioButton.Tag.ToString());
                    if (index >= 0 && index < excluder_time.Length)
                    {
                        excluder_time[index] = newTime;
                        SaveConfig();
                    }
                }
            }
        }


        private void flusher_InitializeRadioButtons()
        {
            // 初始化 RadioButton 的 Tag
            RadioButton[] radioButtons = { exp_flusher_time_lev1, exp_flusher_time_lev2, exp_flusher_time_lev3, exp_flusher_time_lev4, exp_flusher_time_lev5 };
            for (int i = 0; i < radioButtons.Length; i++)
            {
                radioButtons[i].Tag = flusherlevel_st[i];
            }
        }
        private void excluder_InitializeRadioButtons()
        {
            // 初始化 RadioButton 的 Tag
            RadioButton[] radioButtons = { exp_excluder_time_lev1, exp_excluder_time_lev2, exp_excluder_time_lev3, exp_excluder_time_lev4, exp_excluder_time_lev5 };
            for (int i = 0; i < radioButtons.Length; i++)
            {
                radioButtons[i].Tag = excluderlevel_st[i];
            }
        }

        //更新底座環沖排屑機運行規則level按下enter更新

        private void flusher_lev1_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            flusherLevels[0] = flusher_lev1_r.Text.ToString();
        }
        private void flusher_lev1_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(flusherLevels[0], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    flusher_lev2_l.Text = level.ToString();
                    flusherLevelsl[0] = level.ToString();
                    GenerateLevels();
                    flusher_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }
        private void flusher_lev2_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            flusherLevels[1] = flusher_lev2_r.Text.ToString();
        }
        private void flusher_lev2_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(flusherLevels[1], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    flusher_lev3_l.Text = level.ToString();
                    flusherLevelsl[1] = level.ToString();
                    GenerateLevels();
                    flusher_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }
        private void flusher_lev3_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            flusherLevels[2] = flusher_lev3_r.Text.ToString();
        }
        private void flusher_lev3_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(flusherLevels[2], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    flusher_lev4_l.Text = level.ToString();
                    flusherLevelsl[2] = level.ToString();
                    GenerateLevels();
                    flusher_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }
        private void flusher_lev4_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            flusherLevels[3] = flusher_lev4_r.Text.ToString();
        }
        private void flusher_lev4_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(flusherLevels[3], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    flusher_lev5_l.Text = level.ToString();
                    flusherLevelsl[3] = level.ToString();
                    GenerateLevels();
                    flusher_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }

        private void excluder_lev1_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            excluderLevels[0] = excluder_lev1_r.Text.ToString();
        }
        private void excluder_lev1_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(excluderLevels[0], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    excluder_lev2_l.Text = level.ToString();
                    excluderLevelsl[0] = level.ToString();
                    GenerateLevels();
                    excluder_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }
        private void excluder_lev2_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            excluderLevels[1] = excluder_lev2_r.Text.ToString();
        }
        private void excluder_lev2_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(excluderLevels[1], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    excluder_lev3_l.Text = level.ToString();
                    excluderLevelsl[1] = level.ToString();
                    GenerateLevels();
                    excluder_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }
        private void excluder_lev3_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            excluderLevels[2] = excluder_lev3_r.Text.ToString();
        }
        private void excluder_lev3_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(excluderLevels[2], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    excluder_lev4_l.Text = level.ToString();
                    excluderLevelsl[2] = level.ToString();
                    GenerateLevels();
                    excluder_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }
        private void excluder_lev4_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            excluderLevels[3] = excluder_lev4_r.Text.ToString();
        }
        private void excluder_lev4_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(excluderLevels[3], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    excluder_lev5_l.Text = level.ToString();
                    excluderLevelsl[3] = level.ToString();
                    GenerateLevels();
                    excluder_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }


    }
}