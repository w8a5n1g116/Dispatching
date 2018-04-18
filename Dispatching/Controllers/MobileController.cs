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
        private readonly IUserService _dUserService;

        private readonly ISaltOrderService _saltOrderSerive;
        private readonly ISaltOrderGoodsService _saltOrderGoodsService;
        private readonly ISaltTerminalService _saltTerminalService;
        private readonly ISaltGoodsService _saltGoodsService;

        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
        

        public MobileController()
        {
            _orderSerive = new OrderService(null);
            _orderGoodsService = new OrderGoodsService(null);
            _userService = new UserService(null);
            _terminalService = new TerminalService(null);
            _goodsService = new GoodsService(null);
            _terminalWXUserService = new TerminalWXUserService(null);
            _dUserService = new UserService(null);

            _saltOrderSerive = new SaltOrderService(null);
            _saltOrderGoodsService = new SaltOrderGoodsService(null);
            _saltTerminalService = new SaltTerminalService(null);
            _saltGoodsService = new SaltGoodsService(null);

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

        #region 酒品配送

        

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
        public string DeleteOrder()
        {
            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string OrderIDString = dic["OrderID"];

            int OrderID = Convert.ToInt32(OrderIDString);

            Order order = _orderSerive.Find(p => p.ID == OrderID);

            if(order != null)
            {
                bool isok = true;
                var ordergoodsList = order.OrderGoods.ToList();

                for(int i = 0; i < ordergoodsList.Count; i++)
                {
                    var ordergoods = ordergoodsList[i];
                    isok = _orderGoodsService.Delete(ordergoods);
                }

                isok =_orderSerive.Delete(order);

                retdic.Add("data", isok);
            }
            else
            {
                retdic.Add("data", false);
            }

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
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

        #endregion 酒品配送 酒品配送

        #region 盐品配送

        [HttpPost]
        public string GetSaltGoods()
        {
            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string pageNumString = dic["pageNum"];

            string name = null;
            if (dic.Where(p => p.Key == "name").Any())
            {
                name = dic["name"];
            }

            int pageNum = Convert.ToInt32(pageNumString) - 1;

            List<SaltGoods> goodsList;

            if (!string.IsNullOrEmpty(name))
            {
                goodsList = _saltGoodsService.FindList(p => p.Name.Contains(name), "", true).ToList();
            }
            else
            {
                goodsList = _saltGoodsService.FindList(p => true, "", true).ToList();
            }

            goodsList = PagingHelper<SaltGoods>.GetPagedDataTable(pageNum, 10, goodsList.Count, goodsList);

            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            retdic.Add("data", goodsList);
            retdic.Add("pageNum", pageNum);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        [HttpPost]
        public string GetSaltGoodsDetail()
        {
            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string GoodsIDString = dic["GoodsID"];
            int GoodsID = Convert.ToInt32(GoodsIDString);

            SaltGoods goods = _saltGoodsService.Find(p => p.ID == GoodsID);

            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            retdic.Add("data", goods);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        [HttpPost]
        public string PlaceSaltOrder()
        {
            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            try
            {

                var sr = new StreamReader(Request.InputStream);
                var stream = sr.ReadToEnd();

                SaltTerminalGoodsCount tgc = JsonConvert.DeserializeObject<SaltTerminalGoodsCount>(stream);
                //string TerminalID = dic["TerminalID"];
                //string GoodsCountListString = dic["GoodsCountList"];
                List<SaltGoodsCount> goodsCountList = tgc.GoodsCountList;//JsonConvert.DeserializeObject<List<GoodsCount>>(GoodsCountListString);

                for (int i = 0; i < goodsCountList.Count; i++)
                {
                    if (goodsCountList[i].GoodsID == 0)
                    {
                        goodsCountList.Remove(goodsCountList[i]);
                    }
                }

                SaltTerminal terminal = _saltTerminalService.Find(p => p.ID == tgc.TerminalID);

                SaltOrder order = new SaltOrder();
                order.DUser = _userService.Find(p => p.ID == tgc.UserID);
                order.UserID = tgc.UserID;
                order.SaltTerminal = terminal;
                order.TerminalID = terminal.ID;
                order.CreateTime = DateTime.Now;

                //order = _orderSerive.Add(order);

                int categoryCount = 0;
                double countPrice = 0;
                double countReabte = 0;
                double countIncome = 0;

                foreach (var goodsCount in goodsCountList)
                {
                    SaltGoods goods = _saltGoodsService.Find(p => p.ID == goodsCount.GoodsID);

                    SaltOrderGoods orderGoods = new SaltOrderGoods();
                    orderGoods.SaltGoods = goods;
                    orderGoods.GoodsID = goods.ID;
                    orderGoods.Count = goodsCount.Count;
                    orderGoods.CountPrice = goodsCount.Count * goods.Price;
                    orderGoods.CreateTime = DateTime.Now;
                    orderGoods.Rebate = goodsCount.Rebate;
                    orderGoods.Income = goodsCount.Income;
                    orderGoods.OrderID = order.ID;
                    orderGoods.SaltOrder = order;

                    //orderGoods = _orderGoodsService.Add(orderGoods);

                    order.SaltOrderGoods.Add(orderGoods);

                    categoryCount += 1;
                    countPrice += orderGoods.CountPrice;
                    countReabte += orderGoods.Rebate;
                    countIncome += orderGoods.Income;
                }

                order.CategoryCount = categoryCount;
                order.CountPrice = countPrice;
                order.CountRebate = countReabte;
                order.CountIncome = countIncome;

                order.OrderStatus = "下达";

                order.OrderType = tgc.OrderType;

                order.PayType = tgc.PayType;

                _saltOrderSerive.Add(order);
            }
            catch (Exception e)
            {
                retdic.Add("data", e.ToString());

                return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
            }

            retdic.Add("data", true);


            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        [HttpPost]
    
        public string GetSaltOrder()
        {

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string pageNumString = dic["pageNum"];
            string UserIDString = dic["UserID"];

            DateTime todayStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            DateTime todayEnd = todayStart.AddDays(1);

            int UserID = Convert.ToInt32(UserIDString);

            string name = null;
            if (dic.Where(p => p.Key == "name").Any())
            {
                name = dic["name"];
            }

            int pageNum = Convert.ToInt32(pageNumString) - 1;

            List<SaltOrder> orderList;

            if (!string.IsNullOrEmpty(name))
            {
                orderList = _saltOrderSerive.FindList(p => p.UserID == UserID && p.SaltTerminal.Name.Contains(name) && p.CreateTime > todayStart && p.CreateTime < todayEnd, "", true).ToList();
            }
            else
            {
                orderList = _saltOrderSerive.FindList(p => p.UserID == UserID && p.CreateTime > todayStart && p.CreateTime < todayEnd, "CreateTime", true).ToList();
            }

            orderList = PagingHelper<SaltOrder>.GetPagedDataTable(pageNum, 10, orderList.Count, orderList);

            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            retdic.Add("data", orderList);
            retdic.Add("pageNum", pageNum);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }
        [HttpPost]
        public string DeleteSaltOrder()
        {
            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string OrderIDString = dic["OrderID"];

            int OrderID = Convert.ToInt32(OrderIDString);

            SaltOrder saltOrder = _saltOrderSerive.Find(p => p.ID == OrderID);

            if (saltOrder != null)
            {
                bool isok = true;
                var ordergoodsList = saltOrder.SaltOrderGoods.ToList();

                for (int i = 0; i < ordergoodsList.Count; i++)
                {
                    var ordergoods = ordergoodsList[i];
                    isok = _saltOrderGoodsService.Delete(ordergoods);
                }

                isok = _saltOrderSerive.Delete(saltOrder);

                retdic.Add("data", isok);
            }
            else
            {
                retdic.Add("data", false);
            }

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        public string GetSaltOrderByID()
        {

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string OrderIDString = dic["OrderID"];
            int OrderID = Convert.ToInt32(OrderIDString);

            SaltOrder order = _saltOrderSerive.Find(p => p.ID == OrderID);

            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            retdic.Add("data", order);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        [HttpPost]
        public string GetSaltOrderGoods()
        {

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string OrderIDString = dic["OrderID"];
            int OrderID = Convert.ToInt32(OrderIDString);

            List<SaltOrderGoods> orderGoodsList = _saltOrderGoodsService.FindList(p => p.OrderID == OrderID, "CreateTime", true).ToList();

            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            retdic.Add("data", orderGoodsList);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        [HttpPost]
        public string GetSaltTerminals()
        {
            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string pageNumString = dic["pageNum"];

            string TerminalName = null;

            if (dic.Where(p => p.Key == "name").Any())
            {
                TerminalName = dic["name"];
            }

            int pageNum = Convert.ToInt32(pageNumString) - 1;

            List<SaltTerminal> terminalsList = new List<SaltTerminal>();

            if (!string.IsNullOrEmpty(TerminalName))
            {
                terminalsList = _saltTerminalService.FindList(p => p.Name.Contains(TerminalName), "CreateTime", true).ToList();
            }
            else
            {
                terminalsList = _saltTerminalService.FindList(p => true, "CreateTime", true).ToList();
            }

            terminalsList = PagingHelper<SaltTerminal>.GetPagedDataTable(pageNum, 10, terminalsList.Count, terminalsList);

            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            retdic.Add("data", terminalsList);
            retdic.Add("pageNum", pageNum);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        [HttpPost]
        public string AddSaltTerminal()
        {
            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string TerminalName = dic["TerminalName"];
            string Address = dic["Address"];
            string Contact = dic["Contact"];
            string Phone = dic["Phone"];
            string UserIDString = dic["UserID"];

            int UserID = Convert.ToInt32(UserIDString);

            if (_saltTerminalService.Find(p => p.Name == TerminalName) == null)
            {
                SaltTerminal terminal = new SaltTerminal();

                terminal.Name = TerminalName;
                terminal.Address = Address;
                terminal.Contact = Contact;
                terminal.Phone = Phone;
                terminal.CreateTime = DateTime.Now;

                DUser user = _dUserService.Find(p => p.ID == UserID);
                terminal.DUser = user;
                terminal.SaltUserID = user.ID;

                _saltTerminalService.Add(terminal);

                retdic.Add("data", true);
            }
            else
            {
                retdic.Add("data", false);
            }       

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        public string GetOrderByStatus()
        {
            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string OrderStatus = dic["OrderStatus"];
            string pageNumString = dic["pageNum"];
            int pageNum = Convert.ToInt32(pageNumString) - 1;

            string UserIDString = null;
            int UserID = 0;
            if (dic.Where(p => p.Key == "UserID").Any() && !string.IsNullOrEmpty(dic["UserID"]))
            {
                UserIDString = dic["UserID"];
                UserID = Convert.ToInt32(UserIDString);
            }

            string TerminalName = null;
            if (dic.Where(p => p.Key == "name").Any())
            {
                TerminalName = dic["name"];
            }

            List<SaltOrder> orderList = new List<SaltOrder>();

            if (OrderStatus == "下达")
            {
                orderList = _saltOrderSerive.FindList(p => p.OrderStatus == "下达", "CreateTime",false).ToList();  
            }
            else if (OrderStatus == "通过")
            {
                orderList = _saltOrderSerive.FindList(p => p.OrderStatus == "通过", "CreateTime", false).ToList();
            }
            else if (OrderStatus == "驳回")
            {
                orderList = _saltOrderSerive.FindList(p => p.OrderStatus == "驳回", "CreateTime", false).ToList();
            }
            else if (OrderStatus == "送达")
            {
                orderList = _saltOrderSerive.FindList(p => p.OrderStatus == "送达", "CreateTime", false).ToList();
            }
            else if (OrderStatus == "结算")
            {
                orderList = _saltOrderSerive.FindList(p => p.OrderStatus == "结算", "CreateTime", false).ToList();
            }
            else if (OrderStatus == "待收现")
            {
                orderList = _saltOrderSerive.FindList(p => p.OrderStatus == "待收现", "CreateTime", false).ToList();
            }
            else if (OrderStatus == "已收现")
            {
                orderList = _saltOrderSerive.FindList(p => p.OrderStatus == "已收现", "CreateTime", false).ToList();
            }

            if (!string.IsNullOrEmpty(TerminalName))
            {
                orderList = orderList.Where(p => p.SaltTerminal.Name.Contains(TerminalName)).ToList();
            }

            if (!string.IsNullOrEmpty(UserIDString))
            {
                orderList = orderList.Where(p => p.UserID == UserID).ToList();
            }

            orderList = PagingHelper<SaltOrder>.GetPagedDataTable(pageNum, 10, orderList.Count, orderList);

            retdic.Add("data", orderList);
            retdic.Add("pageNum", pageNum);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        public string ChangeOrderStatus()
        {
            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string ToOrderStatus = dic["ToOrderStatus"];
            string OrderIDString = dic["OrderID"];
            int OrderID = Convert.ToInt32(OrderIDString);

            SaltOrder order = _saltOrderSerive.Find(p => p.ID == OrderID);

            if(order != null)
            {
                if(ToOrderStatus == "通过")
                {
                    order.OrderStatus = "通过";
                    retdic.Add("data", true);
                }
                else if(ToOrderStatus == "驳回")
                {
                    order.OrderStatus = "驳回";
                    retdic.Add("data", true);
                }
                else if (ToOrderStatus == "送达")
                {
                    order.OrderStatus = "送达";
                    retdic.Add("data", true);
                }
                else if (ToOrderStatus == "结算")
                {
                    order.OrderStatus = "结算";
                    retdic.Add("data", true);
                }
                else if (ToOrderStatus == "已收现")
                {
                    order.OrderStatus = "已收现";
                    retdic.Add("data", true);
                }
                else
                {
                    retdic.Add("data", false);
                }

                _saltOrderSerive.Update(order);
            }

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        public string GetOrderByType()
        {
            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string PayType = dic["PayType"];
            string pageNumString = dic["pageNum"];
            int pageNum = Convert.ToInt32(pageNumString) - 1;
            string UserIDString = dic["UserID"];
            int UserID = Convert.ToInt32(UserIDString);

            string TerminalName = null;
            if (dic.Where(p => p.Key == "name").Any())
            {
                TerminalName = dic["name"];
            }

            List<SaltOrder> orderList = new List<SaltOrder>();

            if (PayType == "压批")
            {
                orderList = _saltOrderSerive.FindList(p => p.PayType == "压批" && p.UserID == UserID && p.OrderStatus != "驳回", "CreateTime", false).ToList();
            }
            else if (PayType == "现金")
            {
                orderList = _saltOrderSerive.FindList(p => p.PayType == "现金" && p.UserID == UserID && p.OrderStatus != "驳回", "CreateTime", false).ToList();
            }

            if (!string.IsNullOrEmpty(TerminalName))
            {
                orderList = orderList.Where(p => p.SaltTerminal.Name.Contains(TerminalName)).ToList();
            }

            orderList = PagingHelper<SaltOrder>.GetPagedDataTable(pageNum, 10, orderList.Count, orderList);

            retdic.Add("data", orderList);
            retdic.Add("pageNum", pageNum);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        public string GetOrderCountByType()
        {
            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string PayType = dic["PayType"];
            string UserIDString = dic["UserID"];
            int UserID = Convert.ToInt32(UserIDString);

            List<SaltOrder> orderList = new List<SaltOrder>();

            if (PayType == "压批")
            {
                orderList = _saltOrderSerive.FindList(p => p.PayType == "压批" && p.UserID == UserID && p.OrderStatus != "驳回", "CreateTime", false).ToList();
            }
            else if (PayType == "现金")
            {
                orderList = _saltOrderSerive.FindList(p => p.PayType == "现金" && p.UserID == UserID && p.OrderStatus != "驳回", "CreateTime", false).ToList();
            }

            retdic.Add("data", orderList.Count);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        public string GetOrderCountByStatus()
        {
            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string OrderStatus = dic["OrderStatus"];

            List<SaltOrder> orderList = new List<SaltOrder>();

            if (OrderStatus == "下达")
            {
                orderList = _saltOrderSerive.FindList(p => p.OrderStatus == "下达", "CreateTime", false).ToList();
            }
            else if (OrderStatus == "通过")
            {
                orderList = _saltOrderSerive.FindList(p => p.OrderStatus == "通过", "CreateTime", false).ToList();
            }
            else if (OrderStatus == "驳回")
            {
                orderList = _saltOrderSerive.FindList(p => p.OrderStatus == "驳回", "CreateTime", false).ToList();
            }
            else if (OrderStatus == "送达")
            {
                orderList = _saltOrderSerive.FindList(p => p.OrderStatus == "送达", "CreateTime", false).ToList();
            }
            else if (OrderStatus == "结算")
            {
                orderList = _saltOrderSerive.FindList(p => p.OrderStatus == "结算", "CreateTime", false).ToList();
            }
            else if (OrderStatus == "待收现")
            {
                orderList = _saltOrderSerive.FindList(p => p.OrderStatus == "待收现", "CreateTime", false).ToList();
            }
            else if (OrderStatus == "已收现")
            {
                orderList = _saltOrderSerive.FindList(p => p.OrderStatus == "已收现", "CreateTime", false).ToList();
            }

            retdic.Add("data", orderList.Count);

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        public string ChangeOrderType()
        {
            Dictionary<string, Object> retdic = new Dictionary<string, object>();

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string ToPayType = dic["ToPayType"];
            string OrderIDString = dic["OrderID"];
            int OrderID = Convert.ToInt32(OrderIDString);

            SaltOrder order = _saltOrderSerive.Find(p => p.ID == OrderID);

            if (order != null)
            {
                if (ToPayType == "压批")
                {
                    order.PayType = "压批";
                    retdic.Add("data", true);
                }
                else if (ToPayType == "现金")
                {
                    order.PayType = "现金";
                    order.OrderStatus = "待收现";
                    retdic.Add("data", true);
                }
                else
                {
                    retdic.Add("data", false);
                }

                _saltOrderSerive.Update(order);
            }

            return JsonConvert.SerializeObject(retdic, Formatting.Indented, timeFormat);
        }

        [HttpPost]
        public string SaltUserAnalysis()
        {

            var sr = new StreamReader(Request.InputStream);
            var stream = sr.ReadToEnd();

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream);
            string UserIDString = dic["UserID"];

            int UserID = Convert.ToInt32(UserIDString);

            DateTime? startTime = DateTime.Now, endTime = DateTime.Now;

            if (dic.Where(p => p.Key == "StartTime").Any())
            {
                startTime = Convert.ToDateTime(dic["StartTime"]);
            }

            if (dic.Where(p => p.Key == "EndTime").Any())
            {
                endTime = Convert.ToDateTime(dic["EndTime"]);
            }

            SaltUserStatistic us = new SaltUserStatistic();

            List<SaltUserOrderGoods> userOrderGoodsList = new List<SaltUserOrderGoods>();

            DUser user = _userService.Find(p => p.ID == UserID);

            if (user != null)
            {
                DateTime startTime1 = (DateTime)startTime;
                DateTime endTime1 = (DateTime)endTime;
                DateTime startToday, endToday;
                startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
                endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

                List<SaltOrder> orderList = _saltOrderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday && p.DUser.ID == user.ID, "", true).ToList();

                us.UserID = user.ID;
                us.UserName = user.Name;

                int CategoryCount = 0, Count = 0;
                double CountPrice = 0;

                foreach (var order in orderList)
                {
                    CategoryCount += order.CategoryCount;
                    CountPrice += order.CountPrice;

                    foreach (var orderGoods in order.SaltOrderGoods)
                    {
                        Count += orderGoods.Count;
                    }

                    foreach (var orderGoods in order.SaltOrderGoods)
                    {
                        if (!userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).Any())
                        {
                            SaltUserOrderGoods uog = new SaltUserOrderGoods();

                            uog.GoodsID = orderGoods.SaltGoods.ID;
                            uog.GoodsName = orderGoods.SaltGoods.Name;
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

            List<SaltUserOrderGoods> retuserOrderGoodsList = new List<SaltUserOrderGoods>();

            foreach (var userOrderGoods in userOrderGoodsList)
            {
                if (!retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).Any())
                {
                    SaltUserOrderGoods uog = new SaltUserOrderGoods();

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

        public class SaltUserStatistic
        {
            public int UserID { get; set; }
            public string UserName { get; set; }
            public int OrderCount { get; set; }
            public int CategoryCount { get; set; }
            public int Count { get; set; }
            public double CountPrice { get; set; }
            public ICollection<SaltUserOrderGoods> UserOrderGoodsList { get; set; }

        }

        public class SaltTerminalGoodsCount
        {
            public int TerminalID { get; set; }
            public int UserID { get; set; }
            public List<SaltGoodsCount> GoodsCountList { get; set; }

            public string PayType { get; set; }

            public string OrderType { get; set; }
        }

        public class SaltGoodsCount
        {
            public int GoodsID { get; set; }
            public int Count { get; set; }

            public double Rebate { get; set; }

            public double Income { get; set; }
        }     


        public ActionResult SaltTerminalAnalysis()
        {
            string code = Request.QueryString["code"];

            WeChatCommon wcc = new WeChatCommon();

            TerminalWXUser wxUser = new TerminalWXUser();

            try
            {
                wx_backdata<wx_oauth2token> oauth2token = wcc.GetOauth2AccessToken(code);

                wxUser = _terminalWXUserService.Find(p => p.OpenID == oauth2token.ResponseData.openid);

                if (wxUser == null)
                {
                    return View("Error");
                }
            }
            catch (Exception e)
            {
                return View("Error");
            }


            int id = (int)wxUser.TerminalID;

            TerminalStatistic ts = new TerminalStatistic();

            List<UserOrderGoods> userOrderGoodsList = new List<UserOrderGoods>();

            Terminal terminal = _terminalService.Find(p => p.ID == id);

            if (terminal != null)
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
        public ActionResult SaltTerminalAnalysis(int id, DateTime? startTime, DateTime? endTime)
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

        
        #endregion 
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

   

    public class SaltTerminalStatistic
    {
        public int TerminalID { get; set; }
        public string TerminalName { get; set; }
        public int OrderCount { get; set; }
        public int CategoryCount { get; set; }
        public int Count { get; set; }
        public double CountPrice { get; set; }

        public ICollection<UserOrderGoods> UserOrderGoodsList { get; set; }
    }

    public class SaltUserOrderGoods
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