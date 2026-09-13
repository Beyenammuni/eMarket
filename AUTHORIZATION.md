# eMarket Authorization

This package uses two authorization levels:

1. Global roles: `SuperAdmin`, `Admin`, `Seller`, `Customer`, `DeliveryDriver`, `StoreManager`.
2. Business permissions: permissions are evaluated against the active business membership and its `BusinessRole`.

## Global role rules

| Area | Endpoint | Permission / rule | Roles |
|---|---|---|---|
| Auth | POST /api/auth/register | Public | Anonymous |
| Auth | POST /api/auth/login | Public | Anonymous |
| Auth | POST /api/auth/forgot-password | Public | Anonymous |
| Auth | POST /api/auth/reset-password | Public | Anonymous |
| Auth | POST /api/auth/change-password | Authenticated | Any |
| Business | POST /api/businesses | Business.Create | Seller, Admin, SuperAdmin |
| Business | POST /api/businesses/{id}/select | Authenticated business selection | Seller, StoreManager, Admin, SuperAdmin |
| Business | GET /api/businesses/my | Authenticated | Any authenticated user |
| Cart | GET /api/cart | Cart.View | Customer |
| Cart | POST /api/cart/items | Cart.Manage | Customer |
| Cart | DELETE /api/cart/items/{productId} | Cart.Manage | Customer |
| Cart | DELETE /api/cart | Cart.Manage | Customer |
| Orders | POST /api/orders | Orders.Create/checkout | Customer |
| Orders | GET /api/orders | Orders.View (own orders) | Customer |
| Orders | GET /api/orders/{id} | Orders.View (own order) | Customer |
| Orders | PATCH /api/orders/{id}/pay | Payment/Order payment | Customer |
| Orders | PATCH /api/orders/{id}/processing | Orders.Manage | Seller, StoreManager, Admin, SuperAdmin |
| Orders | PATCH /api/orders/{id}/ship | Orders.Manage | Seller, StoreManager, Admin, SuperAdmin |
| Orders | PATCH /api/orders/{id}/deliver | Orders.Deliver | Seller, StoreManager, DeliveryDriver, Admin, SuperAdmin |
| Orders | PATCH /api/orders/{id}/cancel | Orders.Cancel | Customer, Seller, StoreManager, Admin, SuperAdmin |
| Payments | POST /api/payments/orders/{orderId} | Payment processing | Customer |
| Payments | POST /api/payments/webhook | Provider callback; no user role | Payment provider only |
| Subscriptions | POST /api/subscriptions | Subscriptions.Create | Customer |
| Subscriptions | GET /api/subscriptions | Subscriptions.View (own) | Customer |
| Subscriptions | GET /api/subscriptions/{id} | Subscriptions.View (own) | Customer |
| Subscriptions | PUT /api/subscriptions/{id} | Subscriptions.Update (own) | Customer |
| Subscriptions | POST /api/subscriptions/{id}/skip | Subscriptions.Skip (own) | Customer |
| Subscriptions | POST /api/subscriptions/{id}/cancel | Subscriptions.Cancel (own) | Customer |
| Subscriptions | POST /api/subscriptions/{id}/generate-order | System/admin operation | Admin, SuperAdmin |
| Admin | GET /api/admin/dashboard | Admin.Dashboard.View | Admin, SuperAdmin |

## Business permission matrix

| Business role | Main permissions |
|---|---|
| Owner | Full business/product/category/order/payment management |
| Manager | Business update/member management; product create/update/activate/stock; category create/update/activate; order management; payment view |
| InventoryManager | Business view; product view/stock; category view |
| Cashier | Business/product view; order management; payment view/manage |
| DeliveryManager | Business view; order view/deliver |
| Accountant | Business/order/payment view |
| Employee | Business/product/category view |

## Business-scoped endpoints

Business-scoped endpoints use `RequireBusinessPermission(...)`.

The filter resolves the business from:

1. route value `businessId`, when present;
2. otherwise the `businessId` claim in the selected-business JWT.

`Admin` and `SuperAdmin` bypass business membership checks.

## Important order authorization limitation

The current `Order` aggregate does not contain a `BusinessId`, and `OrderItem` also does not snapshot the owning business. Therefore seller order-status endpoints can enforce the global seller/store role but cannot yet perform a strict seller-to-order business ownership check.

For production multi-vendor order isolation, the order model should be changed to split an order by business (recommended) or otherwise persist business ownership before seller order-management authorization is finalized.
