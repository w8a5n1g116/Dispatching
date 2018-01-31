using FineUIMvc.EmptyProject.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Xml;
using System.IO;
using log4net;

namespace FineUIMvc.EmptyProject.Tools
{
    public class WeChatCommon
    {
        private static readonly string WEIXIN_CONFIG_PATH = "/App_Data/WeiXin.xml";
        private static readonly ILog logs = LogHelper.GetInstance();

        #region 【构造】微信配置
        /// <summary>
        /// 初始化微信参数配置
        /// </summary>
        public WeChatCommon()
        {
            XmlDocument Xml = new XmlDocument();
            Xml.Load(HttpContext.Current.Server.MapPath(WEIXIN_CONFIG_PATH));
            XmlNode appid = Xml.SelectSingleNode("weixin/appid");
            XmlNode appsecret = Xml.SelectSingleNode("weixin/appsecret");
            XmlNode accesstoken = Xml.SelectSingleNode("weixin/accesstoken");
            XmlNode token = Xml.SelectSingleNode("weixin/token");
            if (appid != null)
            {
                this.appid = appid.InnerText;
            }
            if (appsecret != null)
            {
                this.secret = appsecret.InnerText;
            }
            if (accesstoken != null)
            {
                if (check_access_token())
                {
                    this.accesstoken = accesstoken.InnerText;
                }
                else
                {
                    var backdate = get_access_token();
                    if(backdate.ResponseState == false)
                        logs.Info(JsonConvert.SerializeObject(backdate));
                    //更新当前access_token
                    if (backdate.ResponseState)
                    {
                        this.accesstoken = backdate.ResponseData.access_token;
                        accesstoken.InnerText = backdate.ResponseData.access_token;
                        Xml.Save(HttpContext.Current.Server.MapPath(WEIXIN_CONFIG_PATH));
                    }
                }
            }
            if (token != null)
            {
                this.token = token.InnerText;
            }
        }
        #endregion

            #region 【配置】微信基础参数
            /// <summary>
            /// 服务器标识
            /// </summary>
        private string token = "dispatching";
        /// <summary>
        /// 开发者ID(AppID(应用ID))
        /// </summary>
        private string appid = "wx73121a739e14df05";
        /// <summary>
        /// (AppSecret(应用密钥))
        /// </summary>
        private string secret = "9bfc5d837d9814878bac667212d39eaa";
        /// <summary>
        ///接口连接凭据
        /// </summary>
        private string accesstoken = "";

        #endregion

