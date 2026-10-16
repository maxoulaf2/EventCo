<#
    Remplace la base locale (conteneur eventco-postgres) par une copie de la base de prod Neon.

    Usage : depuis la racine du repo, `pwsh -File scripts/clone-prod-db.ps1`.
    URL de la base : variable d'environnement NEON_DATABASE_URL, sinon demandée au lancement.
    Format attendu : URI Postgres (Neon > Connect), connexion directe (hôte sans "-pooler") :
    postgresql://<user>:<password>@<host>/<db>?sslmode=require
#>

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

$sourceUrl = $env:NEON_DATABASE_URL
if (-not $sourceUrl) {
    $secure = Read-Host "URL de la base Neon (postgresql://...)" -AsSecureString
    $sourceUrl = [Net.NetworkCredential]::new("", $secure).Password
}
if ($sourceUrl -notmatch '^postgres(ql)?://') {
    throw "URL invalide : attendu un URI postgresql://... (pas la connection string ADO.NET 'Host=...;')."
}

$answer = Read-Host "La base locale 'eventco' va être écrasée. Continuer ? (o/N)"
if ($answer -ne "o") { Write-Host "Annulé."; exit 0 }

Push-Location $repoRoot
try {
    docker compose up -d --wait postgres
    if ($LASTEXITCODE -ne 0) { throw "Échec du démarrage de PostgreSQL (docker compose)." }

    Write-Host "==> Recréation de la base locale..." -ForegroundColor Cyan
    docker exec eventco-postgres psql -U eventco -d postgres -v ON_ERROR_STOP=1 `
        -c "DROP DATABASE IF EXISTS eventco WITH (FORCE);" -c "CREATE DATABASE eventco OWNER eventco;"
    if ($LASTEXITCODE -ne 0) { throw "Échec de la recréation de la base locale." }

    # Client Postgres 18 (>= version du serveur Neon, sinon pg_dump refuse) dans un conteneur jetable partageant le réseau
    # d'eventco-postgres : pas de client Postgres à installer, et le dump ne transite pas par un pipe
    # PowerShell (qui altère l'encodage sous Windows PowerShell 5.1).
    # L'URL est passée par variable d'environnement (-e sans valeur) pour ne pas apparaître dans la ligne de commande.
    Write-Host "==> Copie Neon -> local (pg_dump | psql)..." -ForegroundColor Cyan
    $env:SOURCE_URL = $sourceUrl
    docker run --rm --network container:eventco-postgres -e SOURCE_URL -e PGPASSWORD=eventco postgres:18-alpine `
        sh -c 'set -o pipefail; pg_dump --no-owner --no-acl "$SOURCE_URL" | psql -q -h localhost -U eventco -d eventco'
    if ($LASTEXITCODE -ne 0) { throw "Échec de la copie (cf. erreurs ci-dessus)." }

    Write-Host "Base locale remplacée par la copie de prod." -ForegroundColor Green
}
finally {
    Remove-Item Env:SOURCE_URL -ErrorAction SilentlyContinue
    Pop-Location
}
