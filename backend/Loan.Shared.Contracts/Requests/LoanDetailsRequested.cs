using System.ComponentModel.DataAnnotations;
using Loan.Shared.Contracts.Abstractions;

namespace Loan.Shared.Contracts.Requests;

public record LoanDetailsRequested(
    [Required(AllowEmptyStrings = false)]
    string LoanId, string SessionId) : BaseMessage;

