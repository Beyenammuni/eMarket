using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Businesses.Commands.SelectBusiness;

internal sealed class SelectBusinessCommandHandler
    : IRequestHandler<
        SelectBusinessCommand,
        Result<SelectBusinessResponse>>
{
    private readonly IBusinessDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IJwtService _jwtService;

    public SelectBusinessCommandHandler(
        IBusinessDbContext context,
        ICurrentUser currentUser,
        IJwtService jwtService)
    {
        _context = context;
        _currentUser = currentUser;
        _jwtService = jwtService;
    }

    public async Task<Result<SelectBusinessResponse>> Handle(
        SelectBusinessCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<SelectBusinessResponse>.Failure(
                new Error(
                    "Auth.Unauthorized",
                    "User is not authenticated."));
        }

        var userId = _currentUser.UserId;

        if (userId is null)
        {
            return Result<SelectBusinessResponse>.Failure(
                new Error(
                    "Auth.UserRequired",
                    "User could not be identified."));
        }

        var businessId =
            BusinessId.Create(request.BusinessId);

        // Check that Business exists
        var business = await _context.Businesses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == businessId,
                cancellationToken);

        if (business is null)
        {
            return Result<SelectBusinessResponse>.Failure(
                new Error(
                    "Business.NotFound",
                    "Business was not found."));
        }

        // Check membership
        var member = await _context.BusinessMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.BusinessId == businessId &&
                    x.UserId == userId &&
                    x.IsActive,
                cancellationToken);

        if (member is null)
        {
            return Result<SelectBusinessResponse>.Failure(
                new Error(
                    "Business.Forbidden",
                    "You are not an active member of this business."));
        }

        // Generate JWT with selected Business
        var token = _jwtService.GenerateToken(
            userId,
            _currentUser.Email ?? string.Empty,
            _currentUser.Roles,
            businessId,
            member.Role);

        return Result<SelectBusinessResponse>.Success(
            new SelectBusinessResponse(
                business.Id.Value,
                business.Name.Value,
                member.Role.Name,
                token));
    }
}
