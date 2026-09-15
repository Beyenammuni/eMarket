using eMarket.Application.Common.Interfaces;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace eMarket.Application.Subscriptions.Queries.GetMySubscription;
internal sealed class GetMySubscriptionsQueryHandler : IRequestHandler<GetMySubscriptionsQuery,Result<IReadOnlyCollection<GetMySubscriptionResponse>>>
{
 private readonly ISubscriptionDbContext _context; private readonly ICurrentUser _user;
 public GetMySubscriptionsQueryHandler(ISubscriptionDbContext context,ICurrentUser user){_context=context;_user=user;}
 public async Task<Result<IReadOnlyCollection<GetMySubscriptionResponse>>> Handle(GetMySubscriptionsQuery request,CancellationToken ct){
  if(!_user.IsAuthenticated||_user.UserId is null)return Result<IReadOnlyCollection<GetMySubscriptionResponse>>.Failure(new("Auth.Unauthorized","User is not authenticated."));
  var rows=await _context.Subscriptions.AsNoTracking().Include(x=>x.Items).Where(x=>x.UserId==_user.UserId).OrderByDescending(x=>x.CreatedAt).ToListAsync(ct);
  return Result<IReadOnlyCollection<GetMySubscriptionResponse>>.Success(rows.Select(s=>new GetMySubscriptionResponse(s.Id.Value,s.BusinessId.Value,s.DeliveryDay,s.NextDeliveryDate,s.Status.ToString(),s.Items.Select(i=>new SubscriptionItemResponse(i.ProductId.Value,i.Quantity)).ToList())).ToList());
 }
}
