# FMS Backend

Fight Management System — Backend API (.NET 8)

## Architecture

Clean Architecture with 4 layers:

- **FMS.Domain** — Entities, enums, value objects, domain logic
- **FMS.Application** — Use cases, interfaces, configuration contracts
- **FMS.Infrastructure** — EF Core, PostgreSQL, MassTransit (sagas), external integrations
- **FMS.Api** — Minimal API host, SignalR hubs, health checks

## Tech Stack

- .NET 8 (LTS)
- PostgreSQL 16
- MassTransit (Sagas/State Machines) — fight lifecycle orchestration
- SignalR — realtime client updates
- EF Core 8 — persistence
- Serilog — structured logging

## Prerequisites

- .NET SDK 8.0.403+
- Docker & Docker Compose
- (Optional) VS Code with Dev Containers extension

## Quick Start

```bash
# Start PostgreSQL
docker compose up -d

# Run the API (Development mode — auto-migrates DB)
dotnet run --project src/FMS.Api/FMS.Api.csproj
```

## Endpoints

| Endpoint | Description |
|---|---|
| `GET /api/status` | System status (version, mode, environment) |
| `GET /api/bootstrap` | DB connectivity and migration status |
| `GET /health/ready` | Readiness probe (includes DB) |
| `GET /health/live` | Liveness probe |

## Configuration

Runtime mode is controlled via `Fms:Mode` in appsettings:

- **Lan** — LAN-first operation, no internet required
- **Cloud** — Cloud-ready with managed services (future)

## Environment Profiles

| Profile | File | Usage |
|---|---|---|
| Development | `appsettings.Development.json` | Local dev with auto-migration |
| Lan | `appsettings.Lan.json` | On-site LAN deployment |
| CloudReady | `appsettings.CloudReady.json` | Cloud deployment (placeholder) |
