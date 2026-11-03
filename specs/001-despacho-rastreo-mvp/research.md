# Research (Fase 0): Despacho y rastreo de carga

Formato: **Decisión** · **Razón** · **Alternativas descartadas**.

## R1. Integración con el RNDC
- **Decisión**: Encapsular el RNDC tras `IRndcGateway` (enviar remesa, enviar manifiesto, anular, consultar).
  Los envíos van a una cola (Service Bus) y un worker los procesa con reintentos exponenciales e idempotencia
  por `viajeId + tipoOperación`.
- **Razón**: Es obligación legal (principio II) y la norma está en actualización: el Ministerio de Transporte
  publicó un proyecto de resolución para actualizar el sistema RNDC. Aislarlo limita el impacto de los cambios.
- **Alternativas**: Digitación manual en el portal del RNDC (es el problema actual); llamada síncrona desde la API.
- **Pendiente**: `[NEEDS CLARIFICATION]` mecanismo técnico oficial vigente, credenciales de la empresa
  habilitada, ambiente de pruebas y esquema de mensajes. Responsable: Cumplimiento + TI.

## R2. App móvil para conductores
- **Decisión**: .NET MAUI con SQLite local y cola de sincronización.
- **Razón**: Mismo lenguaje y modelos que la API (equipo .NET); acceso a cámara, GPS y firma.
- **Alternativas**: Flutter / React Native (otro stack para el equipo); PWA (rastreo en segundo plano limitado en iOS).
- **Riesgo**: El GPS en segundo plano requiere código específico por plataforma y permisos explícitos;
  se prueba en dispositivos reales de gama media desde la fase 4.

## R3. Frecuencia de ubicación
- **Decisión**: Una ubicación cada 5 minutos solo con viaje *En ruta*.
- **Razón**: Concesión de calidad acordada en la hoja de alcance (Parte V); cuida batería y datos del conductor.
- **Alternativas**: Tiempo real (más costo y consumo; se evalúa en fase 2); integrar GPS satelital de terceros.

## R4. Autenticación
- **Decisión**: Microsoft Entra ID para personal interno. Conductores: pendiente (`FR-014`).
- **Opciones conductores**: OTP por SMS con cédula (simple, costo por mensaje) o Entra External ID (más robusto).

## R5. Enlace público de rastreo
- **Decisión**: URL con token firmado y no adivinable (`/t/{token}`), solo lectura, expira 30 días tras la entrega.
- **Razón**: El portal con cuenta quedó en fase 2; el enlace cubre la necesidad de visibilidad sin registro.

## R7. Versión de .NET: MVP en .NET 8, evolución en .NET 10
- **Decisión**: API, workers y librerías compartidas en `net8.0`; app MAUI en `net10.0`. Las librerías
  compartidas (`Domain`, contratos DTO) se compilan en `net8.0` y la app las referencia sin problema
  (un proyecto .NET 10 puede consumir librerías `net8.0`).
- **Razón**: Decisión del proyecto para el MVP. La app no puede ir en .NET 8: .NET MAUI 8 está sin soporte
  desde el 14-may-2025 y Google Play exige apuntar a Android 16 (API 36) desde el 31-ago-2026.
- **Riesgo aceptado**: .NET 8 dejó de recibir parches de seguridad el 10-nov-2026. Mitigaciones:
  imagen base de contenedor actualizada, escaneo de dependencias en CI, exposición mínima (WAF/Front Door),
  y migración temprana.
- **Plan de migración a .NET 10 (inicio de fase 2)**:
  1. Cambiar `TargetFramework` a `net10.0` en un `Directory.Build.props` central.
  2. Actualizar paquetes de Microsoft (`Microsoft.AspNetCore.*`, `Microsoft.EntityFrameworkCore.*`) a 10.x.
  3. Revisar los cambios incompatibles de ASP.NET Core y EF Core de las versiones 9 y 10.
  4. Ejecutar toda la suite de pruebas (contrato RNDC, integración, E2E) y desplegar primero en staging.
- **Facilitadores desde el día 1**: versión de .NET centralizada en `Directory.Build.props` y `global.json`;
  advertencias de obsolescencia tratadas como error; sin dependencias que no publiquen versión para .NET 10.

## R6. Mapas
- **Decisión**: Azure Maps (visualización y geocodificación).
- **Razón**: Misma suscripción y facturación de Azure; concesión de costo de la Parte V (no se construyen mapas).
- **Alternativa**: Google Maps Platform (válida si el equipo ya tiene contrato).
