using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace OnTarget.Models.Manufacturing
{
    public class BowCaseLabelWorkOrderModel
    {

        public int ID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Work Order")]
        [Display(Name = "Work Order Number")]
        public string WorkOrderNumber { get; set; }

        [Required(ErrorMessage = "Please Select if the Work Order is a Custom Bow")]
        [Display(Name = "Custom Bow (Y/N)")]
        public bool CustomBow { get; set; }

        [Display(Name = "Custom Description")]
        public string CustomDescription { get; set; }

        public int CurrentCaseNumber { get; set; }

        [Display(Name = "Complete (Y/N)")]
        public bool Complete { get; set; }

    }
}
