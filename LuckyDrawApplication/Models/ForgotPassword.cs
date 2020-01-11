using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LuckyDrawApplication.Models
{
    public class ForgotPassword
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [MaxLength(100, ErrorMessage = "Maximum 100 characters")]
        [Display(Name = "Email Address")]
        public String EmailAddress { get; set; }

        [Display(Name = "Token")]
        public String Token { get; set; }
    }
}