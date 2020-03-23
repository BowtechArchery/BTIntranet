using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnTarget.Models.Warranty
{
    public class PartNumbersModel
    {

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Part Number")]
        [Display(Name = "Part Number")]
        public string ItemCode { get; set; }

    }
}