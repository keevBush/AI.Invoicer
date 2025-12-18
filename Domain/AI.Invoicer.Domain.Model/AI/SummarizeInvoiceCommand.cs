using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Domain.Model.AI
{
    public class SummarizeInvoiceCommand : AiCommand
    {
        public string Query { get; set; } = string.Empty;
        public SummarizeInvoiceCommand() : base("summarizeInvoice") { }
    }
}
