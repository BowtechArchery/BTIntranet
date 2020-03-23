using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetDataLibrary.Models.Shipping
{
    public class LabelDataModel
    {
        public string WorkOrderNumber{ get; set; }

        public string PartDescription { get; set; }

        public string PartNumber { get; set; }

        public int Quantity { get; set; }

        public string SalesOrderNumber { get; set; }

        public string UPC { get; set; }

        public string Warehouse { get; set; }

        public string Zone { get; set; }

        public string UOM { get; set; }
    }
}
