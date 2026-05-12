using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Notifications.Commands;
using HRMS.Domain.Enums;
using HRMS.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Common.EventHandlers;

public class NotificationEventHandler(
    IMediator mediator,
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    ILogger<NotificationEventHandler> logger) : 
    INotificationHandler<LeaveStatusChangedEvent>,
    INotificationHandler<ExpenseStatusChangedEvent>,
    INotificationHandler<TravelStatusChangedEvent>,
    INotificationHandler<EmployeeCreatedEvent>,
    INotificationHandler<OrganizationProvisionedEvent>
{
    public async Task Handle(LeaveStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var leave = notification.LeaveRequest;
        var userId = leave.Employee.UserId;
        if (userId == Guid.Empty) return;

        var prefs = await unitOfWork.NotificationPreferences.GetByUserIdAsync(userId, cancellationToken);
        if (prefs is { InAppEnabled: false } or { LeaveNotifications: false }) return;

        var title = $"Leave {leave.Status}";
        var message = $"Your leave request for {leave.StartDate:dd MMM} to {leave.EndDate:dd MMM} has been {leave.Status}.";
        if (!string.IsNullOrEmpty(notification.Reason)) message += $" Reason: {notification.Reason}";

        await mediator.Send(new CreateNotificationCommand(userId, title, message, NotificationType.Info, leave.Id.ToString(), "Leave"), cancellationToken);

        if (prefs is { EmailEnabled: true })
        {
            try 
            { 
                await emailService.SendAsync(new EmailMessage(leave.Employee.Email, title, message), cancellationToken); 
            }
            catch (Exception ex) 
            { 
                logger.LogError(ex, "Failed to send leave email notification."); 
            }
        }
    }

    public async Task Handle(ExpenseStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var claim = notification.ExpenseClaim;
        var userId = claim.Employee.UserId;
        if (userId == Guid.Empty) return;

        var prefs = await unitOfWork.NotificationPreferences.GetByUserIdAsync(userId, cancellationToken);
        if (prefs is { InAppEnabled: false } or { ExpenseNotifications: false }) return;

        var title = $"Expense Claim {claim.Status}";
        var message = $"Your expense claim of {claim.Amount} for {claim.Description} has been {claim.Status}.";

        await mediator.Send(new CreateNotificationCommand(userId, title, message, NotificationType.Info, claim.Id.ToString(), "Expense"), cancellationToken);

        if (prefs is { EmailEnabled: true })
        {
            try 
            { 
                await emailService.SendAsync(new EmailMessage(claim.Employee.Email, title, message), cancellationToken); 
            }
            catch (Exception ex) 
            { 
                logger.LogError(ex, "Failed to send expense email notification."); 
            }
        }
    }

    public async Task Handle(TravelStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var travel = notification.TravelRequest;
        var userId = travel.Employee.UserId;
        if (userId == Guid.Empty) return;

        var prefs = await unitOfWork.NotificationPreferences.GetByUserIdAsync(userId, cancellationToken);
        if (prefs is { InAppEnabled: false } or { TravelNotifications: false }) return;

        var title = $"Travel Request {travel.Status}";
        var message = $"Your travel request to {travel.Destination} has been {travel.Status}.";

        await mediator.Send(new CreateNotificationCommand(userId, title, message, NotificationType.Info, travel.Id.ToString(), "Travel"), cancellationToken);

        if (prefs is { EmailEnabled: true })
        {
            try 
            { 
                await emailService.SendAsync(new EmailMessage(travel.Employee.Email, title, message), cancellationToken); 
            }
            catch (Exception ex) 
            { 
                logger.LogError(ex, "Failed to send travel email notification."); 
            }
        }
    }

    public async Task Handle(EmployeeCreatedEvent notification, CancellationToken cancellationToken)
    {
        var employee = notification.Employee;
        var userId = employee.UserId;
        if (userId == Guid.Empty) return;

        var title = "Welcome to the Team!";
        var message = $"Hello {employee.FirstName}, your HRMS account has been created. Your temporary password is: {notification.TempPassword}";

        await mediator.Send(new CreateNotificationCommand(userId, title, message, NotificationType.Success), cancellationToken);

        try 
        { 
            await emailService.SendAsync(new EmailMessage(employee.Email, title, message), cancellationToken); 
        }
        catch (Exception ex) 
        { 
            logger.LogError(ex, "Failed to send welcome email."); 
        }
    }

    public async Task Handle(OrganizationProvisionedEvent notification, CancellationToken cancellationToken)
    {
        var org = notification.Organization;
        var title = $"Welcome to HRMS-Lite: {org.Name}";
        var message = $"Hello, your organization '{org.Name}' has been provisioned. " +
                      $"You can log in at your slug: {org.Slug}. " +
                      $"Your temporary password is: {notification.TempPassword}";

        try 
        { 
            await emailService.SendAsync(new EmailMessage(notification.AdminEmail, title, message), cancellationToken); 
        }
        catch (Exception ex) 
        { 
            logger.LogError(ex, "Failed to send organization welcome email."); 
        }
    }
}
