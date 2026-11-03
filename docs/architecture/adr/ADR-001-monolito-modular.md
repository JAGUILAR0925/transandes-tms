# ADR-001: Monolito modular con arquitectura limpia

**Estado**: Propuesta · **Fecha**: 2026-11-03

## Contexto
Equipo de 3 personas, presupuesto fijo (COP 450 M) y un MVP con 5 historias que comparten el mismo modelo de viaje.

## Decisión
Un solo despliegue de API (más un worker) organizado en módulos (`Despacho`, `Flota`, `Ejecucion`, `Rastreo`, `Rndc`, `Reportes`) con capas Domain → Application → Infrastructure. Límites entre módulos verificados con pruebas de arquitectura.

## Alternativas descartadas
- **Microservicios**: Sobrecosto operativo (despliegues, observabilidad distribuida, consistencia eventual) desproporcionado para el tamaño del equipo.
- **Monolito en capas sin módulos**: Acoplamiento entre funcionalidades; dificulta extraer el módulo RNDC o Rastreo si crecen.

## Consecuencias
Despliegue y depuración simples; la extracción futura de un módulo a servicio es posible porque los límites ya existen. Requiere disciplina para no cruzar tablas entre módulos.
