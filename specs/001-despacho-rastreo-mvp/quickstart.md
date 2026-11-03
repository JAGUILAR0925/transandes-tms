# Quickstart: validar el MVP de punta a punta

1. **Preparar datos**: cliente "Alimentos del Valle (ficticio)", vehículo `TAC-123` (capacidad 10.000 kg,
   SOAT y RTM vigentes) y conductor con licencia C2 vigente.
2. **US1**: crear orden Medellín → Barranquilla de 8.000 kg, crear viaje y asignar `TAC-123`.
   ✔ Estado *Asignado*. Repetir con un vehículo de SOAT vencido → ✔ rechazo con mensaje.
3. **US2**: confirmar despacho contra el simulador RNDC. ✔ *Despachado* con número de manifiesto.
   Forzar respuesta de rechazo en el simulador → ✔ *Rechazado RNDC* con error legible.
4. **US3**: en la app, con el celular en modo avión, registrar *En cargue*, *En ruta*, una novedad y la
   prueba de entrega. Quitar modo avión. ✔ Eventos en la web en orden, sin duplicados, en < 2 min (SC-006).
5. **US4**: abrir el enlace `/t/{token}` en una ventana privada. ✔ Estado y ubicación visibles sin sesión.
6. **US5**: generar el reporte del mes y exportar a Excel. ✔ % de entregas a tiempo coincide con el cálculo manual.
