# ADR-005: Rastreo para clientes con enlace firmado y sin cuenta

**Estado**: Propuesta · **Fecha**: 2026-11-03

## Contexto
Los clientes exigen visibilidad (Parte I) pero el portal con cuenta se pospuso a fase 2 (concesión de tiempo, Parte V).

## Decisión
Cada viaje despachado tiene un token aleatorio de 128 bits (`/t/{token}`), solo lectura, sin datos personales, que expira 30 días después de la entrega. Limitación de tasa en Front Door.

## Alternativas descartadas
- **Portal con cuenta**: Más alcance y tiempo; previsto para fase 2.
- **Enviar el estado por correo**: No da ubicación ni estado en el momento.

## Consecuencias
Visibilidad inmediata con mínimo desarrollo. Quien tenga el enlace ve el estado: se mitiga sin datos personales y con expiración.
