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
    public class StatisticController : Controller
    {
        private readonly IOrderService _orderSerive;
        private readonly IOrderGoodsService _orderGoodsService;
        private readonly IUserService _userSerivce;
        private readonly ITerminalService _terminalService;

        public StatisticController()
        {
            _orderSerive = new OrderService(null);
            _orderGoodsService = new OrderGoodsService(null);
            _userSerivce = new UserService(null);
            _terminalService = new TerminalService(null);
        }
        // GET: Statistic
        public ActionResult UserAnalysis()
        {
            DateTime today = DateTime.Now;
            DateTime startToday, endToday;
            startToday = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
            endToday = new DateTime(today.AddDays(1).Year, today.AddDays(1).Month, today.AddDays(1).Day, 0, 0, 0);

            List<DUser> userList = _userSerivce.FindList(p => p.Permission == 2, "", true).ToList();

            List<Order> orderList = _orderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday,"",true).ToList();

            List<UserStatistic> userStatisticList = new List<UserStatistic>();

            foreach(var user in userList)
            {
                UserStatistic us = new UserStatistic();

                us.UserID = user.ID;
                us.UserName = user.Name;

                int CategoryCount = 0, Count = 0;
                double CountPrice = 0;

                List<Order> userOrderList = orderList.Where(p => p.UserID == user.ID).ToList();

                foreach(var order in userOrderList)
                {
                    CategoryCount += order.CategoryCount;
                    CountPrice += order.CountPrice;

                    foreach(var orderGoods in order.OrderGoods)
                    {
                        Count += orderGoods.Count;
                    }
                }

                us.OrderCount = userOrderList.Count;

                us.CategoryCount = CategoryCount;
                us.Count = Count;
                us.CountPrice = CountPrice;

                userStatisticList.Add(us);
            }

            //ViewBag.Grid1RecordCount = userStatisticList.Count;

            ViewBag.Grid1DataSource = userStatisticList;//PagingHelper<UserStatistic>.GetPagedDataTable(0, 10, userStatisticList.Count, userStatisticList);

            return View();
        }

        [HttpPost]
        public ActionResult UserAnalysis(DateTime startTime,DateTime endTime, JArray fields)
        {

            List<UserStatistic> userStatisticList = new List<UserStatistic>();

            if (startTime != null && endTime != null)
            {
                DateTime startTime1 = (DateTime)startTime;
                DateTime endTime1 = (DateTime)endTime;
                DateTime startToday, endToday;
                startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
                endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

                List<DUser> userList = _userSerivce.FindList(p => p.Permission == 2, "", true).ToList();

                List<Order> orderList = _orderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday, "", true).ToList();

                foreach (var user in userList)
                {
                    UserStatistic us = new UserStatistic();

                    us.UserID = user.ID;
                    us.UserName = user.Name;

                    int CategoryCount = 0, Count = 0;
                    double CountPrice = 0;

                    List<Order> userOrderList = orderList.Where(p => p.UserID == user.ID).ToList();

                    foreach (var order in userOrderList)
                    {
                        CategoryCount += order.CategoryCount;
                        CountPrice += order.CountPrice;

                        foreach (var orderGoods in order.OrderGoods)
                        {
                            Count += orderGoods.Count;
                        }
                    }

                    us.OrderCount = userOrderList.Count;

                    us.CategoryCount = CategoryCount;
                    us.Count = Count;
                    us.CountPrice = CountPrice;

                    userStatisticList.Add(us);
                }

                //ViewBag.Grid1RecordCount = userStatisticList.Count;
            }

            var grid1 = UIHelper.Grid("Grid1");
            grid1.DataSource(userStatisticList, fields);

            return UIHelper.Result();
        }


        public ActionResult UserAnalysisDetail(int id, DateTime startTime, DateTime endTime)
        {
            DateTime startTime1 = (DateTime)startTime;
            DateTime endTime1 = (DateTime)endTime;
            DateTime startToday, endToday;
            startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
            endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

            DUser user = _userSerivce.Find(p =>p.ID == id);

            List<Order> orderList = _orderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday && p.UserID == user.ID, "", true).ToList();

            List<UserOrderGoods> userOrderGoodsList = new List<UserOrderGoods>();

            foreach(var order in orderList)
            {
                foreach(var orderGoods in order.OrderGoods)
                {
                    if(!userOrderGoodsList.Where(p =>p.GoodsID == orderGoods.ID).Any())
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

            ViewBag.Grid1DataSource = retuserOrderGoodsList;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserAnalysisDetail_Close(JArray fields)
        {
            return UIHelper.Result();
        }

        public ActionResult TerminalAnalysis()
        {
            DateTime today = DateTime.Now;
            DateTime startToday, endToday;
            startToday = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
            endToday = new DateTime(today.AddDays(1).Year, today.AddDays(1).Month, today.AddDays(1).Day, 0, 0, 0);

            List<Terminal> terminalList = _terminalService.FindList(p => true, "", true).ToList();

            int terminalCount = terminalList.Count;

            terminalList = PagingHelper<Terminal>.GetPagedDataTable(0, 10, terminalList.Count, terminalList);

            List<Order> orderList = _orderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday, "", true).ToList();

            List<TerminalStatistic> terminalStatisticList = new List<TerminalStatistic>();

            foreach (var terminal in terminalList)
            {
                TerminalStatistic ts = new TerminalStatistic();

                ts.TerminalID = terminal.ID;
                ts.TerminalName = terminal.Name;

                int CategoryCount = 0, Count = 0;
                double CountPrice = 0;

                List<Order> userOrderList = orderList.Where(p => p.TerminalID == terminal.ID).ToList();

                foreach (var order in userOrderList)
                {
                    CategoryCount += order.CategoryCount;
                    CountPrice += order.CountPrice;

                    foreach (var orderGoods in order.OrderGoods)
                    {
                        Count += orderGoods.Count;
                    }
                }

                ts.OrderCount = userOrderList.Count;

                ts.CategoryCount = CategoryCount;
                ts.Count = Count;
                ts.CountPrice = CountPrice;

                terminalStatisticList.Add(ts);
            }

            ViewBag.Grid1RecordCount = terminalCount;

            ViewBag.Grid1DataSource = terminalStatisticList;//PagingHelper<TerminalStatistic>.GetPagedDataTable(0, 10, terminalStatisticList.Count, terminalStatisticList);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TerminalAnalysis_PageIndexChanged(JArray Grid1_fields, int Grid1_pageIndex, DateTime startTime, DateTime endTime,string terminalName)
        {
            List<TerminalStatistic> terminalStatisticList = new List<TerminalStatistic>();

            if (startTime != null && endTime != null)
            {
                DateTime startTime1 = (DateTime)startTime;
                DateTime endTime1 = (DateTime)endTime;
                DateTime startToday, endToday;
                startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
                endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

                List<Terminal> terminalList;

                if (!string.IsNullOrEmpty(terminalName))
                {
                    terminalList = _terminalService.FindList(p => p.Name.Contains(terminalName), "", true).ToList();
                }
                else
                {
                    terminalList = _terminalService.FindList(p => true, "", true).ToList();
                }

                //int terminalCount = terminalList.Count;

                terminalList = PagingHelper<Terminal>.GetPagedDataTable(Grid1_pageIndex, 10, terminalList.Count, terminalList);

                List<Order> orderList = _orderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday, "", true).ToList();

                foreach (var terminal in terminalList)
                {
                    TerminalStatistic ts = new TerminalStatistic();

                    ts.TerminalID = terminal.ID;
                    ts.TerminalName = terminal.Name;

                    int CategoryCount = 0, Count = 0;
                    double CountPrice = 0;

                    List<Order> userOrderList = orderList.Where(p => p.TerminalID == terminal.ID).ToList();

                    foreach (var order in userOrderList)
                    {
                        CategoryCount += order.CategoryCount;
                        CountPrice += order.CountPrice;

                        foreach (var orderGoods in order.OrderGoods)
                        {
                            Count += orderGoods.Count;
                        }
                    }

                    ts.OrderCount = userOrderList.Count;

                    ts.CategoryCount = CategoryCount;
                    ts.Count = Count;
                    ts.CountPrice = CountPrice;

                    terminalStatisticList.Add(ts);
                }
            }

            var grid1 = UIHelper.Grid("Grid1");  
                     
            //var recordCount = terminalCount;

            //grid1.RecordCount(recordCount);

            var dataSource = terminalStatisticList;//PagingHelper<TerminalStatistic>.GetPagedDataTable(Grid1_pageIndex, 10, terminalStatisticList.Count, terminalStatisticList);
            grid1.DataSource(dataSource, Grid1_fields);

            return UIHelper.Result();
        }


        [HttpPost]
        public ActionResult TerminalAnalysis(DateTime startTime, DateTime endTime,string terminalName, JArray fields)
        {

            List<TerminalStatistic> terminalStatisticList = new List<TerminalStatistic>();

            int terminalCount = 0;

            if (startTime != null && endTime != null)
            {
                DateTime startTime1 = (DateTime)startTime;
                DateTime endTime1 = (DateTime)endTime;
                DateTime startToday, endToday;
                startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
                endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

                List<Terminal> terminalList;
                
                if(!string.IsNullOrEmpty(terminalName))
                {
                    terminalList = _terminalService.FindList(p => p.Name.Contains(terminalName), "", true).ToList();
                }
                else
                {
                    terminalList = _terminalService.FindList(p => true, "", true).ToList();
                }
                

                terminalCount = terminalList.Count;

                terminalList = PagingHelper<Terminal>.GetPagedDataTable(0, 10, terminalList.Count, terminalList);

                List<Order> orderList = _orderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday, "", true).ToList();

                foreach (var terminal in terminalList)
                {
                    TerminalStatistic ts = new TerminalStatistic();

                    ts.TerminalID = terminal.ID;
                    ts.TerminalName = terminal.Name;

                    int CategoryCount = 0, Count = 0;
                    double CountPrice = 0;

                    List<Order> userOrderList = orderList.Where(p => p.TerminalID == terminal.ID).ToList();

                    foreach (var order in userOrderList)
                    {
                        CategoryCount += order.CategoryCount;
                        CountPrice += order.CountPrice;

                        foreach (var orderGoods in order.OrderGoods)
                        {
                            Count += orderGoods.Count;
                        }
                    }

                    ts.OrderCount = userOrderList.Count;

                    ts.CategoryCount = CategoryCount;
                    ts.Count = Count;
                    ts.CountPrice = CountPrice;

                    terminalStatisticList.Add(ts);
                }

                //ViewBag.Grid1RecordCount = userStatisticList.Count;
            }

            var grid1 = UIHelper.Grid("Grid1");

            grid1.RecordCount(terminalCount);

            grid1.DataSource(terminalStatisticList, fields);

            return UIHelper.Result();
        }


        public ActionResult TerminalAnalysisDetail(int id, DateTime startTime, DateTime endTime)
        {
            DateTime startTime1 = (DateTime)startTime;
            DateTime endTime1 = (DateTime)endTime;
            DateTime startToday, endToday;
            startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
            endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

            Terminal terminal = _terminalService.Find(p => p.ID == id);

            List<Order> orderList = _orderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday && p.TerminalID == terminal.ID, "", true).ToList();

            List<UserOrderGoods> userOrderGoodsList = new List<UserOrderGoods>();

            foreach (var order in orderList)
            {
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

            ViewBag.Grid1DataSource = retuserOrderGoodsList;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TerminalAnalysisDetail_Close(JArray fields)
        {
            return UIHelper.Result();
        }

        public class UserStatistic
        {
            public int UserID { get; set; }
            public string UserName { get; set; }
            public int OrderCount { get; set; }
            public int CategoryCount { get; set; }
            public int Count { get; set; }
            public double CountPrice { get; set; }

        }

        public class TerminalStatistic
        {
            public int TerminalID { get; set; }
            public string TerminalName { get; set; }
            public int OrderCount { get; set; }
            public int CategoryCount { get; set; }
            public int Count { get; set; }
            public double CountPrice { get; set; }

        }

        public class UserOrderGoods
        {
            public int GoodsID { get; set; }
            public string GoodsName { get; set; }
            public int Count { get; set; }
            public double CountPrice { get; set; }
        }

    }
}