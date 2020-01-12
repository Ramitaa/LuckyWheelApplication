using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LuckyDrawApplication.Models
{
    public class SimpleUser
    {
        [Display(Name = "Purchaser ID")]
        public int PurchaserID { get; set; }
    }
}