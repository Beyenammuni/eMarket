using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Subscriptions;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace eMarket.Application.Subscriptions.Queries.GetMySubscription;
internal sealed class GetMySubscriptionQueryHandler : IRequestHandler<GetMySubscriptionQuery,Result<GetMySubscriptionResponse>>
{
 private readonly ISubscriptionDbContext _context; private readonly ICurrentUser _user;
 public GetMySubscriptionQueryHandler(ISubscriptionDbContext context,ICurrentUser user){_context=context;_user=user;}
 public async Task<Result<GetMySubscriptionResponse>> Handle(GetMySubscriptionQuery request,CancellationToken ct){
  if(!_user.IsAuthenticated||_user.UserId is null)return Result<GetMySubscriptionResponse>.Failure(new("Auth.Unauthorized","User is not authenticated."));
  var s=await _context.Subscriptions.AsNoTracking().Include(x=>x.Items).FirstOrDefaultAsync(x=>x.Id==SubscriptionId.Create(request.SubscriptionId)&&x.UserId==_user.UserId,ct);
  if(s is null)return Result<GetMySubscriptionResponse>.Failure(SubscriptionErrors.NotFound);
  return Result<GetMySubscriptionResponse>.Success(new(s.Id.Value,s.BusinessId.Value,s.DeliveryDay,s.NextDeliveryDate,s.Status.ToString(),s.Items.Select(i=>new SubscriptionItemResponse(i.ProductId.Value,i.Quantity)).ToList()));
 }
}
