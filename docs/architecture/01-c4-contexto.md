# C4 nivel 1 — Diagrama de contexto

Muestra TransAndes TMS como una caja negra: quién lo usa y con qué sistemas externos se integra.

> Notación C4 dibujada con `flowchart` de Mermaid (la sintaxis `C4Context` de Mermaid es experimental y su
> distribución automática superpone etiquetas). Colores C4 estándar: persona azul oscuro, sistema propio azul,
> externos gris.

```mermaid
flowchart TB
  coord["<b>Coordinador de despacho</b><br/>[Persona]<br/>Crea órdenes y viajes, asigna recursos, atiende novedades"]
  cond["<b>Conductor</b><br/>[Persona]<br/>Reporta estados, novedades y prueba de entrega"]
  ger["<b>Gerencia</b><br/>[Persona]<br/>Consulta indicadores"]
  cump["<b>Cumplimiento</b><br/>[Persona]<br/>Supervisa manifiestos RNDC"]
  cli["<b>Cliente generador de carga</b><br/>[Persona externa]<br/>Consulta su envío con un enlace"]

  tms["<b>TransAndes TMS</b><br/>[Sistema de software]<br/>Planea, despacha y rastrea viajes de carga; reporta al RNDC"]

  rndc["<b>RNDC</b><br/>[Sistema externo]<br/>Registro Nacional de Despachos de Carga — MinTransporte"]
  idp["<b>Microsoft Entra ID</b><br/>[Sistema externo]<br/>Identidad del personal"]
  maps["<b>Azure Maps</b><br/>[Sistema externo]<br/>Mapas y geocodificación"]
  sms["<b>Proveedor SMS</b><br/>[Sistema externo]<br/>OTP a conductores (pendiente FR-014)"]
  erp["<b>Sistema contable actual</b><br/>[Sistema externo]<br/>Facturación electrónica"]

  coord -- "Usa<br/>[Web · HTTPS]" --> tms
  ger -- "Consulta reportes<br/>[Web · HTTPS]" --> tms
  cump -- "Revisa envíos RNDC<br/>[Web · HTTPS]" --> tms
  cond -- "Usa<br/>[App móvil · HTTPS]" --> tms
  cli -- "Abre enlace de rastreo<br/>[Navegador · HTTPS]" --> tms
  tms -- "Envía remesas y manifiestos<br/>[Servicio web]" --> rndc
  tms -- "Autentica usuarios<br/>[OIDC]" --> idp
  tms -- "Muestra mapas<br/>[HTTPS]" --> maps
  tms -- "Envía OTP<br/>[HTTPS]" --> sms
  tms -- "Exporta datos del viaje<br/>[Archivo / API]" --> erp

  classDef person fill:#08427b,stroke:#052e56,color:#fff
  classDef extperson fill:#686868,stroke:#4d4d4d,color:#fff
  classDef system fill:#1168bd,stroke:#0b4884,color:#fff
  classDef ext fill:#999999,stroke:#6b6b6b,color:#fff
  class coord,cond,ger,cump person
  class cli extperson
  class tms system
  class rndc,idp,maps,sms,erp ext
```

## Notas

- El **RNDC** es la dependencia crítica: su mecanismo técnico exacto está pendiente de confirmar con el
  Ministerio de Transporte (`research.md` R1).
- El **cliente** no tiene cuenta en el MVP; solo usa el enlace público (portal con cuenta en fase 2).
- La **facturación electrónica** sigue en el sistema contable; el TMS solo le entrega los datos del viaje.
