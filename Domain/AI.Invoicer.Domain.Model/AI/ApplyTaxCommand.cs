using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Domain.Model.AI
{
    public class ApplyTaxCommand : AiCommand
    {
        public string TaxName { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public ApplyTaxCommand() : base("applyTax") { }
    }
}
