# TransAndes.Mobile (pendiente)

App .NET MAUI **10** para conductores (ADR-003, ADR-006). No va en .NET 8: MAUI 8 está sin soporte
y Google Play exige apuntar a Android 16 (API 36).

Se genera en las tareas **T003** y **T033–T037**. Requiere el SDK de .NET 10 y el workload de MAUI:

```bash
dotnet workload install maui
dotnet new maui -n TransAndes.Mobile -o . -f net10.0
```

Puede referenciar `TransAndes.Domain` (compilado en `net8.0`) sin cambios.
