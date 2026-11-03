---
description: "Desglose de tareas del MVP de despacho y rastreo — TransAndes TMS"
---

# Tasks: Despacho y rastreo de carga (MVP)

**Input**: Design documents from `/specs/001-despacho-rastreo-mvp/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api.yaml

**Tests**: Incluidas. La constitución (principio III) exige TDD en `Domain` y `Rndc`.

**Organization**: Tareas agrupadas por historia de usuario; cada una es un incremento independiente.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: se puede hacer en paralelo (archivos distintos, sin dependencias)
- **[Story]**: historia a la que pertenece (US1…US5)

---

## Phase 1: Setup (Shared Infrastructure)

- [x] T001 Crear la solución y los proyectos según plan.md en `src/` y `tests/`; backend en `net8.0`, versión centralizada en `Directory.Build.props` y `global.json` (research.md R7)
- [ ] T002 [P] Crear el workspace Angular en `src/transandes-web/`
- [ ] T003 [P] Crear el proyecto .NET MAUI en `src/TransAndes.Mobile/` con `net10.0-android;net10.0-ios` (target Android API 36)
- [x] T004 [P] Configurar formato y análisis estático (`.editorconfig`, analizadores, ESLint)
- [x] T005 [P] Pipeline CI (compilar, pruebas, cobertura ≥ 80 % en Domain/Rndc) en `.github/workflows/ci.yml`
- [ ] T006 [P] Infraestructura Azure en `infra/main.bicep` (Container Apps, SQL, Blob, Service Bus, SignalR, Key Vault, App Insights)

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: Ninguna historia empieza antes de terminar esta fase.

- [x] T007 Prueba de la máquina de estados del viaje en `tests/TransAndes.Domain.Tests/ViajeEstadosTests.cs` (debe fallar)
- [x] T008 Implementar `Viaje` y su máquina de estados en `src/TransAndes.Domain/Viajes/Viaje.cs`
- [ ] T009 [P] Entidades Cliente, Vehiculo, Conductor, OrdenDeCargue en `src/TransAndes.Domain/`
- [ ] T010 DbContext, configuraciones y primera migración en `src/TransAndes.Infrastructure/Persistence/`
- [ ] T011 [P] Autenticación Entra ID y autorización por roles (FR-013) en `src/TransAndes.Api/Auth/`
- [ ] T012 [P] Auditoría de cambios de estado (FR-011) en `src/TransAndes.Infrastructure/Auditing/`
- [ ] T013 [P] Logs estructurados + OpenTelemetry con `correlationId` en `src/TransAndes.Api/Observability/`
- [ ] T014 [P] Cifrado de datos personales del conductor (Ley 1581) en `src/TransAndes.Infrastructure/Security/`

**Checkpoint**: Base lista; las historias pueden avanzar.

---

## Phase 3: User Story 1 - Planear y despachar un viaje (Priority: P1) 🎯 MVP

**Goal**: El coordinador crea órdenes y viajes y asigna vehículo y conductor válidos.

**Independent Test**: Crear orden → viaje → asignación; el viaje queda *Asignado*.

### Tests for User Story 1 ⚠️

- [ ] T015 [P] [US1] Pruebas de validación de asignación (documentos vencidos, capacidad, disponibilidad) en `tests/TransAndes.Domain.Tests/AsignacionTests.cs`
- [ ] T016 [P] [US1] Prueba de integración `POST /ordenes`, `POST /viajes`, `PUT /viajes/{id}/asignacion` en `tests/TransAndes.Api.IntegrationTests/DespachoTests.cs`

### Implementation for User Story 1

- [ ] T017 [US1] Casos de uso CrearOrden, CrearViaje, AsignarRecursos en `src/TransAndes.Application/Despacho/`
- [ ] T018 [US1] Endpoints de órdenes y viajes según `contracts/api.yaml` en `src/TransAndes.Api/Endpoints/`
- [ ] T019 [P] [US1] Pantalla de órdenes y tablero de viajes en `src/transandes-web/src/app/despacho/`
- [ ] T020 [P] [US1] Selector de vehículos/conductores disponibles con alertas de vencimiento en `src/transandes-web/src/app/despacho/asignacion/`

**Checkpoint**: US1 funcional y desplegable (SC-001, SC-005).

---

## Phase 4: User Story 2 - Reportar el manifiesto al RNDC (Priority: P2)

**Goal**: Envío automático e idempotente al RNDC al confirmar el despacho.

**Bloqueante**: resolver `[NEEDS CLARIFICATION]` de R1 (mecanismo y esquema oficiales del RNDC).

### Tests for User Story 2 ⚠️

- [ ] T021 [P] [US2] Pruebas de contrato contra simulador WireMock.Net (aceptado, rechazado, timeout) en `tests/TransAndes.Rndc.ContractTests/`
- [ ] T022 [P] [US2] Prueba de idempotencia: dos reintentos no generan dos manifiestos en `tests/TransAndes.Rndc.ContractTests/IdempotenciaTests.cs`

### Implementation for User Story 2

- [ ] T023 [US2] `IRndcGateway` y mapeo Viaje → remesa/manifiesto en `src/TransAndes.Rndc/`
- [ ] T024 [US2] Endpoint `POST /viajes/{id}/despacho` que encola en Service Bus en `src/TransAndes.Api/Endpoints/DespachoEndpoints.cs`
- [ ] T025 [US2] Worker con reintentos exponenciales y alerta tras 3 fallos (FR-005) en `src/TransAndes.Workers/RndcWorker.cs`
- [ ] T026 [P] [US2] Persistir solicitud/respuesta del RNDC (FR-006) en `src/TransAndes.Infrastructure/Rndc/`
- [ ] T027 [P] [US2] Notificación del resultado por SignalR y vista de errores RNDC en `src/transandes-web/src/app/rndc/`
- [ ] T028 [US2] Flujo de anulación en RNDC antes de liberar vehículo (edge case) en `src/TransAndes.Application/Despacho/AnularViaje.cs`

**Checkpoint**: Viajes despachados con manifiesto (SC-003).

---

## Phase 5: User Story 3 - App del conductor y prueba de entrega (Priority: P3)

**Goal**: Estados, novedades y prueba de entrega desde el celular, con o sin señal.

**Bloqueante parcial**: definir autenticación de conductores (FR-014).

### Tests for User Story 3 ⚠️

- [ ] T029 [P] [US3] Prueba de sincronización idempotente por `eventId` en `tests/TransAndes.Api.IntegrationTests/EventosTests.cs`
- [ ] T030 [P] [US3] Prueba de cola offline (guardar, reintentar, ordenar por hora del dispositivo) en `tests/TransAndes.Mobile.Tests/SyncQueueTests.cs`

### Implementation for User Story 3

- [ ] T031 [US3] Autenticación de conductores según decisión de FR-014 en `src/TransAndes.Api/Auth/Conductores/`
- [ ] T032 [US3] Endpoint `POST /viajes/{id}/eventos` idempotente en `src/TransAndes.Api/Endpoints/EventosEndpoints.cs`
- [ ] T033 [US3] Almacenamiento SQLite y cola de sincronización en `src/TransAndes.Mobile/Data/`
- [ ] T034 [P] [US3] Pantallas: mis viajes, detalle y registro de estados/novedades en `src/TransAndes.Mobile/Views/`
- [ ] T035 [P] [US3] Captura de prueba de entrega (fotos, receptor, firma) y carga a Blob en `src/TransAndes.Mobile/Views/PruebaEntrega/`

**Checkpoint**: Prueba de entrega digital (SC-004, SC-006).

---

## Phase 6: User Story 4 - Rastreo y enlace para el cliente (Priority: P4)

**Goal**: Mapa de viajes activos y enlace público de rastreo por viaje.

- [ ] T036 [P] [US4] Prueba de expiración del enlace a los 30 días (FR-010) en `tests/TransAndes.Api.IntegrationTests/RastreoPublicoTests.cs`
- [ ] T037 [US4] Envío de ubicación cada 5 min en segundo plano (Android/iOS) en `src/TransAndes.Mobile/Platforms/`
- [ ] T038 [US4] Endpoints `POST /viajes/{id}/ubicaciones` y `GET /t/{token}` en `src/TransAndes.Api/Endpoints/RastreoEndpoints.cs`
- [ ] T039 [P] [US4] Mapa de viajes activos con Azure Maps en `src/transandes-web/src/app/mapa/`
- [ ] T040 [P] [US4] Página pública de rastreo responsive en `src/transandes-web/src/app/rastreo-publico/`

**Checkpoint**: Clientes con visibilidad del viaje.

---

## Phase 7: User Story 5 - Reportes básicos (Priority: P5)

- [ ] T041 [P] [US5] Prueba del cálculo de % entregas a tiempo en `tests/TransAndes.Application.Tests/ReportesTests.cs`
- [ ] T042 [US5] Consultas de indicadores (FR-012) en `src/TransAndes.Application/Reportes/`
- [ ] T043 [P] [US5] Tablero de indicadores y exportación .xlsx en `src/transandes-web/src/app/reportes/`

**Checkpoint**: Medición de SC-001 a SC-005.

---

## Phase 8: Polish & Cross-Cutting Concerns

- [ ] T044 [P] Pruebas E2E del flujo completo (quickstart.md) en `tests/transandes-web-e2e/`
- [ ] T045 [P] Prueba de carga de ubicaciones (120 vehículos) y de despacho (p95 < 300 ms)
- [ ] T046 Revisión de seguridad: OWASP Top 10, permisos por rol, secretos en Key Vault
- [ ] T047 Piloto con 20 vehículos, capacitación a coordinadores y conductores, ajuste y salida a producción
- [ ] T048 [P] Escaneo de vulnerabilidades de dependencias e imagen base en CI (mitigación por .NET 8 sin soporte) en `.github/workflows/ci.yml`

---

## Phase 9: Migración del backend a .NET 10 (primera entrega de la fase 2 del producto)

**Purpose**: Cerrar la excepción de la constitución v1.1.0 antes de añadir funcionalidades nuevas.

- [ ] T049 Cambiar `TargetFramework` a `net10.0` y actualizar `global.json` en `Directory.Build.props`
- [ ] T050 Actualizar paquetes `Microsoft.AspNetCore.*` y `Microsoft.EntityFrameworkCore.*` a 10.x y corregir cambios incompatibles
- [ ] T051 Ejecutar toda la suite (contrato RNDC, integración, E2E) y desplegar en staging antes de producción
- [ ] T052 Retirar la excepción de la constitución (versión 1.2.0)

---

## Dependencies & Execution Order

- **Setup (F1)** → **Foundational (F2)** → historias.
- **US1** no depende de otras historias. **US2** depende de US1 (necesita viajes asignados).
- **US3** depende de US2 (el conductor solo ve viajes *Despachados*). **US4** depende de US3 (la app envía ubicación).
- **US5** solo depende de US1 y puede avanzar en paralelo a US2–US4 con datos de prueba.
- Dentro de cada historia: pruebas → dominio/casos de uso → endpoints → interfaz.

## Parallel Example: User Story 1

```text
En paralelo: T015 (pruebas de dominio) · T016 (prueba de integración)
Luego: T017 → T018
En paralelo: T019 (tablero web) · T020 (selector de asignación)
```

## Implementation Strategy

1. **MVP primero**: F1 + F2 + US1 → demo con coordinadores.
2. **Incremental**: US2 (requisito legal) → US3 → US4 → US5, desplegando al final de cada fase.
3. Validar cada checkpoint contra los criterios SC antes de pasar a la siguiente historia.
