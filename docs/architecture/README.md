# Propuesta arquitectónica — TransAndes TMS (MVP)

> Ejemplo didáctico. TransAndes Carga S.A.S. es una empresa ficticia; las cifras son ilustrativas.
> Fuente de requisitos: [`spec.md`](../../specs/001-despacho-rastreo-mvp/spec.md) ·
> Plan: [`plan.md`](../../specs/001-despacho-rastreo-mvp/plan.md) ·
> Principios: [`constitution.md`](../../.specify/memory/constitution.md) v1.1.0

**Versión**: 1.0 · **Fecha**: 2026-11-03 · **Estado**: Propuesta para revisión

## 1. Resumen

TransAndes TMS es un **monolito modular** en .NET 8 (API + worker) con una web Angular para coordinadores
y gerencia, una app **offline-first** en .NET MAUI 10 para conductores y una **integración asíncrona e
idempotente con el RNDC** del Ministerio de Transporte, hospedado en Azure. La arquitectura prioriza
cumplimiento normativo, operación sin señal y simplicidad para un equipo pequeño.

## 2. Mapa de documentos

| # | Documento | Contenido | Responde a |
|---|-----------|-----------|------------|
| 1 | [C4 nivel 1 — Contexto](01-c4-contexto.md) | Sistema, actores y sistemas externos | ¿Quién usa el sistema y con qué se integra? |
| 2 | [C4 nivel 2 — Contenedores](02-c4-contenedores.md) | Aplicaciones, datos, mensajería | ¿Qué se despliega y cómo se comunica? |
| 3 | [C4 nivel 3 — Componentes](03-c4-componentes.md) | Módulos de la API y del worker | ¿Cómo se organiza el código por dentro? |
| 4 | [Diagrama de clases](04-clases.md) | Modelo de dominio, puertos y casos de uso | ¿Cuáles son las entidades y reglas? |
| 5 | [Diagramas de estado](05-estados.md) | Viaje, envío RNDC, evento offline | ¿Qué transiciones son válidas? |
| 6 | [Diagramas de secuencia](06-secuencias.md) | Despacho + RNDC, sincronización offline, rastreo público | ¿Cómo colaboran los componentes en el tiempo? |
| 7 | [Decisiones (ADR)](adr/) | 6 decisiones con alternativas | ¿Por qué se diseñó así? |

Las fuentes Mermaid están embebidas en cada documento y también en [`diagrams/`](diagrams/) (`.mmd` + `.svg`).
GitHub y Azure DevOps renderizan Mermaid directamente.

## 3. Impulsores arquitectónicos

| Impulsor | Origen | Táctica arquitectónica |
|----------|--------|------------------------|
| Cumplimiento RNDC sin doble digitación | FR-004…006, SC-003, Principio II | Módulo `Rndc` aislado tras un puerto (`IRndcGateway`); envío por cola con outbox e idempotencia |
| Operación sin señal | FR-007, SC-006, Principio IV | App offline-first: SQLite local + cola de eventos con `eventId` generado en el dispositivo |
| Visibilidad para clientes | FR-009/010, US4 | Enlace público con token firmado; ubicaciones cada 5 min; SignalR para la web |
| Auditoría y trazabilidad | FR-006/011, Principio VI | Auditoría de solo inserción; OpenTelemetry con `correlationId` = id de viaje |
| Datos personales (Ley 1581 de 2012) | Constitución | Cifrado en reposo/tránsito, campos sensibles cifrados, RBAC, secretos en Key Vault |
| Equipo pequeño y presupuesto fijo | Objetivo 5, Principio V | Monolito modular, servicios PaaS administrados, un solo repositorio |
| Cambios normativos frecuentes | research.md R1 | Reglas RNDC en un solo módulo con pruebas de contrato contra simulador |

## 4. Estilo y vistas

- **Estilo**: monolito modular + arquitectura limpia (Domain → Application → Infrastructure/Api).
  Los módulos (`Despacho`, `Flota`, `Ejecucion`, `Rastreo`, `Rndc`, `Reportes`) se comunican por casos de uso
  y eventos de dominio en proceso; solo el RNDC cruza un límite asíncrono (Service Bus).
- **Patrones clave**: Ports & Adapters (RNDC, almacenamiento de archivos, mapas), Transactional Outbox,
  Idempotent Receiver, Retry con backoff exponencial + Dead Letter Queue, CQRS ligero (consultas de reportes
  separadas), Offline-first con sincronización por eventos.

## 5. Atributos de calidad (escenarios medibles)

| Atributo | Escenario | Medida |
|----------|-----------|--------|
| Disponibilidad | El RNDC no responde durante 2 h | El despacho se registra; los envíos se reintentan y se completan al volver, sin duplicados |
| Rendimiento | 15 coordinadores operando en hora pico | p95 < 300 ms en la API de despacho |
| Resiliencia móvil | Conductor 4 h sin señal | 0 eventos perdidos; visibles en la web < 2 min tras reconectar |
| Seguridad | Enlace público compartido por error | Solo muestra estado y ubicación aproximada; expira 30 días tras entrega; sin datos personales |
| Mantenibilidad | Cambia el esquema del RNDC | El cambio se limita al módulo `Rndc` y sus pruebas de contrato |
| Observabilidad | Un cliente reclama un viaje | Toda la traza del viaje se recupera por `correlationId` en Application Insights |

## 6. Seguridad

- **Identidad**: Microsoft Entra ID para personal interno (OIDC + JWT); conductores según FR-014
  (pendiente: OTP por SMS o Entra External ID). Roles: Coordinador, Conductor, Gerencia, Cumplimiento, Administrador.
- **Datos**: TDE en Azure SQL; cédula y celular del conductor con cifrado a nivel de columna; fotos y firmas en
  Blob privado con SAS de corta duración.
- **Perímetro**: Azure Front Door con WAF delante de la API y la web (mitigación adicional por .NET 8 sin soporte, ADR-006).
- **Secretos**: Key Vault + identidades administradas; ningún secreto en el repositorio ni en la app móvil.

## 7. Riesgos y deuda técnica

| Riesgo | Impacto | Mitigación |
|--------|---------|------------|
| Mecanismo técnico del RNDC sin confirmar (`[NEEDS CLARIFICATION]`) | Alto — bloquea US2 | Puerto `IRndcGateway` + simulador; adaptador real cuando haya especificación oficial |
| Backend en .NET 8 sin parches desde 10-nov-2026 | Medio | WAF, escaneo de dependencias, migración en fase 2 (ADR-006, tasks T049–T052) |
| GPS en segundo plano en iOS/Android | Medio | Pruebas en dispositivos reales desde la fase 4; frecuencia de 5 min |
| Adopción por conductores afiliados | Alto — afecta SC-004 | UX mínima, capacitación en piloto, registro asistido por coordinador (pendiente de decisión) |
