
namespace eMarket.Application.Businesses.Queries.GetBusinessActivityLogs;

public sealed record GetBusinessActivityLogsResponse(
    IReadOnlyList<BusinessActivityLogItem> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record BusinessActivityLogItem(
    Guid Id,
    Guid ActorUserId,
    Guid? TargetUserId,
    string Action,
    string Description,
    DateTime CreatedAt);
