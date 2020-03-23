using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnTarget.Models.SupplyChain
{
    public class WorkOrderModel
    {
 
        public string WorkOrderNumber { get; set; }

        public string ItemCode { get; set; }

        public string ItemDescription { get; set; }

        public int Quantity { get; set; }

        public string SalesOrderNumber { get; set; }

        public string IssuedBy { get; set; }

        public string ItemCategory { get; set; }
    }
}
