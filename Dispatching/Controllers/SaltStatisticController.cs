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
    public class SaltStatisticController : Controller
    {
        private readonly ISaltOrderService _saltOrderSerive;
        private readonly ISaltOrderGoodsService _saltOrderGoodsService;
        private readonly IUserService _userSerivce;
        private readonly ISaltTerminalService _saltTerminalService;

        public SaltStatisticController()
        {
            _saltOrderSerive = new SaltOrderService(null);
            _saltOrderGoodsService = new SaltOrderGoodsService(null);
            _userSerivce = new UserService(null);
            _saltTerminalService = new SaltTerminalService(null);
        }
        // GET: Statistic
        public ActionResult UserAnalysis()
        {
            DateTime today = DateTime.Now;
            DateTime startToday, endToday;
            startToday = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
            endToday = new DateTime(today.AddDays(1).Year, today.AddDays(1).Month, today.AddDays(1).Day, 0, 0, 0);

            List<DUser> userList = _userSerivce.FindList(p => p.Role == 1, "", true).ToList();

            List<SaltOrder> orderList = _saltOrderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday,"",true).ToList();

            List<UserStatistic> userStatisticList = new List<UserStatistic>();

            foreach(var user in userList)
            {
                UserStatistic us = new UserStatistic();

                us.UserID = user.ID;
                us.UserName = user.Name;

                int CategoryCount = 0, Count = 0;
                double CountPrice = 0, CountRebate = 0, CountIncome = 0;

                List<SaltOrder> userOrderList = orderList.Where(p => p.UserID == user.ID).ToList();

                foreach(var order in userOrderList)
                {
                    CategoryCount += order.CategoryCount;
                    CountPrice += order.CountPrice;
                    CountRebate += order.CountRebate;
                    CountIncome += order.CountIncome;

                    foreach (var orderGoods in order.SaltOrderGoods)
                    {
                        Count += orderGoods.Count;
                    }
                }

                us.OrderCount = userOrderList.Count;

                us.CategoryCount = CategoryCount;
                us.Count = Count;
                us.CountPrice = CountPrice;
                us.CountRebate = CountRebate;
                us.CountIncome = CountIncome;

                userStatisticList.Add(us);
            }

            //ViewBag.Grid1RecordCount = userStatisticList.Count;

            ViewBag.Grid1DataSource = userStatisticList;//PagingHelper<UserStatistic>.GetPagedDataTable(0, 10, userStatisticList.Count, userStatisticList);

            return View();
        }

        [HttpPost]
        public ActionResult UserAnalysis(DateTime startTime,DateTime endTime, string orderStatus, string orderType, string payType, JArray fields)
        {

            List<UserStatistic> userStatisticList = new List<UserStatistic>();

            if (startTime != null && endTime != null)
            {
                DateTime startTime1 = (DateTime)startTime;
                DateTime endTime1 = (DateTime)endTime;
                DateTime startToday, endToday;
                startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
                endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

                List<DUser> userList = _userSerivce.FindList(p => p.Role == 1, "", true).ToList();

                List<SaltOrder> orderList = _saltOrderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday, "", true).ToList();

                if (!string.IsNullOrEmpty(orderStatus))
                {
                    orderList = orderList.Where(p => p.OrderStatus == orderStatus).ToList();
                }

                if (!string.IsNullOrEmpty(orderType))
                {
                    orderList = orderList.Where(p => p.OrderType.Contains(orderType)).ToList();
                }

                if (!string.IsNullOrEmpty(payType))
                {
                    orderList = orderList.Where(p => p.PayType == payType).ToList();
                }

                foreach (var user in userList)
                {
                    UserStatistic us = new UserStatistic();

                    us.UserID = user.ID;
                    us.UserName = user.Name;

                    int CategoryCount = 0, Count = 0;
                    double CountPrice = 0,CountRebate =0,CountIncome =0;

                    List<SaltOrder> userOrderList = orderList.Where(p => p.UserID == user.ID).ToList();

                    foreach (var order in userOrderList)
                    {
                        CategoryCount += order.CategoryCount;
                        CountPrice += order.CountPrice;
                        CountRebate += order.CountRebate;
                        CountIncome += order.CountIncome;

                        foreach (var orderGoods in order.SaltOrderGoods)
                        {
                            Count += orderGoods.Count;
                        }
                    }

                    us.OrderCount = userOrderList.Count;

                    us.CategoryCount = CategoryCount;
                    us.Count = Count;
                    us.CountPrice = CountPrice;
                    us.CountRebate = CountRebate;
                    us.CountIncome = CountIncome;

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

            List<SaltOrder> orderList = _saltOrderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday && p.UserID == user.ID, "", true).ToList();

            List<UserOrderGoods> userOrderGoodsList = new List<UserOrderGoods>();

            foreach(var order in orderList)
            {
                foreach(var orderGoods in order.SaltOrderGoods)
                {
                    if(!userOrderGoodsList.Where(p =>p.GoodsID == orderGoods.ID).Any())
                    {
                        UserOrderGoods uog = new UserOrderGoods();

                        uog.GoodsID = orderGoods.SaltGoods.ID;
                        uog.GoodsName = orderGoods.SaltGoods.Name;
                        uog.Count = orderGoods.Count;
                        uog.CountPrice = orderGoods.CountPrice;
                        uog.CountRebate = orderGoods.Rebate;
                        uog.CountIncome = orderGoods.Income;

                        userOrderGoodsList.Add(uog);
                    }
                    else
                    {
                        userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).FirstOrDefault().Count += orderGoods.Count;
                        userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).FirstOrDefault().CountPrice += orderGoods.CountPrice;
                        userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).FirstOrDefault().CountRebate += orderGoods.Rebate;
                        userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).FirstOrDefault().CountIncome += orderGoods.Income;
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
                    uog.CountRebate = userOrderGoods.CountRebate;
                    uog.CountIncome = userOrderGoods.CountIncome;

                    retuserOrderGoodsList.Add(uog);
                }
                else
                {
                    retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).FirstOrDefault().Count += userOrderGoods.Count;
                    retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).FirstOrDefault().CountPrice += userOrderGoods.CountPrice;
                    retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).FirstOrDefault().CountRebate += userOrderGoods.CountRebate;
                    retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).FirstOrDefault().CountIncome += userOrderGoods.CountIncome;

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

            List<SaltTerminal> terminalList = _saltTerminalService.FindList(p => true, "", true).ToList();

            int terminalCount = terminalList.Count;

            terminalList = PagingHelper<SaltTerminal>.GetPagedDataTable(0, 10, terminalList.Count, terminalList);

            List<SaltOrder> orderList = _saltOrderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday, "", true).ToList();

            List<TerminalStatistic> terminalStatisticList = new List<TerminalStatistic>();

            foreach (var terminal in terminalList)
            {
                TerminalStatistic ts = new TerminalStatistic();

                ts.TerminalID = terminal.ID;
                ts.TerminalName = terminal.Name;

                int CategoryCount = 0, Count = 0;
                double CountPrice = 0, CountRebate = 0, CountIncome = 0;

                List<SaltOrder> userOrderList = orderList.Where(p => p.TerminalID == terminal.ID).ToList();

                foreach (var order in userOrderList)
                {
                    CategoryCount += order.CategoryCount;
                    CountPrice += order.CountPrice;
                    CountRebate += order.CountRebate;
                    CountIncome += order.CountIncome;

                    foreach (var orderGoods in order.SaltOrderGoods)
                    {
                        Count += orderGoods.Count;
                    }
                }

                ts.OrderCount = userOrderList.Count;

                ts.CategoryCount = CategoryCount;
                ts.Count = Count;
                ts.CountPrice = CountPrice;
                ts.CountRebate = CountRebate;
                ts.CountIncome = CountIncome;
                ts.DUser = terminal.DUser;

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

                List<SaltTerminal> terminalList;

                if (!string.IsNullOrEmpty(terminalName))
                {
                    terminalList = _saltTerminalService.FindList(p => p.Name.Contains(terminalName), "", true).ToList();
                }
                else
                {
                    terminalList = _saltTerminalService.FindList(p => true, "", true).ToList();
                }

                //int terminalCount = terminalList.Count;

                terminalList = PagingHelper<SaltTerminal>.GetPagedDataTable(Grid1_pageIndex, 10, terminalList.Count, terminalList);

                List<SaltOrder> orderList = _saltOrderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday, "", true).ToList();

                foreach (var terminal in terminalList)
                {
                    TerminalStatistic ts = new TerminalStatistic();

                    ts.TerminalID = terminal.ID;
                    ts.TerminalName = terminal.Name;

                    int CategoryCount = 0, Count = 0;
                    double CountPrice = 0, CountRebate = 0, CountIncome = 0;

                    List<SaltOrder> userOrderList = orderList.Where(p => p.TerminalID == terminal.ID).ToList();

                    foreach (var order in userOrderList)
                    {
                        CategoryCount += order.CategoryCount;
                        CountPrice += order.CountPrice;
                        CountRebate += order.CountRebate;
                        CountIncome += order.CountIncome;

                        foreach (var orderGoods in order.SaltOrderGoods)
                        {
                            Count += orderGoods.Count;
                        }
                    }

                    ts.OrderCount = userOrderList.Count;

                    ts.CategoryCount = CategoryCount;
                    ts.Count = Count;
                    ts.CountPrice = CountPrice;
                    ts.CountRebate = CountRebate;
                    ts.CountIncome = CountIncome;
                    ts.DUser = terminal.DUser;

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

                List<SaltTerminal> terminalList;
                
                if(!string.IsNullOrEmpty(terminalName))
                {
                    terminalList = _saltTerminalService.FindList(p => p.Name.Contains(terminalName), "", true).ToList();
                }
                else
                {
                    terminalList = _saltTerminalService.FindList(p => true, "", true).ToList();
                }
                

                terminalCount = terminalList.Count;

                terminalList = PagingHelper<SaltTerminal>.GetPagedDataTable(0, 10, terminalList.Count, terminalList);

                List<SaltOrder> orderList = _saltOrderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday, "", true).ToList();

                foreach (var terminal in terminalList)
                {
                    TerminalStatistic ts = new TerminalStatistic();

                    ts.TerminalID = terminal.ID;
                    ts.TerminalName = terminal.Name;

                    int CategoryCount = 0, Count = 0;
                    double CountPrice = 0, CountRebate = 0, CountIncome = 0;

                    List<SaltOrder> userOrderList = orderList.Where(p => p.TerminalID == terminal.ID).ToList();

                    foreach (var order in userOrderList)
                    {
                        CategoryCount += order.CategoryCount;
                        CountPrice += order.CountPrice;
                        CountRebate += order.CountRebate;
                        CountIncome += order.CountIncome;

                        foreach (var orderGoods in order.SaltOrderGoods)
                        {
                            Count += orderGoods.Count;
                        }
                    }

                    ts.OrderCount = userOrderList.Count;

                    ts.CategoryCount = CategoryCount;
                    ts.Count = Count;
                    ts.CountPrice = CountPrice;
                    ts.CountRebate = CountRebate;
                    ts.CountIncome = CountIncome;
                    ts.DUser = terminal.DUser;

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

            SaltTerminal terminal = _saltTerminalService.Find(p => p.ID == id);

            List<SaltOrder> orderList = _saltOrderSerive.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday && p.TerminalID == terminal.ID, "", true).ToList();

            List<UserOrderGoods> userOrderGoodsList = new List<UserOrderGoods>();

            foreach (var order in orderList)
            {
                foreach (var orderGoods in order.SaltOrderGoods)
                {
                    if (!userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).Any())
                    {
                        UserOrderGoods uog = new UserOrderGoods();

                        uog.GoodsID = orderGoods.SaltGoods.ID;
                        uog.GoodsName = orderGoods.SaltGoods.Name;
                        uog.Count = orderGoods.Count;
                        uog.CountPrice = orderGoods.CountPrice;
                        uog.CountRebate = orderGoods.Rebate;
                        uog.CountIncome = orderGoods.Income;

                        userOrderGoodsList.Add(uog);
                    }
                    else
                    {
                        userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).FirstOrDefault().Count += orderGoods.Count;
                        userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).FirstOrDefault().CountPrice += orderGoods.CountPrice;
                        userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).FirstOrDefault().CountRebate += orderGoods.Rebate;
                        userOrderGoodsList.Where(p => p.GoodsID == orderGoods.ID).FirstOrDefault().CountIncome += orderGoods.Income;
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
                    uog.CountRebate = userOrderGoods.CountRebate;
                    uog.CountIncome = userOrderGoods.CountIncome;

                    retuserOrderGoodsList.Add(uog);
                }
                else
                {
                    retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).FirstOrDefault().Count += userOrderGoods.Count;
                    retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).FirstOrDefault().CountPrice += userOrderGoods.CountPrice;
                    retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).FirstOrDefault().CountRebate += userOrderGoods.CountRebate;
                    retuserOrderGoodsList.Where(p => p.GoodsID == userOrderGoods.GoodsID).FirstOrDefault().CountIncome += userOrderGoods.CountIncome;
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

        public ActionResult UserTerminalAnalysis()
        {
            DateTime today = DateTime.Now;
            DateTime startToday, endToday;
            startToday = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
            endToday = new DateTime(today.AddDays(1).Year, today.AddDays(1).Month, today.AddDays(1).Day, 0, 0, 0);

            List<DUser> userList = _userSerivce.FindList(p => p.Role == 1 && p.Permission == 0, "", true).ToList();

            int userCount = userList.Count;

            List<UserTerminalStatistic> userTerminalStatisticList = new List<UserTerminalStatistic>();

            foreach (var user in userList)
            {
                UserTerminalStatistic uts = new UserTerminalStatistic();

                uts.UserID = user.ID;
                uts.UserName = user.Name;

                List<SaltTerminal> terminalList = _saltTerminalService.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday && p.DUser.ID == user.ID,"",true).ToList();

                uts.TermimalCount = terminalList.Count;

                userTerminalStatisticList.Add(uts);
            }

            ViewBag.Grid1RecordCount = userCount;

            ViewBag.Grid1DataSource = userTerminalStatisticList;//PagingHelper<TerminalStatistic>.GetPagedDataTable(0, 10, terminalStatisticList.Count, terminalStatisticList);

            return View();
        }

        [HttpPost]
        public ActionResult UserTerminalAnalysis(DateTime startTime, DateTime endTime, string userName, JArray fields)
        {

            List<DUser> userList = _userSerivce.FindList(p => p.Role == 1 && p.Permission == 0, "", true).ToList();

            int userCount = userList.Count;

            List<UserTerminalStatistic> userTerminalStatisticList = new List<UserTerminalStatistic>();

            foreach (var user in userList)
            {
                UserTerminalStatistic uts = new UserTerminalStatistic();

                uts.UserID = user.ID;
                uts.UserName = user.Name;

                List<SaltTerminal> terminalList = _saltTerminalService.FindList(p => p.CreateTime < endTime && p.CreateTime > startTime && p.DUser.ID == user.ID, "", true).ToList();

                uts.TermimalCount = terminalList.Count;

                if (!string.IsNullOrEmpty(userName))
                {
                    if (user.Name.Contains(userName))
                    {
                        userTerminalStatisticList.Add(uts);
                    }
                }
                else
                {
                    userTerminalStatisticList.Add(uts);
                }
                
            }

            

            var grid1 = UIHelper.Grid("Grid1");

            grid1.RecordCount(userCount);

            grid1.DataSource(userTerminalStatisticList, fields);

            return UIHelper.Result();
        }

        public ActionResult UserTerminalAnalysisDetail(int id, DateTime startTime, DateTime endTime)
        {
            DateTime startTime1 = (DateTime)startTime;
            DateTime endTime1 = (DateTime)endTime;
            DateTime startToday, endToday;
            startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
            endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

            DUser user = _userSerivce.Find(p => p.ID == id);

            List<SaltTerminal> terminalList = _saltTerminalService.FindList(p => p.CreateTime < endToday && p.CreateTime > startToday && p.DUser.ID == user.ID, "", true).ToList();

            ViewBag.Grid1DataSource = terminalList;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserTerminalAnalysisDetail_Close(JArray fields)
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
            public double CountRebate { get; set; }
            public double CountIncome { get; set; }

        }

        public class TerminalStatistic
        {
            public int TerminalID { get; set; }
            public string TerminalName { get; set; }
            public int OrderCount { get; set; }
            public int CategoryCount { get; set; }
            public int Count { get; set; }
            public double CountPrice { get; set; }
            public double CountRebate { get; set; }
            public double CountIncome { get; set; }
            public DUser DUser { get; set; }

        }

        public class UserOrderGoods
        {
            public int GoodsID { get; set; }
            public string GoodsName { get; set; }
            public int Count { get; set; }
            public double CountPrice { get; set; }
            public double CountRebate { get; set; }
            public double CountIncome { get; set; }
        }

        public class UserTerminalStatistic
        {
            public int UserID { get; set; }
            public string UserName { get; set; }
            public int TermimalCount { get; set; }
        }
    }
}