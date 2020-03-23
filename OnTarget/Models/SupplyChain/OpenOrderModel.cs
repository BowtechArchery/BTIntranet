using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnTarget.Models.SupplyChain
{
    public class OpenOrderModel
    {
        [Display(Name = "Sales Order Number")]
        public string SalesOrderNumber { get; set; }

        [Display(Name = "Sales Order Line Number")]
        public string SalesOrderLineNumber { get; set; }

        [Display(Name = "Item Code")]
        public string ItemCode { get; set; }

        [Display(Name = "Item Description")]
        public string ItemDescription { get; set; }

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Display(Name = "Customer Number")]
        public string CustomerNumber { get; set; }

        [Display(Name = "Dealer Status")]
        public string DealerStatus { get; set; }

        [Display(Name = "Order Status")]
        public string OrderStatus { get; set; }

        [Display(Name = "Allocated Quantity")]
        public double AllocatedQuantity { get; set; }

        [Display(Name = "Priority")]
        public int Priority { get; set; }

        [Display(Name = "Open Quantity")]
        public double OpenQuantity { get; set; }

        [Display(Name = "Order Date")]
        public string OrderDate { get; set; }

        [Display(Name = "Required Date")]
        public string RequiredDate { get; set; }

        [Display(Name = "Promise Date")]
        public string PromiseDate { get; set; }

        [Display(Name = "Modified Date")]
        public string ModifiedDate { get; set; }

        [Display(Name = "DNS Before")]
        public string DNSBefore { get; set; }

        [Display(Name = "Comments")]
        public string Commments { get; set; }

        [Display(Name = "Work Order Number")]
        public string WorkOrderNumber { get; set; }

        [Display(Name = "Custom Bow Model")]
        public string CustomBowModel { get; set; }

        [Display(Name = "Custom Bow Hand")]
        public string CustomBowHand { get; set; }

        [Display(Name = "Custom Bow Draw Weight")]
        public string CustomBowDrawWeight { get; set; }

        [Display(Name = "Custom Bow Riser")]
        public string CustomBowRiserColor { get; set; }

        [Display(Name = "Custom Bow Limb Color")]
        public string CustomBowLimbColor { get; set; }

        [Display(Name = "Custom Bow Grip")]
        public string CustomBowGrip { get; set; }

        [Display(Name = "Custom Bow Orbit")]
        public string CustomBowOrbit { get; set; }

    }
}
