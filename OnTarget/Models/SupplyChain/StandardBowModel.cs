using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnTarget.Models.SupplyChain
{
    public class StandardBowModel
    {
        public string ParentItemCode { get; set; }

        public string ChildItemCode { get; set; }

        public int PartOrder { get; set; }

        public string Color { get; set; }
    }
}
