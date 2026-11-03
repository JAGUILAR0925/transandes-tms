# Diagramas de estado

## 5.1 Ciclo de vida del Viaje

Fuente de verdad: `Viaje` en el dominio. Toda transición queda auditada (FR-011).

```mermaid
stateDiagram-v2
  direction LR
  [*] --> Planeado : CrearViaje
  Planeado --> Asignado : Asignar [política aprobada]
  Asignado --> Planeado : Desasignar
  Asignado --> Despachado : Manifiesto aceptado por RNDC
  Asignado --> RechazadoRndc : RNDC rechaza
  RechazadoRndc --> Asignado : Corregir datos y reenviar
  Despachado --> EnRuta : Conductor inicia ruta
  EnRuta --> EnRuta : Novedad / ubicación
  EnRuta --> Entregado : Prueba de entrega completa
  Planeado --> Anulado : Anular
  Asignado --> Anulado : Anular
  RechazadoRndc --> Anulado : Anular
  Despachado --> Anulado : Anular [anulación RNDC aceptada]
  EnRuta --> Anulado : Anular [anulación RNDC aceptada]
  Entregado --> [*]
  Anulado --> [*]

  note right of Asignado
    Al confirmar despacho se crea
    ManifiestoRndc (Pendiente) y el
    viaje espera aquí la respuesta
  end note
```

| Transición | Guarda | Efecto |
|------------|--------|--------|
| Planeado → Asignado | Documentos vigentes, capacidad ≥ peso, recursos libres | Vehículo y conductor quedan ocupados |
| Asignado → Despachado | Manifiesto `Aceptado` | Se guarda el número; se genera `TokenRastreo`; la app recibe el viaje |
| Asignado → RechazadoRndc | Manifiesto `Rechazado` | Mensaje legible al coordinador (SignalR) |
| EnRuta → Entregado | Prueba de entrega con fotos, receptor y firma | `TokenRastreo` expira en 30 días; recursos se liberan |
| Despachado/EnRuta → Anulado | Anulación aceptada en RNDC | Recursos se liberan |

## 5.2 Envío al RNDC (`ManifiestoRndc`)

Garantiza **cero manifiestos duplicados** aunque haya reintentos o caídas del RNDC.

```mermaid
stateDiagram-v2
  direction LR
  [*] --> Pendiente : ConfirmarDespacho (outbox)
  Pendiente --> Enviando : Worker toma el mensaje
  Enviando --> Aceptado : Respuesta OK + número
  Enviando --> Rechazado : Error de negocio (datos)
  Enviando --> ErrorTransitorio : Timeout / 5xx / sin red
  ErrorTransitorio --> Enviando : Reintento con backoff [intentos < 3]
  ErrorTransitorio --> Fallido : intentos = 3 → DLQ + alerta
  Fallido --> Pendiente : Reenvío manual (Cumplimiento)
  Rechazado --> Pendiente : Coordinador corrige y reenvía
  Aceptado --> AnulacionPendiente : AnularViaje
  AnulacionPendiente --> Anulado : RNDC confirma anulación
  Aceptado --> [*]
  Anulado --> [*]

  note right of ErrorTransitorio
    Antes de reenviar se consulta
    al RNDC por la clave de
    idempotencia: si ya existe,
    pasa directo a Aceptado
  end note
```

## 5.3 Evento offline en la app del conductor

```mermaid
stateDiagram-v2
  direction LR
  [*] --> Capturado : Conductor registra (eventId = GUID)
  Capturado --> EnCola : Guardado en SQLite
  EnCola --> Enviando : Hay conexión
  Enviando --> Confirmado : 200 OK (nuevo o repetido)
  Enviando --> EnCola : Error de red / timeout
  Enviando --> ConError : 4xx (dato inválido)
  ConError --> EnCola : Conductor corrige
  Confirmado --> [*]
```

Los adjuntos (fotos, firma) se suben a Blob con URL SAS **antes** de enviar el evento; el evento solo lleva
la URL. Si la subida falla, el evento permanece `EnCola`.
