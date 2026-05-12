namespace HRMS.API.Models;

/// <summary>
/// JSON body for reject actions (shared by leaves, expenses, travel).
/// </summary>
public sealed class RejectReasonRequest
{
    public string? Reason { get; init; }
}
