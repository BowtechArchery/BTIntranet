using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnTarget.Models.Manufacturing
{
    public class BowCaseSelectWorkOrderViewModel
    {
       
            [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Work Order")]
            [Display(Name = "Work Order Number")]
            public string WorkOrderNumber { get; set; }

            [Display(Name = "Printer")]
            public int SelectedPrinterID { get; set; }

            [Display(Name = "Select a Printer")]
            public List<SelectListItem> LabelPrinters { get; set; }
    }
}
