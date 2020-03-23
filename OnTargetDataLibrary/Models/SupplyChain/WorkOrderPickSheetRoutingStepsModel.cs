using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetDataLibrary.Models.SupplyChain
{
    public class WorkOrderPickSheetRoutingStepsModel
    {
        public string WorkOrderNumber { get; set; }

        public string Step { get; set; }

        public string Costed { get; set; }

        public string ResourceNumber { get; set; }

        public string ActivityNumber { get; set; }

        public string ActivityDescription { get; set; }

        public decimal MinutePerActivity { get; set; }

        public decimal TotalMinutes { get; set; }

        public decimal TotalHours { get; set; }

        public string AcceptedQuantity { get; set; }

        public string RejectedQuantity { get; set; }

    }
}
