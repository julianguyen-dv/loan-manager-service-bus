using FastEndpoints;
using Loan.Shared.Contract.Abstractions.Interfaces;
using Loan.Shared.Contracts.Requests;
using Loan.StorageProvider.Models;
using Microsoft.Azure.Amqp;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Loan.Application.Features.Loan.CreateLoan;
using Server.Loan.Application.Interfaces;
using Server.Loan.Contracts.Features.Loan.FetchLoan;
using Server.Loan.Contracts.Features.Loan.GetLoans;
using Server.Loan.Contracts.Features.Loan.Notifications;
using Server.Loan.Contracts.Features.Loan.SubmitLoan;
using Server.Loan.Infrastructure.Interfaces;

namespace Server.Loan.Infrastructure.Services.Handlers;

/// <summary>
/// Handler for loan detail fetch requests
/// </summary>
internal class GetLoanDetailsRequestHandler(
    ILogger<GetLoanDetailsRequestHandler> logger,
    ILoanRepositoryFactory loanRepositoryFactory,
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

        // Create persistence entity from the fetched loan details
        var loanEntity = new LoanEntity
        {
            LoanId = loanDetails.Id,
            LoanAmount = loanDetails.LoanAmount,
            LoanTerm = loanDetails.LoanTerm,
            LoanPurpose = loanDetails.LoanPurpose,
            BankInformation = new BankInformationEntity(
                BankName: loanDetails.BankName,
                AccountType: loanDetails.BankAccountType,
                AccountNumber: loanDetails.BankAccountNumber),
            PersonalInformation = new PersonalInformationEntity(
                FullName: loanDetails.FullName,
                Email: loanDetails.Email,
                DateOfBirth: DateOnly.FromDateTime(loanDetails.DateOfBirth)),
            LoanStatus = loanDetails.LoanStatus
        };

        // Save the fetched loan details to the draft storage
        var loanDraftsRepository = loanRepositoryFactory.Create(Enums.StorageType.Draft);
        var saveDraftResult = await loanDraftsRepository.SaveLoanAsync(loanEntity);

        if (!saveDraftResult)
        {
            logger.LogError("Failed to save loan draft for ID {LoanId}", loanDetailsRequest.LoanId);
            return;
        }

        // Publish reply message that loan details have been fetched
        var eventNotification = new LoanReply(loanEntity.LoanId, loanEntity.LoanStatus, loanDetailsRequest.SessionId);
        await eventNotification.PublishAsync(cancellation: cancellationToken);

        await Task.CompletedTask;
    }
}
