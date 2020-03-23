using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetDataLibrary.Models.Manufacturing
{
    public class BowCaseLabelWorkOrderModel
    {

        public int ID { get; set; }

        public string WorkOrderNumber { get; set; }

        public bool CustomBow { get; set; }

        public string CustomDescription { get; set; }

        public int CurrentCaseNumber { get; set; }

        public bool Complete { get; set; }
    }
}
