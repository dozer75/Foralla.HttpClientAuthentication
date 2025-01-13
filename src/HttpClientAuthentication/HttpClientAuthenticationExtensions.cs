// Copyright © 2024 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using KISS.HttpClientAuthentication.Configuration;
using KISS.HttpClientAuthentication.Constants;
using KISS.HttpClientAuthentication.Handlers;
using KISS.HttpClientAuthentication.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KISS.HttpClientAuthentication
{
    /// <summary>
    ///     Various extension methods that is used in conjunction with HttpClientAuthentication.
    /// </summary>
    public static class HttpClientAuthenticationExtensions
    {
        /// <summary>
        ///     Adds a <see cref="HttpClientAuthenticationHandler"/> to the <paramref name="builder"/>
        ///     where the configuration is based on <see cref="IHttpClientBuilder.Name"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/> to add the authentication handler.</param>
        /// <returns>The <paramref name="builder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        ///     See the exception for more information. It is most likely that the exception is thrown because
        ///     the <paramref name="builder"/>.<see cref="IHttpClientBuilder.Services"/> is missing a
        ///     <see cref="IConfiguration"/> service.
        /// </exception>
        public static IHttpClientBuilder AddAuthenticatedHttpMessageHandler(this IHttpClientBuilder builder)
        {
            return builder is not null
                ? builder.AddAuthenticatedHttpMessageHandler(builder.Name)
                : throw new ArgumentNullException(nameof(builder));
        }

        /// <summary>
        ///     Adds a <see cref="HttpClientAuthenticationHandler"/> to the <paramref name="builder"/>
        ///     where the configuration is based on <paramref name="configSection"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/> to add the authentication handler.</param>
        /// <param name="configSection">The configuration section to be used to configure the authentication handler.</param>
        /// <returns>The <paramref name="builder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="configSection"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        ///     See the exception for more information. It is most likely that the exception is thrown because
        ///     the <paramref name="builder"/>.<see cref="IHttpClientBuilder.Services"/> is missing a
        ///     <see cref="IConfiguration"/> service.
        /// </exception>
        public static IHttpClientBuilder AddAuthenticatedHttpMessageHandler(this IHttpClientBuilder builder, string configSection)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configSection);

            builder.Services.AddMemoryCache();

            builder.Services.TryAddSingleton<IOAuth2Provider, OAuth2Provider>();

            builder.Services.TryAddTransient<NoAuthenticationHandler>();
            builder.Services.TryAddTransient<ApiKeyAuthenticationHandler>();
            builder.Services.TryAddTransient<BasicAuthenticationHandler>();
            builder.Services.TryAddTransient<OAuth2AuthenticationHandler>();

            return builder.AddHttpMessageHandler(p => p.GetAuthenticationHandler(configSection));
        }

        private static DelegatingHandler GetAuthenticationHandler(this IServiceProvider services, string configurationName)
        {
            IConfigurationSection configSection = services.GetRequiredService<IConfiguration>().GetSection(configurationName);
            HttpClientAuthenticationConfiguration configuration = configSection.Get<HttpClientAuthenticationConfiguration>()
                ?? throw new ArgumentException($"Could not find the configuration section for {configurationName} that has values for {nameof(HttpClientAuthenticationConfiguration)}.",
                                               nameof(configurationName));


            return configuration.AuthenticationProvider switch
            {
                AuthenticationProvider.None => services.GetRequiredService<NoAuthenticationHandler>(),
                AuthenticationProvider.ApiKey => services.CreateHandler<ApiKeyAuthenticationHandler, ApiKeyConfiguration>(configurationName, nameof(configuration.ApiKey), configuration.ApiKey),
                AuthenticationProvider.Basic => services.CreateHandler<BasicAuthenticationHandler, BasicConfiguration>(configurationName, nameof(configuration.Basic), configuration.Basic),
                AuthenticationProvider.OAuth2 => services.CreateHandler<OAuth2AuthenticationHandler, OAuth2Configuration>(configurationName, nameof(configuration.OAuth2), configuration.OAuth2),
                _ => throw new InvalidOperationException($"{nameof(configuration.AuthenticationProvider)} value {configuration.AuthenticationProvider} is not supported."),
            };
        }

        private static THandler CreateHandler<THandler, TConfiguration>(this IServiceProvider services, string configurationName, string configurationType,
                                                                        TConfiguration? configuration)
            where THandler : BaseAuthenticationHandler<TConfiguration>
        {
            THandler handler = services.GetRequiredService<THandler>();

            handler.Configuration = configuration ??
                throw new InvalidOperationException($"Missing {configurationType} configuration for configuration {configurationName}.");

            return handler;
        }
    }
}
