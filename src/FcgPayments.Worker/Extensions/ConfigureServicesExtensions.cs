using FcgPayments.IoC;
using FcgPayments.Infrastructure.Messaging;
using FcgPayments.Worker.Consumers;

namespace FcgPayments.Worker.Extensions;

public static class ConfigureServicesExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureWorkerDependencies(configuration);

        services.AddMassTransitRabbitMq(configuration, x =>
        {
            x.AddConsumer<OrderPlacedConsumer>();
        });

        return services;
    }
}
