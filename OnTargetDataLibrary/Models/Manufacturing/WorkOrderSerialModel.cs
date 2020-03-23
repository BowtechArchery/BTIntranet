using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetDataLibrary.Models.Manufacturing
{
    public class WorkOrderSerialModel
    {
        public string ID { get; set; }
        public string SerialNumber { get; set; }

        public string WorkOrderNumber { get; set; }

        public string BuildDate { get; set; }

        public string ItemCode { get; set; }

        public string ItemDescription { get; set; }

        public string BuilderName { get; set; }

    }
}
