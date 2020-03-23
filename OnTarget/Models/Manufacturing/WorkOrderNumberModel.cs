using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnTarget.Models.Manufacturing
{
    public class WorkOrderNumberModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Work Order")]
        [Display(Name = "Work Order Number")]
        public string WorkOrderNumber { get; set; }
    }
}
