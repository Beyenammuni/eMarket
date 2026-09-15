using eMarket.Domain.Businesses.ValueObjects;
using static eMarket.Application.Common.Authorization.Permissions;

namespace eMarket.Application.Common.Authorization;

public static class BusinessRolePermissions
{
    private static readonly IReadOnlyDictionary<int, HashSet<string>> PermissionsMap =
        new Dictionary<int, HashSet<string>>
        {
            [BusinessRole.Owner.Id] =
            [
                Business.View,
                Business.Update,
                Business.ManageMembers,
                Business.ManageRoles,
                Business.TransferOwnership,

                Products.View,
                Products.Create,
                Products.Update,
                Products.Delete,
                Products.Activate,
                Products.Deactivate,
                Products.ManageStock,

                Categories.View,
                Categories.Create,
                Categories.Update,
                Categories.Delete,
                Categories.Activate,
                Categories.Deactivate,

                Orders.View,
                Orders.Manage,
                Orders.Cancel,
                Orders.Deliver,

                Payments.View,
                Payments.Manage
            ],

            [BusinessRole.Manager.Id] =
            [
                Business.View,
                Business.Update,
                Business.ManageMembers,

                Products.View,
                Products.Create,
                Products.Update,
                Products.Activate,
                Products.Deactivate,
                Products.ManageStock,

                Categories.View,
                Categories.Create,
                Categories.Update,
                Categories.Activate,
                Categories.Deactivate,

                Orders.View,
                Orders.Manage,
                Orders.Cancel,
                Orders.Deliver,

                Payments.View
            ],

            [BusinessRole.InventoryManager.Id] =
            [
                Business.View,
                Products.View,
                Products.ManageStock,
                Categories.View
            ],

            [BusinessRole.Cashier.Id] =
            [
                Business.View,
                Products.View,
                Orders.View,
                Orders.Manage,
                Payments.View,
                Payments.Manage
            ],

            [BusinessRole.DeliveryManager.Id] =
            [
                Business.View,
                Orders.View,
                Orders.Deliver
            ],

            [BusinessRole.Accountant.Id] =
            [
                Business.View,
                Orders.View,
                Payments.View
            ],

            [BusinessRole.Employee.Id] =
            [
                Business.View,
                Products.View,
                Categories.View
            ]
        };

    public static bool HasPermission(
        BusinessRole role,
        string permission)
        => PermissionsMap.TryGetValue(role.Id, out var permissions) &&
           permissions.Contains(permission);
}
