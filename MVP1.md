# eMarket MVP 1 – Final Scope

## Authentication
- Register: Username, FirstName, LastName, Email, PhoneNumber, Password, ConfirmPassword
- Login with email/password and JWT
- Change password for authenticated users
- Forgot password with one-time hashed reset token
- Reset password with expiration and confirmation
- Reset tokens are revoked after password change/reset
- `Auth:ExposeResetToken` is development-only and must remain false in production

## Multi-vendor marketplace
- Businesses and active members
- Owner/member roles
- Business selection and business-scoped authorization
- Categories and products
- Product activation/deactivation, soft delete, stock management

## Sales
- Cart
- Orders and order lifecycle
- Mock payment gateway for development/MVP testing

## Weekly grocery subscription
- Customer creates a weekly basket for one business
- Delivery day and first delivery date
- Add/update subscription items
- View one/all subscriptions
- Skip the next delivery
- Cancel subscription
- Due subscriptions automatically generate weekly orders through a background worker every 15 minutes
- Stock is checked and decremented when a subscription order is generated
- Generated orders start in `PendingPayment`

## Deliberate MVP limitation
The payment provider is currently the existing `MockPaymentGateway`. Real recurring card billing (Stripe/PayPal/etc.) and production email delivery are not implemented yet. They should be integrated after React/frontend integration, without changing the subscription domain contract.

## Database
Run one new EF migration after pulling this version:

```powershell
dotnet ef migrations add Mvp1AuthUsernameAndSubscriptions --project src/eMarket.Infrastructure --startup-project src/eMarket.Api
dotnet ef database update --project src/eMarket.Infrastructure --startup-project src/eMarket.Api
```

The migration must be generated against your local database/model so existing users can receive a safe username value.
