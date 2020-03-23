using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnTarget.Models.SupplyChain
{
    public class CustomBowModel
    {
        public string BowModel { get; set; }

        public string BowHand { get; set; }

        public string BowDrawWeight { get; set; }

        public string BowRiserColor { get; set; }

        public string BowLimbColor { get; set; }

        public string BowGrip { get; set; }

        public string BowOrbit  { get; set; }
    }
}
