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
        // Define the maxmimum time to wait for the reply to be received
        var timeout = TimeSpan.FromSeconds(3);

        // Create a cancellation token that will cancel after the timeout period
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(timeout);

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

            ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync(cancellationToken: linkedCts.Token) ?? throw new InvalidOperationException($"No message received for loan ID {request.LoanId}");

            var body = receivedMessage.Body.ToString();
            logger.LogInformation("Received message for loan ID {LoanId}: {Body}", request.LoanId, body);

            var envelope = JsonConvert.DeserializeObject<MessageEnvelope>(body) ?? throw new InvalidOperationException($"Failed to deserialize message for loan ID {request.LoanId}");
            
            var loanDetailsResponse = JsonConvert.DeserializeObject<LoanDetailsResponse>(envelope.MessageContent);

            ArgumentNullException.ThrowIfNull(loanDetailsResponse, nameof(loanDetailsResponse));

            await receiver.CompleteMessageAsync(receivedMessage, ct);
            return loanDetailsResponse;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Timeout while waiting for loan details for request {Request}", request);

            // Handle the timeout by throwing an exception or returning a default value
            throw new TimeoutException($"No response received for loan ID {request.LoanId} within the timeout period of {timeout.TotalSeconds} seconds.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error receiving loan details for request {Request}", request);
            throw;
        }
    }
}

