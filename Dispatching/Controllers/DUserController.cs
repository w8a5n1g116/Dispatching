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
    public class DUserController : BaseController
    {
        private readonly IUserService _dUserService;

        public DUserController()
        {
            _dUserService = new UserService(null);
        }
        // GET: DUser
        public ActionResult DUserIn()
        {
            List<DUser> dUserList = _dUserService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            ViewBag.Grid1RecordCount = dUserList.Count;

            ViewBag.Grid1DataSource = PagingHelper<DUser>.GetPagedDataTable(0, 10, dUserList.Count, dUserList);

            return View(dUserList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DUserIn_PageIndexChanged(JArray Grid1_fields, int Grid1_pageIndex)
        {
            List<DUser> dUserList = _dUserService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = dUserList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<DUser>.GetPagedDataTable(Grid1_pageIndex, 10, dUserList.Count, dUserList);
            grid1.DataSource(dataSource, Grid1_fields);

            return UIHelper.Result();
        }

        public ActionResult DUserInDetail(int? id)
        {
            if (id != null)
            {
                DUser dUser = _dUserService.Find(p => p.ID == id);

                ViewBag.DUser = dUser;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DUserInDetail_Close(JArray fields)
        {
            List<DUser> dUserList = _dUserService.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = dUserList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<DUser>.GetPagedDataTable(0, 10, dUserList.Count, dUserList);
            grid1.DataSource(dataSource, fields);

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveDUser(FormCollection values)
        {
            DUser dUser;
            int ID = Convert.ToInt32(values["ID"]);
            if (ID == 0)
            {
                dUser = new DUser();
            }
            else
            {
                dUser = _dUserService.Find(p => p.ID == ID);
            }

            dUser.Name = values["Name"];
            dUser.Password = values["Password"];
            dUser.Phone = values["Phone"];
            dUser.CreateTime = DateTime.Now;
            dUser.Permission = Convert.ToInt32(values["Permission"]);
            dUser.Role = Convert.ToInt32(values["Role"]);

            if (ID == 0)
            {
                dUser = _dUserService.Add(dUser);
            }
            else
            {
                _dUserService.Update(dUser);
            }

            PageContext.RegisterStartupScript(ActiveWindow.GetHidePostBackReference());

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDUser(int id)
        {
            try
            {


                DUser dUser = _dUserService.Find(p => p.ID == id);

                if (dUser != null)
                {
                    _dUserService.Delete(dUser);
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
        public ActionResult Permission_SelectedIndexChanged(string permission, string permission_text)
        {
            ListItemCollection RoleItems = new ListItemCollection(new DropDownList());
            if(permission_text == "酒品配送")
            {
                RoleItems.Add("送货员", "11");
            }
            else if (permission_text == "商品配送")
            {
                RoleItems.Add("配送员", "3");
                RoleItems.Add("业务员", "1");
                RoleItems.Add("结算员", "4");
                RoleItems.Add("经理", "2");
            }
                

            var Role = UIHelper.DropDownList("Role");
            Role.LoadData(RoleItems.ToArray());

            return UIHelper.Result();
        }
    }
}