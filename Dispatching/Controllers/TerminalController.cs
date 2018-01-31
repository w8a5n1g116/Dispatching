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
    public class TerminalController : BaseController
    {
        private readonly ITerminalService _terminalService;
        private static readonly ILog logs = LogHelper.GetInstance();

        public TerminalController()
        {
            _terminalService = new TerminalService(null);
        }
        // GET: Terminal
        public ActionResult TerminalIn()
        {
            List<Terminal> terminalList = _terminalService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            ViewBag.Grid1RecordCount = terminalList.Count;

            ViewBag.Grid1DataSource = PagingHelper<Terminal>.GetPagedDataTable(0, 10, terminalList.Count, terminalList);

            return View(terminalList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TerminalIn_PageIndexChanged(JArray Grid1_fields, int Grid1_pageIndex, string terminalName)
        {
            List<Terminal> terminalList = _terminalService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            if (!string.IsNullOrEmpty(terminalName))
            {
                terminalList = terminalList.Where(p => p.Name.Contains(terminalName)).ToList();
            }

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = terminalList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<Terminal>.GetPagedDataTable(Grid1_pageIndex, 10, terminalList.Count, terminalList);
            grid1.DataSource(dataSource, Grid1_fields);

            return UIHelper.Result();
        }

        [HttpPost]
        public ActionResult TerminalIn(string terminalName, JArray fields)
        {
            List<Terminal> terminalList = _terminalService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            if(!string.IsNullOrEmpty(terminalName))
            {
                terminalList = terminalList.Where(p => p.Name.Contains(terminalName)).ToList();
            }

            var grid1 = UIHelper.Grid("Grid1");

            grid1.RecordCount(terminalList.Count);

            grid1.DataSource(PagingHelper<Terminal>.GetPagedDataTable(0, 10, terminalList.Count, terminalList), fields);

            return UIHelper.Result();
        }

        public ActionResult TerminalInDetail(int? id)
        {
            if (id != null)
            {
                Terminal terminal = _terminalService.Find(p => p.ID == id);

                ViewBag.Terminal = terminal;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TerminalInDetail_Close(JArray fields)
        {
            List<Terminal> terminalList = _terminalService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = terminalList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<Terminal>.GetPagedDataTable(0, 10, terminalList.Count, terminalList);
            grid1.DataSource(dataSource, fields);

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveTerminal(FormCollection values)
        {
            Terminal terminal;
            int ID = Convert.ToInt32(values["ID"]);
            if (ID == 0)
            {
                terminal = new Terminal();
            }
            else
            {
                terminal = _terminalService.Find(p => p.ID == ID);
            }

            terminal.Name = values["Name"];
            terminal.Address = values["Address"];
            terminal.Phone = values["Phone"];
            terminal.CreateTime = DateTime.Now;


            if (ID == 0)
            {
                terminal = _terminalService.Add(terminal);
            }
            else
            {
                _terminalService.Update(terminal);
            }

            PageContext.RegisterStartupScript(ActiveWindow.GetHidePostBackReference());

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTerminal(int id)
        {
            try
            {


                Terminal terminal = _terminalService.Find(p => p.ID == id);

                if (terminal != null)
                {
                    _terminalService.Delete(terminal);
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
            Terminal terminal = _terminalService.Find(p => p.ID == id);

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

                _terminalService.Update(terminal);
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