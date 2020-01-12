using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LuckyDrawApplication.Models
{
    public class EventView
    {
        [Display(Name = "Event ID")]
        public int EventID { get; set; }

        [Required]
        [CustomValidationNo]
        [Display(Name = "Event Code")]
        [MaxLength(5, ErrorMessage = "Maximum 5 characters")]
        public string EventCode { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Minumum 8 characters")]
        [MaxLength(20, ErrorMessage = "Maximum 20 characters")]
        [Display(Name = "Event Password")]
        public string EventPassword { get; set; }

        [Required]
        [Display(Name = "Event Location")]
        [MaxLength(50, ErrorMessage = "Maximum 50 characters")]
        public string EventLocation { get; set; }

        [Required]
        [Display(Name = "Staff Prize Data")]
        public HttpPostedFileBase StaffPrizeData { get; set; }
    }
}