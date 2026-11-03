# Feature Specification: Despacho y rastreo de carga (MVP)

**Feature Branch**: `001-despacho-rastreo-mvp`

**Created**: 2026-11-03

**Status**: Draft

**Input**: Hoja de alcance de TransAndes Carga S.A.S. (empresa ficticia): "Planear, despachar y rastrear
cada viaje en un solo sistema, reportar los manifiestos al RNDC sin doble digitación y dar a los clientes
visibilidad del estado de su carga."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Planear y despachar un viaje (Priority: P1) 🎯 MVP

El coordinador de despacho registra una orden de cargue de un cliente, crea el viaje y le asigna un
vehículo y un conductor disponibles, sin usar Excel ni llamadas.

**Why this priority**: Es el núcleo de la operación; sin viajes en el sistema no existe nada más
(RNDC, rastreo ni reportes).

**Independent Test**: Crear una orden, convertirla en viaje y asignar vehículo y conductor; el viaje
queda en estado *Asignado* y el conductor y el vehículo dejan de aparecer como disponibles.

**Acceptance Scenarios**:

1. **Given** una orden de cargue con origen, destino, peso y fecha, **When** el coordinador crea el viaje,
   **Then** el viaje queda en estado *Planeado* con un consecutivo único.
2. **Given** un viaje *Planeado*, **When** el coordinador asigna un vehículo con documentos vigentes
   (SOAT y revisión técnico-mecánica) y un conductor con licencia vigente, **Then** el viaje pasa a *Asignado*.
3. **Given** un vehículo con SOAT vencido, **When** el coordinador intenta asignarlo, **Then** el sistema
   lo impide y muestra el documento vencido.
4. **Given** un vehículo cuya capacidad es menor que el peso de la carga, **When** se intenta asignar,
   **Then** el sistema lo rechaza indicando la capacidad faltante.

---

### User Story 2 - Reportar el manifiesto al RNDC automáticamente (Priority: P2)

Al confirmar el despacho, el sistema genera y envía al RNDC la información del viaje (remesa y manifiesto)
con los datos ya registrados, y guarda el número de manifiesto devuelto.

**Why this priority**: Es obligación legal para despachar; elimina la doble digitación y los rechazos
que hoy causan reprocesos.

**Independent Test**: Con un viaje *Asignado* contra el ambiente de pruebas del RNDC (o un simulador),
confirmar el despacho y verificar que el viaje pasa a *Despachado* con número de manifiesto.

**Acceptance Scenarios**:

1. **Given** un viaje *Asignado* con datos completos, **When** el coordinador confirma el despacho,
   **Then** el sistema envía la información al RNDC y, si es aceptada, guarda el número de manifiesto
   y el viaje pasa a *Despachado*.
2. **Given** que el RNDC rechaza el envío, **When** llega la respuesta, **Then** el viaje queda en
   *Rechazado RNDC* con el mensaje de error legible y el coordinador puede corregir y reenviar.
3. **Given** que el RNDC no responde, **When** se agota el tiempo de espera, **Then** el envío se
   reintenta automáticamente sin duplicar el manifiesto.

---

### User Story 3 - App del conductor: estados del viaje y prueba de entrega (Priority: P3)

El conductor ve sus viajes en el celular, reporta los estados (en cargue, en ruta, novedad, en descargue,
entregado) y registra la prueba de entrega con foto y firma del receptor, aun sin señal.

**Why this priority**: Da trazabilidad real del viaje y la prueba de entrega digital que piden los clientes
(objetivo 4: 90 % de conductores usando la app).

**Independent Test**: Con un viaje *Despachado*, el conductor completa todos los estados en modo avión;
al recuperar señal, los eventos aparecen en la web en el orden correcto y sin duplicados.

**Acceptance Scenarios**:

1. **Given** un conductor autenticado, **When** abre la app, **Then** ve sus viajes despachados con
   origen, destino, cliente y número de manifiesto.
