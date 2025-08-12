using Bff.Interfaces;
using FastEndpoints;
using Loan.Shared.Contracts.Common;
using Loan.Shared.Contracts.Requests;
using Loan.StorageProvider.Models;

namespace Bff.EndPoints.Loan.Submit;


//TODO: apply outbox pattern 
internal class SubmitLoanEndPoint(ILoanDraftStorageProvider draftStorageProvider, ILoanPublisher loanPublisher):Endpoint<SubmitLoanRequest, SubmitLoanResponse>
{
    public override void Configure()
    {
        Post("/");
        Group<LoanGroup>(); 
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Submit a loan application";
            s.Description = "Submit a loan application to the system";
            s.Response<SubmitLoanResponse>();
        });
    }
    public override async Task HandleAsync(SubmitLoanRequest req, CancellationToken ct)
    {
        //1.- Create the loan entity
        var draft = new LoanEntity
        {
            LoanId = Guid.NewGuid().ToString(),
            LoanAmount = req.LoanAmount,
            LoanTerm = req.LoanTerm,
            LoanPurpose = req.LoanPurpose,
            BankInformation= new BankInformationEntity(
                req.BankInformation.BankName, 
                req.BankInformation.AccountType, 
                req.BankInformation.AccountNumber),
            PersonalInformation = new PersonalInformationEntity(
                req.PersonalInformation.FullName, 
                req.PersonalInformation.Email, 
                DateOnly.FromDateTime(req.PersonalInformation.DateOfBirth)),
        };

        //2.- Save the loan entity to the storage provider

        var draftCreationResult = await draftStorageProvider.CreateLoanAsync(draft);

        //3.- Publish the loan creation event to the message broker
        var draftDto = new LoanDetails(
                Id: draft.LoanId,
                LoanAmount: draft.LoanAmount,
                LoanTerm: draft.LoanTerm,
                LoanPurpose: draft.LoanPurpose,
                BankAccountNumber: draft.BankInformation.AccountNumber,
                BankAccountType: draft.BankInformation.AccountType,
                BankName: draft.BankInformation.BankName,
                FullName: draft.PersonalInformation.FullName,
                Email: draft.PersonalInformation.Email,
                DateOfBirth: draft.PersonalInformation.DateOfBirth.ToDateTime(TimeOnly.MinValue),
                LoanStatus: -1
            );

        var draftCreationRequest = new LoanSubmissionRequested(draftDto);

        await loanPublisher.PublishLoanSubmittedAsync(draftCreationRequest, ct);

        //4.- Return the loan ID to the client
        await SendOkAsync(new SubmitLoanResponse(draftCreationResult.LoanId), cancellation: ct);
    }
}
