using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using System.IO;
using Owin;
using System.Web.Http;

using System.Collections.Generic;
using System.Web.Http.Cors;

using System.Linq;
using System.Text;

[assembly: OwinStartup(typeof(TranData.Startup))]

namespace TranData
{

    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888
            HttpConfiguration config = new HttpConfiguration();
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{action}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
            //将配置注入OWIN管道中
            app.UseWebApi(config);
            
        }

    }
}
