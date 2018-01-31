using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL.Model;

namespace FineUIMvc.EmptyProject.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.NodeList = TreeNodeListByRole();

            return View();
        }

        public ActionResult Hello()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnHello_Click()
        {
            Alert.Show("你好 FineUI！", MessageBoxIcon.Warning);

            return UIHelper.Result();
        }



        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnLogin_Click(string tbxUserName, string tbxPassword)
        {
            if (tbxUserName == "admin" && tbxPassword == "admin")
            {
                ShowNotify("成功登录！", MessageBoxIcon.Success);
            }
            else
            {
                ShowNotify("用户名或密码错误！", MessageBoxIcon.Error);
            }

            return UIHelper.Result();
        }


        // GET: Themes
        public ActionResult Themes()
        {
            return View();
        }


        private List<TreeNode> TreeNodeListByRole()
        {
            DUser user = (DUser)Session["User"];

            

            ///////////////////////////
            TreeNode TerminalManage = new TreeNode();
            TerminalManage.Text = "终端管理";
            TerminalManage.NavigateUrl = "~/Terminal/TerminalIn";

            TreeNode UserManage = new TreeNode();
            UserManage.Text = "用户管理";
            UserManage.NavigateUrl = "~/DUser/DUserIn";

            TreeNode GoodsManage = new TreeNode();
            GoodsManage.Text = "商品管理";
            GoodsManage.NavigateUrl = "~/Goods/GoodsIn";

            TreeNode OrderManage = new TreeNode();
            OrderManage.Text = "订单管理";
            OrderManage.NavigateUrl = "~/Order/OrderList";

            TreeNode UserStatisticManage = new TreeNode();
            UserStatisticManage.Text = "用户订单统计";
            UserStatisticManage.NavigateUrl = "~/Statistic/UserAnalysis";

            TreeNode TerminalStatisticManage = new TreeNode();
            TerminalStatisticManage.Text = "终端订单统计";
            TerminalStatisticManage.NavigateUrl = "~/Statistic/TerminalAnalysis";


            List<TreeNode> nodeList = new List<TreeNode>();


            if (user.Permission == 1)
            {
                nodeList.Add(OrderManage);
                nodeList.Add(TerminalManage);
                nodeList.Add(GoodsManage);
                nodeList.Add(UserManage);
                nodeList.Add(UserStatisticManage);
                nodeList.Add(TerminalStatisticManage);
            }
            else
            {
                nodeList.Add(OrderManage);
                nodeList.Add(UserStatisticManage);
                nodeList.Add(TerminalStatisticManage);
            }

            

            return nodeList;
        }
    }
}