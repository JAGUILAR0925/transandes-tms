# TransAndes TMS

Aplicación de gestión de transporte de mercancías (TMS) hecha con IA usando la metodología **Spec-Driven Development**.

> Ejemplo didáctico. TransAndes Carga S.A.S. es una empresa ficticia; las cifras son ilustrativas.

Paquete de especificación en la estructura de **GitHub Spec Kit**, construido a partir de la
*Hoja de trabajo para aclarar el alcance del proyecto*.

## Estado del código

| Componente | Estado |
|------------|--------|
| Solución .NET 8 (`src/`, `tests/`) | ✅ Esqueleto con arquitectura limpia; compila con análisis estricto |
| Dominio: máquina de estados del `Viaje` | ✅ Implementada con 9 pruebas (T007–T008) |
| API (`/health`) y Worker RNDC | 🟡 Esqueleto |
| Web Angular, app MAUI 10, infraestructura Bicep | ⏳ Pendiente (ver README de cada carpeta) |

## Requisitos y compilación

- SDK de **.NET 8** (fijado en `global.json`; la versión de framework está en `Directory.Build.props`).

```bash
dotnet restore TransAndes.Tms.sln
dotnet build TransAndes.Tms.sln
dotnet test TransAndes.Tms.sln
dotnet run --project src/TransAndes.Api   # GET /health
```

Reglas de compilación: advertencias como errores, analizadores `latest-recommended` y uso de APIs
obsoletas como error (para facilitar la migración a .NET 10, ADR-006).

## Estructura del código

```text
src/
├── TransAndes.Domain/          # Entidades y reglas (sin dependencias)
├── TransAndes.Application/     # Casos de uso y puertos (IRndcGateway…)
├── TransAndes.Infrastructure/  # EF Core, Blob, Key Vault (pendiente)
├── TransAndes.Rndc/            # Adaptador RNDC (pendiente de aclaración)
├── TransAndes.Api/             # ASP.NET Core 8
├── TransAndes.Workers/         # Worker RNDC
├── TransAndes.Mobile/          # .NET MAUI 10 (pendiente)
└── transandes-web/             # Angular (pendiente)
tests/                          # xUnit por capa
infra/                          # Bicep (pendiente)
```

## De la hoja de alcance a los artefactos SDD

| Hoja de alcance | Artefacto SDD |
|-----------------|---------------|
| Parte I – Problema y necesidades | `spec.md` → Input, historias de usuario |
| Parte II – Alternativa elegida (app a medida) | `plan.md` → Summary; `research.md` |
| Parte III – Partes interesadas y criterios | `spec.md` → roles (FR-013) y Success Criteria |
| Parte IV – Objetivos SMART | `spec.md` → SC-001 … SC-004 |
| Parte V – Concesiones y alcance | `constitution.md` (fuera de alcance), `research.md` R3/R5/R6, `spec.md` Assumptions |

## Estructura

```text
.specify/memory/constitution.md          # Principios no negociables del proyecto
specs/001-despacho-rastreo-mvp/
├── spec.md          # QUÉ y POR QUÉ: historias P1–P5, FR, SC, supuestos
├── plan.md          # CÓMO: stack .NET + Angular + Azure, arquitectura, fases
├── research.md      # Decisiones técnicas y alternativas descartadas
├── data-model.md    # Entidades y máquina de estados del viaje
├── contracts/api.yaml  # Contrato OpenAPI (extracto)
├── quickstart.md    # Cómo validar el MVP de punta a punta
└── tasks.md         # 52 tareas por historia, con paralelismo [P]
docs/architecture/
├── README.md        # Propuesta arquitectónica: impulsores, calidad, seguridad, riesgos
├── 01…03-c4-*.md    # C4 niveles 1-3 (contexto, contenedores, componentes)
├── 04-clases.md     # Modelo de dominio, puertos y casos de uso
├── 05-estados.md    # Viaje, envío RNDC, evento offline
├── 06-secuencias.md # Despacho + RNDC, sincronización offline, rastreo público
├── adr/             # 6 decisiones de arquitectura
└── diagrams/        # Fuentes .mmd y exportaciones .svg
```

**Versiones de .NET:** backend del MVP en .NET 8 (excepción documentada) y app MAUI en .NET 10.
El backend migra a .NET 10 al inicio de la fase 2 (`research.md` R7, `tasks.md` fase 9).

## Cómo usarlo con un agente (Claude Code, Copilot, etc.)

1. Copia esta carpeta en la raíz del repositorio.
2. Resuelve primero los `[NEEDS CLARIFICATION]` (con Spec Kit: `/speckit-clarify`).
3. Revisa `plan.md` y `tasks.md`; luego pide al agente implementar por fases
   (con Spec Kit: `/speckit-implement`), validando cada *Checkpoint*.
4. Al terminar, verifica que código y especificación sigan alineados (`/speckit-converge`).

Flujo completo de Spec Kit: `/speckit-constitution` → `/speckit-specify` → `/speckit-clarify` →
`/speckit-plan` → `/speckit-tasks` → `/speckit-implement` → `/speckit-converge`.

## Aclaraciones abiertas

1. Mecanismo técnico oficial del RNDC (servicio, esquema, ambiente de pruebas, credenciales).
2. Autenticación de conductores (FR-014).
3. Plazo de retención de datos (FR-015).
4. Conductores sin smartphone (edge case en `spec.md`).
