using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnTarget.Models.Sales
{
    public class CustomerCoordinatesModel
    {
        public string Site { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Customer Number")]
        [Display(Name = "Customer Number")]
        public string CustNum { get; set; }

        [Display(Name = "Customer Name")]
        public string CustName { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public string Country { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }
    }
}
