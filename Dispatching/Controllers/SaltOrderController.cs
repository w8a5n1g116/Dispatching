using BLL.IService;
using BLL.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL.Model;
using FineUIMvc.EmptyProject.Tools;
using Newtonsoft.Json.Linq;

namespace FineUIMvc.EmptyProject.Controllers
{
    public class SaltOrderController : BaseController
    {
        private readonly ISaltOrderService _saltOrderSerive;
        private readonly ISaltOrderGoodsService _saltOrderGoodsService;
        private readonly IUserService _userSerivce;

        public SaltOrderController()
        {
            _saltOrderSerive = new SaltOrderService(null);
            _saltOrderGoodsService = new SaltOrderGoodsService(null);
            _userSerivce = new UserService(null);
        }

        // GET: Order
        public ActionResult SaltOrderList()
        {
            DateTime startTime1 = DateTime.Now;
            DateTime endTime1 = DateTime.Now;
            DateTime startToday, endToday;
            startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
            endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

            List<SaltOrder> orderList = _saltOrderSerive.FindList(p => startToday < p.CreateTime && p.CreateTime < endToday, "", true).OrderByDescending(p => p.CreateTime).ToList();

            ViewBag.Grid1RecordCount = orderList.Count;

            SaltOrder summaryOrder = new SaltOrder();

            foreach (var order in orderList)
            {
                summaryOrder.CountPrice += order.CountPrice;
                summaryOrder.CountIncome += order.CountIncome;
                summaryOrder.CountRebate += order.CountRebate;
            }

            ViewBag.CountPrice = summaryOrder.CountPrice;
            ViewBag.CountIncome = summaryOrder.CountIncome;
            ViewBag.CountRebate = summaryOrder.CountRebate;

            //JObject summary = new JObject();
            ////summary.Add("major", "全部合计");
            //summary.Add("CountPrice", summaryOrder.CountPrice);
            //summary.Add("CountIncome", summaryOrder.CountIncome);
            //summary.Add("CountRebate", summaryOrder.CountRebate);

            //ViewBag.SummaryData = summary;

            ViewBag.Grid1DataSource = PagingHelper<SaltOrder>.GetPagedDataTable(0, 10, orderList.Count, orderList);

            DUser user = (DUser)Session["User"];

            if(user.Permission == 3)
            {
                return View("SaltOrderListWithDelete");
            }
            else
            {
                return View();
            }          
        }

