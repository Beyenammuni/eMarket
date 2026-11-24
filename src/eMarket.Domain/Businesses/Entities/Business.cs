using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Businesses.Events;
using eMarket.Domain.Businesses.Rules;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Businesses.Entities;

public sealed class Business : AggregateRoot<BusinessId>
{
    private const decimal DefaultPlatformCommissionRate = 10m;
    private const decimal DefaultShippingFee = 0m;

    private readonly List<BusinessMember> _members = new();
    private readonly List<Branch> _branches = [];

    public IReadOnlyCollection<Branch> Branches =>
        _branches.AsReadOnly();
    private Business()
    {
    }

    private Business(
    BusinessId id,
    BusinessName name,
    BusinessType type,
    BusinessMerchantDetails merchantDetails)
    {
        Id = id;
        Name = name;
        Type = type;
        MerchantDetails = merchantDetails;
        Status = BusinessStatus.Pending;

        PlatformCommissionRate =
            DefaultPlatformCommissionRate;
        ShippingFee = DefaultShippingFee;
        FreeShippingThreshold = null;
        CreatedAt = DateTime.UtcNow;
    }

    public BusinessName Name { get; private set; } = default!;

    public BusinessType Type { get; private set; } = default!;

    public BusinessStatus Status { get; private set; }

    public decimal PlatformCommissionRate { get; private set; }
    public decimal ShippingFee { get; private set; }

    public decimal? FreeShippingThreshold { get; private set; }
    public string? IyzicoSubMerchantKey { get; private set; }

    public string? IyzicoSubMerchantStatus { get; private set; }
    public BusinessMerchantDetails MerchantDetails { get; private set; } = default!;

    public DateTime CreatedAt { get; private set; }

    public bool SupportsSubscriptions { get; private set; } = false;

    public bool IsApproved { get; private set; }
    public IReadOnlyCollection<BusinessMember> Members =>
        _members.AsReadOnly();

    public static Result<Business> Create(
    BusinessName name,
    BusinessType type,
    BusinessMerchantDetails merchantDetails,
    UserId ownerId)
    {
        var business = new Business(
            BusinessId.New(),
            name,
            type,
            merchantDetails);

        business.AddInitialOwner(ownerId);

        business.AddDomainEvent(
            new BusinessCreatedDomainEvent(
                business.Id));

        return Result<Business>.Success(business);
    }

    private void AddInitialOwner(UserId userId)
    {
        _members.Add(
            new BusinessMember(
                Id,
                userId,
                BusinessRole.Owner));
    }

