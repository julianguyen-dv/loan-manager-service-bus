using Azure.Messaging.ServiceBus;
using Bff.Interfaces;
using Loan.Shared.Contracts.Models;
using Loan.Shared.Contracts.Requests;
using Microsoft.Azure.Amqp.Framing;
using Newtonsoft.Json;

namespace Bff.Services;

internal class LoanPublisher(ServiceBusClient mainBusClient) : ILoanPublisher
{
    public async Task PublishLoanSubmittedAsync(LoanSubmissionRequested command, CancellationToken cancellationToken)
    {
        var commandJson = JsonConvert.SerializeObject(command);
        var envelop = new MessageEnvelope(nameof(LoanSubmissionRequested), commandJson);
        var json = JsonConvert.SerializeObject(envelop);
        await mainBusClient
             .CreateSender(Loan.Shared.Contracts.Constants.Topics.LoanQueueName)
             .SendMessageAsync(new ServiceBusMessage(json), cancellationToken);
    }

    public async Task PublishLoanDetailsRequestedAsync(LoanDetailsRequested command, CancellationToken ct)
    {
        var commandJson = JsonConvert.SerializeObject(command);
        var envelop = new MessageEnvelope(nameof(LoanDetailsRequested), commandJson);
        var json = JsonConvert.SerializeObject(envelop);
        await mainBusClient
            .CreateSender(Loan.Shared.Contracts.Constants.Topics.LoanQueueName)
            .SendMessageAsync(new ServiceBusMessage(json), ct);

        ServiceBusSessionReceiver receiver = await mainBusClient.AcceptSessionAsync(Loan.Shared.Contracts.Constants.Topics.LoanRepliesQueueName, command.SessionId, cancellationToken: ct);

        ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync(cancellationToken: ct);

        if (receivedMessage is null)
        {
            throw new InvalidOperationException($"No message received for loan ID {command.LoanId}");
        }

        // output successful message information
        var messageContent = receivedMessage.Body.ToString();
        Console.WriteLine($"Received message for loan ID {command.LoanId}: {messageContent}");

        await receiver.CompleteMessageAsync(receivedMessage, ct);

    }
}
