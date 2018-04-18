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
    public class SaltTerminalService : BaseService<SaltTerminal>, ISaltTerminalService
    {
        public SaltTerminalService(Model context) : base(new SaltTerminalRepository(context))
        {

        }
    }
}
