using Azure.Messaging.ServiceBus;
using FastEndpoints;
using Loan.Shared.Contracts.Abstractions;
using Loan.Shared.Contracts.Common;
using Loan.Shared.Contracts.Constants;
using Loan.Shared.Contracts.Models;
using Loan.Shared.Contracts.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Loan.Contracts.Features.Loan.Common;
using Server.Loan.Contracts.Features.Loan.Notifications;

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
            // Create the corresponding message based on the event type
            var message = CreateMessageFromEventType(reply);

            // Serialize the message to JSON
            var messageContent = JsonConvert.SerializeObject(message);

            // Create the envelope with the message type and its content
            var envelope = new MessageEnvelope("LoanReply", messageContent);

            // Serialize the complete envelope
            var envelopeJson = JsonConvert.SerializeObject(envelope);

            // Send the message to the service bus
            await mainBusClient
                .CreateSender(Topics.LoanRepliesQueueName)
                .SendMessageAsync(new ServiceBusMessage(envelopeJson)
                {
                    SessionId = reply.SessionId
                }, cancellationToken);

            logger.LogInformation("Successfully processed loan reply for loan {LoanId}",  reply.LoanDetails.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing loan reply: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Creates the appropriate message based on the event type
    /// </summary>
    /// <param name="eventType">Type of the event</param>
    /// <param name="loanId">ID of the loan</param>
    /// <returns>A message of the appropriate type</returns>
    private BaseMessage CreateMessageFromEventType(LoanReply loanReply)
    {
        return loanReply.EventType switch
        {
            nameof(LoanDetailsResponse) => new LoanDetailsResponse(MapToLoanDetails(loanReply.LoanDetails), loanReply.SessionId),
            _ => new LoanDetailsResponse(MapToLoanDetails(loanReply.LoanDetails), "") 
        };
    }

    private static LoanDetails MapToLoanDetails(LoanDto loanDto)
    {
        return new LoanDetails(
            Id: loanDto.Id,
            LoanAmount: loanDto.LoanAmount,
            LoanTerm: loanDto.LoanTerm,
            LoanPurpose: loanDto.LoanPurpose,
            BankAccountNumber: loanDto.BankAccountNumber,
            BankAccountType: loanDto.BankAccountType,
            BankName: loanDto.BankName,
            FullName: loanDto.FullName,
            Email: loanDto.Email,
            DateOfBirth: loanDto.DateOfBirth,
            LoanStatus: loanDto.LoanStatus
        );
    }
}
