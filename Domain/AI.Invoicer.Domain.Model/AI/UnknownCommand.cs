using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Domain.Model.AI
{
    public class UnknownCommand : AiCommand
    {
        public string Reason { get; set; } = "Action non reconnue ou JSON invalide.";
        public UnknownCommand() : base("unknown") { }
    }
}
