using System;
using System.Diagnostics;  // Process 類所在命名空間
using System.Net;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Threading.Tasks; // Task 類所在命名空間
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using static Logic.Focas1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Timers;
using cnc_gui;
namespace Logic
{

    public class core
    {
        private static readonly object lockObj = new object();
        private static bool OnOff = true;   // 用來控制是否繼續運行
        public static Task mainTask = null;  // 保存運行的 Task
        public static Task excluderTask = null;// 用於執行排屑機的Task
        public static Task Rdspmeter = null;// 用於執行主軸負載的Task
        private static CancellationTokenSource cancellationTokenSource; // 用來取消任務的標記
        public static ushort FFlibHndl;
        private static string FAddress;
        private static ushort FPort;
        public static short R;
        private static int T = 0;  // 計數器
        private static int C;
        private static int CurrentParam = 5; //目前c值

        public core()
        {
            JsonRead();
            R = Focas1.cnc_allclibhndl3(FAddress, FPort, 1, out FFlibHndl);
        }

        public static async Task Main(bool start)

        {
            var mc = new core();
            if (start)
            {
                if (mainTask == null || mainTask.IsCompleted)
                {
                    // 當按下啟動按鈕時，開始新的 Task
                    cancellationTokenSource = new CancellationTokenSource();
                    var token = cancellationTokenSource.Token;
                    OnOff = true;

                    // 開啟背景執行的主任務
                    mainTask = Task.Run(async () =>
                    {
                        // 持續運行直到 OnOff 設置為 false 或任務被取消
                        while (OnOff && !token.IsCancellationRequested)
                        {
                            var task = Task.Delay(100000);
                            try
                            {
                                mc.RunPythonScript(); // 拍照
                                mc.Flusher(0);
                            }
                            catch (Exception ex)
                            {
                                break;
                            }
                            task.Wait();
                            T += 1;
                        }
                    }, token);
                    excluderTask = Task.Run(() =>
                    {
                        while (OnOff && !token.IsCancellationRequested)
                        {
                            mc.Excluder(CurrentParam);
                            lock (lockObj)
                            {
                                // 每當 T == 5 時，更新參數
                                if (T == 5)
                                {
                                    CurrentParam = C;//改變排屑機作動頻率
                                    C = 0;
                                }
                            }
                        }
                    }, token);
                    //主軸負載
                    Rdspmeter = Task.Run(() =>
                    {
                        while (OnOff && !token.IsCancellationRequested)
                        {
                            long get = mc.GetData();
                            mc.Writejson(get);
                            Thread.Sleep(5000);//以後改1秒
                        }
                    }, token);
                }
            }
            else
            {
                // 當按下停止按鈕時，取消任務並設置 OnOff 為 false
                if (mainTask != null && !mainTask.IsCompleted)
                {
                    OnOff = false;
                    cancellationTokenSource.Cancel();
                    try
                    {
                        await mainTask;  // 等待主任務結束
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine("任務已取消");
                    }
                }
            }
        }
        //抓參數
        public void JsonRead()
        {
            string configFilePath = "config.json";
            //讀取 JSON 文件内容
            string jsonContent = File.ReadAllText(configFilePath);
            //將JSON字符串解析為JObject
            JObject jsonObject = JObject.Parse(jsonContent);
            FAddress = (string)jsonObject["Ccnip"];
            FPort = (ushort)jsonObject["Cncport"];
        }
        //寫主軸負載進去json檔
        void Writejson(long m)
        {

        }

        //拍照
        void RunPythonScript()
        {
            Process p = new Process();

            // 設定 Python檔的絕對路徑
            string filePath = @"C:\Users\User\Desktop\cnc_gui_s-master\cnc_gui\takepicture.py";//檔案路徑
            p.StartInfo.FileName = @"C:\python\python.exe"; // Python直譯器
            p.StartInfo.Arguments = filePath;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true; // 不顯示控制台視窗
            p.Start();
        }
        //影像積屑量級判斷
        void ImageJudgment()
        {
            Process process = new Process();

            // 設定 Python檔的絕對路徑
            string filePath = @"C:\Users\User\Desktop\cnc_gui_s-master\cnc_gui\takepicture.py";//檔案路徑
            process.StartInfo.FileName = @"C:\python\python.exe"; // Python直譯器
            process.StartInfo.Arguments = filePath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true; // 不顯示控制台視窗
            process.Start();
        }
        //讀取主軸負載
        public long GetData()
        {
            long data = -1;
            short data_num = 1;
            Focas1.Odbspload spindleLoad = new Focas1.Odbspload();
            var ret = Focas1.cnc_rdspmeter(FFlibHndl, 0, ref data_num, spindleLoad);

            if (ret == Focas1.EW_OK)
            {
                data = spindleLoad.spload_data.spload.data;
            }
            else
            {
                Console.WriteLine("Failed to read spindle load.");
            }
            return data;
        }

        //10進制轉2進制陣列
        int[] ConvertToBinaryArray(int decimalNumber)
        {
            int[] binaryarr = new int[8];
            for (int i = 0; i < 8; i++)
            {
                binaryarr[7 - i] = (decimalNumber >> i) & 1;
            }
            return binaryarr;
        }

        //2進制轉10進制
        int ConvertBinaryArrayToDecimal(int[] binaryArray)
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

