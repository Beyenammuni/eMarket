# eMarket Production Readiness

## What was hardened

- Secrets removed from the committed production configuration.
- JWT configuration is validated at startup; weak/missing keys fail fast.
- JWT lifetime is configurable instead of hard-coded.
- Production cannot start with the mock payment gateway.
- Payment webhooks require a shared secret and use constant-time comparison.
- Global exception handling returns safe ProblemDetails and a trace id.
- SQL Server concurrency conflicts are mapped to HTTP 409.
- Product, order, and subscription aggregates use SQL Server rowversion concurrency tokens.
- Global API rate limiting plus stricter authentication rate limiting.
- CORS is configuration-driven.
- Forwarded headers are enabled for reverse-proxy deployments such as Coolify.
- Security response headers are added.
- Liveness and readiness endpoints are available at `/health/live` and `/health/ready`.
- Swagger is disabled by default outside Development.
- A non-root Docker image is included.
- Original EF migrations are restored into the repository.

## Local development secrets

The Development configuration intentionally leaves JWT and webhook secrets empty. Configure them with .NET User Secrets:

```bash
dotnet user-secrets set "Jwt:Key" "<random-secret-at-least-32-bytes>" --project src/eMarket.Api
dotnet user-secrets set "Payments:WebhookSecret" "development-webhook-secret" --project src/eMarket.Api
```

## Required production environment variables

Set these in Coolify/your secret manager; do not commit them:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<SQL Server connection string>
Jwt__Key=<random secret, at least 32 bytes>
Jwt__Issuer=eMarket
Jwt__Audience=eMarket.Api
Cors__AllowedOrigins__0=https://your-frontend.example
Payments__Provider=<real-provider>
Payments__WebhookSecret=<provider webhook secret>
```

A real payment gateway implementation must be registered for the selected provider. The repository intentionally fails fast if Production is configured with `Mock` or with an unknown provider.

## Database deployment

Do not call `EnsureCreated()` in production. Use EF migrations or a reviewed SQL migration script.

After reviewing the generated migration:

```bash
dotnet ef migrations add ProductionHardening \
  --project src/eMarket.Infrastructure \
  --startup-project src/eMarket.Api

dotnet ef database update \
  --project src/eMarket.Infrastructure \
  --startup-project src/eMarket.Api
```

For CI/CD, prefer generating an idempotent SQL script and applying it as a controlled deployment step:

```bash
dotnet ef migrations script --idempotent \
  --project src/eMarket.Infrastructure \
  --startup-project src/eMarket.Api \
  --output artifacts/emarket.sql
```

## Important marketplace rule

The current checkout creates a customer order containing products from potentially multiple businesses, while an order does not persist a BusinessId. Seller-side order status operations therefore cannot safely determine which seller owns each order.

Before opening the marketplace to multiple sellers, choose one of these designs:

1. **Recommended:** create one child/vendor order per BusinessId during checkout and optionally keep a parent checkout/order group for the customer.
2. **Temporary restriction:** reject a cart containing products from more than one business.

Do not rely only on the seller JWT role for this authorization decision.

## Payment and subscription limitations

The mock gateway is development-only. Production needs a real provider with signed webhooks, idempotency, provider-side payment verification, and refund handling.

The subscription engine currently generates weekly orders but does not implement real recurring monthly card billing. Recurring billing must be connected to the payment provider before marketing subscriptions as an automatic paid recurring service.

### Existing database username migration

If the database was created before `Users.Username` existed, do **not** apply the generated migration blindly. SQL Server cannot add a required unique column to existing rows without a data backfill.

1. Generate the migration.
2. Review the generated `Username` operations.
3. In the migration, add `Username` as nullable first, backfill unique values, then make it non-null and create the unique index.
4. Alternatively, execute `scripts/PrepareExistingDatabase.sql` as part of a reviewed one-time deployment migration and remove the duplicate `AddColumn` operation from the generated migration.
5. Verify the duplicate query returns **zero rows** before creating the unique index.
