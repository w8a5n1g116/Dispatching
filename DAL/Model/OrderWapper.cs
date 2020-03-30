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
    public class OrderWapper
    {
        public OrderWapper()
        {

        }

        [Key]
        public int ID { get; set; }

        public int? OrderID { get; set; }

        [ForeignKey("OrderID")]
        [JsonIgnore]
        public virtual Order Order { get; set; }

        public int? WapperID { get; set; }

        [ForeignKey("WapperID")]
        public virtual Wapper Wapper { get; set; }

        [Display(Name = "单项总价")]
        public double CountPrice { get; set; }

        [Display(Name = "单项数量")]
        public int Count { get; set; }

        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

    }
}
