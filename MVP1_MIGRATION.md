# Database migration for MVP 1

The source now contains the final MVP 1 model, including `Users.Username`, `PasswordResetTokens`, `Subscriptions`, and `SubscriptionItems`.

From the solution root, run:

```powershell
dotnet ef migrations add Mvp1AuthUsernameAndSubscriptions --project src/eMarket.Infrastructure --startup-project src/eMarket.Api
dotnet ef database update --project src/eMarket.Infrastructure --startup-project src/eMarket.Api
```

If your existing database already has users, inspect the generated migration before applying it. The `Username` column is required and existing rows need a unique value. A safe migration strategy is to add it nullable, populate legacy rows with a deterministic value such as `user_<first 8 chars of Id>`, then make it non-null and add the unique index.
