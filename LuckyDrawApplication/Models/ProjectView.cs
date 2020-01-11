using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LuckyDrawApplication.Models
{
    public class ProjectView
    {
        [Display(Name = "Project ID")]
        public int ProjectID { get; set; }

        [Required]
        [Display(Name = "Project Name")]
        [MaxLength(30, ErrorMessage = "Maximum 30 characters")]
        public string ProjectName { get; set; }

        [Required]
        [Display(Name = "Event ID")]
        public int EventID { get; set; }

        [Display(Name = "Units Sold")]
        [CustomValidationNo]
        public int NoOfProject { get; set; }

        [Required]
        [Display(Name = "Prize Category")]
        [MaxLength(100, ErrorMessage = "Maximum of 100 characters")]
        public string PrizeCategory { get; set; }

        [Display(Name = "Event Name")]
        public string EventName { get; set; }

        [Required]
        [Display(Name = "Prize Data")]
        public HttpPostedFileBase PrizeData { get; set; }
    }
}