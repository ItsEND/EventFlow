[CmdletBinding()]
param(
    [string]$Output = "artifacts/templates"
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot "templates/Platform.Templates/Platform.Templates.csproj"
$nugetConfig = Join-Path $repositoryRoot "templates/Platform.Templates/NuGet.Config"
$packageOutput = Join-Path $repositoryRoot $Output

New-Item -ItemType Directory -Path $packageOutput -Force | Out-Null

& dotnet restore $projectPath --configfile $nugetConfig
if ($LASTEXITCODE -ne 0) {
    throw "Не удалось подготовить пакет шаблонов."
}

& dotnet pack $projectPath --configuration Release --output $packageOutput --no-restore
if ($LASTEXITCODE -ne 0) {
    throw "Не удалось собрать пакет шаблонов."
}

$package = Get-ChildItem $packageOutput -Filter "EventFlow.Platform.Templates.*.nupkg" |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if ($null -eq $package) {
    throw "Собранный пакет шаблонов не найден."
}

& dotnet new install $package.FullName --force
if ($LASTEXITCODE -ne 0) {
    throw "Не удалось установить пакет шаблонов."
}

Write-Host "Шаблоны установлены:"
Write-Host "  dotnet new platform-sln -n MyPlatform"
Write-Host "  dotnet new platform-service --help"