2. **Given** un viaje *En ruta*, **When** el conductor reporta una novedad (varada, retén, cierre de vía)
   con foto opcional, **Then** el coordinador la ve en la web en menos de 1 minuto si hay señal.
3. **Given** que el conductor está en el punto de entrega, **When** captura foto de la mercancía,
   nombre, documento y firma del receptor, **Then** el viaje pasa a *Entregado* con la prueba adjunta.
4. **Given** que no hay señal, **When** el conductor registra eventos, **Then** se guardan localmente
   y se sincronizan al volver la conexión sin duplicarse.

---

### User Story 4 - Rastreo y enlace para el cliente (Priority: P4)

El coordinador ve en un mapa la última ubicación de cada viaje activo; el cliente recibe un enlace por viaje
para consultar estado, ubicación aproximada y prueba de entrega, sin crear una cuenta.

**Why this priority**: Responde a la exigencia de los clientes para renovar contratos. El portal completo
de clientes se dejó para la fase 2 (concesión de tiempo de la Parte V).

**Independent Test**: Con un viaje *En ruta* que envía ubicaciones, abrir el enlace del cliente en un
navegador sin sesión y ver estado y ubicación actualizados.

**Acceptance Scenarios**:

1. **Given** un viaje *En ruta*, **When** la app envía la ubicación, **Then** el mapa del coordinador
   se actualiza con una frecuencia de 5 minutos.
2. **Given** un viaje despachado, **When** el coordinador comparte el enlace con el cliente, **Then** el
   cliente ve estado, última ubicación, hora estimada y, al finalizar, la prueba de entrega.
3. **Given** un viaje entregado hace más de 30 días, **When** alguien abre el enlace, **Then** el enlace
   ya no muestra la ubicación y solo indica "Entregado".

---

### User Story 5 - Reportes básicos de operación (Priority: P5)

La gerencia consulta indicadores de entregas a tiempo, viajes por cliente y tiempo de asignación,
y los exporta a Excel.

**Why this priority**: Permite medir los objetivos SMART del proyecto; puede llegar después del núcleo.

**Independent Test**: Con datos de prueba de un mes, el reporte muestra el % de entregas a tiempo
calculado igual que un cálculo manual sobre los mismos datos.

**Acceptance Scenarios**:

1. **Given** viajes entregados en un rango de fechas, **When** la gerencia filtra por cliente,
   **Then** ve el % de entregas a tiempo, total de viajes y novedades.
2. **Given** un reporte en pantalla, **When** se exporta, **Then** se descarga un .xlsx con los mismos datos.

---

### Edge Cases

- ¿Qué pasa si el conductor cambia de celular a mitad de un viaje? Debe iniciar sesión y recuperar el viaje
  sin perder los eventos ya sincronizados.
- ¿Qué pasa si se anula un viaje ya reportado al RNDC? Debe ejecutarse la anulación en el RNDC antes de
  liberar el vehículo.
- ¿Cómo se manejan dos eventos con hora del dispositivo desordenada? Se ordenan por hora del dispositivo y se
  conserva la hora de recepción en el servidor.
- ¿Qué pasa si el conductor de un vehículo afiliado no tiene smartphone? [NEEDS CLARIFICATION: ¿se permite
  que el coordinador registre los estados en nombre del conductor?]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema MUST permitir registrar órdenes de cargue con cliente, origen, destino, tipo de
  mercancía, peso, volumen y fecha requerida.
- **FR-002**: El sistema MUST convertir una orden en viaje con un consecutivo único y estados:
  Planeado → Asignado → Despachado → En ruta → Entregado (y Anulado / Rechazado RNDC).
- **FR-003**: El sistema MUST validar al asignar: documentos del vehículo y del conductor vigentes,
  capacidad suficiente y que no estén asignados a otro viaje activo.
- **FR-004**: El sistema MUST enviar al RNDC la información de remesa y manifiesto del viaje al confirmar
  el despacho y guardar el número de manifiesto.
