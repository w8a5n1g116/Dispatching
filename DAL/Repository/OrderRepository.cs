﻿using DAL.BaseRepository;
using DAL.IRepository;
using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(Model.Model _nConnext) : base(_nConnext)
        {
        }
    }
}
