using Bff.Interfaces;
using FastEndpoints;
using Loan.Shared.Contracts.Requests;

namespace Bff.EndPoints.Loan.GetLoanStatus;


internal class GetLoanStatusEndpoint(
    ILoanPublisher loanPublisher,
    ILoanReplyConsumer loanReplyConsumer) : Endpoint<GetLoanStatusRequest, GetLoanStatusResponse>
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
        var response = await loanReplyConsumer.ReceiveLoanDetailsAsync(loanStatusRequest, ct);

        //3.- Return the loan status to the client
        await SendOkAsync(new GetLoanStatusResponse(response.LoanDetails.Id, response.LoanDetails.LoanStatus), cancellation: ct);
    }
}
