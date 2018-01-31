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
    public class Order
    {

        public Order()
        {
            OrderGoods = new HashSet<OrderGoods>();
        }

        [Key]
        public int ID { get; set; }

        public int? UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual DUser DUser { get; set; }

        public int? TerminalID { get; set; }

        [ForeignKey("TerminalID")]
        public virtual Terminal Terminal { get; set; }

        [Display(Name = "总价")]
        public double CountPrice { get; set; }

        [Display(Name = "种类数")]
        public int CategoryCount { get; set; }

        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

        public virtual ICollection<OrderGoods> OrderGoods { get; set; }
    }
}
