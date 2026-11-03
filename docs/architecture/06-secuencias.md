# Diagramas de secuencia

## 6.1 Confirmar despacho y reportar al RNDC (US2)

Patrón **Transactional Outbox**: el cambio de estado y el mensaje se guardan en la misma transacción;
el coordinador no espera al RNDC.

```mermaid
sequenceDiagram
  autonumber
  actor C as Coordinador
  participant W as Web (Angular)
  participant A as API TMS
  participant DB as Azure SQL
  participant WK as Worker RNDC
  participant SB as Service Bus
  participant R as RNDC
  participant S as SignalR

  C->>W: Confirmar despacho del viaje
  W->>A: POST /viajes/{id}/despacho
  A->>DB: Cargar Viaje (Asignado)
  A->>A: Validar datos completos
  rect rgb(235, 244, 255)
    note over A,DB: Una sola transacción
    A->>DB: Crear ManifiestoRndc (Pendiente)
    A->>DB: Insertar mensaje en Outbox
  end
  A-->>W: 202 Accepted
  W-->>C: "Enviando al RNDC…"

  WK->>DB: Leer Outbox pendiente
  WK->>SB: Publicar EnvioRndc(viajeId, clave)
  WK->>DB: Marcar Outbox publicado
  SB->>WK: Entregar mensaje
  WK->>DB: ManifiestoRndc → Enviando
  WK->>R: Consultar por clave de idempotencia
  alt Ya existe en RNDC
    R-->>WK: Número de manifiesto
  else No existe
    WK->>R: Enviar remesa y manifiesto
    R-->>WK: Respuesta
  end

  alt Aceptado
    WK->>DB: Manifiesto Aceptado + Viaje Despachado + solicitud/respuesta
    WK->>S: ResultadoRndc(ok, número)
    S-->>W: Notificación
    W-->>C: Viaje despachado ✔
  else Rechazado (error de datos)
    WK->>DB: Manifiesto Rechazado + Viaje RechazadoRndc
    WK->>S: ResultadoRndc(error legible)
    S-->>W: Notificación
    W-->>C: Corregir y reenviar
  else Error transitorio
    WK->>DB: Intentos + 1 (ErrorTransitorio)
    WK->>SB: Abandonar mensaje (reintento con backoff)
    note over WK,SB: Tras 3 intentos → Dead Letter + alerta a Cumplimiento
  end
```

## 6.2 Sincronización offline de eventos y prueba de entrega (US3)

```mermaid
sequenceDiagram
  autonumber
  actor D as Conductor
  participant M as App MAUI
  participant L as SQLite local
  participant B as Blob Storage
  participant A as API TMS
  participant DB as Azure SQL
  participant S as SignalR
  participant W as Web (Coordinador)

  note over D,M: Sin señal
  D->>M: Registrar entrega (fotos, receptor, firma)
  M->>L: Guardar adjuntos y evento (eventId = GUID)
  M-->>D: "Guardado — se enviará al tener señal"

  note over M,A: Conexión recuperada
  M->>A: Solicitar URLs SAS de subida
  A-->>M: URLs (corta duración)
  M->>B: Subir fotos y firma
  B-->>M: 201 Created
  M->>A: POST /viajes/{id}/eventos [lote ordenado por hora del dispositivo]
  A->>DB: Cargar Viaje
  loop Cada evento
    alt eventId nuevo
      A->>A: Viaje.RegistrarEvento() + reglas de estado
      A->>DB: Guardar evento (+ PruebaDeEntrega si aplica)
    else eventId repetido
      A->>A: Ignorar (idempotente)
    end
  end
  A-->>M: 200 OK (aceptados / repetidos)
  M->>L: Marcar eventos Confirmados
  A->>S: Evento "Viaje entregado"
  S-->>W: Actualizar tablero
```

## 6.3 Rastreo público del cliente (US4)

```mermaid
sequenceDiagram
  autonumber
  actor CL as Cliente
  participant W as Página pública (Angular)
  participant A as API TMS
  participant DB as Azure SQL
  participant MP as Azure Maps

  CL->>W: Abrir https://…/t/{token}
  W->>A: GET /t/{token} (sin sesión)
  A->>DB: Buscar viaje por token
  alt Token inválido
    A-->>W: 404
    W-->>CL: "Enlace no válido"
  else Token vencido (entrega + 30 días)
    A-->>W: 410 Gone
    W-->>CL: "Entregado" (sin ubicación)
  else Vigente
    A->>DB: Estado, última ubicación, ETA, prueba de entrega
    A-->>W: 200 (sin datos personales del conductor)
    W->>MP: Renderizar mapa con la ubicación aproximada
    W-->>CL: Estado del envío
  end
```

Controles: token aleatorio de 128 bits, limitación de tasa por IP en Front Door, sin cédula ni celular del
conductor en la respuesta, y fotos de entrega servidas con SAS de corta duración.
