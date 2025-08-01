using FastEndpoints;

namespace Server.Loan.Contracts.Features.Loan.Notifications;

public record LoanReply(string LoanId, int LoanStatus, string SessionId) : IEvent;
