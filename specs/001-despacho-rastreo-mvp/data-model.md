# Data Model (Fase 1): Despacho y rastreo de carga

## Entidades

| Entidad | Campos clave | Reglas |
|---------|--------------|--------|
| **Cliente** | Id, Nit, RazonSocial, Contactos[] | NIT único |
| **OrdenDeCargue** | Id, ClienteId, Origen, Destino, TipoMercancia, PesoKg, VolumenM3, FechaRequerida | Peso > 0; origen ≠ destino |
| **Viaje** | Id, Consecutivo, OrdenId, VehiculoId?, ConductorId?, Estado, FechaPlaneada, FechaEntregaReal?, TokenRastreo | Ver máquina de estados |
| **Vehiculo** | Id, Placa, Tipo, CapacidadKg, Vinculacion (Propio/Afiliado), VenceSoat, VenceRtm | Placa única; no asignable si un vencimiento < hoy |
| **Conductor** | Id, Cedula, Nombre, Celular, LicenciaCategoria, VenceLicencia | Cédula única; datos personales cifrados |
| **ManifiestoRndc** | Id, ViajeId, NumeroManifiesto?, EstadoEnvio, Intentos, UltimoError?, SolicitudJson, RespuestaJson | Un manifiesto vigente por viaje |
| **EventoDeViaje** | EventId (GUID del dispositivo), ViajeId, Tipo, Detalle?, FotoUrl?, HoraDispositivo, HoraServidor, Usuario | EventId único → idempotencia |
| **PruebaDeEntrega** | Id, ViajeId, FotosUrl[], ReceptorNombre, ReceptorDocumento, FirmaUrl, Lat, Lon, Hora | Obligatoria para pasar a *Entregado* |
| **Ubicacion** | Id, ViajeId, Lat, Lon, HoraDispositivo | Solo con viaje *En ruta* |
| **Auditoria** | Id, Entidad, EntidadId, Accion, Usuario, Fecha, Origen (Web/App/Worker) | Solo inserción |

## Máquina de estados del Viaje

```text
Planeado ──asignar──► Asignado ──confirmar despacho──► [envío RNDC]
                                                         ├─ aceptado ─► Despachado ──iniciar──► EnRuta ──entregar──► Entregado
                                                         └─ rechazado ► RechazadoRndc ──corregir y reenviar──► [envío RNDC]
Asignado ──desasignar──► Planeado
Planeado | Asignado | RechazadoRndc ──anular──► Anulado
Despachado | EnRuta ──anular (requiere anulación en RNDC)──► Anulado
```

## Relaciones

- Cliente 1—N OrdenDeCargue · OrdenDeCargue 1—1 Viaje (MVP)
- Viaje N—1 Vehiculo · Viaje N—1 Conductor
- Viaje 1—1 ManifiestoRndc vigente · Viaje 1—N EventoDeViaje · Viaje 1—N Ubicacion · Viaje 1—0..1 PruebaDeEntrega

> Diagramas detallados (incluido el ciclo del envío RNDC): `docs/architecture/05-estados.md`.
