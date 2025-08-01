using Ardalis.Result;
using FastEndpoints;
using Server.Loan.Contracts.Features.Loan.Common;
using Server.Loan.Contracts.Features.Loan.FetchLoan;
using Server.Loan.Infrastructure.Interfaces;

namespace Server.Loan.Infrastructure.Integrations;

internal class FetchLoanQueryHandler(ILoanRepositoryFactory loanRepositoryFactory) : CommandHandler<FetchLoanQuery, Result<FetchLoanQueryResponse>>
{
    private readonly ILoanRepositoryFactory _loanRepositoryFactory = loanRepositoryFactory;

    public override async Task<Result<FetchLoanQueryResponse>> ExecuteAsync(FetchLoanQuery query, CancellationToken ct = default)
    {
        var loanRepository = _loanRepositoryFactory.Create(Enums.StorageType.Database);
        var loan = await loanRepository.GetLoanByIdAsync(query.LoanId);

        if (loan is null)
        {
            return Result<FetchLoanQueryResponse>.NotFound($"Loan with ID {query.LoanId} not found.");
        }

        var item = new LoanDto(
            Id: loan.LoanId,
            LoanAmount: loan.LoanAmount,
            LoanTerm: loan.LoanTerm,
            LoanPurpose: loan.LoanPurpose,
            LoanStatus: loan.LoanStatus,
            BankAccountNumber: loan.BankInformation.AccountNumber,
            BankAccountType: loan.BankInformation.AccountType,
            BankName: loan.BankInformation.BankName,
            FullName: loan.PersonalInformation.FullName,
            Email: loan.PersonalInformation.Email,
            DateOfBirth: loan.PersonalInformation.DateOfBirth.ToDateTime(TimeOnly.MinValue)
        );

        return new FetchLoanQueryResponse(item);
    }
}
