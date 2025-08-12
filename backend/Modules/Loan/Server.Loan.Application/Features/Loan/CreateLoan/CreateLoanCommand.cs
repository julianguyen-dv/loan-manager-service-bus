using Ardalis.Result;
using FastEndpoints;
using Loan.Shared.Contracts.Common;

namespace Server.Loan.Application.Features.Loan.CreateLoan;

internal record CreateLoanCommand(
   LoanDetails LoanDetails) : ICommand<Result<CreateLoanCommandResponse>>;