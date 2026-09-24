<#
    Démarre l'environnement de dev EventCo en une seule commande :
    Docker (PostgreSQL) -> migrations EF Core -> backend (dotnet run) -> frontend (npm run dev).

    Usage : depuis la racine du repo, `pwsh -File scripts/start-dev.ps1` (ou `.\scripts\start-dev.ps1`).
#>

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

function Test-DockerRunning {
    # 2>&1 sur une commande native fait remonter stderr comme une ErrorRecord :
    # avec $ErrorActionPreference = "Stop", ça devient une exception au lieu
    # d'être simplement ignoré. On redirige donc stdout et stderr séparément,
    # et on neutralise temporairement le "Stop" par sécurité.
    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = "SilentlyContinue"
    try {
        docker info 1>$null 2>$null
        return $LASTEXITCODE -eq 0
    }
    finally {
        $ErrorActionPreference = $previousPreference
    }
}

Write-Host "==> Vérification de Docker..." -ForegroundColor Cyan
if (-not (Test-DockerRunning)) {
    Write-Host "Docker n'est pas démarré, lancement de Docker Desktop..." -ForegroundColor Yellow
    $dockerDesktopExe = "$env:ProgramFiles\Docker\Docker\Docker Desktop.exe"
    if (-not (Test-Path $dockerDesktopExe)) {
        throw "Docker Desktop introuvable à '$dockerDesktopExe'. Démarre-le manuellement puis relance ce script."
    }
    Start-Process $dockerDesktopExe

    $timeoutSeconds = 90
    $elapsed = 0
    while (-not (Test-DockerRunning)) {
        if ($elapsed -ge $timeoutSeconds) {
            throw "Docker Desktop n'a pas fini de démarrer après $timeoutSeconds secondes. Réessaie une fois Docker prêt."
        }
        Start-Sleep -Seconds 2
        $elapsed += 2
    }
    Write-Host "Docker est prêt." -ForegroundColor Green
}
else {
    Write-Host "Docker est déjà démarré." -ForegroundColor Green
}

Push-Location $repoRoot
try {
    Write-Host "==> Démarrage de PostgreSQL (docker compose)..." -ForegroundColor Cyan
    docker compose up -d postgres
    if ($LASTEXITCODE -ne 0) { throw "Échec de 'docker compose up -d postgres'." }

    Write-Host "==> Attente que PostgreSQL soit prêt (healthcheck)..." -ForegroundColor Cyan
    $timeoutSeconds = 60
    $elapsed = 0
    while ((docker inspect -f "{{.State.Health.Status}}" eventco-postgres 2>$null) -ne "healthy") {
        if ($elapsed -ge $timeoutSeconds) {
            throw "PostgreSQL n'est pas devenu 'healthy' après $timeoutSeconds secondes."
        }
        Start-Sleep -Seconds 2
        $elapsed += 2
    }
    Write-Host "PostgreSQL est prêt." -ForegroundColor Green

    Write-Host "==> Mise à jour de la base de données (migrations EF Core)..." -ForegroundColor Cyan
    dotnet ef database update --project src/EventCo.Infrastructure --startup-project src/EventCo.Api
    if ($LASTEXITCODE -ne 0) {
        throw "Échec de 'dotnet ef database update'. Si l'outil n'est pas installé : 'dotnet tool install --global dotnet-ef'."
    }
}
finally {
    Pop-Location
}

Write-Host "==> Démarrage du backend (dotnet run, profil https)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList @(
    "-NoExit", "-Command",
    "Set-Location '$repoRoot'; dotnet run --project src/EventCo.Api --launch-profile https"
) -WindowStyle Normal

Write-Host "==> Démarrage du frontend (npm run dev -- --host)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList @(
    "-NoExit", "-Command",
    "Set-Location '$repoRoot\client'; npm run dev -- --host"
) -WindowStyle Normal

Write-Host ""
Write-Host "EventCo démarre dans deux nouvelles fenêtres (backend / frontend) :" -ForegroundColor Green
Write-Host "  - API      : https://localhost:7166"
Write-Host "  - Frontend : http://localhost:5173 (et accessible sur le réseau local, cf. URL 'Network' affichée par Vite)"
Write-Host "  - pgAdmin (optionnel) : docker compose --profile tools up -d pgadmin -> http://localhost:5050"
Write-Host ""
Write-Host "Pour tester depuis un téléphone sur le même réseau : set 'Frontend:BaseUrl' (user-secrets) sur l'IP LAN du PC," -ForegroundColor DarkGray
Write-Host "sinon les liens d'invitation envoyés par email pointeront vers localhost." -ForegroundColor DarkGray
