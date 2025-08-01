using Azure.Messaging.ServiceBus;
using FastEndpoints;
using Loan.Shared.Contracts.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Loan.Contracts.Features.Loan.Notifications;
using Server.Loan.Domain.Aggregates.Loan.ValueObjects;

namespace Server.Loan.Infrastructure.Integrations.Notifications;

/// <summary>
/// Dispatches loan responses to the service bus
/// </summary>
internal class LoanReplyHandler(ServiceBusClient mainBusClient, ILogger<LoanNotificationHandler> logger) : IEventHandler<LoanReply>
{

    /// <summary>
    /// Processes an incoming loan reply and dispatches it to the service bus
    /// </summary>
    /// <param name="reply">The loan reply to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task HandleAsync(LoanReply reply, CancellationToken cancellationToken)
    {
        try
        {
            // Serialize the message to JSON
            var messageContent = JsonConvert.SerializeObject(reply);

            // Create the envelope with the message type and its content
            var envelope = new MessageEnvelope("LoanReply", messageContent);

            // Serialize the complete envelope
            var envelopeJson = JsonConvert.SerializeObject(envelope);

            // Send the message to the service bus
            await mainBusClient
                .CreateSender("loan-replies")
                .SendMessageAsync(new ServiceBusMessage(envelopeJson)
                {
                    SessionId = reply.SessionId
                }, cancellationToken);

            logger.LogInformation("Successfully processed loan reply for loan {LoanId}",  reply.LoanId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing loan reply: {Message}", ex.Message);
            throw;
        }
    }
}
