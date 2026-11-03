# ADR-003: App del conductor offline-first en .NET MAUI

**Estado**: Propuesta · **Fecha**: 2026-11-03

## Contexto
Los conductores recorren vías sin cobertura; SC-006 exige que los eventos lleguen sin pérdidas al recuperar señal.

## Decisión
La app guarda viajes y eventos en SQLite; cada evento lleva `eventId` (GUID) generado en el dispositivo. Una cola local reintenta el envío por lotes ordenados por hora del dispositivo; la API es idempotente por `eventId`. Adjuntos se suben primero a Blob con SAS.

## Alternativas descartadas
- **PWA**: Rastreo en segundo plano y almacenamiento limitados en iOS.
- **Flutter / React Native**: Otro lenguaje para un equipo .NET; no se comparten modelos con la API.

## Consecuencias
Funciona sin señal y reutiliza C# y librerías compartidas. Requiere probar GPS en segundo plano en dispositivos reales y gestionar conflictos de hora del dispositivo.
