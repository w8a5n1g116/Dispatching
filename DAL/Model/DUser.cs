using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class DUser
    {
        public DUser()
        {
            Terminal = new HashSet<Terminal>();
            SaltTerminal = new HashSet<SaltTerminal>();
        }

        [Key]
        public int ID { get; set; }

        [Display(Name = "姓名")]
        [StringLength(50)]
        public string Name { get; set; }

        [Display(Name = "密码")]
        [StringLength(50)]
        public string Password { get; set; }

        [Display(Name = "电话")]
        [StringLength(50)]
        public string Phone { get; set; }

        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

        [Display(Name = "权限")]
        public int Permission { get; set; }

        [Display(Name = "角色")]
        public int Role { get; set; }

        [JsonIgnore]
        public virtual ICollection<Terminal> Terminal { get; set; }

        [JsonIgnore]
        public virtual ICollection<SaltTerminal> SaltTerminal { get; set; }

    }
}
