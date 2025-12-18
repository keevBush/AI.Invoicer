using AI.Invoicer.Domain.Model.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Infrastructure.Interface
{
    public interface IInferenceService
    {
        Task InitializeAsync(string modelPath);
        Task<string> GenerateResponseAsync(string prompt);
    }
}