        [HttpPost]
        public ActionResult SaltOrderList(DateTime startTime, DateTime endTime, string userName,string terminalName,string orderStatus,string orderType,string payType, JArray fields)
        {

            DateTime startTime1 = (DateTime)startTime;
            DateTime endTime1 = (DateTime)endTime;
            DateTime startToday, endToday;
            startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
            endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

            List<SaltOrder> orderList = new List<SaltOrder>();

            if (startTime != null && endTime != null)
            {
                orderList = _saltOrderSerive.FindList(p => startToday < p.CreateTime && p.CreateTime < endToday, "", true).OrderByDescending(p => p.CreateTime).ToList();
            }
            else
            {
                orderList = _saltOrderSerive.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();
            }

            if(!string.IsNullOrEmpty(userName)||!string.IsNullOrEmpty(terminalName))
            {
                orderList = orderList.Where(p => p.DUser.Name.Contains(userName)&&p.SaltTerminal.Name.Contains(terminalName)).ToList();
            }

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

            SaltOrder summaryOrder = new SaltOrder();

            foreach (var order in orderList)
            {
                summaryOrder.CountPrice += order.CountPrice;
                summaryOrder.CountIncome += order.CountIncome;
                summaryOrder.CountRebate += order.CountRebate;
            }

            UIHelper.Label("CountPrice").Text(summaryOrder.CountPrice.ToString());
            UIHelper.Label("CountIncome").Text(summaryOrder.CountIncome.ToString());
            UIHelper.Label("CountRebate").Text(summaryOrder.CountRebate.ToString());

            var grid1 = UIHelper.Grid("Grid1");

            grid1.RecordCount(orderList.Count);

            grid1.DataSource(PagingHelper<SaltOrder>.GetPagedDataTable(0, 10, orderList.Count, orderList), fields);

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaltOrderList_PageIndexChanged(JArray Grid1_fields, int Grid1_pageIndex, DateTime startTime, DateTime endTime, string userName, string terminalName, string orderStatus, string orderType, string payType)
        {

            DateTime startTime1 = (DateTime)startTime;
            DateTime endTime1 = (DateTime)endTime;
            DateTime startToday, endToday;
            startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
            endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

            List<SaltOrder> orderList = new List<SaltOrder>();

            if (startTime != null && endTime != null)
            {
                orderList = _saltOrderSerive.FindList(p => startToday < p.CreateTime && p.CreateTime < endToday, "", true).OrderByDescending(p => p.CreateTime).ToList();
            }
            else
            {
                orderList = _saltOrderSerive.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();
            }

            if (!string.IsNullOrEmpty(userName) || !string.IsNullOrEmpty(terminalName))
            {
                orderList = orderList.Where(p => p.DUser.Name.Contains(userName) && p.SaltTerminal.Name.Contains(terminalName)).ToList();
            }

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

            SaltOrder summaryOrder = new SaltOrder();

            foreach (var order in orderList)
            {
                summaryOrder.CountPrice += order.CountPrice;
                summaryOrder.CountIncome += order.CountIncome;
                summaryOrder.CountRebate += order.CountRebate;
            }

            UIHelper.Label("CountPrice").Text(summaryOrder.CountPrice.ToString());
            UIHelper.Label("CountIncome").Text(summaryOrder.CountIncome.ToString());
            UIHelper.Label("CountRebate").Text(summaryOrder.CountRebate.ToString());

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = orderList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<SaltOrder>.GetPagedDataTable(Grid1_pageIndex, 10, orderList.Count, orderList);
            grid1.DataSource(dataSource, Grid1_fields);

            return UIHelper.Result();
        }

        public ActionResult SaltOrderDetail(int id)
        {
            SaltOrder order = _saltOrderSerive.Find(p => p.ID == id);

            List<SaltOrderGoods> orderGoodsList = order.SaltOrderGoods.ToList();

            ViewBag.Grid1RecordCount = orderGoodsList.Count;

            ViewBag.Grid1DataSource = PagingHelper<SaltOrderGoods>.GetPagedDataTable(0, 10, orderGoodsList.Count, orderGoodsList);

            ViewBag.ID = id;

            return View(orderGoodsList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaltOrderDetail_PageIndexChanged(JArray Grid1_fields, int Grid1_pageIndex, int id)
        {
            SaltOrder order = _saltOrderSerive.Find(p => p.ID == id);

            List<SaltOrderGoods> orderGoodsList = order.SaltOrderGoods.ToList();

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = orderGoodsList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<SaltOrderGoods>.GetPagedDataTable(Grid1_pageIndex, 10, orderGoodsList.Count, orderGoodsList);
            grid1.DataSource(dataSource, Grid1_fields);

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaltOrderDetail_Close(JArray fields)
        {
            List<SaltOrder> orderList = _saltOrderSerive.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = orderList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<SaltOrder>.GetPagedDataTable(0, 10, orderList.Count, orderList);
            grid1.DataSource(dataSource, fields);

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSaltOrder(int id)
        {
            try
            {


                SaltOrder order = _saltOrderSerive.Find(p => p.ID == id);

                if (order != null)
                {
                    while(order.SaltOrderGoods.Count >0)
                    {
                        _saltOrderGoodsService.Delete(order.SaltOrderGoods.ToList().FirstOrDefault());
                    }
                                            
                    _saltOrderSerive.Delete(order);
                }
            }
            catch (Exception e)
            {
                Alert.Show("无法删除!");
                return UIHelper.Result();
            }


            return UIHelper.Result();
        }
    }
}