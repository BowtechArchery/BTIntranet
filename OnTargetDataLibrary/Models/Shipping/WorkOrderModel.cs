using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetDataLibrary.Models.Shipping
{
    public class WorkOrderModel
    {
        public string WorkOrderNumber { get; set; }

        public string ItemCode { get; set; }

        public string ItemDescription { get; set; }

        public int Quantity { get; set; }

        public string SalesOrderNumber { get; set; }

    }
}
