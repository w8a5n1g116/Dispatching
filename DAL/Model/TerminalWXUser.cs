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
    public class TerminalWXUser
    {
        public TerminalWXUser()
        {

        }

        [Key]
        public int ID { get; set; }

        [Display(Name = "微信用户OpenID")]
        [StringLength(50)]
        public string OpenID { get; set; }

        public int? TerminalID { get; set; }

        [ForeignKey("TerminalID")]
        public virtual Terminal Terminal { get; set; }
    }
}
