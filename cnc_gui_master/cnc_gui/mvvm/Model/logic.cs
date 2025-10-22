using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using static cnc_gui.Focas1;
using cnc_gui;
using System.Threading.Tasks;
using LiveCharts.Maps;
using System.Windows;
using System.Reflection.Emit;
using static cnc_gui.DatabaseModel;
using System.Web.SessionState;
using MySqlX.XDevAPI.Common;
using System.Net.Http;
using MySql.Data.MySqlClient;
namespace cnc_gui
{
    public class core
    {
        private static readonly object lockObj = new object();
        public static Thread mainThread;
        public static Thread ExcluderThread;
        public static Thread RdspmeterThread;
        public static Thread checkOnOFFThread;
        private static bool start;
        private static homeViewModel viewModel;
        public static string CncIp;
        public static ushort CncPort;
        public static short FIdCode;  // cnc沖水點位idcord
        public static short EIdCode;  // cnc排屑點位idcord
        public static ushort Fdatano; // cnc沖水點位address
        public static long spindle;
        public static short connect_r4;
        private static DatabaseModel databaseModel;
        public static int threshold = 0;
        private static int Total_flusher_time;
        public static double ExFlusher_Energy = 0;
        public static double ExExcluder_Energy = 0;
        public static double Flusher_Energy = 0;
        public static double Excluder_Energy = 0;
        public static double CCD_Energy = 0;
        public static double WIFI_Energy = 0;
        public static double present_Energy = 0;
        public static Stopwatch countTime;
        public static float cameracoast = 0.000000347f;
        public static float wificoast = 0.000004167f;
        public static float ipccoast = 0.000033f;
        public static int level2_time;
        public static int level3_time;
        public static int level4_time;
        public static int level5_time;
        public static int threshold_id = 0;
        public static float ExcluderEnergyEfficiency = 0;
        private static int total_threshold;
        private static string connectionString = "server=localhost;database=cnc_db;user=root;password=ncut2024;";
        private flusher_excluder_onoff flusher_excluder_onoff_ { get; set; }
        public core()
        {
            viewModel = new homeViewModel();
            databaseModel = new DatabaseModel();
            LoadData();
        }

        public static void MainStart()
        {
            var core = new core();
            start = true;
            _ = Task.Run(async () => await Flusher());
            _ = Task.Run(() => Excluder());
            _ = Task.Run(() => Rdspmeter());
        }

        public static async Task MainStopAsync()
        {
            start = false;

            // 等待每個任務結束
            if (mainThread != null && mainThread.IsAlive)
            {
                await Task.Run(() => mainThread.Join(1000));
            }

            if (RdspmeterThread != null && RdspmeterThread.IsAlive)
            {
                await Task.Run(() => RdspmeterThread.Join(1000));
            }

            if (ExcluderThread != null && ExcluderThread.IsAlive)
            {
                await Task.Run(() => ExcluderThread.Join(1000));
            }
            ExcluderThread = null;
            mainThread = null;
            RdspmeterThread = null;
        }


