using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranData.Api
{
    class Model
    {
        public class ApiResult
        {
            /// <summary>
            /// 接受状态
            /// </summary>
            public string Status { get; set; }
            /// <summary>
            /// 错误消息。
            /// </summary>
            public string Msg { get; set; }

            //数据
            public string Retdata { get; set; }
        }
        public class actcmd
        {
            public int code { get; set; }
            public DateTime actiontime { get; set; }
        }
    }


}
