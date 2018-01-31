using BLL.BaseService;
using BLL.IService;
using DAL.Model;
using DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class OrderService : BaseService<Order>, IOrderService
    {
        public OrderService(Model context) : base(new OrderRepository(context))
        {

        }
    }
}
