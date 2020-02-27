
using Microsoft.Owin.Hosting;

using System;
using System.Text;
using System.Configuration;
using System.Timers;
using Microsoft.Win32;
using System.Security.Cryptography;

using System.Data;
using log4net;

using TranData;
using System.Collections;
using GeneralDef;
using NETSDKHelper;
using System.Diagnostics;
using TranData.Driver;
using System.Net;
using WebSocketSharp.Server;
using System.Threading.Tasks;

namespace TranData
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            #region Timer
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "进入系统");
            #endregion
            foreach (System.Diagnostics.Process pro in System.Diagnostics.Process.GetProcessesByName("webrtc"))
            {
                pro.Kill();
            }
            InitUniSdk();
            string baseAddress = "http://+:16080/"; //绑定所有地址，外网可以用ip访问 需管理员权限
            //string baseAddress = $"http://{GetAddressIP()}:16080/"; //绑定所有地址，外网可以用ip访问 需管理员权限
            // 启动 OWIN host 
            WebApp.Start<Startup>(url: baseAddress);  // 这个是OK的
            InitRealTime();
            //StartRtc();
            Console.WriteLine(baseAddress);
            Console.WriteLine($"访问地址：{GetAddressIP()}");
            Console.ReadLine();

        }
        private static string GetAddressIP()
        {
            ///获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }

        private static void InitUniSdk()
        {
            int iRet = NETDEVSDK.NETDEV_Init();
            if (NETDEVSDK.TRUE != iRet)
            {
                Debug.WriteLine("it is not a admin oper");
            }
            var ptz = PTZControl.Instance;
        }

        private static void StartRtc()
        {
            try
            {
                Process proc = new Process();
                var path = AppDomain.CurrentDomain.BaseDirectory;
                var flieName = path + @"webrtc\webrtc.exe";
                var root = path + @"webrtc\html";

                proc.StartInfo.CreateNoWindow = false;
                proc.StartInfo.UseShellExecute = false;

                proc.StartInfo.FileName = flieName;
                proc.StartInfo.Arguments = $" -w {root} rtsp://weathercam.gsis.edu.hk/axis-media/media.amp";
                proc.Start();

                //Thread.Sleep(5000);//暂停3秒

                //foreach (System.Diagnostics.Process pro in System.Diagnostics.Process.GetProcessesByName("webrtc"))
                //{
                //    pro.Kill();
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void InitRealTime()
        {
            try
            {
                var wssv = new WebSocketServer($"ws://{GetAddressIP()}:16081");
                wssv.AddWebSocketService<Ctrl>("/Ctrl");
                wssv.Start();
                Console.WriteLine($"ws://{wssv.Address.ToString()}:{wssv.Port}");
                //Console.ReadKey(true);
                //wssv.Stop();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

    }
}
