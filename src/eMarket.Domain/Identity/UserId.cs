using eMarket.SharedKernel.Common;
namespace eMarket.Domain.Identity;

    public sealed record UserId(Guid Value)
        : StronglyTypedId(Value)
    {
        public static UserId New()
            => new(Guid.NewGuid());
    }
