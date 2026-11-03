# ADR-006: Backend del MVP en .NET 8 y app en .NET 10

**Estado**: Aceptada (con excepción documentada) · **Fecha**: 2026-11-03

## Contexto
.NET 8 terminó soporte el 10-nov-2026. .NET MAUI 8 está sin soporte desde el 14-may-2025 y Google Play exige apuntar a Android 16 (API 36) desde el 31-ago-2026.

## Decisión
API, worker y librerías compartidas en `net8.0` por decisión del proyecto; app MAUI en `net10.0`. Migrar el backend a .NET 10 al inicio de la fase 2 (tasks T049–T052). Versión centralizada en `Directory.Build.props` y `global.json`.

## Alternativas descartadas
- **.NET 10 en todo desde el inicio**: Técnicamente preferible; pospuesto por decisión del líder técnico.
- **.NET 8 también en la app**: No se podría publicar ni actualizar en Google Play.

## Consecuencias
Riesgo de vulnerabilidades sin parche mientras dure el MVP. Mitigación: WAF en Front Door, escaneo de dependencias e imagen base en CI (T048), exposición mínima y migración temprana.
