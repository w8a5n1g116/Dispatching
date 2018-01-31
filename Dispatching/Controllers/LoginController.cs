using BLL.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL.Model;
using System.Configuration;
using BLL.Service;
using log4net;
using FineUIMvc.EmptyProject.Tools;

namespace FineUIMvc.EmptyProject.Controllers
{
    public class LoginController : BaseController
    {
        private readonly IUserService _userService;
        private static readonly ILog logs = LogHelper.GetInstance();

        public LoginController()
        {
            //Model context = new Model(ConfigurationManager.ConnectionStrings["QualityManage"].ConnectionString);
            _userService = new UserService(null);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string UserName, string Password)
        {
            DUser user = _userService.Find(UserName);

            if (user != null && user.Password == Password)
            {
                //SessionExtension.Set<LocalUser>(HttpContext.Session, "User", user);
                Session["User"] = user;

                //logs.Info(user.Name + "登录成功");
                return Redirect("/Home/Index");
            }
            else
            {
                ShowNotify("用户名或密码错误！", MessageBoxIcon.Error);
            }

            return UIHelper.Result();
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Login");
        }
    }
}