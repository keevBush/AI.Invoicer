using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Domain.Model.AI
{
    public class UpdateHeaderCommand : AiCommand
    {
        public DateTime? DueDate { get; set; }
        public string? PoNumber { get; set; }
        public string? Status { get; set; }
        public UpdateHeaderCommand() : base("updateHeader") { }
    }
}
