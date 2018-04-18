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
    public class SaltTerminal
    {
        public SaltTerminal()
        {
            SaltOrder = new HashSet<SaltOrder>();
        }

        [Key]
        public int ID { get; set; }

        [Display(Name = "名称")]
        [StringLength(50)]
        public string Name { get; set; }

        [Display(Name = "联系人")]
        [StringLength(50)]
        public string Contact { get; set; }

        [Display(Name = "电话")]
        [StringLength(50)]
        public string Phone { get; set; }

        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

        [Display(Name = "积分")]
        public int Point { get; set; }

        [Display(Name = "地址")]
        [StringLength(50)]
        public string Address { get; set; }

        [Display(Name = "微信二维码地址")]
        [StringLength(200)]
        public string WXQCCodeAddress { get; set; }

        public int? SaltUserID { get; set; }

        [JsonIgnore]
        [ForeignKey("SaltUserID")]
        public virtual DUser DUser { get; set; }

        [JsonIgnore]
        public virtual ICollection<SaltOrder> SaltOrder { get; set; }
    }
}
