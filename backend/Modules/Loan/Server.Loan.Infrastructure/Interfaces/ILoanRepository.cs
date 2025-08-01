using Loan.StorageProvider.Models;
using Server.Loan.Domain.Aggregates.Loan.Enums;

namespace Server.Loan.Infrastructure.Interfaces;

internal interface ILoanRepository
{
    Task<List<LoanEntity>> GetAllLoansAsync();
    Task<LoanEntity?> GetLoanByIdAsync(string loanId);
    Task<LoanEntity> CreateLoanAsync(LoanEntity loan);
    Task<bool> UpdateLoanStatusAsync(string loanId, LoanStatus newStatus);
    Task<bool> SubmitLoanAsync(string loanId);
    Task<bool> SaveLoanAsync(LoanEntity loan);
}
