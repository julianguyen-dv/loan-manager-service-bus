using FastEndpoints;
using Loan.Shared.Contract.Abstractions.Interfaces;
using Loan.Shared.Contracts.Requests;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Loan.Application.Features.Loan.CreateLoan;
using Server.Loan.Application.Interfaces;
using Server.Loan.Contracts.Features.Loan.SubmitLoan;

namespace Server.Loan.Infrastructure.Services.Handlers;

/// <summary>
/// Handler for loan submission requests
/// </summary>
internal class SubmitLoanRequestHandler(
    ILogger<SubmitLoanRequestHandler> logger, 
    ISchemaValidator schemaValidator
    ) : IMessageHandler
{
    public async Task HandleAsync(string messageContent, CancellationToken cancellationToken)
    {
       
        var draftLoanSubmission = JsonConvert.DeserializeObject<LoanSubmissionRequested>(messageContent);

        if (draftLoanSubmission is null)
        {
            logger.LogError("Failed to deserialize SubmitLoanRequest");
            return;
        }

        var validationResult = await schemaValidator.ValidateAsync(draftLoanSubmission);

        if(!validationResult)
        {
            logger.LogError("Validation failed for loan submission request");
            return;
        }


        logger.LogInformation("Processing loan submission request for ID {LoanId}", draftLoanSubmission.LoanDetails.Id);

        var draft = draftLoanSubmission.LoanDetails;

        if (draft is null)
        {
            logger.LogError("Loan with ID {LoanId} not found", draftLoanSubmission.LoanDetails.Id);
            return;
        }

        var createCommand = new CreateLoanCommand(draft);
        var createLoanResult = await createCommand.ExecuteAsync(cancellationToken);

        if(!createLoanResult.IsSuccess)
        {
            // send notification that loan creation failed
            logger.LogError("Failed to create loan with ID {LoanId}", draft.Id);
            return;
        }

        logger.LogInformation("Loan with ID {LoanId} successfully created", draft.Id);
        var createdLoanId = createLoanResult.Value.LoanId;
        var submitLoanCommand = new SubmitLoanCommand(createdLoanId);

        var submitLoanResult = await submitLoanCommand.ExecuteAsync(cancellationToken);
        if (!submitLoanResult.IsSuccess)
        {
            // send notification that loan submission failed
            logger.LogError("Failed to submit loan with ID {LoanId}", draft.Id);
            return;
        }
 
        await Task.CompletedTask;
    }
}
