# C4 nivel 3 — Diagrama de componentes

Zoom dentro de la **API TMS** y el **Worker RNDC**. Cada componente es un módulo con una responsabilidad clara;
el código vive en los proyectos de `plan.md` (`Api`, `Application`, `Domain`, `Infrastructure`, `Rndc`, `Workers`).

## 3.1 API TMS

```mermaid
flowchart TB
  web["<b>Web de despacho</b><br/>[Contenedor · Angular]"]
  app["<b>App del conductor</b><br/>[Contenedor · .NET MAUI 10]"]

  subgraph api["API TMS  [Contenedor · ASP.NET Core 8]"]
    direction TB
    ep["<b>Endpoints REST</b><br/>[Minimal APIs]<br/>Rutas de contracts/api.yaml"]
    auth["<b>Autenticación y roles</b><br/>[ASP.NET Core Auth]<br/>JWT Entra ID · FR-013"]

    desp["<b>Módulo Despacho</b><br/>CrearOrden · CrearViaje<br/>AsignarRecursos<br/>ConfirmarDespacho · AnularViaje"]
    flota["<b>Módulo Flota</b><br/>Vehículos · Conductores<br/>Vencimientos"]
    ejec["<b>Módulo Ejecución</b><br/>SincronizarEventos<br/>RegistrarPruebaEntrega"]
    rast["<b>Módulo Rastreo</b><br/>RegistrarUbicacion<br/>GenerarEnlace · RastreoPublico"]
    rep["<b>Módulo Reportes</b><br/>[Consultas · CQRS ligero]<br/>Indicadores · .xlsx"]

    dom["<b>Dominio</b><br/>Viaje (máquina de estados)<br/>PoliticaAsignacion · eventos de dominio"]
    outbox["<b>Outbox</b><br/>[EF Core]<br/>Mensajes en la misma transacción"]
    audit["<b>Auditoría</b><br/>[Interceptor EF Core]"]
    repo["<b>Persistencia</b><br/>[EF Core DbContext]"]
    hub["<b>Hub de notificaciones</b><br/>[SignalR]"]
  end

  sql[("<b>Azure SQL</b><br/>[Contenedor]")]
  sig["<b>Azure SignalR Service</b><br/>[Contenedor]"]

  web --> ep
  app --> ep
  ep --> auth
  ep --> desp & flota & ejec & rast & rep
  desp & ejec --> dom
  desp -- "Encola envío RNDC" --> outbox
  desp & flota & ejec & rast --> repo
  repo --> sql
  outbox --> sql
  audit --> sql
  rep -- "Solo lectura" --> sql
  ejec & rast -- "Notifica" --> hub
  hub --> sig

  classDef container fill:#438dd5,stroke:#2e6295,color:#fff
  classDef comp fill:#85bbf0,stroke:#5d82a8,color:#000
  class web,app,sql,sig container
  class ep,auth,desp,flota,ejec,rast,rep,dom,outbox,audit,repo,hub comp
  classDef mod fill:#5fa3e6,stroke:#2e6295,color:#000
  class desp,flota,ejec,rast,rep mod
  style api fill:#ffffff,stroke:#2e6295,stroke-width:2px,stroke-dasharray:6 4
```

## 3.2 Worker RNDC

```mermaid
flowchart LR
  sql[("<b>Azure SQL</b><br/>[Contenedor]")]
  bus[["<b>Service Bus</b><br/>rndc-envios<br/>[Contenedor]"]]

  subgraph wk["Worker RNDC  [Contenedor · .NET 8]"]
    direction TB
    pub["<b>Publicador de outbox</b><br/>[BackgroundService]"]
    cons["<b>Consumidor de envíos</b><br/>[ServiceBusProcessor]<br/>Idempotente: viajeId + operación"]
    gw["<b>IRndcGateway</b><br/>[Puerto]<br/>EnviarRemesa · EnviarManifiesto<br/>Anular · Consultar"]
    map["<b>Mapeador RNDC</b><br/>[Adaptador]<br/>Viaje → esquema RNDC"]
    cli["<b>Cliente RNDC</b><br/>[HTTP + Polly]<br/>Timeout · backoff · circuit breaker"]
    store["<b>Registro de envíos</b><br/>[EF Core]<br/>Solicitud/respuesta · FR-006"]
    exp["<b>Expirador de enlaces</b><br/>[BackgroundService]"]
  end

  rndc["<b>RNDC</b><br/>[Sistema externo]"]
  sig["<b>Azure SignalR</b><br/>[Contenedor]"]

  sql -- "Lee outbox" --> pub
  pub -- "Publica" --> bus
  bus -- "Entrega" --> cons
  cons --> gw
  gw --> map
  gw --> cli
  cli -- "Servicio web" --> rndc
  cons -- "Registra resultado" --> store
  store --> sql
  cons -- "Notifica" --> sig
  exp --> sql

  classDef container fill:#438dd5,stroke:#2e6295,color:#fff
  classDef comp fill:#85bbf0,stroke:#5d82a8,color:#000
  classDef ext fill:#999999,stroke:#6b6b6b,color:#fff
  class sql,bus,sig container
  class pub,cons,gw,map,cli,store,exp comp
  class rndc ext
  style wk fill:#ffffff,stroke:#2e6295,stroke-width:2px,stroke-dasharray:6 4
```

## Reglas de dependencia

- `Domain` no depende de nada. `Application` depende solo de `Domain` y de puertos (interfaces).
- `Infrastructure` y `Rndc` implementan los puertos; `Api` y `Workers` componen todo con inyección de dependencias.
- Un módulo no accede a las tablas de otro módulo: se comunica por casos de uso o eventos de dominio.
- Se valida en CI con pruebas de arquitectura (p. ej. NetArchTest).
