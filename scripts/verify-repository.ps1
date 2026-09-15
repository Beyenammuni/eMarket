$ErrorActionPreference = 'Stop'

Write-Host 'Verifying eMarket repository...' -ForegroundColor Cyan
$root = (Get-Location).Path

$nestedGit = Get-ChildItem -Path $root -Directory -Force -Recurse -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -eq '.git' }
if ($nestedGit.Count -ne 1) { throw "Expected exactly one .git directory at repository root; found $($nestedGit.Count)." }

$generated = Get-ChildItem -Path $root -Directory -Force -Recurse -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -in @('.vs','bin','obj') }
if ($generated) { throw 'Generated/local directories (.vs/bin/obj) must not be in the source package.' }

$bad = Get-ChildItem -Path $root -Force -Recurse -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -match '\\(src-BACKUP|compare-report\.txt|compare-source-only\.txt|missing-from-src\.txt|git-files\.txt)($|\\)' }
if ($bad) { throw 'Backup/audit packaging artifacts detected.' }

$solution = Join-Path $root 'eMarket.slnx'
if (-not (Test-Path $solution)) { throw 'eMarket.slnx is missing from repository root.' }

$projects = @(Get-ChildItem -Path (Join-Path $root 'src'), (Join-Path $root 'tests') -Filter *.csproj -Recurse)
if ($projects.Count -ne 8) { throw "Expected 8 projects, found $($projects.Count)." }

$tracked = @(git ls-files)
if ($LASTEXITCODE -ne 0) { throw 'Git repository is not initialized.' }
$badTracked = $tracked | Where-Object { $_ -match '(^|/)(bin|obj|\.vs)/|src-BACKUP|(^|/)\.git/' }
if ($badTracked) { throw 'Git is tracking generated, backup, or nested repository content.' }

Write-Host "OK: $($tracked.Count) tracked files." -ForegroundColor Green
Write-Host 'OK: one Git repository, no backups, no generated build output, all 8 projects present.' -ForegroundColor Green
