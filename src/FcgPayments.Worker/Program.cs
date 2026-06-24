using FcgPayments.Worker.Extensions;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.AddServiceDefaults();

builder.Services.ConfigureServices(builder.Configuration);

var host = builder.Build();
host.Run();
