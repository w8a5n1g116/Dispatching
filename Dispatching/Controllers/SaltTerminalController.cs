using BLL.IService;
using BLL.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL.Model;
using FineUIMvc.EmptyProject.Tools;
using Newtonsoft.Json.Linq;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using FineUIMvc.EmptyProject.Models;
using log4net;

namespace FineUIMvc.EmptyProject.Controllers
{
    public class SaltTerminalController : BaseController
    {
        private readonly ISaltTerminalService _saltTerminalService;
        private static readonly ILog logs = LogHelper.GetInstance();

        public SaltTerminalController()
        {
            _saltTerminalService = new SaltTerminalService(null);
        }
        // GET: Terminal
        public ActionResult SaltTerminalIn()
        {
            List<SaltTerminal> terminalList = _saltTerminalService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            ViewBag.Grid1RecordCount = terminalList.Count;

            ViewBag.Grid1DataSource = PagingHelper<SaltTerminal>.GetPagedDataTable(0, 10, terminalList.Count, terminalList);

            return View(terminalList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaltTerminalIn_PageIndexChanged(JArray Grid1_fields, int Grid1_pageIndex, string terminalName)
        {
            List<SaltTerminal> terminalList = _saltTerminalService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            if (!string.IsNullOrEmpty(terminalName))
            {
                terminalList = terminalList.Where(p => p.Name.Contains(terminalName)).ToList();
            }

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = terminalList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<SaltTerminal>.GetPagedDataTable(Grid1_pageIndex, 10, terminalList.Count, terminalList);
            grid1.DataSource(dataSource, Grid1_fields);

            return UIHelper.Result();
        }

        [HttpPost]
        public ActionResult SaltTerminalIn(string terminalName, JArray fields)
        {
            List<SaltTerminal> terminalList = _saltTerminalService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            if(!string.IsNullOrEmpty(terminalName))
            {
                terminalList = terminalList.Where(p => p.Name.Contains(terminalName)).ToList();
            }

            var grid1 = UIHelper.Grid("Grid1");

            grid1.RecordCount(terminalList.Count);

            grid1.DataSource(PagingHelper<SaltTerminal>.GetPagedDataTable(0, 10, terminalList.Count, terminalList), fields);

            return UIHelper.Result();
        }

        public ActionResult SaltTerminalInDetail(int? id)
        {
            if (id != null)
            {
                SaltTerminal terminal = _saltTerminalService.Find(p => p.ID == id);

                ViewBag.SaltTerminal = terminal;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaltTerminalInDetail_Close(JArray fields)
        {
            List<SaltTerminal> terminalList = _saltTerminalService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = terminalList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<SaltTerminal>.GetPagedDataTable(0, 10, terminalList.Count, terminalList);
            grid1.DataSource(dataSource, fields);

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSaltTerminal(FormCollection values)
        {
            SaltTerminal terminal;
            int ID = Convert.ToInt32(values["ID"]);
            if (ID == 0)
            {
                terminal = new SaltTerminal();
            }
            else
            {
                terminal = _saltTerminalService.Find(p => p.ID == ID);
            }

            terminal.Name = values["Name"];
            terminal.Contact = values["Contact"];
            terminal.Address = values["Address"];
            terminal.Phone = values["Phone"];
            terminal.CreateTime = DateTime.Now;


            if (ID == 0)
            {
                terminal = _saltTerminalService.Add(terminal);
            }
            else
            {
                _saltTerminalService.Update(terminal);
            }

            PageContext.RegisterStartupScript(ActiveWindow.GetHidePostBackReference());

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSaltTerminal(int id)
        {
            try
            {


                SaltTerminal terminal = _saltTerminalService.Find(p => p.ID == id);

                if (terminal != null)
                {
                    _saltTerminalService.Delete(terminal);
                }
            }
            catch (Exception e)
            {
                Alert.Show("无法删除!");
                return UIHelper.Result();
            }


            return UIHelper.Result();
        }

        public ActionResult ShowQRCode(int id)
        {
            SaltTerminal terminal = _saltTerminalService.Find(p => p.ID == id);

            if(string.IsNullOrEmpty(terminal.WXQCCodeAddress))
            {
                WeChatCommon wcc = new WeChatCommon();

                wx_backdata<wx_ticket> ticketdata = wcc.GetQRcodeTicket(id);

                if(ticketdata.ResponseState == false)
                {
                    logs.Error(ticketdata.ErrorData.errcode + ":" + ticketdata.ErrorData.errmsg);

                    Alert.Show("系统出现问题！");
                    return UIHelper.Result();
                }

                terminal.WXQCCodeAddress = ticketdata.ResponseData.url;

                _saltTerminalService.Update(terminal);
            }
            

            //string url = "http://"+HttpContext.Request.Url.Host + "/Mobile/TerminalAnalysis/"+id;
            string url = terminal.WXQCCodeAddress;

            QRCodeGenerator qrcg = new QRCodeGenerator();
            QRCodeData qrcd = qrcg.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrc = new QRCode(qrcd);

            Bitmap bit = qrc.GetGraphic(10, Color.Black, Color.White,true);

            //Graphics g = Graphics.FromImage(bit);

            MemoryStream ms = new MemoryStream();
            bit.Save(ms, ImageFormat.Png);

            Response.ClearContent();
            Response.ContentType = "image/png";
            Response.BinaryWrite(ms.ToArray());

            //g.Dispose();

            return View();
        }
    } 
}