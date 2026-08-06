using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Businesses;

public static class BusinessErrors
{
    public static readonly Error NameIsRequired =
        new(
            "Business.Name.Required",
            "Business name is required.");

    public static readonly Error NameTooLong =
        new(
            "Business.Name.TooLong",
            "Business name cannot exceed 150 characters.");

    public static readonly Error SameName =
        new(
            "Business.Name.Same",
            "Business already has this name.");

    public static readonly Error AlreadyActive =
        new(
            "Business.Status.AlreadyActive",
            "Business is already active.");
    public static readonly Error AlreadyExists =
        new(
            "Business.Status.AlreadyExists",
            "Business is already exists.");

    public static readonly Error AlreadySuspended =
        new(
            "Business.Status.AlreadySuspended",
            "Business is already suspended.");

    public static readonly Error AlreadyClosed =
        new(
            "Business.Status.AlreadyClosed",
            "Business is already closed.");

    public static readonly Error MemberAlreadyExists =
        new(
            "Business.Member.AlreadyExists",
            "The user is already a member of this business.");

    public static readonly Error MemberNotFound =
        new(
            "Business.Member.NotFound",
            "The specified member was not found.");

    public static readonly Error NotOwner =
        new(
            "Business.Member.NotOwner",
            "Only the owner can perform this operation.");

    public static readonly Error CannotRemoveLastOwner =
        new(
            "Business.Member.LastOwner",
            "A business must always have at least one owner.");

    public static readonly Error CannotTransferOwnershipToSelf =
        new(
            "Business.Member.TransferToSelf",
            "Ownership cannot be transferred to the same user.");

    public static readonly Error NewOwnerMustBeMember =
        new(
            "Business.Member.NewOwnerNotFound",
            "The new owner must already be a member of the business.");

    public static readonly Error BusinessNotActive =
        new(
            "Business.Status.NotActive",
            "The business must be active to perform this operation.");

    public static readonly Error MemberAlreadyHasRole =
    new(
        "Business.Member.AlreadyHasRole",
        "The member already has this role.");
    public static readonly Error NotFound =
    new(
        "Business.Member.NotFound",
        "The Business not fount.");

    public static readonly Error CannotChangeOwnerRole =
        new(
            "Business.Member.CannotChangeOwnerRole",
            "Owner role can only be changed through ownership transfer.");

    public static readonly Error NewOwnerNotFound =
    new(
        "Business.Member.NewOwnerNotFound",
        "The new owner must already be a member of the business.");

    public static readonly Error CurrentOwnerNotFound =
        new(
            "Business.Member.CurrentOwnerNotFound",
            "The current owner was not found.");

    public static readonly Error TransferToSameOwner =
        new(
            "Business.Member.TransferToSameOwner",
            "Ownership cannot be transferred to the same user.");
}
