using eMarket.Domain.Businesses.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Businesses.Commands.CreateBusiness
{
    public sealed record CreateBusinessCommand(string Name, int Type) : IRequest<Result<CreateBusinessResponse>>;
}
