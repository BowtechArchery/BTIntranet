using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnTargetDataLibrary.Models.LabelPrinting
{
    public class LabelPrinterModel
    {
        public int PrinterID { get; set; }
        public string PrinterDescription { get; set; }
        public string PrinterName { get; set; }
    }
}
