using TransAndes.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<RndcWorker>();

var host = builder.Build();
host.Run();
