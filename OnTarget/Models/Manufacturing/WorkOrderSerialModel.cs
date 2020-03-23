using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnTarget.Models.Manufacturing
{
    public class WorkOrderSerialModel
    {
        public string ID { get; set; }

        [Required(ErrorMessage = "Please Enter a Serial Number")]
        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "Please Enter a Work Order")]
        [Display(Name = "Work Order Number")]
        public string WorkOrderNumber { get; set; }

        [Required(ErrorMessage = "Please Enter a Build Date")]
        [Display(Name = "Build Date")]
        public string BuildDate { get; set; }

        [Display(Name = "Item Code")]
        public string ItemCode { get; set; }

        [Display(Name = "Item Description")]
        public string ItemDescription { get; set; }

        [Required(ErrorMessage = "Please Enter a Builder Name")]
        [Display(Name = "Builder Name")]
        public string BuilderName { get; set; }
    }
}
