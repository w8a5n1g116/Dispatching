using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Http;
using FineUIMvc.EmptyProject.Tools;

namespace FineUIMvc.EmptyProject
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            ModelBinders.Binders.Add(typeof(JArray), new JArrayModelBinder());
            ModelBinders.Binders.Add(typeof(JObject), new JObjectModelBinder());
            GlobalConfiguration.Configure(WebApiConfig.Register);

            //启动程序时创建公众号自定义菜单
            string body = "{\"button\":[{\"type\": \"view\",\"name\": \"查询订单\",\"url\": \"https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx73121a739e14df05&redirect_uri=http://stopno.net/Mobile/TerminalAnalysis&response_type=code&scope=snsapi_base&state=0#wechat_redirect\"}]}";

            WeChatCommon wcc = new WeChatCommon();
            wcc.CreateCustomerMenu(body);

        }
    }
}
