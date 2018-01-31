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
    public class TerminalService : BaseService<Terminal>, ITerminalService
    {
        public TerminalService(Model context) : base(new TerminalRepository(context))
        {

        }
    }
}
