using BLL.IService;
using BLL.Service;
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using FineUIMvc.EmptyProject.Tools;
using FineUIMvc.EmptyProject.Models;
using Newtonsoft.Json.Converters;

namespace FineUIMvc.EmptyProject.Controllers
{
    public class MobileController :Controller
    {
        private readonly IOrderService _orderSerive;
        private readonly IOrderGoodsService _orderGoodsService;
        private readonly IUserService _userService;
        private readonly ITerminalService _terminalService;
        private readonly IGoodsService _goodsService;
        private readonly ITerminalWXUserService _terminalWXUserService;

        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
        

        public MobileController()
        {
            _orderSerive = new OrderService(null);
            _orderGoodsService = new OrderGoodsService(null);
            _userService = new UserService(null);
            _terminalService = new TerminalService(null);
            _goodsService = new GoodsService(null);
            _terminalWXUserService = new TerminalWXUserService(null);

            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        }

       [HttpPost]
        public string Login()
        {

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string,string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string UserName = dic["UserName"];
            string Password = dic["Password"];
            DUser user = _userService.Find(UserName);

            if (user != null && user.Password == Password)
            {
                //SessionExtension.Set<LocalUser>(HttpContext.Session, "User", user);
                Session["User"] = user;

                return JsonConvert.SerializeObject(user);
            }
            else
            {
                return "{}";
            }
        }

