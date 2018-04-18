using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class SaltOrder
    {

        public SaltOrder()
        {
            SaltOrderGoods = new HashSet<SaltOrderGoods>();
        }

        [Key]
        public int ID { get; set; }

        public int? UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual DUser DUser { get; set; }

        public int? TerminalID { get; set; }

        [ForeignKey("TerminalID")]
        public virtual SaltTerminal SaltTerminal { get; set; }

        [Display(Name = "总价")]
        public double CountPrice { get; set; }

        [Display(Name = "总返利")]
        public double CountRebate { get; set; }

        [Display(Name = "总收款")]
        public double CountIncome { get; set; }

        [Display(Name = "种类数")]
        public int CategoryCount { get; set; }

        [Display(Name = "订单状态")]
        public string OrderStatus { get; set; }

        [Display(Name = "订单类型")]
        public string OrderType { get; set; }

        [Display(Name = "支付类型")]
        public string PayType { get; set; }

        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

        public virtual ICollection<SaltOrderGoods> SaltOrderGoods { get; set; }
    }
}
