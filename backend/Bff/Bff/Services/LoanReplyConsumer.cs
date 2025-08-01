using Azure.Messaging.ServiceBus;
using Bff.Interfaces;
using Loan.Shared.Contracts.Models;
using Loan.Shared.Contracts.Requests;
using Loan.Shared.Contracts.Responses;
using Newtonsoft.Json;

namespace Bff.Services;
internal class LoanReplyConsumer(
    ILogger<LoanReplyConsumer> logger, 
    ServiceBusClient mainBusClient) : ILoanReplyConsumer
{
    public async Task<LoanDetailsResponse> ReceiveLoanDetailsAsync(LoanDetailsRequested request, CancellationToken ct)
    {
        try
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "LoanDetailsRequested cannot be null");
            }
            ServiceBusSessionReceiver receiver = await mainBusClient.AcceptSessionAsync(
                Loan.Shared.Contracts.Constants.Topics.LoanRepliesQueueName,
                request.SessionId,
                cancellationToken: ct);

            ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync(cancellationToken: ct) ?? throw new InvalidOperationException($"No message received for loan ID {request.LoanId}");

            var body = receivedMessage.Body.ToString();
            logger.LogInformation("Received message for loan ID {LoanId}: {Body}", request.LoanId, body);

            var envelope = JsonConvert.DeserializeObject<MessageEnvelope>(body) ?? throw new InvalidOperationException($"Failed to deserialize message for loan ID {request.LoanId}");
            
            var loanDetailsResponse = JsonConvert.DeserializeObject<LoanDetailsResponse>(envelope.MessageContent);

            ArgumentNullException.ThrowIfNull(loanDetailsResponse, nameof(loanDetailsResponse));

            await receiver.CompleteMessageAsync(receivedMessage, ct);
            return loanDetailsResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error receiving loan details for request {Request}", request);
            throw;
        }
    }
}

