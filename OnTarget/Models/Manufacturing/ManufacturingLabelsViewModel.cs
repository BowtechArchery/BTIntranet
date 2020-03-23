using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace OnTarget.Models.Manufacturing
{
    public class ManufacturingLabelsViewModel
    {
        [Remote(action: "LabelClassSelected", controller: "Manufacturing", ErrorMessage = "Please Select a Label Class")]
        [Display(Name = "Label Class")]
        public int SelectedClassID { get; set; }

        [Display(Name = "Select a Label Class")]
        public List<SelectListItem> LabelClasses { get; set; }

        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Part Number")]
        [Display(Name = "Part Number")]
        public string PartNumber { get; set; }

        [Display(Name = "Work Order Number")]
        public string WorkOrderNumber { get; set; }

        //[Range(0, int.MaxValue, ErrorMessage = "Please enter valid Quantity")]
        //[Required(ErrorMessage = "Please Enter Quantity")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Label Text")]
        public string LabelText { get; set; }

        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Please enter valid Number")]
        [Required(ErrorMessage = "Please Enter Number of Copies")]
        [Display(Name = "Number of Copies")]
        public int NumberOfCopies { get; set; }

        //[Remote(action: "PrinterSelected", controller: "Warranty", ErrorMessage = "Please Select a Printer")]
        //[Required(ErrorMessage = "Please Select a Printer")]
        [Display(Name = "Printer")]
        public int SelectedPrinterID { get; set; }

        [Display(Name = "Select a Printer")]
        public List<SelectListItem> LabelPrinters { get; set; }
    }
}
