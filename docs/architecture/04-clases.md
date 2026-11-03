# Diagrama de clases

Modelo de dominio (DDD táctico) alineado con [`data-model.md`](../../specs/001-despacho-rastreo-mvp/data-model.md).
Equivale al nivel 4 (código) del modelo C4 para el núcleo del sistema.

## 4.1 Modelo de dominio

**Agregados**: `Viaje` (raíz; contiene eventos, ubicaciones y prueba de entrega), `ManifiestoRndc`,
`Vehiculo`, `Conductor`, `Cliente`, `OrdenDeCargue`. Entre agregados solo se referencian identificadores.

```mermaid
classDiagram
  direction LR

  class Viaje {
    <<AggregateRoot>>
    +Guid Id
    +string Consecutivo
    +Guid OrdenId
    +Guid? VehiculoId
    +Guid? ConductorId
    +EstadoViaje Estado
    +DateOnly FechaPlaneada
    +DateTime? FechaEntregaReal
    +TokenRastreo? Token
    +Asignar(Vehiculo, Conductor, PoliticaAsignacion)
    +MarcarDespachado(string numeroManifiesto)
    +MarcarRechazadoRndc(string error)
    +IniciarRuta()
    +RegistrarEvento(EventoDeViaje) bool
    +RegistrarUbicacion(Coordenada, DateTime)
    +Entregar(PruebaDeEntrega)
    +Anular(string motivo)
  }

  class EstadoViaje {
    <<enumeration>>
    Planeado
    Asignado
    RechazadoRndc
    Despachado
    EnRuta
    Entregado
    Anulado
  }

  class EventoDeViaje {
    <<Entity>>
    +Guid EventId
    +TipoEvento Tipo
    +string? Detalle
    +string? FotoUrl
    +DateTime HoraDispositivo
    +DateTime HoraServidor
    +string Usuario
  }

  class TipoEvento {
    <<enumeration>>
    EnCargue
    EnRuta
    Novedad
    EnDescargue
    Entregado
  }

  class PruebaDeEntrega {
    <<Entity>>
    +List~string~ FotosUrl
    +string ReceptorNombre
    +string ReceptorDocumento
    +string FirmaUrl
    +Coordenada Lugar
    +DateTime Hora
  }

  class Ubicacion {
    <<Entity>>
    +Coordenada Punto
    +DateTime HoraDispositivo
  }

  class Coordenada {
    <<ValueObject>>
    +decimal Lat
    +decimal Lon
  }

  class TokenRastreo {
    <<ValueObject>>
    +string Valor
    +DateTime? ExpiraEn
    +EstaVigente(DateTime ahora) bool
  }

  class ManifiestoRndc {
    <<AggregateRoot>>
    +Guid Id
    +Guid ViajeId
    +string? NumeroManifiesto
    +EstadoEnvioRndc Estado
    +int Intentos
    +string? UltimoError
    +ClaveIdempotencia Clave
    +RegistrarIntento(RespuestaRndc)
    +PuedeReintentar() bool
  }

  class EstadoEnvioRndc {
    <<enumeration>>
    Pendiente
    Enviando
    Aceptado
    Rechazado
    ErrorTransitorio
    Fallido
    AnulacionPendiente
    Anulado
  }

  class Vehiculo {
    <<AggregateRoot>>
    +Guid Id
    +Placa Placa
    +string Tipo
    +int CapacidadKg
    +Vinculacion Vinculacion
    +DateOnly VenceSoat
    +DateOnly VenceRtm
    +DocumentosVigentes(DateOnly hoy) bool
  }

  class Conductor {
    <<AggregateRoot>>
    +Guid Id
    +Cedula Cedula
    +string Nombre
    +string Celular
    +string LicenciaCategoria
    +DateOnly VenceLicencia
    +LicenciaVigente(DateOnly hoy) bool
  }

  class Cliente {
    <<AggregateRoot>>
    +Guid Id
    +Nit Nit
    +string RazonSocial
  }

  class OrdenDeCargue {
    <<AggregateRoot>>
    +Guid Id
    +Guid ClienteId
    +string Origen
    +string Destino
    +string TipoMercancia
    +int PesoKg
    +decimal VolumenM3
    +DateOnly FechaRequerida
  }

  class PoliticaAsignacion {
    <<DomainService>>
    +Validar(Viaje, Vehiculo, Conductor, OrdenDeCargue, DateOnly hoy) ResultadoValidacion
  }

  Viaje "1" *-- "0..*" EventoDeViaje : registra
  Viaje "1" *-- "0..*" Ubicacion : traza
  Viaje "1" *-- "0..1" PruebaDeEntrega : cierra con
  Viaje --> EstadoViaje
  Viaje --> TokenRastreo
  EventoDeViaje --> TipoEvento
  Ubicacion --> Coordenada
  PruebaDeEntrega --> Coordenada
  ManifiestoRndc --> EstadoEnvioRndc
  ManifiestoRndc ..> Viaje : ViajeId
  Viaje ..> OrdenDeCargue : OrdenId
  Viaje ..> Vehiculo : VehiculoId
  Viaje ..> Conductor : ConductorId
  OrdenDeCargue ..> Cliente : ClienteId
  PoliticaAsignacion ..> Vehiculo
  PoliticaAsignacion ..> Conductor
```

