using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetDataLibrary.Models.SupplyChain
{
    public class StandardBowPartColorsModel
    {
        public int ID { get; set; }

        public string ParentItemCode { get; set; }

        public string ChildItemCode { get; set; }

        public int PartOrder { get; set; }

        public string Color { get; set; }
    }
}
