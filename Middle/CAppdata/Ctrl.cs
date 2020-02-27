using NETSDKHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TranData
{
    public class Ctrl : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            var data = e.Data;
            Debug.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + data);
            //return;
            try
            {
                switch (data)
                {
                    case "Up":
                        TranData.Driver.PTZControl.Instance.Control((int)NETDEV_PTZ_E.NETDEV_PTZ_TILTUP);
                        break;
                    case "Right":
                        TranData.Driver.PTZControl.Instance.Control((int)NETDEV_PTZ_E.NETDEV_PTZ_PANRIGHT);
                        break;
                    case "Down":
                        TranData.Driver.PTZControl.Instance.Control((int)NETDEV_PTZ_E.NETDEV_PTZ_TILTDOWN);
                        break;
                    case "Left":
                        TranData.Driver.PTZControl.Instance.Control((int)NETDEV_PTZ_E.NETDEV_PTZ_PANLEFT);
                        break;
                    case "ZoomIn":
                        TranData.Driver.PTZControl.Instance.ControlZoomTele();
                        break;
                    case "ZoomOut":
                        TranData.Driver.PTZControl.Instance.ControlZoomWide();
                        break;
                    case "VideoUrl":
                        Send(TranData.Driver.PTZControl.Instance.VideoUrl);
                        break;
                    case "Stop":
                        TranData.Driver.PTZControl.Instance.Control((int)NETDEV_PTZ_E.NETDEV_PTZ_ALLSTOP);
                        break;
                    default:
                        Console.WriteLine($"msg:{data}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"msg ex:{ex.Message}");
                throw;
            }
          
        }
    }
}
