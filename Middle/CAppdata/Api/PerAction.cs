using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TranData.Api.Model;

namespace TranData.Api
{
    class PerAction
    {
        private static readonly ILog logC = LogManager.GetLogger(typeof(Program));
        static string IPC_NAME { get { return ConfigurationManager.AppSettings["IPCName"]; } }
        static string IPC_TITLE { get { return ConfigurationManager.AppSettings["IPCTitle"]; } }

        public static ApiResult Act(actcmd values)
        {
            ApiResult res = new ApiResult();
            res.Status = "-3";
            res.Msg = "意外失败";
           // prt_person = values;
          //  res = Print_function();
            return res;
        }

        
    }


}
