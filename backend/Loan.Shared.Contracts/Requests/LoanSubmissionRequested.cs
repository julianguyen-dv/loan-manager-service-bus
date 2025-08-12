using Loan.Shared.Contracts.Abstractions;
using Loan.Shared.Contracts.Common;
using System.ComponentModel.DataAnnotations;

namespace Loan.Shared.Contracts.Requests;

public record LoanSubmissionRequested(
    [Required(AllowEmptyStrings = false)]
    LoanDetails LoanDetails) : BaseMessage;
