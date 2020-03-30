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
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderSerive;
        private readonly IOrderGoodsService _orderGoodsService;
        private readonly IUserService _userSerivce;

        public OrderController()
        {
            _orderSerive = new OrderService(null);
            _orderGoodsService = new OrderGoodsService(null);
            _userSerivce = new UserService(null);
        }

        // GET: Order
        public ActionResult OrderList()
        {
            DateTime startTime1 = DateTime.Now;
            DateTime endTime1 = DateTime.Now;
            DateTime startToday, endToday;
            startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
            endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

            List<Order> orderList = _orderSerive.FindList(p => startToday < p.CreateTime && p.CreateTime < endToday, "", true).OrderByDescending(p => p.CreateTime).ToList();

            ViewBag.Grid1RecordCount = orderList.Count;

            ViewBag.Grid1DataSource = PagingHelper<Order>.GetPagedDataTable(0, 10, orderList.Count, orderList);

            DUser user = (DUser)Session["User"];

            if(user.Permission == 1)
            {
                return View("OrderListWithDelete", orderList);
            }
            else
            {
                return View(orderList);
            }          
        }

        [HttpPost]
        public ActionResult OrderList(DateTime startTime, DateTime endTime, string userName,string terminalName, JArray fields)
        {

            DateTime startTime1 = (DateTime)startTime;
            DateTime endTime1 = (DateTime)endTime;
            DateTime startToday, endToday;
            startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
            endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

            List<Order> orderList = new List<Order>();

            if (startTime != null && endTime != null)
            {
                orderList = _orderSerive.FindList(p => startToday < p.CreateTime && p.CreateTime < endToday, "", true).OrderByDescending(p => p.CreateTime).ToList();
            }
            else
            {
                orderList = _orderSerive.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();
            }

            if(!string.IsNullOrEmpty(userName)||!string.IsNullOrEmpty(terminalName))
            {
                orderList = orderList.Where(p => p.DUser.Name.Contains(userName)&&p.Terminal.Name.Contains(terminalName)).ToList();
            }

            var grid1 = UIHelper.Grid("Grid1");

            grid1.RecordCount(orderList.Count);

            grid1.DataSource(PagingHelper<Order>.GetPagedDataTable(0, 10, orderList.Count, orderList), fields);

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OrderList_PageIndexChanged(JArray Grid1_fields, int Grid1_pageIndex, DateTime startTime, DateTime endTime, string userName, string terminalName)
        {

            DateTime startTime1 = (DateTime)startTime;
            DateTime endTime1 = (DateTime)endTime;
            DateTime startToday, endToday;
            startToday = new DateTime(startTime1.Year, startTime1.Month, startTime1.Day, 0, 0, 0);
            endToday = new DateTime(endTime1.AddDays(1).Year, endTime1.AddDays(1).Month, endTime1.AddDays(1).Day, 0, 0, 0);

            List<Order> orderList = new List<Order>();

            if (startTime != null && endTime != null)
            {
                orderList = _orderSerive.FindList(p => startToday < p.CreateTime && p.CreateTime < endToday, "", true).OrderByDescending(p => p.CreateTime).ToList();
            }
            else
            {
                orderList = _orderSerive.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();
            }

            if (!string.IsNullOrEmpty(userName) || !string.IsNullOrEmpty(terminalName))
            {
                orderList = orderList.Where(p => p.DUser.Name.Contains(userName) && p.Terminal.Name.Contains(terminalName)).ToList();
            }

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = orderList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<Order>.GetPagedDataTable(Grid1_pageIndex, 10, orderList.Count, orderList);
            grid1.DataSource(dataSource, Grid1_fields);

            return UIHelper.Result();
        }

        public ActionResult OrderDetail(int id)
        {
            Order order = _orderSerive.Find(p => p.ID == id);

            List<OrderGoods> orderGoodsList = order.OrderGoods.ToList();

            ViewBag.Grid1RecordCount = orderGoodsList.Count;

            ViewBag.Grid1DataSource = PagingHelper<OrderGoods>.GetPagedDataTable(0, 10, orderGoodsList.Count, orderGoodsList);

            List<OrderWapper> orderWappersList = order.OrderWapper.ToList();

            ViewBag.Grid2RecordCount = orderWappersList.Count;

            ViewBag.Grid2DataSource = PagingHelper<OrderWapper>.GetPagedDataTable(0, 10, orderWappersList.Count, orderWappersList);

            ViewBag.ID = id;

            return View(orderGoodsList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OrderDetail_PageIndexChanged(JArray Grid1_fields, int Grid1_pageIndex, int id)
        {
            Order order = _orderSerive.Find(p => p.ID == id);

            List<OrderGoods> orderGoodsList = order.OrderGoods.ToList();

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = orderGoodsList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<OrderGoods>.GetPagedDataTable(Grid1_pageIndex, 10, orderGoodsList.Count, orderGoodsList);
            grid1.DataSource(dataSource, Grid1_fields);

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OrderDetail2_PageIndexChanged(JArray Grid2_fields, int Grid2_pageIndex, int id)
        {
            Order order = _orderSerive.Find(p => p.ID == id);

            List<OrderWapper> orderWapperList = order.OrderWapper.ToList();

            var grid2 = UIHelper.Grid("Grid2");

            var recordCount = orderWapperList.Count;

            grid2.RecordCount(recordCount);

            var dataSource = PagingHelper<OrderWapper>.GetPagedDataTable(Grid2_pageIndex, 10, orderWapperList.Count, orderWapperList);
            grid2.DataSource(dataSource, Grid2_fields);

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OrderDetail_Close(JArray fields)
        {
            List<Order> orderList = _orderSerive.FindList(p => true, "", true).OrderByDescending(p => p.CreateTime).ToList();

            var grid1 = UIHelper.Grid("Grid1");

            var recordCount = orderList.Count;

            grid1.RecordCount(recordCount);

            var dataSource = PagingHelper<Order>.GetPagedDataTable(0, 10, orderList.Count, orderList);
            grid1.DataSource(dataSource, fields);

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteOrder(int id)
        {
            try
            {


                Order order = _orderSerive.Find(p => p.ID == id);

                if (order != null)
                {
                    while(order.OrderGoods.Count >0)
                    {
                        _orderGoodsService.Delete(order.OrderGoods.ToList().FirstOrDefault());
                    }
                                            
                    _orderSerive.Delete(order);
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