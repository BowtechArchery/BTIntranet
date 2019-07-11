using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnTarget.Models.SupplyChain
{
    public class ExclusionItemModel
    {

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Item Code")]
        [Display(Name = "Item Code")]
        public string ItemCode { get; set; }

        [Display(Name = "Item Description")]
        public string ItemDesc { get; set; }
        
    }
}