        [HttpPost]
        public string GetGoods()
        {
            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string pageNumString = dic["pageNum"];

            string name = null;
            if (dic.Where(p => p.Key =="name" ).Any())
            {
                name = dic["name"];
            }
            
            int pageNum = Convert.ToInt32(pageNumString) -1 ;

            List<Goods> goodsList;

            if (!string.IsNullOrEmpty(name))
            {
                goodsList = _goodsService.FindList(p => p.Name.Contains(name), "", true).ToList();
            }
            else
            {
                goodsList = _goodsService.FindList(p => true, "", true).ToList();
            }

            goodsList = PagingHelper<Goods>.GetPagedDataTable(pageNum,10,goodsList.Count, goodsList);

            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            retdic.Add("data", goodsList);
            retdic.Add("pageNum", pageNum);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        [HttpPost]
        public string GetGoodsDetail()
        {
            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string GoodsIDString = dic["GoodsID"];
            int GoodsID = Convert.ToInt32(GoodsIDString);

            Goods goods = _goodsService.Find(p => p.ID == GoodsID);

            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            retdic.Add("data", goods);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        [HttpPost]
        public string PlaceOrder()
        {
            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            try { 

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            TerminalGoodsCount tgc = JsonConvert.DeserializeObject<TerminalGoodsCount>(stream);
            //string TerminalID = dic["TerminalID"];
            //string GoodsCountListString = dic["GoodsCountList"];
            List<GoodsCount> goodsCountList = tgc.GoodsCountList;//JsonConvert.DeserializeObject<List<GoodsCount>>(GoodsCountListString);

            for(int i =0;i< goodsCountList.Count;i++)
            {
                if(goodsCountList[i].GoodsID == 0)
                {
                    goodsCountList.Remove(goodsCountList[i]);
                }
            }

            Terminal terminal = _terminalService.Find(p => p.ID == tgc.TerminalID);

            Order order = new Order();
            order.DUser = _userService.Find(p => p.ID == tgc.UserID);
            order.UserID = tgc.UserID;
            order.Terminal = terminal;
            order.TerminalID = terminal.ID;
            order.CreateTime = DateTime.Now;

            //order = _orderSerive.Add(order);

            int categoryCount = 0;
            double countPrice = 0;

            foreach (var goodsCount in goodsCountList)
            {
                Goods goods = _goodsService.Find(p => p.ID == goodsCount.GoodsID);

                OrderGoods orderGoods = new OrderGoods();
                orderGoods.Goods = goods;
                orderGoods.GoodsID = goods.ID;
                orderGoods.Count = goodsCount.Count;
                orderGoods.CountPrice = goodsCount.Count * goods.Price;
                orderGoods.CreateTime = DateTime.Now;
                orderGoods.OrderID = order.ID;
                orderGoods.Order = order;

                //orderGoods = _orderGoodsService.Add(orderGoods);

                order.OrderGoods.Add(orderGoods);

                categoryCount += 1;
                countPrice += orderGoods.CountPrice;
            }

            order.CategoryCount = categoryCount;
            order.CountPrice = countPrice;

            _orderSerive.Add(order);
            }catch(Exception e)
            {
                retdic.Add("data", e.ToString());

                return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
            }
            
            retdic.Add("data", true);

            
            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        [HttpPost]
        public string GetOrder()
        {

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();
            
            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string pageNumString = dic["pageNum"];
            string UserIDString = dic["UserID"];

            DateTime todayStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,0,0,0);
            DateTime todayEnd = todayStart.AddDays(1);

            int UserID = Convert.ToInt32(UserIDString);

            string name = null;
            if (dic.Where(p => p.Key == "name").Any())
            {
                name = dic["name"];
            }

            int pageNum = Convert.ToInt32(pageNumString) - 1;

            List<Order> orderList;

            if (!string.IsNullOrEmpty(name))
            {
                orderList = _orderSerive.FindList(p => p.UserID == UserID && p.Terminal.Name.Contains(name) && p.CreateTime > todayStart && p.CreateTime < todayEnd, "", true).ToList();
            }
            else
            {
                orderList = _orderSerive.FindList(p => p.UserID == UserID && p.CreateTime > todayStart && p.CreateTime < todayEnd, "CreateTime", true).ToList();
            }

            orderList = PagingHelper<Order>.GetPagedDataTable(pageNum, 10, orderList.Count, orderList);

            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            retdic.Add("data", orderList);
            retdic.Add("pageNum", pageNum);

            return JsonConvert.SerializeObject(retdic,Formatting.Indented, timeFormat);
        }

        [HttpPost]
        public string GetOrderGoods()
        {

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string OrderIDString = dic["OrderID"];
            int OrderID = Convert.ToInt32(OrderIDString);

            List<OrderGoods> orderGoodsList = _orderGoodsService.FindList(p => p.OrderID == OrderID, "CreateTime", true).ToList();

            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            retdic.Add("data", orderGoodsList);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        [HttpPost]
        public string GetTerminalID()
        {

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string url = dic["url"];

            Terminal terminal = _terminalService.Find(p => p.WXQCCodeAddress == url);

            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            retdic.Add("data", terminal.ID);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        [HttpPost]
        public string UserAnalysis()
        {

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string UserIDString = dic["UserID"];

            int UserID = Convert.ToInt32(UserIDString);

            DateTime? startTime = DateTime.Now, endTime = DateTime.Now;

            if(dic.Where(p => p.Key == "StartTime").Any())
            {
                startTime = Convert.ToDateTime(dic["StartTime"]);
            }

            if (dic.Where(p => p.Key == "EndTime").Any())
            {
                endTime = Convert.ToDateTime(dic["EndTime"]);
            }

            UserStatistic us = new UserStatistic();

            List<UserOrderGoods> userOrderGoodsList = new List<UserOrderGoods>();

            DUser user = _userService.Find(p => p.ID == UserID);

            if (user != null)
            {
                DateTime startTime1 = (DateTime)startTime;
                DateTime endTime1 = (DateTime)endTime;
                DateTime startToday, endToday;
                startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
                endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

                List<Order> orderList = _orderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday && p.DUser.ID == user.ID, "", true).ToList();                

                us.UserID = user.ID;
                us.UserName = user.Name;

                int CategoryCount = 0, Count = 0;
                double CountPrice = 0;

                foreach (var order in orderList)
                {
                    CategoryCount += order.CategoryCount;
                    CountPrice += order.CountPrice;

                    foreach (var orderGoods in order.OrderGoods)
                    {
                        Count += orderGoods.Count;
                    }

                    foreach (var orderGoods in order.OrderGoods)
                    {
                        if (!userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).Any())
                        {
                            UserOrderGoods uog = new UserOrderGoods();

                            uog.GoodsID = orderGoods.Goods.ID;
                            uog.GoodsName = orderGoods.Goods.Name;
                            uog.Count = orderGoods.Count;
                            uog.CountPrice = orderGoods.CountPrice;

                            userOrderGoodsList.Add(uog);
                        }
                        else
                        {
                            userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).FirstOrDefault().Count += orderGoods.Count;
                            userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).FirstOrDefault().CountPrice += orderGoods.CountPrice;
                        }


                    }
                }

                us.OrderCount = orderList.Count;

                us.CategoryCount = CategoryCount;
                us.Count = Count;
                us.CountPrice = CountPrice;

                
            }

            List<UserOrderGoods> retuserOrderGoodsList = new List<UserOrderGoods>();

            foreach (var userOrderGoods in userOrderGoodsList)
            {
                if (!retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).Any())
                {
                    UserOrderGoods uog = new UserOrderGoods();

                    uog.GoodsID = userOrderGoods.GoodsID;
                    uog.GoodsName = userOrderGoods.GoodsName;
                    uog.Count = userOrderGoods.Count;
                    uog.CountPrice = userOrderGoods.CountPrice;

                    retuserOrderGoodsList.Add(uog);
                }
                else
                {
                    retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).FirstOrDefault().Count += userOrderGoods.Count;
                    retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).FirstOrDefault().CountPrice += userOrderGoods.CountPrice;
                }


            }

            us.UserOrderGoodsList = retuserOrderGoodsList;

            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            retdic.Add("data", us);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        public class UserStatistic
        {
            public int UserID { get; set; }
            public string UserName { get; set; }
            public int OrderCount { get; set; }
            public int CategoryCount { get; set; }
            public int Count { get; set; }
            public double CountPrice { get; set; }
            public ICollection<UserOrderGoods> UserOrderGoodsList { get; set; }

        }

        public class TerminalGoodsCount
        {
            public int TerminalID { get; set; }
            public int UserID { get; set; }
            public List<GoodsCount> GoodsCountList { get; set; }
        }

        public class GoodsCount
        {
            public int GoodsID { get; set; }
            public int Count { get; set; }
        }



        public ActionResult TerminalAnalysis()
        {
            string code = Request.QueryString["code"];

            WeChatCommon wcc = new WeChatCommon();

            TerminalWXUser wxUser = new TerminalWXUser();

            try
            {
                wx_backdata<wx_oauth2token> oauth2token = wcc.GetOauth2AccessToken(code);

                wxUser = _terminalWXUserService.Find(p => p.OpenID == oauth2token.ResponseData.openid);

                if(wxUser== null)
                {
                    return View("Error");
                }
            }catch(Exception e)
            {
                return View("Error");
            }
            

            int id = (int)wxUser.TerminalID;

            TerminalStatistic ts = new TerminalStatistic();

            List<UserOrderGoods> userOrderGoodsList = new List<UserOrderGoods>();

            Terminal terminal = _terminalService.Find(p => p.ID == id);

            if(terminal !=null)
            {
                ViewBag.Title = terminal.Name;

                DateTime today = DateTime.Now;
                DateTime startToday, endToday;
                startToday = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
                endToday = new DateTime(today.AddDays(1).Year, today.AddDays(1).Month, today.AddDays(1).Day, 0, 0, 0);

                List<Order> orderList = _orderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday && p.TerminalID == terminal.ID, "", true).ToList();                

                ts.TerminalID = terminal.ID;
                ts.TerminalName = terminal.Name;

                int CategoryCount = 0, Count = 0;
                double CountPrice = 0;

                foreach (var order in orderList)
                {
                    CategoryCount += order.CategoryCount;
                    CountPrice += order.CountPrice;

                    foreach (var orderGoods in order.OrderGoods)
                    {
                        Count += orderGoods.Count;
                    }

                    foreach (var orderGoods in order.OrderGoods)
                    {
                        if (!userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).Any())
                        {
                            UserOrderGoods uog = new UserOrderGoods();

                            uog.GoodsID = orderGoods.Goods.ID;
                            uog.GoodsName = orderGoods.Goods.Name;
                            uog.Count = orderGoods.Count;
                            uog.CountPrice = orderGoods.CountPrice;

                            userOrderGoodsList.Add(uog);
                        }
                        else
                        {
                            userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).FirstOrDefault().Count += orderGoods.Count;
                            userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).FirstOrDefault().CountPrice += orderGoods.CountPrice;
                        }


                    }
                }

                ts.OrderCount = orderList.Count;

                ts.CategoryCount = CategoryCount;
                ts.Count = Count;
                ts.CountPrice = CountPrice;
                
            }


            List<UserOrderGoods> retuserOrderGoodsList = new List<UserOrderGoods>();

            foreach (var userOrderGoods in userOrderGoodsList)
            {
                if (!retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).Any())
                {
                    UserOrderGoods uog = new UserOrderGoods();

                    uog.GoodsID = userOrderGoods.GoodsID;
                    uog.GoodsName = userOrderGoods.GoodsName;
                    uog.Count = userOrderGoods.Count;
                    uog.CountPrice = userOrderGoods.CountPrice;

                    retuserOrderGoodsList.Add(uog);
                }
                else
                {
                    retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).FirstOrDefault().Count += userOrderGoods.Count;
                    retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).FirstOrDefault().CountPrice += userOrderGoods.CountPrice;
                }


            }


            ts.UserOrderGoodsList = retuserOrderGoodsList;

            ViewBag.TerminalStatistic = ts;

            ViewBag.ID = id;

            return View();
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult TerminalAnalysis(int id,DateTime? startTime,DateTime? endTime)
        {
            ViewBag.StartTime = (DateTime)startTime;
            ViewBag.EndTime = (DateTime)endTime;

            TerminalStatistic ts = new TerminalStatistic();

            List<UserOrderGoods> userOrderGoodsList = new List<UserOrderGoods>();

            Terminal terminal = _terminalService.Find(p => p.ID == id);

            if (terminal != null)
            {
                ViewBag.Title = terminal.Name;

                DateTime startTime1 = (DateTime)startTime;
                DateTime endTime1 = (DateTime)endTime;
                DateTime startToday, endToday;
                startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
                endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

                List<Order> orderList = _orderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday && p.TerminalID == terminal.ID, "", true).ToList();                

                ts.TerminalID = terminal.ID;
                ts.TerminalName = terminal.Name;

                int CategoryCount = 0, Count = 0;
                double CountPrice = 0;

                foreach (var order in orderList)
                {
                    CategoryCount += order.CategoryCount;
                    CountPrice += order.CountPrice;

                    foreach (var orderGoods in order.OrderGoods)
                    {
                        Count += orderGoods.Count;
                    }

                    foreach (var orderGoods in order.OrderGoods)
                    {
                        if (!userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).Any())
                        {
                            UserOrderGoods uog = new UserOrderGoods();

                            uog.GoodsID = orderGoods.Goods.ID;
                            uog.GoodsName = orderGoods.Goods.Name;
                            uog.Count = orderGoods.Count;
                            uog.CountPrice = orderGoods.CountPrice;

                            userOrderGoodsList.Add(uog);
                        }
                        else
                        {
                            userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).FirstOrDefault().Count += orderGoods.Count;
                            userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).FirstOrDefault().CountPrice += orderGoods.CountPrice;
                        }


                    }
                }

                ts.OrderCount = orderList.Count;

                ts.CategoryCount = CategoryCount;
                ts.Count = Count;
                ts.CountPrice = CountPrice;

                
            }

            List<UserOrderGoods> retuserOrderGoodsList = new List<UserOrderGoods>();

            foreach (var userOrderGoods in userOrderGoodsList)
            {
                if (!retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).Any())
                {
                    UserOrderGoods uog = new UserOrderGoods();

                    uog.GoodsID = userOrderGoods.GoodsID;
                    uog.GoodsName = userOrderGoods.GoodsName;
                    uog.Count = userOrderGoods.Count;
                    uog.CountPrice = userOrderGoods.CountPrice;

                    retuserOrderGoodsList.Add(uog);
                }
                else
                {
                    retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).FirstOrDefault().Count += userOrderGoods.Count;
                    retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).FirstOrDefault().CountPrice += userOrderGoods.CountPrice;
                }


            }

            ts.UserOrderGoodsList = retuserOrderGoodsList;

            ViewBag.TerminalStatistic = ts;

            ViewBag.ID = id;

            return View();
        }
 
    }

    public class TerminalStatistic
    {
        public int TerminalID { get; set; }
        public string TerminalName { get; set; }
        public int OrderCount { get; set; }
        public int CategoryCount { get; set; }
        public int Count { get; set; }
        public double CountPrice { get; set; }

        public ICollection<UserOrderGoods> UserOrderGoodsList { get; set; }
    }

    public class UserOrderGoods
    {
        public int GoodsID { get; set; }
        public string GoodsName { get; set; }
        public int Count { get; set; }
        public double CountPrice { get; set; }
    }

    public static class PagingHelper<T>
    {
        public static List<T> GetPagedDataTable(int pageIndex, int pageSize, int recordCount, List<T> list)
        {
            //// 对传入的 pageIndex 进行有效性验证//////////////
            //int pageCount = recordCount / pageSize;
            //if (recordCount % pageSize != 0)
            //{
            //    pageCount++;
            //}
            //if (pageIndex > pageCount - 1)
            //{
            //    pageIndex = pageCount - 1;
            //}
            //if (pageIndex < 0)
            //{
            //    pageIndex = 0;
            //}
            ///////////////////////////////////////////////

            List<T> retList = new List<T>();

            int rowbegin = pageIndex * pageSize;
            int rowend = (pageIndex + 1) * pageSize;
            if (rowend > list.Count)
            {
                rowend = list.Count;
            }

            retList = list.Skip(pageIndex * pageSize).Take(pageSize).ToList();

            return retList;
        }
    }
}