    public Result SetPlatformCommissionRate(
        decimal commissionRate)
    {
        if (commissionRate < 0 ||
            commissionRate > 100)
        {
            return Result.Failure(
                new Error(
                    "Business.InvalidCommissionRate",
                    "Platform commission rate must be between 0 and 100."));
        }

        PlatformCommissionRate = commissionRate;

        return Result.Success();
    }
    public Result SetShippingSettings(
    decimal shippingFee,
    decimal? freeShippingThreshold)
    {
        if (shippingFee < 0)
        {
            return Result.Failure(
                new Error(
                    "Business.InvalidShippingFee",
                    "Shipping fee cannot be negative."));
        }

        if (freeShippingThreshold is < 0)
        {
            return Result.Failure(
                new Error(
                    "Business.InvalidFreeShippingThreshold",
                    "Free shipping threshold cannot be negative."));
        }

        ShippingFee = shippingFee;
        FreeShippingThreshold = freeShippingThreshold;

        return Result.Success();
    }
    public Result SetIyzicoSubMerchant(
        string subMerchantKey,
        string status)
    {
        if (string.IsNullOrWhiteSpace(subMerchantKey))
        {
            return Result.Failure(
                new Error(
                    "Business.InvalidSubMerchantKey",
                    "iyzico sub-merchant key cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            return Result.Failure(
                new Error(
                    "Business.InvalidSubMerchantStatus",
                    "iyzico sub-merchant status cannot be empty."));
        }

        IyzicoSubMerchantKey =
            subMerchantKey.Trim();

        IyzicoSubMerchantStatus =
            status.Trim();

        return Result.Success();
    }


    public Result Activate()
    {
        if (Status != BusinessStatus.Pending)
        {
            return Result.Failure(
                new Error(
                    "Business.Status.InvalidActivation",
                    "Only pending businesses can be activated."));
        }

        if (!IsApproved)
        {
            return Result.Failure(
                new Error(
                    "Business.NotApproved",
                    "Business must be approved before activation."));
        }

        if (string.IsNullOrWhiteSpace(IyzicoSubMerchantKey) ||
            !string.Equals(
                IyzicoSubMerchantStatus,
                "SUCCESS",
                StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(
                new Error(
                    "Business.PaymentSetupIncomplete",
                    "iyzico sub-merchant setup must succeed before activation."));
        }

        Status = BusinessStatus.Active;

        AddDomainEvent(
            new BusinessActivatedDomainEvent(Id));

        return Result.Success();
    }


    public Result Suspend()
    {
        if (Status == BusinessStatus.Suspended)
        {
            return Result.Failure(
                BusinessErrors.AlreadySuspended);
        }

        Status = BusinessStatus.Suspended;

        AddDomainEvent(
            new BusinessSuspendedDomainEvent(Id));

        return Result.Success();
    }

    public Result Close()
    {
        if (Status == BusinessStatus.Closed)
        {
            return Result.Failure(
                BusinessErrors.AlreadyClosed);
        }

        Status = BusinessStatus.Closed;

        AddDomainEvent(
            new BusinessClosedDomainEvent(Id));

        return Result.Success();
    }

    public Result AddMember(
        UserId userId,
        BusinessRole role)
    {
        if (new MemberCannotBeAddedTwiceRule(
                _members,
                userId)
            .IsBroken())
        {
            return Result.Failure(
                BusinessErrors.MemberAlreadyExists);
        }

        var member = new BusinessMember(
            Id,
            userId,
            role);

        _members.Add(member);

        AddDomainEvent(
            new MemberJoinedBusinessDomainEvent(
                Id,
                userId));

        return Result.Success();
    }

    public Result RemoveMember(UserId userId)
    {
        var member = GetActiveMember(userId);

        if (member is null)
        {
            return Result.Failure(
                BusinessErrors.MemberNotFound);
        }

        if (new CannotRemoveLastOwnerRule(
                _members,
                userId)
            .IsBroken())
        {
            return Result.Failure(
                BusinessErrors.CannotRemoveLastOwner);
        }

        member.Deactivate();

        AddDomainEvent(
            new MemberRemovedBusinessDomainEvent(
                Id,
                userId));

        return Result.Success();
    }

    public Result ChangeMemberRole(
        UserId userId,
        BusinessRole newRole)
    {
        var member = GetActiveMember(userId);

        if (member is null)
        {
            return Result.Failure(
                BusinessErrors.MemberNotFound);
        }

        if (member.Role == newRole)
        {
            return Result.Failure(
                BusinessErrors.MemberAlreadyHasRole);
        }

        if (member.Role == BusinessRole.Owner)
        {
            return Result.Failure(
                BusinessErrors.CannotChangeOwnerRole);
        }

        member.ChangeRole(newRole);

        AddDomainEvent(
            new MemberRoleChangedDomainEvent(
                Id,
                userId,
                newRole));

        return Result.Success();
    }

    public Result Update(
        BusinessName name,
        BusinessType type)
    {
        Name = name;
        Type = type;

        return Result.Success();
    }

    public Result TransferOwnership(
        UserId currentOwnerId,
        UserId newOwnerId)
    {
        if (currentOwnerId == newOwnerId)
        {
            return Result.Failure(
                BusinessErrors.TransferToSameOwner);
        }

        var currentOwner = _members.FirstOrDefault(x =>
            x.UserId == currentOwnerId &&
            x.IsActive);

        if (currentOwner is null)
        {
            return Result.Failure(
                BusinessErrors.CurrentOwnerNotFound);
        }

        if (currentOwner.Role != BusinessRole.Owner)
        {
            return Result.Failure(
                BusinessErrors.NotOwner);
        }

        var newOwner = _members.FirstOrDefault(x =>
            x.UserId == newOwnerId &&
            x.IsActive);

        if (newOwner is null)
        {
            return Result.Failure(
                BusinessErrors.NewOwnerNotFound);
        }

        currentOwner.ChangeRole(
            BusinessRole.Manager);

        newOwner.ChangeRole(
            BusinessRole.Owner);

        AddDomainEvent(
            new OwnershipTransferredDomainEvent(
                Id,
                currentOwnerId,
                newOwnerId));

        return Result.Success();
    }
public Result UpdateContactDetails(
    string email,
    string phoneNumber,
    string address,
    string city,
    string country)
    {
        var result = MerchantDetails.UpdateContactDetails(
            email,
            phoneNumber,
            address,
            city,
            country);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        MerchantDetails = result.Value;

        return Result.Success();
    }
    public Result UpdateLegalDetails(
        string legalName,
        string? identityNumber,
        string? taxNumber,
        string? taxOffice,
        string? representativeFullName,
        string? representativeIdentityNumber)
    {
        var result = MerchantDetails.UpdateLegalDetails(
            legalName,
            identityNumber,
            taxNumber,
            taxOffice);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        MerchantDetails = result.Value;
        return Result.Success();
    }

    public Result SetSubscriptionSupport(bool enabled)
    {
        SupportsSubscriptions = enabled;
        return Result.Success();
    }

    private BusinessMember? GetActiveMember(
        UserId userId)
    {
        return _members.FirstOrDefault(x =>
            x.UserId == userId &&
            x.IsActive);
    }
    public Result Approve()
    {
        if (Status != BusinessStatus.Pending)
        {
            return Result.Failure(
                new Error(
                    "Business.Status.InvalidApproval",
                    "Only pending businesses can be approved."));
        }

        if (IsApproved)
        {
            return Result.Failure(
                new Error(
                    "Business.AlreadyApproved",
                    "This business is already approved."));
        }

        IsApproved = true;

        return Result.Success();
    }
    public Result AddBranch(Branch branch)
    {
        if (branch.BusinessId != Id)
        {
            return Result.Failure(
                new Error(
                    "Business.Branch.InvalidBusiness",
                    "Branch does not belong to this business."));
        }

        if (_branches.Any(x => x.IsMain) && branch.IsMain)
        {
            return Result.Failure(
                new Error(
                    "Business.Branch.MainAlreadyExists",
                    "Business already has a main branch."));
        }

        if (_branches.Any(x => x.Id == branch.Id))
        {
            return Result.Failure(
                new Error(
                    "Business.Branch.AlreadyExists",
                    "Branch already exists."));
        }

        _branches.Add(branch);

        return Result.Success();
    }
    public Result DeactivateBranch(BranchId branchId)
    {
        var branch = _branches.FirstOrDefault(x => x.Id == branchId);

        if (branch is null)
        {
            return Result.Failure(
                new Error(
                    "Business.Branch.NotFound",
                    "Branch was not found."));
        }

        if (branch.IsMain)
        {
            return Result.Failure(
                new Error(
                    "Business.Branch.CannotDeactivateMain",
                    "The main branch cannot be deactivated."));
        }

        branch.Deactivate();

        return Result.Success();
    }
    public Result ActivateBranch(BranchId branchId)
    {
        var branch = _branches.FirstOrDefault(x => x.Id == branchId);

        if (branch is null)
        {
            return Result.Failure(
                new Error(
                    "Business.Branch.NotFound",
                    "Branch was not found."));
        }

        branch.Activate();

        return Result.Success();
    }
    private bool HasActiveMember(

        UserId userId)
    {
        return _members.Any(x =>
            x.UserId == userId &&
            x.IsActive);
    }

    private BusinessMember? GetOwner()
    {
        return _members.FirstOrDefault(x =>
            x.Role == BusinessRole.Owner &&
            x.IsActive);
    }

}