        public static async Task Flusher()
        {
            while (start)
            {
                ushort FFlibHndl1;
                DateTime startTime = DateTime.Now;
                short R = Focas1.cnc_allclibhndl3(CncIp, CncPort, 1, out FFlibHndl1);

                viewModel.CaptureImage(@"E:\專題ppt合輯\專題\cnc_gui_master\method_AI\origin\origin.jpg");

                Console.WriteLine($"沖水總時間{Total_flusher_time}");
                await ImageProcess();

                int level = databaseModel.GetLevelResult().Flusher_level_result;
                //Random rnd = new Random();
                //int level = rnd.Next(1, 6);
                databaseModel.Updateflusher_excluder_onoff("flusher_state", 1);
                Flusher(level, Fdatano, Fdatano, FIdCode, FFlibHndl1);
                EnergyCal(level);
                Console.WriteLine($"門檻值{threshold}");
                databaseModel.Updateflusher_excluder_onoff("flusher_state", 0);
                DateTime endTime = DateTime.Now;
                TimeSpan duration = (endTime - startTime);
                int remainingTime = (Total_flusher_time * 1000) - (int)duration.TotalMilliseconds;
                if (remainingTime > 0)
                {
                    Console.WriteLine($"在{(float)remainingTime / 1000}s拍照");
                    await Task.Delay(remainingTime);
                }

                Focas1.cnc_freelibhndl(FFlibHndl1);
            }
        }
        public static async void Excluder()
        {
            while (start)
            {
                ushort FFlibHndl2;
                short R2 = Focas1.cnc_allclibhndl3(CncIp, CncPort, 1, out FFlibHndl2);
                if (threshold >= total_threshold)
                {
                    lock (lockObj)
                    {
                        threshold = 0;
                    }
                    int threshold_id = databaseModel.GetMaxId("flusher_history");
                    int last_threshold_zero_id = databaseModel.GetLastThresholdZeroIdBefore(threshold_id);
                    int id_difference = threshold_id - last_threshold_zero_id;
                    Console.WriteLine($"ID 相差數量為：{id_difference}");
                    ExcluderEnergyEfficiency = ((((id_difference * Total_flusher_time) - 120) / (float)(id_difference * Total_flusher_time)) * 100);
                    databaseModel.UpdateLevelResult("excluder_level_result", 5);
                    databaseModel.Updateflusher_excluder_onoff("excluder_state", 1);
                    Excluder(Fdatano, Fdatano, FIdCode, FFlibHndl2);
                    databaseModel.Updateflusher_excluder_onoff("excluder_state", 0);
                    databaseModel.UpdateLevelResult("excluder_level_result", 1);
                }
                if (threshold >= 0 && threshold <= total_threshold * 0.25)
                {
                    databaseModel.UpdateLevelResult("excluder_level_result", 1);
                }
                else if (threshold > total_threshold * 0.25 && threshold <= total_threshold * 0.5)
                {
                    databaseModel.UpdateLevelResult("excluder_level_result", 2);
                }
                else if (threshold > total_threshold * 0.5 && threshold <= total_threshold * 0.75)
                {
                    databaseModel.UpdateLevelResult("excluder_level_result", 3);
                }
                else if (threshold > total_threshold * 0.75 && threshold < total_threshold * 1)
                {
                    databaseModel.UpdateLevelResult("excluder_level_result", 4);
                }
                else
                {
                    databaseModel.UpdateLevelResult("excluder_level_result", 5);
                }


                Focas1.cnc_freelibhndl(FFlibHndl2);
            }
        }

