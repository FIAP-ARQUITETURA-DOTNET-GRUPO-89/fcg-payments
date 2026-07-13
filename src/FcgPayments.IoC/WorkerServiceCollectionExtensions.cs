using FluentValidation;
using MediatR;
using FcgPayments.Application;
using FcgPayments.Domain;
using FcgPayments.Domain.Repositories.Payments;
using FcgPayments.Domain.Services;
using FcgPayments.Infrastructure.Database;
using FcgPayments.Infrastructure.Payments;
using FcgPayments.Infrastructure.Repositories.Payments;
using FcgPayments.SharedKernel.Behaviors;
using FcgPayments.SharedKernel.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FcgPayments.IoC;

public static class WorkerServiceCollectionExtensions
{
    public static void ConfigureWorkerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MassTransitSettings>().Bind(configuration.GetSection("MassTransit"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddOptions<PaymentSimulationSettings>().Bind(configuration.GetSection("PaymentSimulation"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblies(
                typeof(IDomainEntryPoint).Assembly,
                typeof(IApplicationAssembly).Assembly,
                typeof(ValidationBehavior<,>).Assembly)
        );

        services.AddValidatorsFromAssemblyContaining<IApplicationAssembly>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        //Banco
        services.AddDbContext<FcgPaymentsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default"),
                npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null)));

        // Repositories
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Services
        services.AddSingleton<IPaymentApprovalStrategy, RandomPaymentApprovalStrategy>();
    }
}
