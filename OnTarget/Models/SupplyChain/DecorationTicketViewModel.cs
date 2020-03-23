using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace OnTarget.Models.SupplyChain
{
    public class DecorationTicketViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter at Least One Work Order Number")]
        [Display(Name = "Enter Work Order Number(s)")]
        public string WorkOrders { set; get; }

        [Remote(action: "PrinterSelected", controller: "SupplyChain", ErrorMessage = "Please Select a Printer")]
        public int SelectedPrinterID { set; get; }

        [Display(Name = "Select a Printer")]
        public List<SelectListItem> LabelPrinters { set; get; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Select a Creation Date")]
        [Display(Name = "Select Completion Date")]
        public string CompletionDate { set; get; }
    }
}
