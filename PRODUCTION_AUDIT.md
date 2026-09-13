# Production Audit

## Fixed in this build

### Security
- Removed committed production connection string and JWT signing key.
- Added startup validation for connection string and JWT configuration.
- JWT expiration is configurable and no longer hard-coded.
- Added global API rate limiting and stricter authentication throttling.
- Added configuration-driven CORS.
- Added reverse-proxy forwarded headers support.
- Added security response headers.
- Added global exception handling without leaking exception details in Production.
- Added concurrency conflict handling with HTTP 409.
- Added webhook secret verification using constant-time comparison.
- Restricted payment return URLs to configured hosts.
- Production rejects the mock payment provider.

### Reliability
- Added SQL Server rowversion concurrency tokens to Product, Order and Subscription.
- Restored EF migration files that were missing from the previous authorization package.
- Added liveness/readiness health checks.
- Added non-root Docker deployment.
- Added CI warning-as-error build, tests, vulnerability audit and Docker build.

### Authorization
- Synchronized application/shared permission names.
- Added business permission endpoint filters.
- Added defense-in-depth business permission checks in handlers.
- Corrected global role enumeration to Seller, DeliveryDriver, Admin, Customer, StoreManager and SuperAdmin.

## Remaining production gates

These are intentionally not faked or hidden:

1. **Real payment provider:** the repository contains a Mock gateway for Development only. Production requires a real provider implementation and signed webhook verification.
2. **Recurring subscription billing:** weekly order generation exists, but automatic monthly card billing is not implemented. Do not advertise automatic paid recurring billing until the provider integration is complete.
3. **Multi-vendor order ownership:** the current order model is customer-centric and does not yet persist a vendor/business ownership boundary. Seller order management must not be opened broadly until checkout is changed to create one vendor order per business or mixed-vendor carts are explicitly rejected.
4. **Production database migration:** the source contains the original EF migration chain, but the latest model changes (role assignments, password reset tokens, subscriptions and rowversion fields) still require a new reviewed EF migration generated with the installed .NET SDK. Do not use `EnsureCreated()`.
5. **Infrastructure secrets:** set the production environment variables in Coolify/secret management and never commit them.
6. **Observability:** add centralized logs/metrics/traces and an external error/uptime alerting system before a public launch.
7. **Backups:** configure SQL Server backup/restore testing and retention outside the application repository.
