namespace eMarket.SharedKernel.Constants;

/// <summary>
/// Backward-compatible permission constants used by application handlers.
/// The canonical permission set is also exposed from eMarket.Application.Common.Authorization.Permissions.
/// Keep the string values identical across both classes.
/// </summary>
public static class Permissions
{
    public static class Products
    {
        public const string View = "Products.View";
        public const string Create = "Products.Create";
        public const string Update = "Products.Update";
        public const string Delete = "Products.Delete";
        public const string Deactivate = "Products.Deactivate";
        public const string Activate = "Products.Activate";
        public const string ManageStock = "Products.ManageStock";
    }

    public static class Orders
    {
        public const string View = "Orders.View";
        public const string Manage = "Orders.Manage";
        public const string Cancel = "Orders.Cancel";
        public const string Deliver = "Orders.Deliver";
    }

    public static class Business
    {
        public const string View = "Business.View";
        public const string Manage = "Business.Manage";
        public const string Create = "Business.Create";
        public const string Update = "Business.Update";
        public const string ManageMembers = "Business.ManageMembers";
        public const string ManageRoles = "Business.ManageRoles";
        public const string TransferOwnership = "Business.TransferOwnership";
    }

    public static class Categories
    {
        public const string View = "Categories.View";
        public const string Create = "Categories.Create";
        public const string Update = "Categories.Update";
        public const string Delete = "Categories.Delete";
        public const string Activate = "Categories.Activate";
        public const string Deactivate = "Categories.Deactivate";
    }

    public static class Cart
    {
        public const string View = "Cart.View";
        public const string Manage = "Cart.Manage";
    }

    public static class Payments
    {
        public const string View = "Payments.View";
        public const string Process = "Payments.Process";
        public const string Manage = "Payments.Manage";
    }

    public static class Subscriptions
    {
        public const string View = "Subscriptions.View";
        public const string Create = "Subscriptions.Create";
        public const string Update = "Subscriptions.Update";
        public const string Skip = "Subscriptions.Skip";
        public const string Cancel = "Subscriptions.Cancel";
        public const string GenerateOrder = "Subscriptions.GenerateOrder";
    }

    public static class Admin
    {
        public const string DashboardView = "Admin.Dashboard.View";
        public const string UsersView = "Admin.Users.View";
        public const string UsersManageRoles = "Admin.Users.ManageRoles";
        public const string UsersDelete = "Admin.Users.Delete";
    }
}
