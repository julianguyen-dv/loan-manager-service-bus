using FastEndpoints;
using Loan.Shared.Contract.Abstractions.Interfaces;
using Loan.Shared.Contracts.Requests;
using Loan.Shared.Contracts.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Loan.Application.Interfaces;
using Server.Loan.Contracts.Features.Loan.FetchLoan;
using Server.Loan.Contracts.Features.Loan.Notifications;

namespace Server.Loan.Infrastructure.Services.Handlers;

/// <summary>
/// Handler for loan detail fetch requests
/// </summary>
internal class GetLoanDetailsRequestHandler(
    ILogger<GetLoanDetailsRequestHandler> logger,
    ISchemaValidator schemaValidator
    ) : IMessageHandler
{
    public async Task HandleAsync(string messageContent, CancellationToken cancellationToken)
    {
        var loanDetailsRequest = JsonConvert.DeserializeObject<LoanDetailsRequested>(messageContent);

        if (loanDetailsRequest is null)
        {
            logger.LogError("Failed to deserialize GetLoanDetailsRequest");
            return;
        }

        var validationResult = await schemaValidator.ValidateAsync(loanDetailsRequest);

        if (!validationResult)
        {
            logger.LogError("Validation failed for loan details request");
            return;
        }

        logger.LogInformation("Fetching loan information for ID {LoanId}", loanDetailsRequest.LoanId);

        // Check if the loan exists in the draft storage

        // Delegate to command handler to fetch loan details
        var getLoanDetailsCommand = new FetchLoanQuery(loanDetailsRequest.LoanId);
        var getLoanDetailsResult = await getLoanDetailsCommand.ExecuteAsync(cancellationToken);

        if (!getLoanDetailsResult.IsSuccess)
        {
            logger.LogError("Failed to fetch loan details for ID {LoanId}", loanDetailsRequest.LoanId);
            return;
        }

        var loanDetails = getLoanDetailsResult.Value.loan;

        // Publish reply message with fetched loan details
        var eventNotification = new LoanReply(typeof(LoanDetailsResponse).ToString(), loanDetails, loanDetailsRequest.SessionId);
        await eventNotification.PublishAsync(cancellation: cancellationToken);

        await Task.CompletedTask;
    }
}