        public static void Rdspmeter()
        {
            while (start)
            {
                ushort FFlibHndl3;
                short R3 = Focas1.cnc_allclibhndl3(CncIp, CncPort, 1, out FFlibHndl3);
                long get = GetData(FFlibHndl3);
                spindle = get;
                Thread.Sleep(1000);
                Focas1.cnc_freelibhndl(FFlibHndl3);
            }
        }
        private void LoadData()
        {
            databaseModel.UpdateLevelResult("excluder_level_result", 1);
            CncIp = databaseModel.GetIp_Port().Cncip;
            CncPort = StringToUshort(databaseModel.GetIp_Port().Cncport);
            FIdCode = (short)(int)databaseModel.GetColumnValueById("plc_control", "Handl", 1);
            EIdCode = (short)(int)databaseModel.GetColumnValueById("plc_control", "Handl", 2);
            Fdatano = (ushort)(int)databaseModel.GetColumnValueById("plc_control", "Address", 1);
            Total_flusher_time = databaseModel.GetFlusherTimeSetting().Total_flusher_time;
            level2_time = (int)databaseModel.GetColumnValueById("Level_set_time", "Level_2_time", 1);
            level3_time = (int)databaseModel.GetColumnValueById("Level_set_time", "Level_3_time", 1);
            level4_time = (int)databaseModel.GetColumnValueById("Level_set_time", "Level_4_time", 1);
            level5_time = (int)databaseModel.GetColumnValueById("Level_set_time", "Level_5_time", 1);
            total_threshold = databaseModel.GetSetting().Excluder_Period;

        }
        public static void EnergyCal(int level)
        {
            int t = threshold;
            DateTime currentTime = DateTime.Now;
            string timeString = currentTime.ToString("HH:mm:ss");
            TimeSpan timeValue = TimeSpan.Parse(timeString);
            //底座環沖節能效益
            float originalFlusherEnergy = Total_flusher_time * 0.0002689f;
            float FlusherEnergyEfficiency = (float)Math.Round(((originalFlusherEnergy - Flusher_Energy) / originalFlusherEnergy) * 100, 2);
            Console.WriteLine($"原始底座環沖{originalFlusherEnergy}");
            Console.WriteLine($"開系統底座環沖{Flusher_Energy}");
            Console.WriteLine($"總節能{FlusherEnergyEfficiency}");
            //節能效益
            float originalTotalEnergyEfficiency = Total_flusher_time * 0.0002689f + Total_flusher_time * 0.000049f;
            Console.WriteLine($"原始能耗%{originalTotalEnergyEfficiency}");
            float usedEnergy = (float)(Flusher_Energy + Excluder_Energy + cameracoast * Total_flusher_time + wificoast * Total_flusher_time + ipccoast * Total_flusher_time);
            float TotalEnergyEfficiency = (float)Math.Round(((originalTotalEnergyEfficiency - usedEnergy) / originalTotalEnergyEfficiency) * 100, 2);
            //總節能效益
            float sumEnergyEfficiency = 0f;
            string sql = "SELECT AVG(total_energy_sum) FROM flusher_history;";
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        float avgFromDb = Convert.ToSingle(result);
                        sumEnergyEfficiency = (float)Math.Round((avgFromDb + TotalEnergyEfficiency) / 2, 2);
                    }
                    else
                    {
                        sumEnergyEfficiency = TotalEnergyEfficiency;
                    }
                }
            }
            float totalkwhEfficiency = (originalTotalEnergyEfficiency - usedEnergy);
            int? historyNewId = databaseModel.GetMaxId("flusher_history");
            float newTotalKwh;
            if (historyNewId != null)
            {
                object oldKwhObj = databaseModel.GetColumnValueById("flusher_history", "total_kwh_sum", historyNewId.Value);

                if (oldKwhObj != null && oldKwhObj != DBNull.Value)
                {
                    float oldKwh = Convert.ToSingle(oldKwhObj);
                    newTotalKwh = oldKwh + totalkwhEfficiency;
                }
                else
                {
                    newTotalKwh = totalkwhEfficiency;
                }
            }
            else
            {
                newTotalKwh = totalkwhEfficiency;
            }
            databaseModel.Updatelevelhistory(timeValue, level, t, FlusherEnergyEfficiency, ExcluderEnergyEfficiency, TotalEnergyEfficiency, sumEnergyEfficiency, totalkwhEfficiency, newTotalKwh);
            lock (lockObj) { Excluder_Energy = 0; }
        }
        //沖水按鈕
        public static void TestFlusher()
        {
            ushort FFlibHndl4;
            short R5 = Focas1.cnc_allclibhndl3(CncIp, CncPort, 1, out FFlibHndl4);
            try
            {
                int flusher_state_value;
                flusher_state_value = databaseModel.Getflusher_excluder_onoff().flusher_state;
                if (flusher_state_value == 1)
                {
                    int plc_decimal = ReadByteParam(Fdatano, Fdatano, FIdCode, FFlibHndl4);
                    databaseModel.UpdatePlcControl(1, "Decimal_num", plc_decimal);
                    databaseModel.Updateflusher_excluder_onoff("flusher_state", 0);
                }
                else
                {
                    int plc_decimal = ReadByteParam(Fdatano, Fdatano, FIdCode, FFlibHndl4);
                    databaseModel.UpdatePlcControl(1, "Decimal_num", plc_decimal);
                    databaseModel.Updateflusher_excluder_onoff("flusher_state", 1);
                }
                WritePmcData(Fdatano, Fdatano, 1, FIdCode, FFlibHndl4);
            }
            finally
            {
                Focas1.cnc_freelibhndl(FFlibHndl4);
            }
        }

        public static void TestExcluder()
        {
            ushort FFlibHndl4;
            short R6 = Focas1.cnc_allclibhndl3(CncIp, CncPort, 1, out FFlibHndl4);
            try
            {
                int excluder_state_value;
                excluder_state_value = databaseModel.Getflusher_excluder_onoff().excluder_state;
                if (excluder_state_value == 1)
                {

                    int plc_decimal = ReadByteParam(Fdatano, Fdatano, FIdCode, FFlibHndl4);
                    databaseModel.UpdatePlcControl(2, "Decimal_num", plc_decimal);
                    databaseModel.Updateflusher_excluder_onoff("excluder_state", 0);
                }
                else
                {
                    int plc_decimal = ReadByteParam(Fdatano, Fdatano, FIdCode, FFlibHndl4);
                    databaseModel.UpdatePlcControl(2, "Decimal_num", plc_decimal);
                    databaseModel.Updateflusher_excluder_onoff("excluder_state", 1);
                }
                WritePmcData(Fdatano, Fdatano, 0, FIdCode, FFlibHndl4);
            }
            finally
            {
                Focas1.cnc_freelibhndl(FFlibHndl4);
            }
        }

        public static int StringToInt(string inString)
        {
            int.TryParse(inString, out int result);
            return result;
        }
        public static ushort StringToUshort(string inString)
        {
            ushort.TryParse(inString, out ushort result);
            return result;
        }
        public static short StringToShort(string inString)
        {
            short.TryParse(inString, out short result);
            return result;

        }
        //影像處理+AI
        //影像處理+AI
        public static async Task ImageProcess()
        {
            using (HttpClient client = new HttpClient())
            {

                string url2 = @"http://localhost:5002/predict_r2";
                string url3 = @"http://localhost:5003/predict_r3";
                string url4 = @"http://localhost:5004/predict_r4";

                // 同時發送三個請求
                Task<HttpResponseMessage> task2 = client.GetAsync(url2);
                Task<HttpResponseMessage> task3 = client.GetAsync(url3);
                Task<HttpResponseMessage> task4 = client.GetAsync(url4);

                // 等待所有請求完成
                HttpResponseMessage[] responses = await Task.WhenAll(task2, task3, task4);

                foreach (var response in responses)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("回應：" + result);
                    }
                    else
                    {
                        Console.WriteLine("請檢查 API 是否正常運作");
                    }
                }
            }
        }

        //讀取主軸負載
        public static long GetData(ushort FFlibHnd1)
        {
            long data = -1;
            short data_num = 1;
            Focas1.Odbspload spindleLoad = new Focas1.Odbspload();
            var ret = Focas1.cnc_rdspmeter(FFlibHnd1, 0, ref data_num, spindleLoad);

            if (ret == Focas1.EW_OK)
            {
                data = spindleLoad.spload_data.spload.data;



            }
            else
            {
                Console.WriteLine("failed to read");
            }
            return data;
        }

        //10進制轉2進制陣列
        public static int[] ConvertToBinaryArray(int decimalNumber)
        {
            int[] binaryarr = new int[8];
            for (int i = 0; i < 8; i++)
            {
                binaryarr[7 - i] = (decimalNumber >> i) & 1;
            }
            return binaryarr;
        }

        //2進制轉10進制
        public static int ConvertBinaryArrayToDecimal(int[] binaryArray)
        {
            int decimalValue = 0;
            int length = binaryArray.Length;
            for (int i = 0; i < length; i++)
            {
                if (binaryArray[i] != 0 && binaryArray[i] != 1)
                    throw new FormatException("錯誤的二進制數值。");
                decimalValue += binaryArray[length - 1 - i] * (int)Math.Pow(2, i);
            }
            return decimalValue;
        }


        //讀取十進制數值，之前資料型態是long
        public static int ReadByteParam(ushort datano_s, ushort datano_e, short IdCode, ushort FFlibHndl) //起始位置，結束位置，idcode
        {
            ushort length = (ushort)(8 + (datano_e - datano_s + 1));
            Focas1.Iodbpmc buf = new Focas1.Iodbpmc();
            short ret = Focas1.pmc_rdpmcrng(FFlibHndl, IdCode, 0, datano_e, datano_s, length, buf);
            return buf.cdata[0];

        }
        //寫入修改好的10進制數值，要修改的時候就呼叫一次
        public static void WritePmcData(ushort datano_s, ushort datano_e, int i, short IdCode, ushort FFlibHndl) //起始位置，結束位置，i=要修改的bit，idcode
        {
            ReadByteParam(datano_s, datano_e, IdCode, FFlibHndl);
            ushort length = (ushort)(8 + (datano_e - datano_s + 1));
            Focas1.Iodbpmc buf = new Focas1.Iodbpmc();
            short ret = Focas1.pmc_rdpmcrng(FFlibHndl, IdCode, 0, datano_e, datano_s, length, buf);
            int[] binaryArray = ConvertToBinaryArray(buf.cdata[0]);
            if (binaryArray.Length > 0)
            {
                int machineIndex = binaryArray.Length - 1 - i; // 映射i到機器的位址
                binaryArray[machineIndex] = binaryArray[machineIndex] == 0 ? 1 : 0;
            }
            int modifiedDecimalValue = ConvertBinaryArrayToDecimal(binaryArray);
            buf.cdata[0] = (byte)modifiedDecimalValue;
            short rt = Focas1.pmc_wrpmcrng(FFlibHndl, (short)length, buf);
        }
        //底座環沖控制
        public static void Flusher(int level, ushort datano_s, ushort datano_e, short IdCode, ushort FFlibHndl)
        {
            if (level == 1)
            {
                threshold += 0;
            }

            else if (level == 2)
            {
                threshold += (int)databaseModel.GetColumnValueById("Level_set_time", "Level_2_time", 2);
                WritePmcData(datano_s, datano_e, 1, IdCode, FFlibHndl);
                Thread.Sleep(level2_time * 1000);
                WritePmcData(datano_s, datano_e, 1, IdCode, FFlibHndl);
                Flusher_Energy = 0.0002689 * 4;
            }
            else if (level == 3)
            {
                threshold += (int)databaseModel.GetColumnValueById("Level_set_time", "Level_3_time", 2);
                WritePmcData(datano_s, datano_e, 1, IdCode, FFlibHndl);
                Thread.Sleep(level3_time * 1000);
                WritePmcData(datano_s, datano_e, 1, IdCode, FFlibHndl);
                Flusher_Energy = 0.0002689 * 5;
            }
            else if (level == 4)
            {
                threshold += (int)databaseModel.GetColumnValueById("Level_set_time", "Level_4_time", 2);
                WritePmcData(datano_s, datano_e, 1, IdCode, FFlibHndl);
                Thread.Sleep(level4_time * 1000);
                WritePmcData(datano_s, datano_e, 1, IdCode, FFlibHndl);
                Flusher_Energy = 0.0002689 * 7;
            }
            else if (level == 5)
            {
                threshold += (int)databaseModel.GetColumnValueById("Level_set_time", "Level_5_time", 2);
                WritePmcData(datano_s, datano_e, 1, IdCode, FFlibHndl);
                Thread.Sleep(level5_time * 1000);
                WritePmcData(datano_s, datano_e, 1, IdCode, FFlibHndl);
                Flusher_Energy = 0.0002689 * 8;
            }

        }
        //排屑機控制
        public static void Excluder(ushort datano_s, ushort datano_e, short IdCode, ushort FFlibHndl)
        {
            
            lock (lockObj) { Excluder_Energy = 0.000049 * 120; }
            WritePmcData(datano_s, datano_e, 0, IdCode, FFlibHndl);
            Thread.Sleep(120000);
            WritePmcData(datano_s, datano_e, 0, IdCode, FFlibHndl);

        }
    }
    public class Focas1
{
    // Declare constants and methods from FOCAS library
    public const short EW_OK = 0;
    [DllImport("./Fwlib32.dll")]
    public static extern short cnc_allclibhndl3(string ip, ushort port, int timeout, out ushort libhndl);

