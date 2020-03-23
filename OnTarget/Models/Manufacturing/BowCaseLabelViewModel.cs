using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnTarget.Models.Manufacturing
{
    public class BowCaseLabelViewModel
    {
        public string WorkOrderNumber { get; set; }

        public string PartNumber { get; set; }

        public string PartDescription { get; set; }

        public int Quantity { get; set; }

        public int CurrentCaseNumber { get; set; }

        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; }

        [Display(Name = "Bow Builder")]
        public string BowBuilder { get; set; }

        [Display(Name = "Printer")]
        public int SelectedPrinterID { get; set; }

        public string PrinterName { get; set; }

        public string  PrinterDescription { get; set; }
    }
}
