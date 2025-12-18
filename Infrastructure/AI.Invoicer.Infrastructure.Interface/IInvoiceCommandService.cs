using AI.Invoicer.Domain.Model.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Infrastructure.Interface
{
    public interface IInvoiceCommandService
    {
        Task<IEnumerable<AiCommand>> GetCommandsFromPromptAsync(string userPrompt, string? invoiceJsonContext);
    }
}
