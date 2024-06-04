// Copyright © 2024 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using FluentAssertions;
using KISS.HttpClientAuthentication.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace KISS.HttpClientAuthentication.Test
{
    public class HttpClientAuthenticationExtensionsTests
    {
        private const string TestHttpClient = nameof(TestHttpClient);

        [Fact]
        public void TestSimpleAddAuthenticatedHttpMessageHandlerThrowsArgumentNullExceptionWhenBuilderIsNull()
        {
            Action act = () => HttpClientAuthenticationExtensions.AddAuthenticatedHttpMessageHandler(null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
        }

        [Fact]
        public void TestAddAuthenticatedHttpMessageHandlerThrowsArgumentNullExceptionWhenBuilderIsNull()
        {
            Action act = () => HttpClientAuthenticationExtensions.AddAuthenticatedHttpMessageHandler(null!, null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
        }

        [Fact]
        public void TestAddAuthenticatedHttpMessageHandlerThrowsArgumentNullExceptionWhenConfigSectionIsNull()
        {
            Action act = () => new Mock<IHttpClientBuilder>().Object.AddAuthenticatedHttpMessageHandler(null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("configSection");
        }

        [Fact]
        public void TestGetAuthenticationHandlerThrowsInvalidOperationExceptionWhenConfigSectionIsMissing()
        {
            IServiceProvider services = BuildServices([]);

            IHttpClientFactory factory = services.GetRequiredService<IHttpClientFactory>();

            Action act = () => factory.CreateClient(TestHttpClient);

            act.Should().Throw<ArgumentException>().WithParameterName("configurationName")
                        .WithMessage("Could not find the configuration section for TestHttpClient that has values for HttpClientAuthenticationConfiguration.*");
        }

        [Fact]
        public void TestGetAuthenticationHandlerThrowsInvalidOperationExceptionWhenAuthenticationProviderIsNotSupported()
        {
            IServiceProvider services = BuildServices(new Dictionary<string, string?>()
            {
                { $"{TestHttpClient}:AuthenticationProvider", "99" }
            });

            IHttpClientFactory factory = services.GetRequiredService<IHttpClientFactory>();

            Action act = () => factory.CreateClient(TestHttpClient);

            act.Should().Throw<InvalidOperationException>().WithMessage("AuthenticationProvider value 99 is not supported.");
        }

        [Fact]
        public void TestGetAuthenticationHandlerThrowsInvalidOperationExceptionWhenApiKeyConfigurationIsMissing()
        {
            IServiceProvider services = BuildServices(new Dictionary<string, string?>
            {
                { $"{TestHttpClient}:AuthenticationProvider", "ApiKey" }
            });

            IHttpClientFactory factory = services.GetRequiredService<IHttpClientFactory>();

            Action act = () => factory.CreateClient(TestHttpClient);

            act.Should().Throw<InvalidOperationException>().WithMessage("Missing ApiKey configuration for configuration TestHttpClient.");
        }

        [Fact]
        public void TestGetAuthenticationHandlerThrowsInvalidOperationExceptionWhenBasicConfigurationIsMissing()
        {
            IServiceProvider services = BuildServices(new Dictionary<string, string?>
            {
                { $"{TestHttpClient}:AuthenticationProvider", "Basic" }
            });

            IHttpClientFactory factory = services.GetRequiredService<IHttpClientFactory>();

            Action act = () => factory.CreateClient(TestHttpClient);

            act.Should().Throw<InvalidOperationException>().WithMessage("Missing Basic configuration for configuration TestHttpClient.");
        }

        [Fact]
        public void TestGetAuthenticationHandlerThrowsInvalidOperationExceptionWhenOAuth2ConfigurationIsMissing()
        {
            IServiceProvider services = BuildServices(new Dictionary<string, string?>
            {
                { $"{TestHttpClient}:AuthenticationProvider", "OAuth2" }
            });

            IHttpClientFactory factory = services.GetRequiredService<IHttpClientFactory>();

            Action act = () => factory.CreateClient(TestHttpClient);

            act.Should().Throw<InvalidOperationException>().WithMessage("Missing OAuth2 configuration for configuration TestHttpClient.");
        }

        [Fact]
        public void TestGetAuthenticationHandlerReturnsNoAuthenticationHandlerWhenAuthenticationProviderIsNone()
        {
            IServiceProvider services = BuildServices(new Dictionary<string, string?>
            {
                { $"{TestHttpClient}:AuthenticationProvider", "None" }
            });

            IHttpClientFactory factory = services.GetRequiredService<IHttpClientFactory>();

            factory.CreateClient(TestHttpClient);

            Mock<DelegatingHandler> handlerMock = services.GetRequiredService<Mock<DelegatingHandler>>();

            handlerMock.Object.InnerHandler.Should().BeAssignableTo<NoAuthenticationHandler>();
        }

        [Fact]
        public void TestGetAuthenticationHandlerReturnsApiKeyAuthenticationHandlerWhenAuthenticationProviderIsApiKey()
        {
            IServiceProvider services = BuildServices(new Dictionary<string, string?>
            {
                { $"{TestHttpClient}:AuthenticationProvider", "ApiKey" },
                { $"{TestHttpClient}:ApiKey:Header", "" }
            });

            IHttpClientFactory factory = services.GetRequiredService<IHttpClientFactory>();

            factory.CreateClient(TestHttpClient);

            Mock<DelegatingHandler> handlerMock = services.GetRequiredService<Mock<DelegatingHandler>>();

            handlerMock.Object.InnerHandler.Should().BeAssignableTo<ApiKeyAuthenticationHandler>();
        }

        [Fact]
        public void TestGetAuthenticationHandlerReturnsBasicAuthenticationHandlerWhenAuthenticationProviderIsBasic()
        {
            IServiceProvider services = BuildServices(new Dictionary<string, string?>
            {
                { $"{TestHttpClient}:AuthenticationProvider", "Basic" },
                { $"{TestHttpClient}:Basic:Username", "" }
            });

            IHttpClientFactory factory = services.GetRequiredService<IHttpClientFactory>();

            factory.CreateClient(TestHttpClient);

            Mock<DelegatingHandler> handlerMock = services.GetRequiredService<Mock<DelegatingHandler>>();

            handlerMock.Object.InnerHandler.Should().BeAssignableTo<BasicAuthenticationHandler>();
        }

        [Fact]
        public void TestGetAuthenticationHandlerReturnsOAuth2AuthenticationHandlerWhenAuthenticationProviderIsOAuth2()
        {
            IServiceProvider services = BuildServices(new Dictionary<string, string?>
            {
                { $"{TestHttpClient}:AuthenticationProvider", "OAuth2" },
                { $"{TestHttpClient}:OAuth2:AuthorizationScheme", "" }
            });

            IHttpClientFactory factory = services.GetRequiredService<IHttpClientFactory>();

            factory.CreateClient(TestHttpClient);

            Mock<DelegatingHandler> handlerMock = services.GetRequiredService<Mock<DelegatingHandler>>();

            handlerMock.Object.InnerHandler.Should().BeAssignableTo<OAuth2AuthenticationHandler>();
        }

        private static ServiceProvider BuildServices(IEnumerable<KeyValuePair<string, string?>> configuration)
        {
            ServiceCollection services = new();

            services.AddSingleton<IConfiguration>(p => new ConfigurationBuilder()
                                                           .AddInMemoryCollection(configuration)
                                                           .Build())
                    .AddSingleton<Mock<DelegatingHandler>>()
                    .AddHttpClient(TestHttpClient).AddHttpMessageHandler(p => p.GetRequiredService<Mock<DelegatingHandler>>().Object)
                                                  .AddAuthenticatedHttpMessageHandler();

            return services.BuildServiceProvider();
        }
    }
}
