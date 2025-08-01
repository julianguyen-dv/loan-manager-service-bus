using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loan.Shared.Contracts.Abstractions;
using Loan.Shared.Contracts.Common;

namespace Loan.Shared.Contracts.Responses;
public record LoanDetailsResponse(
    [Required(AllowEmptyStrings = false)]
    LoanDetails LoanDetails, string SessionId) : BaseMessage;