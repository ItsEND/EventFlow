# EventFlow Platform Templates

Пакет содержит два шаблона:

- `platform-sln` — каркас нового решения с общими Building Blocks и локальной инфраструктурой;
- `platform-service` — сервис со слоями Api, Application, Domain и Infrastructure, а также Unit- и Integration-тестами.

## Локальная установка

Из корня EventFlow:

```powershell
.\eng\Install-PlatformTemplates.ps1
```

После установки:

```powershell
dotnet new platform-sln -n Sample.Platform
cd Sample.Platform
.\eng\Add-Service.ps1 -Name Catalog
```

Параметры сервисного шаблона можно посмотреть командой:

```powershell
dotnet new platform-service --help
```