        #region 【验证】微信签名
        /// <summary>
        /// 微信签名验证
        /// </summary>
        /// <param name="nonce">随机字符串 </param>
        /// <param name="timestamp">时间戳 </param>
        /// <param name="signature">微信加密签名</param>
        /// <returns>验签是否成功true成功|false失败</returns>
        public bool CheckSign(string nonce, string timestamp, string signature)
        {
            string[] ArrTmp = { token, timestamp, nonce };
            Array.Sort(ArrTmp);     //字典排序
            string tmpStr = string.Join("", ArrTmp);
            tmpStr = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1");
            tmpStr = tmpStr.ToLower();
            if (tmpStr == signature)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region  【验证】access_token是否失效
        /// <summary>
        /// 根据接口返回代码42001验证是否可用
        /// </summary>
        /// <returns>true可用|false失效</returns>
        public bool check_access_token()
        {
            XmlDocument Xml = new XmlDocument();
            Xml.Load(HttpContext.Current.Server.MapPath(WEIXIN_CONFIG_PATH));
            XmlNode access_token = Xml.SelectSingleNode("weixin/accesstoken");
            if (access_token != null)
            {
                this.accesstoken = access_token.InnerText;
            }
            string Url = "https://api.weixin.qq.com/cgi-bin/get_current_selfmenu_info?access_token=" + this.accesstoken;
            string GetResult = ToolKit.GetData(Url);
            if (GetResult.IndexOf("errcode") != -1)
            {
                var ErrorMessage = JsonConvert.DeserializeObject<wx_apperror>(GetResult);
                if (ErrorMessage.errcode == "42001")
                {
                    return false;
                }
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region 【通用】获取可用接口凭据
        /// <summary>
        /// 获取可用的接口凭据
        /// </summary>
        public string Accesstoken()
        {
            if (!check_access_token())
            {
                XmlDocument Xml = new XmlDocument();
                Xml.Load(HttpContext.Current.Server.MapPath(WEIXIN_CONFIG_PATH));
                XmlNode access_token = Xml.SelectSingleNode("weixin/accesstoken");
                if (access_token != null)
                {
                    var backdate = get_access_token();
                    //更新当前access_token
                    if (backdate.ResponseState)
                    {
                        this.accesstoken = backdate.ResponseData.access_token;
                        access_token.InnerText = backdate.ResponseData.access_token;
                        Xml.Save(HttpContext.Current.Server.MapPath(WEIXIN_CONFIG_PATH));
                    }
                }
            }
            return this.accesstoken;
        }
        #endregion

        #region【通用】获取JSON指定类型返回值
        /// <summary>
        /// 【通用】获取JSON指定类型返回值
        /// </summary>
        /// <typeparam name="T">指定非错误的返回类型</typeparam>
        /// <param name="JsonData">序列化前JSON字符</param>
        /// <returns>JSON序列化后数据</returns>
        public wx_backdata<T> GetJson<T>(string JsonData)
        {
            var result = new wx_backdata<T>();
            if (JsonData.IndexOf("errcode") != -1)
            {
                result.ResponseState = false;
                result.ErrorData = JsonConvert.DeserializeObject<wx_apperror>(JsonData);
            }
            else
            {
                result.ResponseState = true;
                result.ResponseData = JsonConvert.DeserializeObject<T>(JsonData);
            }
            return result;
        }
        #endregion

        #region 【access_token】获取唯一接口调用凭据
        /// <summary>
        /// 获取唯一接口调用凭据access_token
        /// </summary>
        /// <returns>access_token获取结果对象</returns>
        public wx_backdata<wx_access_token> get_access_token()
        {
            string Url = "https://api.weixin.qq.com/cgi-bin/token?";
            string grant_type = "client_credential";
            string GetResult = ToolKit.GetData(Url + "grant_type=" + grant_type + "&appid=" + appid + "&secret=" + secret);
            return GetJson<wx_access_token>(GetResult);
        }
        #endregion

        #region【OpenId】 获取用户列表
        /// <summary>
        /// 获取粉丝微信OpenId
        /// </summary>
        /// <returns>接口返回值</returns>
        public wx_backdata<wx_openidlist> GetOpenIdList()
        {
            string Url = "https://api.weixin.qq.com/cgi-bin/user/get?";
            string GetResult = ToolKit.GetData(Url + "access_token=" + this.Accesstoken() + "&next_openid=");
            return GetJson<wx_openidlist>(GetResult);
        }
        #endregion

        #region 【用户信息】获取指定用户信息
        /// <summary>
        /// 获取指定用户信息
        /// </summary>
        /// <param name="openid">用户标识</param>
        /// <returns>接口返回值</returns>
        public wx_backdata<wx_user_info> GetUserInfo(string openid)
        {
            string Url = "https://api.weixin.qq.com/cgi-bin/user/info?";
            string GetResult = ToolKit.GetData(Url + "access_token=" + this.Accesstoken() + "&openid=" + openid + "&lang=zh_CN");
            return GetJson<wx_user_info>(GetResult);
        }
        #endregion

        #region 【账号管理】获取二维码Ticket
        /// <summary>
        /// 获取二维码Ticket
        /// </summary>
        /// <param name="scene_id">场景标识</param>
        /// <returns>接口返回值</returns>
        public wx_backdata<wx_ticket> GetQRcodeTicket(int scene_id)
        {
            string jsonString = "{\"action_name\": \"QR_LIMIT_SCENE\", \"action_info\": {\"scene\": {\"scene_id\": "+ scene_id + "}}}";

            HttpContent content = new StringContent(jsonString);

            string Url = "https://api.weixin.qq.com/cgi-bin/qrcode/create?";
            string GetResult = ToolKit.PostData(Url + "access_token=" + this.Accesstoken(), content);
            return GetJson<wx_ticket>(GetResult);
        }
        #endregion

        #region 【账号管理】根据Ticket获取二维码
        /// <summary>
        /// 获取二维码Ticket
        /// </summary>
        /// <param name="scene_id">场景标识</param>
        /// <returns>接口返回值</returns>
        public HttpResponseMessage GetQRcode(string ticket)
        {
            string Url = "https://mp.weixin.qq.com/cgi-bin/showqrcode?";
            HttpResponseMessage GetResult = ToolKit.GetHttpResponseMessage(Url + "ticket=" + ticket);
            return GetResult;
        }
        #endregion

        #region 【用户管理】根据网页授权的access_token
        /// <summary>
        /// 获取二维码Ticket
        /// </summary>
        /// <param name="scene_id">场景标识</param>
        /// <returns>接口返回值</returns>
        public wx_backdata<wx_oauth2token> GetOauth2AccessToken(string code)
        {
            string Url = "https://api.weixin.qq.com/sns/oauth2/access_token?";
            string GetResult = ToolKit.GetData(Url + "appid="+appid+"&secret="+secret+"&code="+code+ "&grant_type=authorization_code");
            return GetJson<wx_oauth2token>(GetResult);
        }
        #endregion

        #region 【菜单管理】创建自定义菜单
        /// <summary>
        /// 创建自定义菜单
        /// </summary>
        /// <param name="body">创建内容</param>
        /// <returns>接口返回值</returns>
        public void CreateCustomerMenu(string body)
        {
            HttpContent content = new StringContent(body);

            string Url = "https://api.weixin.qq.com/cgi-bin/menu/create?";
            string GetResult = ToolKit.PostData(Url + "access_token=" + this.Accesstoken(), content);
        }
        #endregion


    }

    public static class ToolKit
    {
        public static string GetData(string url)
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage ret = new HttpResponseMessage();

            ret = client.GetAsync(url).Result;

            return ret.Content.ReadAsStringAsync().Result;
        }

        public static HttpResponseMessage GetHttpResponseMessage(string url)
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage ret = new HttpResponseMessage();

            ret = client.GetAsync(url).Result;

            return ret;
        }

        public static string PostData(string url,HttpContent content)
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage ret = new HttpResponseMessage();

            ret = client.PostAsync(url, content).Result;

            return ret.Content.ReadAsStringAsync().Result;
        }
    }
}