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
    public class SaltOrderGoods
    {
        public SaltOrderGoods()
        {

        }

        [Key]
        public int ID { get; set; }

        public int? OrderID { get; set; }

        [ForeignKey("OrderID")]
        [JsonIgnore]
        public virtual SaltOrder SaltOrder { get; set; }

        public int? GoodsID { get; set; }

        [ForeignKey("GoodsID")]
        public virtual SaltGoods SaltGoods { get; set; }

        [Display(Name = "单项总价")]
        public double CountPrice { get; set; }

        [Display(Name = "单项数量")]
        public int Count { get; set; }

        [Display(Name = "单项返利")]
        public double Rebate { get; set; }

        [Display(Name = "单项收款")]
        public double Income { get; set; }

        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

    }
}
