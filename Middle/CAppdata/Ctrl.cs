using NETSDKHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                    case "Down":
                        TranData.Driver.PTZControl.Instance.Control((int)NETDEV_PTZ_E.NETDEV_PTZ_TILTUP);
                        break;
                    case "Up":
                        TranData.Driver.PTZControl.Instance.Control((int)NETDEV_PTZ_E.NETDEV_PTZ_TILTDOWN);
                        break;
                    case "Left":
                        TranData.Driver.PTZControl.Instance.Control((int)NETDEV_PTZ_E.NETDEV_PTZ_PANRIGHT);
                        break;                
                
                    case "Right":
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

                    case "Origin":
                        TranData.Driver.PTZControl.Instance.GotoPreset();
                        break;
                    default:
                        Console.WriteLine($"msg:{data}");
     
                        if ((data.StartsWith("{") && data.EndsWith("}")) || //For object
                            (data.StartsWith("[") && data.EndsWith("]"))) //For array
                        {
                            try
                            {
                                var d = JsonConvert.DeserializeObject<Angle>(data);
                                if (d.Type == "Angle")
                                {
                                    TranData.Driver.PTZControl.Instance.Enqueue(d.X, d.Y, d.Z);
                                }
                            }
                            catch (JsonReaderException jex)
                            {
                                //Exception in parsing json
                                Console.WriteLine(jex.Message);
                            }
                            catch (Exception ex) //some other exception
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }                     
                  
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"msg ex:{ex.Message}");
                throw;
            }
          
        }
        private static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
    public class Angle
    {
        public string Type { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}
