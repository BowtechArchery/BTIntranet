using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
namespace OnTarget.Models.Sales
{
    public class CustomBowFieldModel
    {

        [Display(Name = "Index Key")]
        public string IndexKey { get; set; }

        [Display(Name = "Order Number")]
        public string PepperiOrderNum { get; set; }
       
        [Display(Name = "Item Code")]
        public string ItemCode { get; set; }
       
        [Display(Name = "Item Description")]
        public string ItemDesc { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Bow Model")]
        [Display(Name = "Bow Model")]
        public string CustomBowModel { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Bow Hand")]
        [Display(Name = "Hand")]
        public string CustomBowHand { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Draw Weight")]
        [Display(Name = "Draw Weight")]
        public string CustomBowDrawWeight { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Riser Color")]
        [Display(Name = "Riser Color")]
        public string CustomBowRiserColor { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Limb Color")]
        [Display(Name = "Limb Color")]
        public string CustomBowLimbColor { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a Grip")]
        [Display(Name = "Grip")]
        public string CustomBowGrip { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter an Orbit")]
        [Display(Name = "Orbit")]
        public string CustomBowOrbit { get; set; }
    }
}
