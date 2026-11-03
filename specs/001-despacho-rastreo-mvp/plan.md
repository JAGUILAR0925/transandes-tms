# Implementation Plan: Despacho y rastreo de carga (MVP)

**Branch**: `001-despacho-rastreo-mvp` | **Date**: 2026-11-03 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-despacho-rastreo-mvp/spec.md`

## Summary

Sistema de gestión de transporte (TMS) para TransAndes Carga: web de despacho para coordinadores, app móvil
offline para conductores, integración automática con el RNDC y enlace público de rastreo por viaje.
Enfoque técnico: monolito modular en .NET 8 con arquitectura limpia, web en Angular, app en .NET MAUI 10 con
almacenamiento local SQLite y sincronización idempotente, todo hospedado en Azure. El backend migra a .NET 10
al iniciar la fase 2 del producto (ver research.md R7).

## Technical Context

**Language/Version**: C# / **.NET 8 (`net8.0`)** para API, workers y librerías compartidas (excepción de la
constitución v1.1.0); **.NET 10 (`net10.0-android`, `net10.0-ios`)** para la app MAUI; TypeScript para la web.

**Primary Dependencies**: ASP.NET Core Minimal APIs, EF Core, MediatR (casos de uso), FluentValidation,
Angular (versión con soporte activo; standalone components y signals), .NET MAUI, SQLite-net, SignalR.

**Storage**: Azure SQL Database (transaccional), Azure Blob Storage (fotos y firmas), SQLite en el dispositivo.

**Testing**: xUnit + FluentAssertions, Testcontainers (SQL Server) para integración, Playwright para la web,
pruebas de contrato contra un simulador del RNDC (WireMock.Net).

**Target Platform**: Azure Container Apps (API y workers); Android 10+ e iOS 16+ (app); navegadores modernos (web).

**Project Type**: Mobile + API + Web.

**Performance Goals**: p95 < 300 ms en API de despacho; ingesta de 120 vehículos × 1 ubicación/5 min (~24 req/min, holgado).

**Constraints**: offline-capable en la app; cero manifiestos duplicados en el RNDC; datos personales cifrados (Ley 1581 de 2012).

**Scale/Scope**: 120 vehículos, ~15 coordinadores, ~3.000 viajes/mes, 5 roles, ~25 pantallas web y ~8 pantallas móviles.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principio | Cumple | Evidencia |
|-----------|--------|-----------|
| I. La especificación manda | ✅ | Historias US1–US5 con escenarios; 3 `[NEEDS CLARIFICATION]` abiertos y bloqueantes solo para US2/US3 |
| II. Cumplimiento normativo primero | ✅ | Módulo `Rndc` aislado tras `IRndcGateway`; estado *Despachado* solo con manifiesto aceptado |
| III. Pruebas antes de implementar | ✅ | Pruebas de contrato RNDC y de dominio antes de implementar (ver tasks.md) |
| IV. Diseñado sin señal | ✅ | SQLite + cola de eventos con `eventId` (GUID) generado en el dispositivo |
| V. Simplicidad | ✅ | Un solo despliegue de API + un worker; sin microservicios |
| Restricción "LTS vigente" | ⚠️ Excepción | Backend en .NET 8 (sin soporte desde 10-nov-2026); justificado en Complexity Tracking |
| VI. Observabilidad | ✅ | Application Insights + OpenTelemetry, `correlationId` = id del viaje |

## Project Structure

### Documentation (this feature)

```text
specs/001-despacho-rastreo-mvp/
├── plan.md              # Este archivo
├── research.md          # Fase 0: decisiones técnicas
├── data-model.md        # Fase 1: entidades y estados
├── quickstart.md        # Fase 1: cómo validar el MVP
├── contracts/
│   └── api.yaml         # Fase 1: contrato OpenAPI (extracto)
└── tasks.md             # Fase 2: desglose de tareas
```

### Source Code (repository root)

```text
src/
├── TransAndes.Api/                 # ASP.NET Core: endpoints, auth, SignalR hub
├── TransAndes.Application/         # Casos de uso (comandos/consultas), validaciones
├── TransAndes.Domain/              # Entidades, máquina de estados del viaje, reglas
├── TransAndes.Infrastructure/      # EF Core, Blob, Key Vault, mensajería
├── TransAndes.Rndc/                # IRndcGateway, mapeo y cliente del servicio RNDC
├── TransAndes.Workers/             # Envíos RNDC con reintentos, expiración de enlaces
├── TransAndes.Mobile/              # .NET MAUI: viajes, eventos, prueba de entrega, GPS
└── transandes-web/                 # Angular: despacho, mapa, reportes, enlace público

tests/
├── TransAndes.Domain.Tests/
├── TransAndes.Application.Tests/
├── TransAndes.Rndc.ContractTests/  # contra simulador WireMock.Net
├── TransAndes.Api.IntegrationTests/
├── TransAndes.Mobile.Tests/        # cola offline y sincronización
└── transandes-web-e2e/             # Playwright

infra/
└── main.bicep                      # Azure: Container Apps, SQL, Blob, Service Bus, SignalR, Key Vault, App Insights
```

**Structure Decision**: Mobile + API + Web en un solo repositorio (monorepo). La API y los workers comparten
`Domain` y `Application`. El módulo `Rndc` es un proyecto separado para cumplir el principio II.

### Arquitectura en Azure

```text
[App MAUI] ──HTTPS──► [API · Container Apps] ──► [Azure SQL]
     │                      │   │                 [Blob Storage: fotos/firmas]
     │                      │   └─► [Service Bus: cola rndc-envios] ──► [Worker RNDC] ──► RNDC (MinTransporte)
[Web Angular · Static Web Apps] ◄─SignalR─┘
[Enlace público cliente] ──► API (endpoint anónimo con token firmado)
Identidad: Microsoft Entra ID (personal interno) · Secretos: Key Vault · Telemetría: Application Insights
```

## Fases

| Fase | Entrega | Historias | Ventana estimada |
|------|---------|-----------|------------------|
| 0 | Investigación y aclaraciones (RNDC, auth conductores, retención) | — | Nov 2026 |
| 1 | Base técnica: repo, CI/CD, infraestructura, dominio | — | Dic 2026 |
| 2 | Despacho | US1 | Ene–Feb 2027 |
| 3 | RNDC | US2 | Feb–Mar 2027 |
| 4 | App conductor y prueba de entrega | US3 | Mar–Abr 2027 |
| 5 | Rastreo y enlace al cliente | US4 | Abr–May 2027 |
| 6 | Reportes, piloto con 20 vehículos y salida a producción | US5 | May–Jun 2027 |

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Service Bus + Worker separado | Reintentos e idempotencia hacia el RNDC sin bloquear al coordinador | Llamada síncrona desde la API: un RNDC lento o caído bloquearía el despacho |
| Azure SignalR Service | Novedades y ubicaciones en la web sin recargar | Sondeo cada X segundos: más carga y peor experiencia |
| Backend en .NET 8 (sin soporte) | Decisión del líder técnico para el MVP; migración planeada a .NET 10 | Iniciar en .NET 10: preferible técnicamente, pospuesto por decisión del proyecto |
| Dos versiones de .NET en el repo (8 y 10) | MAUI 8 sin soporte desde may-2025 y sin el nivel de API que exige Google Play | App en .NET 8: no se podría publicar ni actualizar en Google Play |
