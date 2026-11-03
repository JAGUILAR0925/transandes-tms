using TransAndes.Application.Abstracciones;
using TransAndes.Application.Clientes;
using TransAndes.Application.Despacho;
using TransAndes.Application.Flota;
using TransAndes.Infrastructure.EnMemoria;
using TransAndes.Infrastructure.Tiempo;

namespace TransAndes.Api.Configuracion;

public static class ServiciosTms
{
    /// <summary>Registra casos de uso y la persistencia en memoria del MVP (se reemplaza por EF Core en T010).</summary>
    public static IServiceCollection AddTransAndesTms(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IReloj, RelojColombia>();

        services.AddSingleton<IClienteRepository, ClienteRepositoryEnMemoria>();
        services.AddSingleton<IVehiculoRepository, VehiculoRepositoryEnMemoria>();
        services.AddSingleton<IConductorRepository, ConductorRepositoryEnMemoria>();
        services.AddSingleton<IOrdenRepository, OrdenRepositoryEnMemoria>();
        services.AddSingleton<IViajeRepository, ViajeRepositoryEnMemoria>();

        services.AddScoped<RegistrarClienteHandler>();
        services.AddScoped<RegistrarVehiculoHandler>();
        services.AddScoped<RegistrarConductorHandler>();
        services.AddScoped<CrearOrdenHandler>();
        services.AddScoped<CrearViajeHandler>();
        services.AddScoped<AsignarRecursosHandler>();
        services.AddScoped<ObtenerViajeHandler>();
        return services;
    }
}
