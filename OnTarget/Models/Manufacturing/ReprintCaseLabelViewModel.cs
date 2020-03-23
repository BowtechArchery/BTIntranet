using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnTarget.Models.Manufacturing
{
    public class ReprintCaseLabelViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please enter valid Number")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Case Number")]
        [Display(Name = "Case Number")]
        public int CaseNumber { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Serial Number")]
        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; }

        [Display(Name = "Printer")]
        public int SelectedPrinterID { get; set; }

        [Display(Name = "Select a Printer")]
        public List<SelectListItem> LabelPrinters { get; set; }

    }
}