### Invariantes del agregado `Viaje`

1. Solo se asigna en `Planeado` y si `PoliticaAsignacion` aprueba (documentos vigentes, capacidad ≥ peso, recursos libres).
2. `Despachado` exige número de manifiesto aceptado por el RNDC (Principio II).
3. `RegistrarEvento` devuelve `false` si el `EventId` ya existe (idempotencia, FR-007).
4. `Entregado` exige `PruebaDeEntrega` completa (FR-008) y fija `TokenRastreo.ExpiraEn = entrega + 30 días`.
5. Anular desde `Despachado` o `EnRuta` exige anulación previa en el RNDC.

## 4.2 Puertos y casos de uso (capa de aplicación)

```mermaid
classDiagram
  direction LR

  class IRndcGateway {
    <<interface>>
    +EnviarRemesaAsync(MensajeRemesa) Task~RespuestaRndc~
    +EnviarManifiestoAsync(MensajeManifiesto) Task~RespuestaRndc~
    +AnularAsync(string numeroManifiesto) Task~RespuestaRndc~
    +ConsultarAsync(ClaveIdempotencia) Task~RespuestaRndc~
  }
  class RndcSoapGateway {
    <<Adapter>>
  }
  class RndcSimuladorGateway {
    <<Adapter · pruebas>>
  }
  class IViajeRepository {
    <<interface>>
    +ObtenerAsync(Guid) Task~Viaje~
    +GuardarAsync(Viaje) Task
  }
  class IOutbox {
    <<interface>>
    +Agregar(MensajeIntegracion)
  }
  class IAlmacenArchivos {
    <<interface>>
    +GenerarUrlSubida(string ruta) Uri
  }
  class INotificador {
    <<interface>>
    +NotificarAsync(string grupo, object evento) Task
  }
  class IReloj {
    <<interface>>
    +Ahora DateTime
  }

  class ConfirmarDespachoHandler {
    <<UseCase>>
    +Handle(ConfirmarDespachoCommand) Task~Resultado~
  }
  class SincronizarEventosHandler {
    <<UseCase>>
    +Handle(SincronizarEventosCommand) Task~ResultadoSync~
  }
  class AsignarRecursosHandler {
    <<UseCase>>
    +Handle(AsignarRecursosCommand) Task~Resultado~
  }
  class ProcesarEnvioRndcHandler {
    <<UseCase · Worker>>
    +Handle(EnvioRndcMessage) Task
  }

  RndcSoapGateway ..|> IRndcGateway
  RndcSimuladorGateway ..|> IRndcGateway
  AsignarRecursosHandler --> IViajeRepository
  AsignarRecursosHandler --> IReloj
  ConfirmarDespachoHandler --> IViajeRepository
  ConfirmarDespachoHandler --> IOutbox
  SincronizarEventosHandler --> IViajeRepository
  SincronizarEventosHandler --> INotificador
  SincronizarEventosHandler --> IAlmacenArchivos
  ProcesarEnvioRndcHandler --> IRndcGateway
  ProcesarEnvioRndcHandler --> IViajeRepository
  ProcesarEnvioRndcHandler --> INotificador
```

> `RndcSoapGateway` es un nombre provisional: el protocolo real depende de la aclaración pendiente sobre el
> mecanismo técnico del RNDC. El puerto no cambia.
