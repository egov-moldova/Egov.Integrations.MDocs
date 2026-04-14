using System.Net.Security;
using Egov.Extensions.Configuration;
using Egov.Integrations.MDocs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extensions for MDocs Client.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static IServiceCollection AddMDocsClient(this IServiceCollection services)
    {
        services.AddOptions<MDocsClientOptions>()
            .Configure<IOptions<SystemCertificateOptions>>((options, systemCertificateOptions) =>
            {
                var systemCertificateOptionsValue = systemCertificateOptions.Value;
                options.SystemCertificate ??= systemCertificateOptionsValue.Certificate;
                options.SystemCertificateIntermediaries ??= systemCertificateOptionsValue.IntermediateCertificates;
            });

        services.AddHttpClient<IMDocsClient, MDocsClient>()
            .ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<MDocsClientOptions>>().Value;
                client.BaseAddress = options.BaseAddress;
            })
            .ConfigurePrimaryHttpMessageHandler(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MDocsClientOptions>>().Value;
                return new SocketsHttpHandler
                {
                    SslOptions = new SslClientAuthenticationOptions
                    {
                        ClientCertificateContext = SslStreamCertificateContext.Create(options.SystemCertificate!, options.SystemCertificateIntermediaries, true)
                    }
                };
            });
        return services;
    }

    /// <summary>
    /// Register services required for an <see cref="IMDocsClient"/> implementation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The original <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddMDocsClient(this IServiceCollection services, Action<MDocsClientOptions> configureOptions)
    {
        services.Configure(configureOptions);
        return services.AddMDocsClient();
    }

    /// <summary>
    /// Register services required for an <see cref="IMDocsClient"/> implementation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="config">The configuration being bound.</param>
    /// <returns>The original <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddMDocsClient(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<MDocsClientOptions>(config);
        return services.AddMDocsClient();
    }
}