using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnTarget.Models.Warranty
{
    public class WarrantyLabelsViewModel
    {
      
        [Remote(action: "LabelClassSelected", controller: "Warranty", ErrorMessage = "Please Select a Label Class")]
        [Display(Name = "Label Class")]
        public int SelectedClassID { get; set; }

        [Display(Name = "Select a Label Class")]
        public List<SelectListItem> LabelClasses { get; set; }

        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Part Number")]
        [Display(Name = "Part Number")]
        public string PartNumber { get; set; }

        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a RA Number")]
        [Display(Name = "RA #")]
        public string RANumber { get; set; }

        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a RTV")]
        [Display(Name = "RTV")]
        public string RTV { get; set; }

        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Doc ID")]
        [Display(Name = "Doc ID")]
        public string DocID { get; set; }

        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Bow Model")]
        [Display(Name = "Bow Model")]
        public string BowModel { get; set; }

        //[Remote(action: "LabelBowHandSelected", controller: "Warranty", ErrorMessage = "Please Select a Bow Hand")]
        [Display(Name = "Bow Hand")]
        public string SelectedBowHand { get; set; }

        [Display(Name = "Select a Bow Hand")]
        public List<SelectListItem> BowHands { get; set; }

        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Draw Weight")]
        [Display(Name = "Draw Weight")]
        public string DrawWeight { get; set; }

        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Riser Color")]
        [Display(Name = "Riser Color")]
        public string RiserColor { get; set; }

        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Limb Color")]
        [Display(Name = "Limb Color")]
        public string LimbColor { get; set; }

        [Display(Name = "Serial Number")]
        public string SerialNumber{ get; set; }

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
