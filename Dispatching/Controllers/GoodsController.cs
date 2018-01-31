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
    public class GoodsController : BaseController
    {
        private readonly IGoodsService _goodsService;

        public GoodsController()
        {
            _goodsService = new GoodsService(null);
        }
        // GET: Goods
        public ActionResult GoodsIn()
        {
            List<Goods> goodsList = _goodsService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            ViewBag.Grid1RecordCount = goodsList.Count;

            ViewBag.Grid1DataSource = PagingHelper<Goods>.GetPagedDataTable(0, 10, goodsList.Count, goodsList);

            return View(goodsList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GoodsIn_PageIndexChanged(JArray Grid1_fields, int Grid1_pageIndex)
        {
            List<Goods> goodsList = _goodsService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = goodsList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<Goods>.GetPagedDataTable(Grid1_pageIndex, 10, goodsList.Count, goodsList);
            grid1.DataSource(dataSource, Grid1_fields);

            return UIHelper.Result();
        }

        public ActionResult GoodsInDetail(int? id)
        {
            if (id != null)
            {
                Goods goods = _goodsService.Find(p => p.ID == id);

                ViewBag.Goods = goods;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GoodsInDetail_Close(JArray fields)
        {
            List<Goods> goodsList = _goodsService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = goodsList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<Goods>.GetPagedDataTable(0, 10, goodsList.Count, goodsList);
            grid1.DataSource(dataSource, fields);

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveGoods(FormCollection values)
        {
            Goods goods;
            int ID = Convert.ToInt32(values["ID"]);
            if (ID == 0)
            {
                goods = new Goods();
            }
            else
            {
                goods = _goodsService.Find(p => p.ID == ID);
            }

            goods.Name = values["Name"];
            goods.Price = Convert.ToDouble(values["Price"]);
            goods.Picture = values["Picture"];
            goods.Spec = values["Spec"];
            goods.Description = values["Description"];
            goods.CreateTime = DateTime.Now;


            if (ID == 0)
            {
                goods = _goodsService.Add(goods);
            }
            else
            {
                _goodsService.Update(goods);
            }

            PageContext.RegisterStartupScript(ActiveWindow.GetHidePostBackReference());

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteGoods(int id)
        {
            try
            {


                Goods goods = _goodsService.Find(p => p.ID == id);

                if (goods != null)
                {
                    _goodsService.Delete(goods);
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