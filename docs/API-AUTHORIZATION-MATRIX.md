# eMarket API Authorization Matrix

## System roles

| Endpoint area | Customer | Seller | StoreManager | DeliveryDriver | Admin | SuperAdmin |
|---|---:|---:|---:|---:|---:|---:|
| Auth register/login/reset | Public / own account | Public / own account | Public / own account | Public / own account | Public / own account | Public / own account |
| Businesses: create | - | Yes | - | - | Yes | Yes |
| Businesses: select | - | Yes | Yes | - | Yes | Yes |
| Businesses: my | Authenticated | Authenticated | Authenticated | Authenticated | Authenticated | Authenticated |
| Business-scoped operations | By BusinessRole permission | By BusinessRole permission | By BusinessRole permission | By BusinessRole permission | Global bypass + context | Global bypass + context |
| Products: read | Public | Public | Public | Public | Public | Public |
| Categories: read | Public | Public | Public | Public | Public | Public |
| Categories: manage | - | - | - | - | Yes | Yes |
| Cart | Yes | - | - | - | - | - |
| Orders: customer actions | Yes | - | - | - | - | - |
| Orders: processing/shipping | - | Yes | Yes | - | Yes | Yes |
| Orders: delivery | - | Yes | Yes | Yes | Yes | Yes |
| Payments: create | Yes | - | - | - | - | - |
| Subscription: customer | Yes | - | - | - | - | - |
| Subscription order generation | - | - | - | - | Yes | Yes |
| Seller dashboard | - | Yes | - | - | Yes | Yes |
| Admin dashboard | - | - | - | - | Yes | Yes |
| Assign Seller role | - | - | - | - | Yes | Yes |

## Business roles

Business roles are independent from system roles:

- Owner: full business management, catalog, orders and payments.
- Manager: day-to-day business, catalog, stock and order management; cannot transfer ownership.
- InventoryManager: products/stock and category viewing.
- Cashier: orders and payments operations.
- DeliveryManager: delivery-related order operations.
- Accountant: order/payment visibility.
- Employee: basic business/product/category visibility.

A system Customer can still be a business member with an appropriate BusinessRole. System role and BusinessRole must not be confused.
