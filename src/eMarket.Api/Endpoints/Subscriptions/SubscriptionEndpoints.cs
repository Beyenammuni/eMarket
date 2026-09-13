using eMarket.Application.Subscriptions.Commands.CancelSubscription;
using eMarket.Application.Subscriptions.Commands.CreateSubscription;
using eMarket.Application.Subscriptions.Commands.GenerateSubscriptionOrder;
using eMarket.Application.Subscriptions.Commands.SkipSubscription;
using eMarket.Application.Subscriptions.Commands.UpdateSubscription;
using eMarket.Application.Subscriptions.Queries.GetMySubscription;
using MediatR;
using eMarket.Api.Common.Authorization;
using eMarket.Api.Common;
using eMarket.SharedKernel.Constants;

namespace eMarket.Api.Endpoints.Subscriptions;
public static class SubscriptionEndpoints
{
 public static IEndpointRouteBuilder MapSubscriptionEndpoints(this IEndpointRouteBuilder app)
 {
  var group=app.MapGroup("/api/subscriptions").WithTags("Subscriptions");
  group.MapPost("",Create).RequireRoles(Roles.Customer);
  group.MapGet("",GetAll).RequireRoles(Roles.Customer);
  group.MapGet("/{id:guid}",Get).RequireRoles(Roles.Customer);
  group.MapPut("/{id:guid}",Update).RequireRoles(Roles.Customer);
  group.MapPost("/{id:guid}/skip",Skip).RequireRoles(Roles.Customer);
  group.MapPost("/{id:guid}/cancel",Cancel).RequireRoles(Roles.Customer);
  group.MapPost("/{id:guid}/generate-order",GenerateOrder).RequireRoles(Roles.Admin, Roles.SuperAdmin);
  return app;
 }
 static async Task<IResult> Create(CreateSubscriptionCommand command,ISender sender,CancellationToken ct)=>ToResult(await sender.Send(command,ct));
 static async Task<IResult> GetAll(ISender sender,CancellationToken ct)=>ToResult(await sender.Send(new GetMySubscriptionsQuery(),ct));
 static async Task<IResult> Get(Guid id,ISender sender,CancellationToken ct)=>ToResult(await sender.Send(new GetMySubscriptionQuery(id),ct));
 static async Task<IResult> Update(Guid id,UpdateSubscriptionCommand command,ISender sender,CancellationToken ct)=>ToResult(await sender.Send(command with { SubscriptionId=id },ct));
 static async Task<IResult> Skip(Guid id,ISender sender,CancellationToken ct)=>ToResult(await sender.Send(new SkipSubscriptionCommand(id),ct));
 static async Task<IResult> Cancel(Guid id,ISender sender,CancellationToken ct)=>ToResult(await sender.Send(new CancelSubscriptionCommand(id),ct));
 static async Task<IResult> GenerateOrder(Guid id,ISender sender,CancellationToken ct)=>ToResult(await sender.Send(new GenerateSubscriptionOrderCommand(id),ct));
 static IResult ToResult(eMarket.SharedKernel.Results.Result r)=>r.IsFailure?ApiResults.Failure(r.Error):Results.Ok();
 static IResult ToResult<T>(eMarket.SharedKernel.Results.Result<T> r)=>r.IsFailure?ApiResults.Failure(r.Error):Results.Ok(r.Value);
}
