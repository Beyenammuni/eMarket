# eMarket MVP 1 - Admin Dashboard API

## Endpoint

`GET /api/admin/dashboard?days=30`

Requires JWT role `Admin` or `SuperAdmin`.

## Included KPIs

- Users: total, active, new in selected period
- Businesses: total, active
- Products: total, active, low stock, out of stock
- Orders: total, today, and every lifecycle status
- Revenue: total and today
- Payments: total, successful, failed
- Subscriptions: active, cancelled, due in next 7 days
- Revenue/order trend for 7-90 days
- Last 10 orders
- Top 10 products by quantity sold and revenue

## Admin role setup

Run `MVP1_ADMIN_SETUP.sql` after applying EF migrations. Replace `YOUR-ADMIN-EMAIL@example.com` with the admin account email.

Then login again so the new Admin role is included in the JWT.
