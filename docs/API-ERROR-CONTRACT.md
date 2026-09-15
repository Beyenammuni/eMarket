# eMarket API Error Contract

All application-level failures are returned as RFC 7807-style `application/problem+json`.

Example:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to perform this operation.",
  "errorCode": "Business.Forbidden",
  "traceId": "00-..."
}
```

## Status rules

- `400` malformed request, invalid input, missing required business context.
- `401` unauthenticated or invalid/expired JWT.
- `403` authenticated but not authorized.
- `404` resource does not exist.
- `409` business/data conflict or concurrency conflict.
- `429` rate limit exceeded.
- `500` unexpected server error; internal exception details are never exposed in production.

Validation exceptions are handled globally and returned with `errors` grouped by property.
