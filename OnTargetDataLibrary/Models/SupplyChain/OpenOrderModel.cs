using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetDataLibrary.Models.SupplyChain
{
        public class OpenOrderModel
        {
       
        public string SalesOrderNumber { get; set; }
       
        public string SalesOrderLineNumber { get; set; }

        public string ItemCode { get; set; }
      
        public string ItemDescription { get; set; }
     
        public string CustomerName { get; set; }

        public string CustomerNumber { get; set; }

        public string DealerStatus { get; set; }

        public string OrderStatus { get; set; }

        public double AllocatedQuantity { get; set; }

        public int Priority { get; set; }
 
        public double OpenQuantity { get; set; }
   
        public string OrderDate { get; set; }
     
        public string RequiredDate { get; set; }
    
        public string PromiseDate { get; set; }

        public string ModifiedDate { get; set; }
  
        public string DNSBefore { get; set; }

        public string Commments { get; set; }
 
        public string WorkOrderNumber { get; set; }

        public string CustomBowModel { get; set; }
  
        public string CustomBowHand { get; set; }
 
        public string CustomBowDrawWeight { get; set; }

        public string CustomBowRiserColor { get; set; }

        public string CustomBowLimbColor { get; set; }
   
        public string CustomBowGrip { get; set; }

        public string CustomBowOrbit { get; set; }

    }
}
