
using eMarket.Domain.Identity;

namespace eMarket.Domain.Businesses.Entities;

public sealed class BusinessActivityLog
{
    private BusinessActivityLog()
    {
    }

    private BusinessActivityLog(
        BusinessId businessId,
        UserId actorUserId,
        UserId? targetUserId,
        string action,
        string description)
    {
        Id = Guid.NewGuid();
        BusinessId = businessId;
        ActorUserId = actorUserId;
        TargetUserId = targetUserId;
        Action = action;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public BusinessId BusinessId { get; private set; } = default!;

    public UserId ActorUserId { get; private set; } = default!;

    public UserId? TargetUserId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public static BusinessActivityLog Create(
        BusinessId businessId,
        UserId actorUserId,
        UserId? targetUserId,
        string action,
        string description)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be empty.", nameof(action));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        return new BusinessActivityLog(
            businessId,
            actorUserId,
            targetUserId,
            action.Trim(),
            description.Trim());
    }
}
