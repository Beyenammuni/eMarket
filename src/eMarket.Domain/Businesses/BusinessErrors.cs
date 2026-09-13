using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Businesses;

public static class BusinessErrors
{
    

    public static readonly Error NotFound =
        new(
            "Business.NotFound",
            "The business was not found.");

    public static readonly Error AlreadyExists =
        new(
            "Business.AlreadyExists",
            "A business with the same name already exists.");

    public static readonly Error NameIsRequired =
        new(
            "Business.Name.Required",
            "Business name is required.");

    public static readonly Error Forbidden =
        new(
            "Business.Forbidden",
            "You do not have permission to perform this operation.");

    public static readonly Error NameTooLong =
        new(
            "Business.Name.TooLong",
            "Business name cannot exceed 150 characters.");

    public static readonly Error SameName =
        new(
            "Business.Name.Same",
            "The business already has this name.");

    public static readonly Error NotActive =
        new(
            "Business.NotActive",
            "The business is not active.");

    public static readonly Error AlreadyActive =
        new(
            "Business.Status.AlreadyActive",
            "The business is already active.");

    public static readonly Error AlreadySuspended =
        new(
            "Business.Status.AlreadySuspended",
            "The business is already suspended.");

    public static readonly Error AlreadyClosed =
        new(
            "Business.Status.AlreadyClosed",
            "The business is already closed.");



    public static readonly Error Unauthorized =
        new(
            "Business.Unauthorized",
            "You are not authorized to perform this operation.");

    public static readonly Error UserNotAuthenticated =
        new(
            "Business.User.NotAuthenticated",
            "The user is not authenticated.");

    public static readonly Error UserNotMember =
        new(
            "Business.User.NotMember",
            "The user is not a member of this business.");

    public static readonly Error NotOwner =
        new(
            "Business.Authorization.NotOwner",
            "Only the business owner can perform this operation.");

    public static readonly Error InsufficientPermissions =
        new(
            "Business.Authorization.InsufficientPermissions",
            "You do not have sufficient permissions to perform this operation.");


    public static readonly Error MemberAlreadyExists =
        new(
            "Business.Member.AlreadyExists",
            "The user is already a member of this business.");

    public static readonly Error MemberNotFound =
        new(
            "Business.Member.NotFound",
            "The specified member was not found.");

    public static readonly Error MemberAlreadyHasRole =
        new(
            "Business.Member.AlreadyHasRole",
            "The member already has this role.");

    public static readonly Error CannotRemoveLastOwner =
        new(
            "Business.Member.LastOwner",
            "A business must always have at least one owner.");

    public static readonly Error CannotChangeOwnerRole =
        new(
            "Business.Member.CannotChangeOwnerRole",
            "The owner role can only be changed through ownership transfer.");

    public static readonly Error CannotRemoveOwner =
        new(
            "Business.Member.CannotRemoveOwner",
            "The business owner cannot be removed directly. Transfer ownership first.");



    public static readonly Error TransferToSameOwner =
        new(
            "Business.Ownership.TransferToSameOwner",
            "Ownership cannot be transferred to the same user.");

    public static readonly Error CurrentOwnerNotFound =
        new(
            "Business.Ownership.CurrentOwnerNotFound",
            "The current owner was not found.");

    public static readonly Error NewOwnerNotFound =
        new(
            "Business.Ownership.NewOwnerNotFound",
            "The new owner was not found.");

    public static readonly Error NewOwnerMustBeMember =
        new(
            "Business.Ownership.NewOwnerMustBeMember",
            "The new owner must already be an active member of the business.");



    public static readonly Error CannotSelectBusiness =
        new(
            "Business.Selection.NotAllowed",
            "You are not allowed to select this business.");
}
