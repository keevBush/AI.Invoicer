using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Domain.Model.AI
{
    public class ChangePriceCommand : AiCommand
    {
        public int LineNumber { get; set; }
        public decimal NewPrice { get; set; }
        public ChangePriceCommand() : base("changePrice") { }
    }
}
