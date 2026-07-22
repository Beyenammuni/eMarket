namespace eMarket.SharedKernel.Constants;

public static class Permissions
{
    public static class Products
    {
        public const string View = "products.view";
        public const string Create = "products.create";
        public const string Update = "products.update";
        public const string Delete = "products.delete";
    }

    public static class Categories
    {
        public const string View = "categories.view";
        public const string Create = "categories.create";
        public const string Update = "categories.update";
        public const string Delete = "categories.delete";
    }

    public static class Orders
    {
        public const string View = "orders.view";
        public const string Update = "orders.update";
        public const string Cancel = "orders.cancel";
    }

    public static class Stores
    {
        public const string View = "stores.view";
        public const string Create = "stores.create";
        public const string Update = "stores.update";
        public const string Delete = "stores.delete";
    }

    public static class Users
    {
        public const string View = "users.view";
        public const string Create = "users.create";
        public const string Update = "users.update";
        public const string Delete = "users.delete";
    }
}