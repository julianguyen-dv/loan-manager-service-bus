using Ardalis.Result;
using FastEndpoints;

namespace Server.Loan.Contracts.Features.Loan.FetchLoan;

public record FetchLoanQuery(string LoanId) : ICommand<Result<FetchLoanQueryResponse>>;
