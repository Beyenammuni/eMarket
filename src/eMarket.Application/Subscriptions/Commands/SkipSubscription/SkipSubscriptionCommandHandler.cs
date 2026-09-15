using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Subscriptions;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace eMarket.Application.Subscriptions.Commands.SkipSubscription;
internal sealed class SkipSubscriptionCommandHandler : IRequestHandler<SkipSubscriptionCommand, Result<SkipSubscriptionResponse>>
{
 private readonly ISubscriptionDbContext _context; private readonly ICurrentUser _user;
 public SkipSubscriptionCommandHandler(ISubscriptionDbContext context, ICurrentUser user){_context=context;_user=user;}
 public async Task<Result<SkipSubscriptionResponse>> Handle(SkipSubscriptionCommand request,CancellationToken ct){
  if(!_user.IsAuthenticated||_user.UserId is null)return Result<SkipSubscriptionResponse>.Failure(new("Auth.Unauthorized","User is not authenticated."));
  var sub=await _context.Subscriptions.FirstOrDefaultAsync(x=>x.Id==SubscriptionId.Create(request.SubscriptionId)&&x.UserId==_user.UserId,ct);
  if(sub is null)return Result<SkipSubscriptionResponse>.Failure(SubscriptionErrors.NotFound);
  var r=sub.SkipNextDelivery(); if(r.IsFailure)return Result<SkipSubscriptionResponse>.Failure(r.Error);
  await _context.SaveChangesAsync(ct); return Result<SkipSubscriptionResponse>.Success(new(sub.Id.Value,sub.NextDeliveryDate));
 }
}
