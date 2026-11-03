# Constitución de TransAndes TMS

> Ejemplo didáctico. TransAndes Carga S.A.S. es una empresa ficticia y las cifras son ilustrativas.
> Origen: *Hoja de trabajo para aclarar el alcance del proyecto* (Partes I–V).

## Principios fundamentales

### I. La especificación manda
Ningún código se escribe sin una historia de usuario en `spec.md` con criterios de aceptación.
Si el código y la especificación difieren, se corrige primero la especificación y luego el código.
Toda ambigüedad se marca con `[NEEDS CLARIFICATION]` y se resuelve antes de `/speckit.plan`.

### II. Cumplimiento normativo primero (NO NEGOCIABLE)
Ningún viaje puede quedar en estado *Despachado* sin manifiesto electrónico de carga aceptado por el
RNDC del Ministerio de Transporte. Las reglas del RNDC viven en un único módulo (`Rndc`) aislado detrás
de una interfaz, para absorber cambios normativos sin tocar el resto del sistema.

### III. Pruebas antes de implementar
TDD obligatorio en el dominio y en la integración RNDC: la prueba se escribe, falla y luego se implementa.
Cada historia de usuario tiene pruebas de integración que la validan de forma independiente.

### IV. Diseñado para conductores sin señal
La app móvil debe funcionar sin conexión: los eventos del viaje se guardan localmente y se sincronizan
con idempotencia (cada evento lleva un `eventId` único generado en el dispositivo).

### V. Simplicidad y entregas incrementales
Monolito modular antes que microservicios. Cada historia P1…P5 es un incremento desplegable.
Cualquier componente nuevo de infraestructura se justifica en *Complexity Tracking* del plan.

### VI. Observabilidad y trazabilidad
Logs estructurados con `correlationId` por viaje. Cada cambio de estado de un viaje queda auditado
(quién, cuándo, desde dónde). Los envíos al RNDC guardan la solicitud y la respuesta.

## Restricciones técnicas

- Stack: .NET (LTS vigente) para API y app móvil (.NET MAUI), Angular para la web, Azure para hospedaje.
- **Excepción aprobada (v1.1.0):** el backend del MVP (API, workers y librerías compartidas) se construye en
  **.NET 8** aunque su soporte terminó el 10-nov-2026. Condiciones: (a) la app móvil va en .NET 10 (MAUI 8
  no tiene soporte ni cumple el nivel de API exigido por Google Play); (b) no usar APIs obsoletas en .NET 10;
  (c) migrar el backend a .NET 10 como primera entrega de la fase 2, antes de añadir funcionalidades nuevas.
- Datos personales de conductores y clientes tratados conforme a la Ley 1581 de 2012 (habeas data):
  cifrado en tránsito y en reposo, acceso mínimo por rol.
- Secretos solo en Azure Key Vault; nunca en el repositorio.
- Presupuesto tope del proyecto: COP 450 millones (objetivo 5 de la hoja de alcance).

## Flujo de desarrollo y calidad

- Ramas por historia: `001-despacho-rastreo-mvp/US1-…`. PR con al menos una revisión.
- Puertas de calidad en CI: compilación, pruebas, cobertura ≥ 80 % en `Domain` y `Rndc`, análisis estático.
- Fuera de alcance (requiere solicitud de cambio): mantenimiento de flota, combustible, liquidación de pagos
  a conductores, optimización de rutas con IA, integración con ERP de clientes, carga internacional.

## Gobierno

Esta constitución prevalece sobre cualquier otra práctica. Las enmiendas requieren aprobación del líder
técnico y del dueño de producto, y un incremento de versión (MAJOR.MINOR.PATCH).

**Version**: 1.1.0 | **Ratified**: 2026-11-03 | **Last Amended**: 2026-11-03 (excepción .NET 8 para el backend del MVP)
