using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Domain.Model.AI
{
    public class ApplyDiscountCommand : AiCommand
    {
        public decimal? Percentage { get; set; }
        public decimal? FixedAmount { get; set; }
        public ApplyDiscountCommand() : base("applyDiscount") { }
    }
}
