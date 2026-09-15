# Git / GitHub setup

The repository root is the directory containing `eMarket.slnx`. Keep only this root `.git` directory.

## First push

```powershell
git remote add origin https://github.com/YOUR_USERNAME/eMarket.git
git push -u origin main
```

If `origin` already exists:

```powershell
git remote set-url origin https://github.com/YOUR_USERNAME/eMarket.git
git push -u origin main
```

## Normal workflow

```powershell
git add .
git status
git commit -m "your message"
git push
```

## Verify

```powershell
./scripts/verify-repository.ps1
```

`bin/`, `obj/`, `.vs/`, local databases, real credentials, backup folders, and nested Git repositories are excluded.

## Development secrets

Configure the local connection string, JWT key, iyzico API key/secret, and admin seed password through .NET user-secrets or environment variables. Never commit real credentials.

## Recommended local user-secrets

From the repository root:

```powershell
dotnet user-secrets --project src/eMarket.Api init
dotnet user-secrets --project src/eMarket.Api set "ConnectionStrings:DefaultConnection" "Server=BAYAN;Database=eMarket_Dev;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets --project src/eMarket.Api set "Jwt:Key" "YOUR_LONG_LOCAL_JWT_SECRET"
dotnet user-secrets --project src/eMarket.Api set "Payments:Iyzico:ApiKey" "YOUR_IYZICO_SANDBOX_API_KEY"
dotnet user-secrets --project src/eMarket.Api set "Payments:Iyzico:SecretKey" "YOUR_IYZICO_SANDBOX_SECRET_KEY"
dotnet user-secrets --project src/eMarket.Api set "AdminSeed:Password" "YOUR_LOCAL_ADMIN_PASSWORD"
```
