# ADR-002: Integración asíncrona con el RNDC (outbox + cola + idempotencia)

**Estado**: Propuesta · **Fecha**: 2026-11-03

## Contexto
El reporte al RNDC es obligatorio (Principio II); el servicio externo puede ser lento o no estar disponible, y un reintento ingenuo podría duplicar manifiestos.

## Decisión
Confirmar despacho guarda el cambio y un mensaje en una tabla Outbox en la misma transacción. Un worker publica en Service Bus y procesa con reintentos exponenciales (máx. 3), dead letter y consulta previa por clave de idempotencia (`viajeId + operación`). El RNDC queda detrás del puerto `IRndcGateway`.

## Alternativas descartadas
- **Llamada síncrona desde la API**: Un RNDC lento bloquea al coordinador; un timeout deja el estado incierto.
- **Cola sin outbox**: Si la publicación falla después de guardar, se pierde el envío (escritura dual).

## Consecuencias
Cero envíos perdidos o duplicados y despacho no bloqueante. El coordinador ve el resultado por notificación, no en la misma respuesta. Más piezas (worker, cola) justificadas en Complexity Tracking.
