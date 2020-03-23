using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnTarget.Models.SupplyChain
{
    public class WorkOrderPickSheetViewModel
    {

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter at Least One Work Order Number")]
        [Display(Name = "Enter Work Order Number(s)")]
        public string WorkOrders { set; get; }

    }
}
