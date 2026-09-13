# eMarket

Production-oriented multi-vendor grocery marketplace backend built with **.NET 10**, Clean Architecture, DDD, CQRS, MediatR, EF Core and SQL Server.

## Included

- JWT authentication and persistent roles
- Business/store management and business-level authorization
- Product catalog and inventory
- Customer cart and orders
- Payment abstraction with development-only mock gateway
- Weekly subscription engine
- Admin dashboard API
- SQL Server persistence and EF migrations
- Concurrency protection for inventory/orders/subscriptions
- Global exception handling and ProblemDetails
- Rate limiting, CORS, security headers and health checks
- Docker image for reverse-proxy platforms such as Coolify
- CI build, tests, dependency vulnerability audit and container build

## Production gate

The API intentionally refuses to start in Production when the payment provider is still `Mock` or when production payment settings are missing. A real provider with signed webhook verification, idempotency and refunds must be connected before accepting real payments.

See `PRODUCTION_READINESS.md` for deployment, secrets, database migration and marketplace authorization requirements.
