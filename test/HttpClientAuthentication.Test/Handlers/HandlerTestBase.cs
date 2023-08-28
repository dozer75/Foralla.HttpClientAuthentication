// Copyright © 2023 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using System.Net;
using KISS.HttpClientAuthentication.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace KISS.HttpClientAuthentication.Test.Handlers
{
    public abstract class HandlerTestBase
    {

        protected static IServiceProvider BuildServices(string httpClientName, IEnumerable<KeyValuePair<string, string?>> configuration)
        {
            ServiceCollection services = new();

            services.AddSingleton<IConfiguration>(p => new ConfigurationBuilder()
                                                           .AddInMemoryCollection(configuration)
                                                           .Build())
                    .AddSingleton<Mock<IOAuth2Provider>>()
                    .AddSingleton(p => p.GetRequiredService<Mock<IOAuth2Provider>>().Object)
                    .AddSingleton(p =>
                    {
                        Mock<HttpMessageHandler> hmhMock = new();

                        hmhMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                                                                             ItExpr.IsAny<CancellationToken>())
                                           .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

                        return hmhMock;
                    })
                    .AddHttpClient(httpClientName).ConfigurePrimaryHttpMessageHandler(p => p.GetRequiredService<Mock<HttpMessageHandler>>().Object)
                                                  .AddAuthenticatedHttpMessageHandler();

            return services.BuildServiceProvider();
        }
    }
}
