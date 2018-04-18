using DAL.BaseRepository;
using DAL.IRepository;
using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class SaltGoodsRepository : BaseRepository<SaltGoods>, ISaltGoodsRepository
    {
        public SaltGoodsRepository(Model.Model _nConnext) : base(_nConnext)
        {
        }
    }
}
