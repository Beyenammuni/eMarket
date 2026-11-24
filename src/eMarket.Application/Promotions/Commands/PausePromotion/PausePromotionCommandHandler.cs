
using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Promotions;
using eMarket.Domain.Promotions.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;

using PromotionPermissions =
    eMarket.Application.Common.Authorization.Permissions.Promotions;

namespace eMarket.Application.Promotions.Commands.PausePromotion;

internal sealed class PausePromotionCommandHandler
    : IRequestHandler<PausePromotionCommand, Result<PausePromotionResponse>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessAuthorization _authorization;

    public PausePromotionCommandHandler(
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

    public async Task<Result<PausePromotionResponse>> Handle(
        PausePromotionCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
            return Result<PausePromotionResponse>.Failure(
                PromotionErrors.BusinessRequired);

        var authorized = await _authorization.HasPermissionAsync(
            businessId,
            PromotionPermissions.Pause,
            cancellationToken);

        if (!authorized)
            return Result<PausePromotionResponse>.Failure(
                PromotionErrors.Forbidden);

        var promotionId = PromotionId.Create(request.PromotionId);

        var promotion = await _promotionRepository.GetByIdAsync(
            promotionId,
            businessId,
            cancellationToken);

        if (promotion is null)
            return Result<PausePromotionResponse>.Failure(
                PromotionErrors.PromotionNotFound);

        try
        {
            promotion.Pause();
        }
        catch (InvalidOperationException)
        {
            return Result<PausePromotionResponse>.Failure(
                PromotionErrors.InvalidPromotion);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new PausePromotionResponse(
            promotion.Id.Value,
            promotion.Status.ToString());

        return Result<PausePromotionResponse>.Success(response);
    }
}
