using FastEndpoints;
using Server.Loan.Contracts.Features.Loan.Common;

namespace Server.Loan.Contracts.Features.Loan.Notifications;

public record LoanReply(string EventType, LoanDto LoanDetails, string SessionId) : IEvent;
