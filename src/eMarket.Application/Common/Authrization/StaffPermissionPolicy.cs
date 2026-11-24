
namespace eMarket.Application.Common.Authorization;

public static class StaffPermissionPolicy
{
    private static readonly HashSet<string> AssignablePermissions =
    [
        Permissions.Products.View,
        Permissions.Products.Create,
        Permissions.Products.Update,
        Permissions.Products.Delete,
        Permissions.Products.Deactivate,
        Permissions.Products.Activate,
        Permissions.Products.ManageStock,

        Permissions.Categories.View,
        Permissions.Categories.Create,
        Permissions.Categories.Update,
        Permissions.Categories.Delete,
        Permissions.Categories.Activate,
        Permissions.Categories.Deactivate,

        Permissions.Orders.View,
        Permissions.Orders.Manage,
        Permissions.Orders.Cancel,
        Permissions.Orders.Deliver,

        Permissions.Cart.View,
        Permissions.Cart.Manage,

        Permissions.Payments.View,
        Permissions.Payments.Manage,

        Permissions.Subscriptions.View,
        Permissions.Subscriptions.Create,
        Permissions.Subscriptions.Update,
        Permissions.Subscriptions.Skip,
        Permissions.Subscriptions.Cancel,
        Permissions.Subscriptions.GenerateOrder,
        Permissions.Branches.View,
        Permissions.Branches.Manage,
    ];

    public static bool CanCustomize(string permission)
    {
        return !string.IsNullOrWhiteSpace(permission) &&
               AssignablePermissions.Contains(permission);
    }
}
