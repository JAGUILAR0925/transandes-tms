# ADR-004: Notificaciones en tiempo real con Azure SignalR Service

**Estado**: Propuesta · **Fecha**: 2026-11-03

## Contexto
Los coordinadores necesitan ver novedades, ubicaciones y resultados del RNDC sin recargar la página.

## Decisión
API y worker publican eventos a Azure SignalR Service; la web se suscribe por grupos (rol y viaje).

## Alternativas descartadas
- **Sondeo periódico**: Más carga en la API y retraso perceptible.
- **SignalR alojado en la API**: Conexiones persistentes complican el escalado de réplicas.

## Consecuencias
Experiencia inmediata y escalado independiente. Costo adicional de un servicio administrado.
