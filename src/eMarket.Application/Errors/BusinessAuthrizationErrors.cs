using eMarket.SharedKernel.Results;

namespace eMarket.Application.Common.Errors;

public static class BusinessAuthorizationErrors
{
    public static readonly Error Unauthorized =
        new(
            "Business.Unauthorized",
            "User is not authenticated.");

    public static readonly Error BusinessNotFound =
        new(
            "Business.NotFound",
            "Business was not found.");

    public static readonly Error NotMember =
        new(
            "Business.NotMember",
            "User is not a member of this business.");

    public static readonly Error Forbidden =
        new(
            "Business.Forbidden",
            "You do not have permission to perform this operation.");
}
