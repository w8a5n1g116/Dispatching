using BLL.IService;
using BLL.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using FineUIMvc.EmptyProject.Tools;
using DAL.Model;
using log4net;

namespace FineUIMvc.EmptyProject.Controllers
{
    public class WeChatController : Controller
    {
        private readonly ITerminalWXUserService _terminalWXUserService;
        private readonly ITerminalService _terminalService;
        private static readonly ILog logs = LogHelper.GetInstance();

        public WeChatController()
        {
            _terminalWXUserService = new TerminalWXUserService(null);
            _terminalService = new TerminalService(null);
        }
        // GET: WeChat
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Webcatch()
        {
            string token = "dispatching";
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            string echoStr = Request.QueryString["echoStr"];//随机字符串 
            string signature = Request.QueryString["signature"];//微信加密签名
            string timestamp = Request.QueryString["timestamp"];//时间戳 
            string nonce = Request.QueryString["nonce"];//随机数 
            string[] ArrTmp = { token, timestamp, nonce };
            Array.Sort(ArrTmp);     //字典排序
            string tmpStr = string.Join("", ArrTmp);
            tmpStr = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1");
            tmpStr = tmpStr.ToLower();
            if (tmpStr == signature)
            {
                return Content(echoStr);
            }
            else
            {
                return Content("false");
            }
        }

        [HttpPost]
        public ActionResult Webcatch(string a)
        {
            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            XmlDocument Xml = new XmlDocument();
            Xml.LoadXml(stream);
            XmlNode MainXml = Xml.SelectSingleNode("xml");
            XmlNode ToUserName_XML = MainXml.SelectSingleNode("ToUserName");
            XmlNode FromUserName_XML = MainXml.SelectSingleNode("FromUserName");
            XmlNode CreateTime_XML = MainXml.SelectSingleNode("CreateTime");
            XmlNode MsgType_XML = MainXml.SelectSingleNode("MsgType");
            //XmlNode _Content = MainXml.SelectSingleNode("Content");
            //XmlNode MsgId = MainXml.SelectSingleNode("MsgId");
            XmlNode Event_XML = MainXml.SelectSingleNode("Event");
            XmlNode EventKey_XML = MainXml.SelectSingleNode("EventKey");
            XmlNode Ticket_XML = MainXml.SelectSingleNode("Ticket");

            string ToUserName = "", FromUserName = "", CreateTime = "", MsgType = "", Event = "", EventKey = "", Ticket = "";

            if(ToUserName_XML != null)
            {
                ToUserName = ToUserName_XML.InnerText;
            }
            if(FromUserName_XML != null)
            {
                FromUserName = FromUserName_XML.InnerText;
            }
            if (CreateTime_XML != null)
            {
                CreateTime = CreateTime_XML.InnerText;
            }
            if (MsgType_XML != null)
            {
                MsgType = MsgType_XML.InnerText;
            }
            if (Event_XML != null)
            {
                Event = Event_XML.InnerText;
            }
            if (EventKey_XML != null)
            {
                EventKey = EventKey_XML.InnerText;
            }
            if (Ticket_XML != null)
            {
                Ticket = Ticket_XML.InnerText;
            }

            //关注后，用户终端绑定
            if (!string.IsNullOrEmpty(Event) && Event == "subscribe")
            {
                if (!string.IsNullOrEmpty(EventKey) && EventKey.Contains("qrscene"))
                {
                    //去掉外侧包围，取出id
                    string terminalIdString = EventKey;//ToolKit.TakeOffOutside(EventKey);

                    terminalIdString = terminalIdString.Substring(8);

                    int terminalID = Convert.ToInt32(terminalIdString);

                    TerminalWXUser wxUser = _terminalWXUserService.Find(p => p.OpenID == FromUserName);

                    if (wxUser == null)
                    {
                        TerminalWXUser newWxUser = new TerminalWXUser();

                        newWxUser.OpenID = FromUserName;

                        Terminal terminal = _terminalService.Find(p => p.ID == terminalID);

                        newWxUser.TerminalID = terminal.ID;
                        newWxUser.Terminal = terminal;

                        _terminalWXUserService.Add(newWxUser);
                    }
                }
            }

            //回复电话号码
            if (!string.IsNullOrEmpty(Event) && Event == "CLICK")
            {
                if (!string.IsNullOrEmpty(EventKey) && EventKey.Contains("phoneNumber"))
                {
                    string stringxml = "<xml>"
                        + "<ToUserName><![CDATA["+ FromUserName +"]]></ToUserName>"
                        + "<FromUserName><![CDATA[" + ToUserName + "]]></FromUserName>"
                        + "<CreateTime>" + CreateTime +"</CreateTime>"
                        + "<MsgType><![CDATA[text]]></MsgType>"
                        + "<Content><![CDATA[联系电话:13591645115]]></Content>"
                        + "</xml>";

                    return Content(stringxml);
                }
            }

            //取消关注，删除用户终端绑定
            if (!string.IsNullOrEmpty(Event) && Event == "unsubscribe")
            {
                List<TerminalWXUser> wxUserList = _terminalWXUserService.FindList(p => p.OpenID == FromUserName,"",true).ToList();

                for(int i =0;i<wxUserList.Count;i++)
                {
                    _terminalWXUserService.Delete(wxUserList[i]);
                }
            }

            return Content("");
        }
    }
}