        //讀取十進制數值
        public long ReadByteParam(ushort datano_s, ushort datano_e)
        {
            ushort length = (ushort)(8 + (datano_e - datano_s + 1));
            Focas1.Iodbpmc buf = new Focas1.Iodbpmc();
            short ret = Focas1.pmc_rdpmcrng(FFlibHndl, 5, 0, datano_e, datano_s, length, buf);
            return buf.cdata[0];
        }
        //寫入修改好的10進制數值，要修改的時候就呼叫一次
        public void WritePmcData(ushort datano_s, ushort datano_e, int i) //起始位置，結束位置，i=要修改的bit
        {
            ReadByteParam(datano_s, datano_e);
            ushort length = (ushort)(8 + (datano_e - datano_s + 1));
            Focas1.Iodbpmc buf = new Focas1.Iodbpmc();
            short ret = Focas1.pmc_rdpmcrng(FFlibHndl, 5, 0, datano_e, datano_s, length, buf);
            int[] binaryArray = ConvertToBinaryArray(buf.cdata[0]); //10轉2
            if (binaryArray.Length > 0)
            {
                int machineIndex = binaryArray.Length - 1 - i; // 映射i到機器的位址
                binaryArray[machineIndex] = binaryArray[machineIndex] == 0 ? 1 : 0;
            }
            int modifiedDecimalValue = ConvertBinaryArrayToDecimal(binaryArray);//2轉10
            buf.cdata[0] = (byte)modifiedDecimalValue;
            short rt = Focas1.pmc_wrpmcrng(FFlibHndl, (short)length, buf);
        }
        //底座環沖控制
        public void Flusher(int time)
        {
            if (time == 1)
            {

            }

            if (time == 2)
            {
                WritePmcData(7000, 7000, 0);
                Thread.Sleep(2000);
                WritePmcData(7000, 7000, 0);
            }
            if (time == 3)
            {
                WritePmcData(7000, 7000, 0);
                Thread.Sleep(3000);
                WritePmcData(7000, 7000, 0);
            }
            if (time == 4)
            {
                WritePmcData(7000, 7000, 0);
                Thread.Sleep(5000);
                WritePmcData(7000, 7000, 0);
            }
            if (time == 5)
            {
                WritePmcData(7000, 7000, 0);
                Thread.Sleep(6000);
                WritePmcData(7000, 7000, 0);
            }
        }
        //排屑機控制
        void Excluder(int c)
        {
            int T = 100;
            int OnTime;
            int FullTime;
            if (c == 5)
            {
                FullTime = T * 1000;
                OnTime = T * 100;
                var task = Task.Delay(FullTime);
                WritePmcData(7000, 7000, 1);
                Thread.Sleep(OnTime);
                WritePmcData(7000, 7000, 1);
                task.Wait();
            }
            if (c > 5 && c < 11)
            {
                FullTime = T * 1000;
                OnTime = T * 300;
                var task = Task.Delay(FullTime);
                WritePmcData(7000, 7000, 1);
                Thread.Sleep(OnTime);
                WritePmcData(7000, 7000, 1);
                task.Wait();
            }
            if (c > 10 && c < 16)
            {
                FullTime = T * 1000;
                OnTime = T * 500;
                var task = Task.Delay(FullTime);
                WritePmcData(7000, 7000, 1);
                Thread.Sleep(OnTime);
                WritePmcData(7000, 7000, 1);
                task.Wait();
            }
            if (c > 15 && c < 21)
            {
                FullTime = T * 1000;
                OnTime = T * 700;
                var task = Task.Delay(FullTime);
                WritePmcData(7000, 7000, 1);
                Thread.Sleep(OnTime);
                WritePmcData(7000, 7000, 1);
                task.Wait();
            }
            if (c > 20 && c < 26)
            {
                FullTime = T * 1000;
                WritePmcData(7000, 7000, 1);
                Thread.Sleep(FullTime);
                WritePmcData(7000, 7000, 1);
            }
        }
        //換算量級
        //int
    }
    public class Focas1
    {
        // Declare constants and methods from FOCAS library
        public const short EW_OK = 0;
        [DllImport("C:/Users/User/Desktop/cnc_gui_s/cnc_gui/fwlib_fanuc/Fwlib32.dll")]
        public static extern short cnc_allclibhndl3(string ip, ushort port, int timeout, out ushort libhndl);

        [DllImport("C:/Users/User/Desktop/cnc_gui_s/cnc_gui/fwlib_fanuc/Fwlib32.dll")]
        public static extern short cnc_freelibhndl(ushort libhndl);

        [DllImport("C:/Users/User/Desktop/cnc_gui_s/cnc_gui/fwlib_fanuc/Fwlib32.dll")]
        public static extern short cnc_rdspmeter(ushort libhndl, short type, ref short data_num, [MarshalAs(UnmanagedType.LPStruct), Out] Odbspload spmeter);

        [DllImport("C:/Users/User/Desktop/cnc_gui_s/cnc_gui/fwlib_fanuc/Fwlib32.dll")]
        public static extern short pmc_rdpmcrng(ushort FlibHndl, short adr_type, short data_type, ushort s_number, ushort e_number, ushort length, [MarshalAs(UnmanagedType.LPStruct), Out] Iodbpmc buf);

        [DllImport("C:/Users/User/Desktop/cnc_gui_s/cnc_gui/fwlib_fanuc/Fwlib32.dll")]
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