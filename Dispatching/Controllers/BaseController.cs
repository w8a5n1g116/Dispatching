using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL.Model;

namespace FineUIMvc.EmptyProject.Controllers
{
    public class BaseController : Controller
    {
        protected readonly static List<string> VALID_FILE_TYPES = new List<string> { "jpg", "bmp", "gif", "jpeg", "png" };

        protected static bool ValidateFileType(string fileName)
        {
            string fileType = String.Empty;
            int lastDotIndex = fileName.LastIndexOf(".");
            if (lastDotIndex >= 0)
            {
                fileType = fileName.Substring(lastDotIndex + 1).ToLower();
            }

            if (VALID_FILE_TYPES.Contains(fileType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 显示通知对话框
        /// </summary>
        /// <param name="message"></param>
        public virtual void ShowNotify(string message)
        {
            ShowNotify(message, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 显示通知对话框
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageIcon"></param>
        public virtual void ShowNotify(string message, MessageBoxIcon messageIcon)
        {
            ShowNotify(message, messageIcon, Target.Top);
        }

        /// <summary>
        /// 显示通知对话框
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageIcon"></param>
        /// <param name="target"></param>
        public virtual void ShowNotify(string message, MessageBoxIcon messageIcon, Target target)
        {
            Notify n = new Notify();
            n.Target = target;
            n.Message = message;
            n.MessageBoxIcon = messageIcon;
            n.PositionX = Position.Center;
            n.PositionY = Position.Top;
            n.DisplayMilliseconds = 3000;
            n.ShowHeader = false;

            n.Show();
        }

        /// <summary>
        /// 登录验证跳转
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            DUser loacluser = (DUser)filterContext.HttpContext.Session["User"];

            if (
                !(filterContext.RouteData.GetRequiredString("controller") == "Login") &&
                !(filterContext.ActionDescriptor.ActionName == "Login")
                )
            {
                if (loacluser == null)
                {
                    string redirectUrl = filterContext.HttpContext.Request.RawUrl;
                    string fromSmartUnit = filterContext.HttpContext.Request.QueryString["fromSmartUnit"];
                    if (!string.IsNullOrEmpty(fromSmartUnit) && fromSmartUnit == "true")
                        if (redirectUrl != "/")
                            filterContext.Result = new RedirectResult("/Login/Login" + "?redirectUrl=" + redirectUrl);
                        else
                            filterContext.Result = new RedirectResult("/Login/Login");
                    else
                        filterContext.Result = new RedirectResult("/Login/Login");
                    return;
                }

                base.OnActionExecuting(filterContext);
            }

        }
    }
}