using Loan.Shared.Contracts.Requests;
using Loan.Shared.Contracts.Responses;

namespace Bff.Interfaces;

internal interface ILoanPublisher
{
    Task PublishLoanSubmittedAsync(LoanSubmissionRequested command, CancellationToken cancellationToken);
    Task PublishLoanDetailsRequestedAsync(LoanDetailsRequested loanStatusRequest, CancellationToken ct);
}
