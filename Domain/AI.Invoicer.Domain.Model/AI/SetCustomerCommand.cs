using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Domain.Model.AI
{
    public class SetCustomerCommand : AiCommand
    {
        public string CustomerName { get; set; } = string.Empty;
        public SetCustomerCommand() : base("setCustomer") { }
    }
}
