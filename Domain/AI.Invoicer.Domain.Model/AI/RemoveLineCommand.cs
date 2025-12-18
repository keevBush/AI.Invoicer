using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Domain.Model.AI
{
    public class RemoveLineCommand : AiCommand
    {
        public int LineNumber { get; set; }
        public RemoveLineCommand() : base("removeLine") { }
    }
}
