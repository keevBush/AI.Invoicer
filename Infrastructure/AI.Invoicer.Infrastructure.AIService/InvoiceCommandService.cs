using AI.Invoicer.Domain.Model.AI;
using AI.Invoicer.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AI.Invoicer.Infrastructure.AIService
{
    public class InvoiceCommandService(IInferenceService _inferenceService) : IInvoiceCommandService
    {
        /// <summary>
        /// Convertit un prompt utilisateur en une ou plusieurs commandes.
        /// L'IA retourne toujours un tableau JSON de commandes.
        /// </summary>
        public async Task<IEnumerable<AiCommand>> GetCommandsFromPromptAsync(string userPrompt, string? invoiceJsonContext = null)
        {
            if (string.IsNullOrWhiteSpace(userPrompt))
                throw new ArgumentException("Le prompt utilisateur ne peut pas être vide.", nameof(userPrompt));

            // 1. Construire le prompt détaillé pour l'IA
            var fullPrompt = BuildStructuredPrompt(userPrompt, invoiceJsonContext);

            // 2. Appeler le service d'inférence pour obtenir la réponse JSON
            var modelResponseJson = await _inferenceService.GenerateResponseAsync(fullPrompt);

            // 3. Parser la réponse JSON et retourner les commandes
            return ParseResponseToCommands(modelResponseJson?.Replace("]]","]"));
        }

        /// <summary>
        /// Parse la réponse JSON qui doit TOUJOURS être un tableau de commandes.
        /// </summary>
        private List<AiCommand> ParseResponseToCommands(string jsonResponse)
        {
            var commands = new List<AiCommand>();

            if (string.IsNullOrEmpty(jsonResponse.Trim()))
            {
                commands.Add(new UnknownCommand { Feedback = "L'IA n'a pas fourni de réponse." });
                return commands;
            }

            try
            {
                using var jsonDoc = JsonDocument.Parse(jsonResponse);
                var root = jsonDoc.RootElement;

                // La réponse DOIT être un tableau
                if (root.ValueKind != JsonValueKind.Array)
                {
                    commands.Add(new UnknownCommand
                    {
                        Feedback = "Format de réponse invalide. La réponse doit être un tableau JSON."
                    });
                    return commands;
                }

                // Itérer sur chaque commande du tableau
                foreach (var commandElement in root.EnumerateArray())
                {
                    var command = ParseSingleCommand(commandElement);
                    if (command != null)
                    {
                        commands.Add(command);
                    }
                }

                // Si aucune commande valide n'a été parsée
                if (commands.Count == 0)
                {
                    commands.Add(new UnknownCommand
                    {
                        Feedback = "Aucune commande valide trouvée dans la réponse."
                    });
                }
            }
            catch (JsonException ex)
            {
                commands.Add(new UnknownCommand
                {
                    Reason = $"JSON malformé: {ex.Message}",
                    Feedback = "Oups, une erreur technique est survenue lors de l'interprétation de la réponse."
                });
            }

            return commands;
        }

        /// <summary>
        /// Parse un objet JSON unique représentant une commande.
        /// </summary>
        private AiCommand? ParseSingleCommand(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object)
                return null;

            if (!element.TryGetProperty("action", out var actionElement))
                return new UnknownCommand
                {
                    Feedback = "Propriété 'action' manquante dans une commande."
                };

            var action = actionElement.GetString() ?? "unknown";
            element.TryGetProperty("data", out var dataElement);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            AiCommand command = action switch
            {
                "addLine" => dataElement.Deserialize<AddLineCommand>(options) is AddLineCommand addLine ? addLine : new UnknownCommand(),
                "removeLine" => dataElement.Deserialize<RemoveLineCommand>(options) is RemoveLineCommand removeLine ? removeLine : new UnknownCommand(),
                "duplicate" => dataElement.Deserialize<DuplicateLineCommand>(options) is DuplicateLineCommand duplicate ? duplicate : new UnknownCommand(),
                "changePrice" => dataElement.Deserialize<ChangePriceCommand>(options) is ChangePriceCommand changePrice ? changePrice : new UnknownCommand(),
                "updateHeader" => dataElement.Deserialize<UpdateHeaderCommand>(options) is UpdateHeaderCommand updateHeader ? updateHeader : new UnknownCommand(),
                "setCustomer" => dataElement.Deserialize<SetCustomerCommand>(options) is SetCustomerCommand setCustomer ? setCustomer : new UnknownCommand(),
                "applyDiscount" => dataElement.Deserialize<ApplyDiscountCommand>(options) is ApplyDiscountCommand applyDiscount ? applyDiscount : new UnknownCommand(),
                "applyTax" => dataElement.Deserialize<ApplyTaxCommand>(options) is ApplyTaxCommand applyTax ? applyTax : new UnknownCommand(),
                "summarizeInvoice" => dataElement.Deserialize<SummarizeInvoiceCommand>(options) is SummarizeInvoiceCommand summarizeInvoice ? summarizeInvoice : new UnknownCommand(),
                _ => new UnknownCommand(),
            };

            // Assigner le feedback s'il est présent
            if (element.TryGetProperty("feedback", out var feedbackElement))
            {
                command.Feedback = feedbackElement.GetString();
            }

            return command;
        }

        public string BuildStructuredPrompt(string userPrompt, string? invoiceJsonContext)
        {
            var promptBuilder = new StringBuilder();

            promptBuilder.AppendLine("<|system|>");
            promptBuilder.AppendLine("Tu es un assistant comptable expert. Convertis la demande de l'utilisateur en un objet JSON. Cet objet doit contenir une action, les données associées, et un champ 'feedback' contenant une réponse textuelle courte et amicale pour l'utilisateur.");
            promptBuilder.AppendLine("Le format doit TOUJOURS être un tableau dont les items ont de cette structure: {\"action\": \"action_name\", \"data\": {...}, \"feedback\": \"Votre réponse textuelle ici.\" }");
            promptBuilder.AppendLine("Si une demande est complexe, tu peux renvoyer plusieurs actions dans le Json de la réponse .");
            promptBuilder.AppendLine("Le Json que tu renvois est un tableau, rassure toi de correctement formatter le json, pas d'erreur les [] ou les {} .");
            promptBuilder.AppendLine("<|end|>");

            promptBuilder.AppendLine("<|user|>");

            promptBuilder.AppendLine("AVAILABLE_ACTIONS:");
            promptBuilder.AppendLine("1. addLine: Ajoute une nouvelle ligne. Schema: {\"action\": \"addLine\", \"data\": {\"description\": \"string\", \"quantity\": number, \"unitPrice\": number},  \"feedback\": \"string\"}");
            promptBuilder.AppendLine("2. removeLine: Supprime une ligne par son numéro. Schema: {\"action\": \"removeLine\", \"data\": {\"lineNumber\": number},  \"feedback\": \"string\"}");
            promptBuilder.AppendLine("3. duplicate: Duplique une ligne. Schema: {\"action\": \"duplicate\", \"data\": {\"lineNumber\": number}, \"feedback\": \"string\"}");
            promptBuilder.AppendLine("4. changePrice: Modifie le prix d'une ligne. Schema: {\"action\": \"changePrice\", \"data\": {\"lineNumber\": number, \"newPrice\": number}, \"feedback\": \"string\"}");
            promptBuilder.AppendLine("5. updateHeader: Modifie l'en-tête (date d'échéance, etc.). Schema: {\"action\": \"updateHeader\", \"data\": {\"dueDate\": \"date\", \"poNumber\": \"string\"},  \"feedback\": \"string\"}");
            promptBuilder.AppendLine("6. setCustomer: Attribue un client à la facture. Schema: {\"action\": \"setCustomer\", \"data\": {\"customerName\": \"string\"}, \"feedback\": \"string\"}");
            promptBuilder.AppendLine("7. applyDiscount: Applique une remise globale. Schema: {\"action\": \"applyDiscount\", \"data\": {\"percentage\": number} or {\"fixedAmount\": number}, \"feedback\": \"string\"}");
            promptBuilder.AppendLine("8. applyTax: Applique une taxe. Schema: {\"action\": \"applyTax\", \"data\": {\"taxName\": \"string\", \"rate\": number}, \"feedback\": \"string\"}");
            promptBuilder.AppendLine("9. summarizeInvoice: Demande un résumé de la facture. Schema: {\"action\": \"summarizeInvoice\", \"data\": {\"query\": \"totalAmount\" or \"lineCount\"}, \"feedback\": \"string\"}");
            promptBuilder.AppendLine("10. findInvoice: Recherche une facture existante. Schema: {\"action\": \"findInvoice\", \"data\": {\"customerName\": \"string\"}, \"feedback\": \"string\"}");
            promptBuilder.AppendLine("Si la demande est ambiguë, utilise: {\"action\": \"unknown\", \"data\": {\"reason\": \"description de l'ambiguïté\"}, \"feedback\": \"string\"}");

            if (!string.IsNullOrWhiteSpace(invoiceJsonContext))
            {
                promptBuilder.AppendLine("\nUSER_CONTEXT:");
                promptBuilder.AppendLine("L'utilisateur travaille sur la facture suivante :");
                promptBuilder.AppendLine(invoiceJsonContext);
            }

            promptBuilder.AppendLine("\nUSER_REQUEST:");
            promptBuilder.AppendLine(userPrompt);
            promptBuilder.AppendLine("<|end|>");

            promptBuilder.AppendLine("<|assistant|>");
            // 3. Parser la réponse JSON en un objet de commande typé
            return promptBuilder.ToString();
        }

    }
}
