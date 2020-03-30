using BLL.IService;
using BLL.Service;
using DAL.Model;
using FineUIMvc.EmptyProject.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FineUIMvc.EmptyProject.Controllers
{
    public class WapperController : BaseController
    {
        private readonly IWapperService _wapperService;

        public WapperController()
        {
            _wapperService = new WapperService(null);
        }
        // GET: Wapper
        public ActionResult WapperIn()
        {
            List<Wapper> wapperList = _wapperService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            ViewBag.Grid1RecordCount = wapperList.Count;

            ViewBag.Grid1DataSource = PagingHelper<Wapper>.GetPagedDataTable(0, 10, wapperList.Count, wapperList);

            return View(wapperList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult WapperIn_PageIndexChanged(JArray Grid1_fields, int Grid1_pageIndex)
        {
            List<Wapper> wapperList = _wapperService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = wapperList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<Wapper>.GetPagedDataTable(Grid1_pageIndex, 10, wapperList.Count, wapperList);
            grid1.DataSource(dataSource, Grid1_fields);

            return UIHelper.Result();
        }

        public ActionResult WapperInDetail(int? id)
        {
            if (id != null)
            {
                Wapper wapper = _wapperService.Find(p => p.ID == id);

                ViewBag.Wapper = wapper;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult WapperInDetail_Close(JArray fields)
        {
            List<Wapper> wapperList = _wapperService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = wapperList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<Wapper>.GetPagedDataTable(0, 10, wapperList.Count, wapperList);
            grid1.DataSource(dataSource, fields);

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveWapper(FormCollection values)
        {
            Wapper wapper;
            int ID = Convert.ToInt32(values["ID"]);
            if (ID == 0)
            {
                wapper = new Wapper();
            }
            else
            {
                wapper = _wapperService.Find(p => p.ID == ID);
            }

            wapper.Name = values["Name"];
            wapper.Price = Convert.ToDouble(values["Price"]);
            wapper.Picture = values["Picture"];
            wapper.Spec = values["Spec"];
            wapper.Description = values["Description"];
            wapper.CreateTime = DateTime.Now;


            if (ID == 0)
            {
                wapper = _wapperService.Add(wapper);
            }
            else
            {
                _wapperService.Update(wapper);
            }

            PageContext.RegisterStartupScript(ActiveWindow.GetHidePostBackReference());

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteWapper(int id)
        {
            try
            {


                Wapper wapper = _wapperService.Find(p => p.ID == id);

                if (wapper != null)
                {
                    _wapperService.Delete(wapper);
                }
            }
            catch (Exception e)
            {
                Alert.Show("无法删除!");
                return UIHelper.Result();
            }


            return UIHelper.Result();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult filePhoto_FileSelected(HttpPostedFileBase filePhoto, FormCollection values)
        {
            if (filePhoto != null)
            {
                string fileName = filePhoto.FileName;

                if (!ValidateFileType(fileName))
                {
                    // 清空文件上传组件
                    UIHelper.FileUpload("filePhoto").Reset();

                    ShowNotify("无效的文件类型！");
                }
                else
                {
                    fileName = fileName.Replace(":", "_").Replace(" ", "_").Replace("\\", "_").Replace("/", "_");
                    fileName = DateTime.Now.Ticks.ToString() + "_" + fileName;

                    filePhoto.SaveAs(Server.MapPath("~/Upload/" + fileName));

                    UIHelper.Image("imgPhoto").ImageUrl("~/Upload/" + fileName);

                    var textBox = UIHelper.TextBox("Picture");
                    textBox.Text("~/Upload/" + fileName);

                    // 清空文件上传组件（上传后要记着清空，否则点击提交表单时会再次上传！！）
                    UIHelper.FileUpload("filePhoto").Reset();
                }
            }

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnSubmit_Click(FormCollection values)
        {
            var filePhoto = UIHelper.FileUpload("filePhoto");

            var imgPhotoUrl = values["imgPhotoUrl"];

            if (imgPhotoUrl.EndsWith("/res/images/blank.png"))
            {
                filePhoto.MarkInvalid("请先上传商品图片！");
                ShowNotify("请先上传商品图片！");
            }
            else
            {
                //UIHelper.Label("labResult").Text("用户名：" + values["tbxUserName"] + "<br/>" +
                //        "邮箱：" + values["tbxEmail"] + "<br/>" +
                //        "<p>头像：<br /><img src=\"" + imgPhotoUrl + "\" /></p>",
                //        encodeText: false);

                // 清空表单字段
                UIHelper.Image("imgPhoto").ImageUrl(Url.Content("~/res/images/blank.png"));
                filePhoto.Reset();
                UIHelper.TextBox("tbxEmail").Reset();
                UIHelper.TextBox("tbxUserName").Reset();
            }

            return UIHelper.Result();
        }
    }
}