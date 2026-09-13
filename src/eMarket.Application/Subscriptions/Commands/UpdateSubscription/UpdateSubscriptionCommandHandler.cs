using eMarket.Application.Common.Interfaces;
using eMarket.Application.Subscriptions.Commands.CreateSubscription;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Subscriptions;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Subscriptions.Commands.UpdateSubscription;
internal sealed class UpdateSubscriptionCommandHandler : IRequestHandler<UpdateSubscriptionCommand, Result<UpdateSubscriptionResponse>>
{
    private readonly ISubscriptionDbContext _context; private readonly ICatalogDbContext _catalog; private readonly ICurrentUser _user;
    public UpdateSubscriptionCommandHandler(ISubscriptionDbContext context, ICatalogDbContext catalog, ICurrentUser user) { _context=context; _catalog=catalog; _user=user; }
    public async Task<Result<UpdateSubscriptionResponse>> Handle(UpdateSubscriptionCommand request, CancellationToken ct)
    {
        if (!_user.IsAuthenticated || _user.UserId is null) return Result<UpdateSubscriptionResponse>.Failure(new("Auth.Unauthorized","User is not authenticated."));
        var id=SubscriptionId.Create(request.SubscriptionId);
        var sub=await _context.Subscriptions.Include(x=>x.Items).FirstOrDefaultAsync(x=>x.Id==id && x.UserId==_user.UserId,ct);
        if(sub is null) return Result<UpdateSubscriptionResponse>.Failure(SubscriptionErrors.NotFound);
        if(sub.Status==SubscriptionStatus.Cancelled) return Result<UpdateSubscriptionResponse>.Failure(SubscriptionErrors.AlreadyCancelled);
        var products=await _catalog.Products.Where(x=>request.Items.Select(i=>ProductId.Create(i.ProductId)).Contains(x.Id) && x.BusinessId==sub.BusinessId && x.Status==ProductStatus.Active).ToListAsync(ct);
        if(products.Count!=request.Items.Count) return Result<UpdateSubscriptionResponse>.Failure(SubscriptionErrors.DifferentBusinesses);
        var replace = sub.ReplaceItems(request.Items.Select(x => (ProductId.Create(x.ProductId), x.Quantity)));
        if (replace.IsFailure) return Result<UpdateSubscriptionResponse>.Failure(replace.Error);
        sub.SetDeliveryDay(request.DeliveryDay);
        await _context.SaveChangesAsync(ct);
        return Result<UpdateSubscriptionResponse>.Success(new(sub.Id.Value,sub.DeliveryDay,sub.NextDeliveryDate,sub.Status.ToString()));
    }
}
