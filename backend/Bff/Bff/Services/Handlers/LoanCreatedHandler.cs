using Bff.Interfaces.Interfaces;
using Loan.Shared.Contracts.Notifications;
using Newtonsoft.Json;

namespace Bff.Services.Handlers;

public class LoanCreatedHandler(ILogger<LoanCreatedHandler> logger) : IMessageHandler
{
    public Task HandleAsync(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            var loanCreatedEvent = JsonConvert.DeserializeObject<LoanCreated>(messageContent);

            ArgumentNullException.ThrowIfNull(loanCreatedEvent, nameof(loanCreatedEvent));
            logger.LogInformation("Loan created {LoanId}", loanCreatedEvent.LoanId);


        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error trying to deserializing message");
        }

        return Task.CompletedTask;
    }
}
