using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Domain.Model.AI
{
    public class DuplicateLineCommand : AiCommand
    {
        public int LineNumber { get; set; }
        public DuplicateLineCommand() : base("duplicate") { }
    }
}
