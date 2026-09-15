using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Subscriptions;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace eMarket.Application.Subscriptions.Commands.CancelSubscription;
internal sealed class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, Result>
{
 private readonly ISubscriptionDbContext _context; private readonly ICurrentUser _user;
 public CancelSubscriptionCommandHandler(ISubscriptionDbContext context,ICurrentUser user){_context=context;_user=user;}
 public async Task<Result> Handle(CancelSubscriptionCommand request,CancellationToken ct){
  if(!_user.IsAuthenticated||_user.UserId is null)return Result.Failure(new("Auth.Unauthorized","User is not authenticated."));
  var sub=await _context.Subscriptions.FirstOrDefaultAsync(x=>x.Id==SubscriptionId.Create(request.SubscriptionId)&&x.UserId==_user.UserId,ct);
  if(sub is null)return Result.Failure(SubscriptionErrors.NotFound);
  var r=sub.Cancel(); if(r.IsFailure)return r; await _context.SaveChangesAsync(ct); return Result.Success();
 }
}
