using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Businesses.Queries.GetBusinessById;

internal sealed class GetBusinessByIdHandler
    : IRequestHandler<GetBusinessByIdQuery, Result<GetBusinessByIdResponse>>
{
    private readonly IBusinessDbContext _context;

    public GetBusinessByIdHandler(IBusinessDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetBusinessByIdResponse>> Handle(
        GetBusinessByIdQuery request,
        CancellationToken cancellationToken)
    {
        var business = await _context.Businesses
            .AsNoTracking()
            .Where(x => x.Id == BusinessId.Create(request.Id))
            .ToGetBusinessByIdResponse()
            .FirstOrDefaultAsync(cancellationToken);

        if (business is null)
        {
            return Result<GetBusinessByIdResponse>.Failure(
                BusinessErrors.NotFound);
        }

        return Result<GetBusinessByIdResponse>.Success(business);
    }
}
