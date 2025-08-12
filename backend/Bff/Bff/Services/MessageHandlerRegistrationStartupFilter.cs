using Bff.Interfaces.Interfaces;
using Bff.Services.Handlers;
using Loan.Shared.Contracts.Notifications;

namespace Bff.Services;

internal class MessageHandlerRegistrationStartupFilter(
    IMessageHandlerRegistry registry,
    IServiceProvider serviceProvider) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        // Register all message handlers
        registry.RegisterHandler(
            nameof(LoanSubmitted),
            serviceProvider.GetRequiredService<LoanSubmittedHandler>());

        registry.RegisterHandler(nameof(LoanCreated),
            serviceProvider.GetRequiredService<LoanCreatedHandler>());

        registry.RegisterHandler(nameof(LoanDraftAssigned),
            serviceProvider.GetRequiredService<DraftAssignedHandler>());

        registry.RegisterHandler(nameof(LoanApproved),
           serviceProvider.GetRequiredService<LoanApprovedHandler>());

        registry.RegisterHandler(nameof(LoanRejected),
           serviceProvider.GetRequiredService<LoanRejectedHandler>());

        // Add more handlers as needed

        return next;
    }
}