- **FR-005**: El sistema MUST reintentar envíos fallidos al RNDC de forma idempotente y alertar al
  coordinador tras 3 intentos fallidos.
- **FR-006**: El sistema MUST guardar cada solicitud y respuesta del RNDC para auditoría.
- **FR-007**: La app del conductor MUST registrar estados y novedades sin conexión y sincronizarlos sin duplicados.
- **FR-008**: La app MUST capturar la prueba de entrega: foto(s), nombre y documento del receptor y firma.
- **FR-009**: La app MUST enviar la ubicación del vehículo cada 5 minutos mientras el viaje esté *En ruta*.
- **FR-010**: El sistema MUST generar un enlace de rastreo por viaje, de solo lectura y sin sesión, que
  expira 30 días después de la entrega.
- **FR-011**: El sistema MUST auditar cada cambio de estado (usuario, fecha, origen).
- **FR-012**: El sistema MUST ofrecer reportes de % entregas a tiempo, viajes por cliente y tiempo de
  asignación, exportables a Excel.
- **FR-013**: El sistema MUST controlar el acceso por roles: Coordinador, Conductor, Gerencia, Cumplimiento, Administrador.
- **FR-014**: El sistema MUST autenticar a los conductores con [NEEDS CLARIFICATION: ¿cédula + código OTP
  por SMS, o usuario y contraseña?].
- **FR-015**: El sistema MUST conservar los datos de viajes y pruebas de entrega durante
  [NEEDS CLARIFICATION: plazo de retención exigido por la norma y por los contratos con clientes].

### Key Entities

- **Cliente**: generador de carga; NIT, razón social, contactos.
- **OrdenDeCargue**: solicitud del cliente; origen, destino, mercancía, peso, fecha requerida.
- **Viaje**: ejecución de una orden; estado, vehículo, conductor, fechas planeadas y reales.
- **Vehículo**: placa, tipo, capacidad, propio o afiliado, vencimientos de SOAT y revisión técnico-mecánica.
- **Conductor**: cédula, licencia y vencimiento, celular.
- **ManifiestoRndc**: número de manifiesto, estado del envío, solicitud y respuesta.
- **EventoDeViaje**: cambio de estado o novedad, con hora del dispositivo y del servidor.
- **PruebaDeEntrega**: fotos, receptor, firma, ubicación y hora.
- **Ubicación**: latitud, longitud, hora, viaje.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100 % de los viajes nacionales se planean y despachan en el sistema antes del 30-jun-2027.
- **SC-002**: Las entregas fuera de tiempo bajan del 18 % al 8 % en los 6 meses siguientes al lanzamiento.
- **SC-003**: El 95 % de los manifiestos se reportan al RNDC sin digitación manual al cierre de 2027.
- **SC-004**: El 90 % de los conductores registra la prueba de entrega en la app dentro de los 3 meses siguientes al lanzamiento.
- **SC-005**: Un coordinador asigna un viaje en menos de 3 minutos desde que recibe la orden.
- **SC-006**: Los eventos registrados sin señal aparecen en la web en menos de 2 minutos tras recuperar conexión.

## Assumptions

- Los conductores usan celulares Android (mayoría) o iOS con cámara y GPS.
- El RNDC ofrece un mecanismo de integración por servicio web y un ambiente de pruebas.
  [NEEDS CLARIFICATION: confirmar con el Ministerio de Transporte el mecanismo técnico vigente,
  credenciales y versión del esquema, considerando el proyecto de resolución de actualización del RNDC.]
- La facturación electrónica sigue en el sistema contable actual; el MVP solo entrega los datos del viaje.
- Los mapas y la geocodificación se toman de un servicio de terceros (no se desarrollan).
- Fuera de alcance: portal de clientes con cuenta, mantenimiento, combustible, pagos a conductores,
  optimización de rutas con IA, integración con ERP de clientes y carga internacional.
