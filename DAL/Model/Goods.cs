using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class Goods
    {
        public Goods()
        {

        }

        [Key]
        public int ID { get; set; }

        [Display(Name = "名称")]
        [StringLength(50)]
        public string Name { get; set; }

        [Display(Name = "规格")]
        [StringLength(100)]
        public string Spec { get; set; }

        [Display(Name = "描述")]
        [StringLength(200)]
        public string Description { get; set; }

        [Display(Name = "价格")]
        public double Price { get; set; }

        [Display(Name = "图片")]
        [StringLength(500)]
        public string Picture { get; set; }

        [Display(Name = "兑换积分")]
        public int Point { get; set; }

        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

    }
}
