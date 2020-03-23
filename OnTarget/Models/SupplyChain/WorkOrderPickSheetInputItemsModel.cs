using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnTarget.Models.SupplyChain
{
    public class WorkOrderPickSheetInputItemsModel
    {
        public string WorkOrderNumber { get; set; }

        public string Step { get; set; }

        public string ItemCode { get; set; }

        public string ItemDescription { get; set; }

        public decimal Quantity { get; set; }

        public string Source { get; set; }

        public string UOM { get; set; }

        public string Warehouse { get; set; }

        public string Zone { get; set; }

        public decimal StockQuantity { get; set; }

        public string Short { get; set; }
    }
}