    [DllImport("./Fwlib32.dll")]
    public static extern short cnc_freelibhndl(ushort libhndl);

    [DllImport("./Fwlib32.dll")]
    public static extern short cnc_rdspmeter(ushort libhndl, short type, ref short data_num, [MarshalAs(UnmanagedType.LPStruct), Out] Odbspload spmeter);

    [DllImport("./Fwlib32.dll")]
    public static extern short pmc_rdpmcrng(ushort FlibHndl, short adr_type, short data_type, ushort s_number, ushort e_number, ushort length, [MarshalAs(UnmanagedType.LPStruct), Out] Iodbpmc buf);

    [DllImport("./Fwlib32.dll")]
    public static extern short pmc_wrpmcrng(ushort FlibHndl, short length, [MarshalAs(UnmanagedType.LPStruct), In] Iodbpmc buf);
    [StructLayout(LayoutKind.Sequential)]

    public class Odbspload
    {
        public Odbspload_data spload_data = new Odbspload_data();
    }
    [StructLayout(LayoutKind.Sequential)]
    public class Odbspload_data
    {
        public Loadlm spload = new Loadlm();
        public Loadlm spspeed = new Loadlm();
    }
    [StructLayout(LayoutKind.Sequential)]
    public class Loadlm
    {
        public long data;       /* load meter data, motor speed */
        public short dec;        /* place of decimal point */
        public short unit;       /* unit */
        public char name;       /* spindle name */
        public char suff1;      /* subscript of spindle name 1 */
        public char suff2;      /* subscript of spindle name 2 */
        public char reserve;    /* */

    }

    [StructLayout(LayoutKind.Explicit)]
    public class Iodbpmc
    {
        [FieldOffset(0)]
        public short type_a;   /* Kind of PMC address */
        [FieldOffset(2)]
        public short type_d;   /* Type of the PMC data */
        [FieldOffset(4)]
        public ushort datano_s; /* Start PMC address number */
        [FieldOffset(6)]
        public ushort datano_e;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        [FieldOffset(8)]
        public byte[] cdata;
    }
}
}