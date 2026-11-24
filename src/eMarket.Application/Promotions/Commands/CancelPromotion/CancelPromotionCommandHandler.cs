
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Promotions;
using eMarket.Domain.Promotions.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;

using PromotionPermissions =
    eMarket.Application.Common.Authorization.Permissions.Promotions;

namespace eMarket.Application.Promotions.Commands.CancelPromotion;

internal sealed class CancelPromotionCommandHandler
    : IRequestHandler<CancelPromotionCommand, Result<CancelPromotionResponse>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessAuthorization _authorization;

    public CancelPromotionCommandHandler(
        IPromotionRepository promotionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IBusinessAuthorization authorization)
    {
        _promotionRepository = promotionRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    public async Task<Result<CancelPromotionResponse>> Handle(
        CancelPromotionCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
            return Result<CancelPromotionResponse>.Failure(
                PromotionErrors.BusinessRequired);

        var authorized = await _authorization.HasPermissionAsync(
            businessId,
            PromotionPermissions.Delete,
            cancellationToken);

        if (!authorized)
            return Result<CancelPromotionResponse>.Failure(
                PromotionErrors.Forbidden);

        var promotion = await _promotionRepository.GetByIdAsync(
            PromotionId.Create(request.PromotionId),
            businessId,
            cancellationToken);

        if (promotion is null)
            return Result<CancelPromotionResponse>.Failure(
                PromotionErrors.PromotionNotFound);

        try
        {
            promotion.Cancel();
        }
        catch (InvalidOperationException)
        {
            return Result<CancelPromotionResponse>.Failure(
                PromotionErrors.InvalidPromotion);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CancelPromotionResponse>.Success(
            new CancelPromotionResponse(
                promotion.Id.Value,
                promotion.Status.ToString()));
    }
}
