using Loan.Shared.Contracts.Requests;
using Loan.Shared.Contracts.Responses;

namespace Bff.Interfaces;
internal interface ILoanReplyConsumer
{
    Task<LoanDetailsResponse> ReceiveLoanDetailsAsync(LoanDetailsRequested request, CancellationToken ct);
}
