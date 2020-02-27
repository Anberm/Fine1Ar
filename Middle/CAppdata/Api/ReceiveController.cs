using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Drawing;
using static TranData.Api.Model;
using TranData.Driver;
using NETSDKHelper;
using System.Diagnostics;

namespace TranData.Api
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ReceiveController : ApiController
    {
        private static readonly ILog logC = LogManager.GetLogger(typeof(Program));
        private static string barcode_ = "";

        public ReceiveController()
        {
        }

        [HttpGet]
        public string Test(string barcode)
        {
            Console.WriteLine("this is: " + barcode);
            barcode_ = barcode;
            return barcode;
        }

        [HttpPost]
        public HttpResponseMessage Action(string values, string cmd)
        {
            ApiResult res = new ApiResult();
            res.Msg = "成功";
            res.Status = "1";
            actcmd _cmd = new actcmd();
            res = PerAction.Act(_cmd);
            return Request.CreateResponse(res);
        }

        [HttpGet]
        public string GetVideoUrl()
        {
            return TranData.Driver.PTZControl.Instance.VideoUrl;
        }

        [HttpGet]
        public IHttpActionResult PTZControl(int cmd)
        {
            // up 1, right 2 down 3 left 4
            switch (cmd)
            {
                case 1:
                    TranData.Driver.PTZControl.Instance.Control((int)NETDEV_PTZ_E.NETDEV_PTZ_TILTUP);
                    break;
                case 2:
                    TranData.Driver.PTZControl.Instance.Control((int)NETDEV_PTZ_E.NETDEV_PTZ_PANRIGHT);
                    break;
                case 3:
                    TranData.Driver.PTZControl.Instance.Control((int)NETDEV_PTZ_E.NETDEV_PTZ_TILTDOWN);
                    break;
                case 4:
                    TranData.Driver.PTZControl.Instance.Control((int)NETDEV_PTZ_E.NETDEV_PTZ_PANLEFT);
                    break;
                default:
                    TranData.Driver.PTZControl.Instance.Control((int)NETDEV_PTZ_E.NETDEV_PTZ_ALLSTOP);
                    break;
            }
            return Ok();
        }

        [HttpGet]
        public IHttpActionResult PTZZoomControl(int cmd)
        {
            // 1 wide 2 tele
            switch (cmd)
            {
                case 1:
                    TranData.Driver.PTZControl.Instance.ControlZoomWide();
                    break;
                case 2:
                    TranData.Driver.PTZControl.Instance.ControlZoomTele();
                    break;
                default:
                    break;
            }

            return Ok();
        }
    }
}
