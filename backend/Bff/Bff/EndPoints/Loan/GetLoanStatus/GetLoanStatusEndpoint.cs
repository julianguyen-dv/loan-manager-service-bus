using Bff.EndPoints.Loan.GetLoanStatus;
using Bff.Interfaces;
using FastEndpoints;
using Loan.Shared.Contracts.Requests;
using Loan.StorageProvider.Models;

namespace Bff.EndPoints.Loan.GetLoanStatus;


internal class GetLoanStatusEndpoint(ILoanDraftStorageProvider draftStorageProvider, ILoanPublisher loanPublisher) : Endpoint<GetLoanStatusRequest, GetLoanStatusResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<LoanGroup>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Fetch the status of a loan application";
            s.Description = "Fetch the status of a loan application from the system";
            s.Response<GetLoanStatusResponse>();
        });
    }
    public override async Task HandleAsync(GetLoanStatusRequest req, CancellationToken ct)
    {
        //1.- Publish the loan status request to the message broker
        var sessionId = Guid.NewGuid().ToString();
        var loanStatusRequest = new LoanDetailsRequested(req.LoanId, sessionId);

        await loanPublisher.PublishLoanDetailsRequestedAsync(loanStatusRequest, ct);

        Console.WriteLine($"Loan status request published for Loan ID: {req.LoanId}");

        //2.- Retrieve the loan entity from the storage provider
        var loanDraft = await draftStorageProvider.GetLoanByIdAsync(req.LoanId);

        if (loanDraft is null)
        {
            // If the loan draft is not found, return a NotFound response
            await SendNotFoundAsync(cancellation: ct);
            return;
        }

        //3.- Return the loan status to the client
        await SendOkAsync(new GetLoanStatusResponse(loanDraft.LoanId, loanDraft.LoanStatus), cancellation: ct);
    }
}
