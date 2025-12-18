using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Domain.Model.AI
{
    public class AiCommand
    {
        public string Action { get; private set; }

        /// <summary>
        /// Suggestion de réponse textuelle à afficher à l'utilisateur.
        /// </summary>
        public string? Feedback { get; set; }

        public AiCommand(string action)
        {
            Action = action;
        }
    }
}
