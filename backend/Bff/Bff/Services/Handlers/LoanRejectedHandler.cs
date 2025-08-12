using Bff.Interfaces.Interfaces;
using Loan.Shared.Contracts.Notifications;
using Newtonsoft.Json;
using System.Text.Json;

namespace Bff.Services.Handlers;

public class LoanRejectedHandler(ILogger<LoanRejectedHandler> logger) : IMessageHandler
{
    public Task HandleAsync(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            var loanRejectedEvent = JsonConvert.DeserializeObject<LoanApproved>(messageContent);

            ArgumentNullException.ThrowIfNull(loanRejectedEvent, nameof(loanRejectedEvent));
            logger.LogInformation("Loan rejected {LoanId}", loanRejectedEvent.LoanId);


        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error trying to deserializing message");
        }

        return Task.CompletedTask;
    }
}
