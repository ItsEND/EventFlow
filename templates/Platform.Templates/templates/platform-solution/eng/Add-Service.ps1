[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern("^[A-Za-z][A-Za-z0-9]*$")]
    [string]$Name,

    [ValidateSet("postgresql", "sqlserver", "none")]
    [string]$Database = "postgresql",

    [ValidateRange(1024, 65535)]
    [int]$HttpPort = 0,

    [switch]$NoRedis,
    [switch]$NoKafka,
    [switch]$NoOutbox,
    [switch]$NoInbox,

    [string]$TemplateHive = $env:PLATFORM_TEMPLATE_HIVE
)

$ErrorActionPreference = "Stop"

$solutionRoot = Split-Path -Parent $PSScriptRoot
$platformFile = Join-Path $solutionRoot ".platform.json"

if (-not (Test-Path $platformFile)) {
    throw "Файл .platform.json не найден. Запускайте команду из созданного platform-sln решения."
}

$platform = Get-Content $platformFile -Raw | ConvertFrom-Json
$solutionPath = Join-Path $solutionRoot $platform.solution
$serviceOutput = Join-Path $solutionRoot "src/Services/$Name"

if (Test-Path $serviceOutput) {
    throw "Сервис '$Name' уже существует: $serviceOutput"
}

if ($HttpPort -eq 0) {
    $HttpPort = [int]$platform.nextHttpPort
}

$redisEnabled = (-not $NoRedis).ToString().ToLowerInvariant()
$kafkaEnabled = (-not $NoKafka).ToString().ToLowerInvariant()
$outboxEnabled = (-not $NoOutbox -and -not $NoKafka -and $Database -ne "none").ToString().ToLowerInvariant()
$inboxEnabled = (-not $NoInbox -and -not $NoKafka -and $Database -ne "none").ToString().ToLowerInvariant()

$hiveArguments = @()
if (-not [string]::IsNullOrWhiteSpace($TemplateHive)) {
    $hiveArguments = @("--debug:custom-hive", (Resolve-Path $TemplateHive).Path)
}

$dotnetArguments = @(
    "new", "platform-service",
    "--name", $Name,
    "--output", $solutionRoot,
    "--product", $platform.productName,
    "--database", $Database,
    "--redis", $redisEnabled,
    "--kafka", $kafkaEnabled,
    "--outbox", $outboxEnabled,
    "--inbox", $inboxEnabled,
    "--httpPort", $HttpPort,
    "--no-update-check"
)

$dotnetArguments += $hiveArguments

& dotnet @dotnetArguments
if ($LASTEXITCODE -ne 0) {
    throw "Не удалось сгенерировать сервис '$Name'."
}

$serviceProjects = Get-ChildItem $serviceOutput -Recurse -Filter "*.csproj"
$unitProjects = Get-ChildItem (Join-Path $solutionRoot "Tests/Unit") -Recurse -Filter "*$Name*.csproj"
$integrationProjects = Get-ChildItem (Join-Path $solutionRoot "Tests/Integration") -Recurse -Filter "*$Name*.csproj"

function Add-ProjectsToSolution {
    param(
        [System.IO.FileInfo[]]$ProjectFiles,
        [string]$SolutionFolder
    )

    if ($ProjectFiles.Count -eq 0) {
        throw "Не найдены проекты для папки решения '$SolutionFolder'."
    }

    $projectPaths = @($ProjectFiles | ForEach-Object { $_.FullName })
    & dotnet sln $solutionPath add @projectPaths --solution-folder $SolutionFolder --include-references false
    if ($LASTEXITCODE -ne 0) {
        throw "Сервис создан, но проекты не удалось добавить в папку решения '$SolutionFolder'."
    }
}

Add-ProjectsToSolution -ProjectFiles @($serviceProjects) -SolutionFolder "Services/$Name"
Add-ProjectsToSolution -ProjectFiles @($unitProjects) -SolutionFolder "Tests/Unit"
Add-ProjectsToSolution -ProjectFiles @($integrationProjects) -SolutionFolder "Tests/Integration"

$targetsDirectory = Join-Path $solutionRoot "deploy/prometheus/targets"
New-Item -ItemType Directory -Path $targetsDirectory -Force | Out-Null

$target = @(
    [ordered]@{
        targets = @("host.docker.internal:$HttpPort")
        labels = [ordered]@{
            service = $Name
        }
    }
)

$target | ConvertTo-Json -Depth 4 |
    Set-Content (Join-Path $targetsDirectory "$Name.json") -Encoding utf8

if ($HttpPort -ge [int]$platform.nextHttpPort) {
    $platform.nextHttpPort = $HttpPort + 1
}

$platform | ConvertTo-Json -Depth 4 |
    Set-Content $platformFile -Encoding utf8

Write-Host ""
Write-Host "Сервис '$Name' создан на порту $HttpPort."
Write-Host "Проекты добавлены в $($platform.solution)."
Write-Host "Проверка: dotnet build `"$($platform.solution)`""
