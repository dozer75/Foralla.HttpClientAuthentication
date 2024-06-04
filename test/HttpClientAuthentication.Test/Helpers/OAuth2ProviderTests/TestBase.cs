// Copyright © 2024 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using KISS.HttpClientAuthentication.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace KISS.HttpClientAuthentication.Test.Helpers.OAuth2ProviderTests
{
    public abstract class TestBase
    {
        protected static IServiceProvider BuildServices()
        {
            return new ServiceCollection()
                               .AddSingleton<Mock<HttpClient>>()
                               .AddSingleton(p => p.GetRequiredService<Mock<HttpClient>>().Object)
                               .AddSingleton(p =>
                               {
                                   Mock<IHttpClientFactory> clientFactoryMock = new();

                                   clientFactoryMock.Setup(cf => cf.CreateClient(nameof(HttpClientAuthentication)))
                                                    .Returns(p.GetRequiredService<HttpClient>());

                                   return clientFactoryMock;
                               })
                               .AddSingleton(p => p.GetRequiredService<Mock<IHttpClientFactory>>().Object)
                               .AddSingleton<Mock<ILogger<OAuth2Provider>>>()
                               .AddSingleton(p => p.GetRequiredService<Mock<ILogger<OAuth2Provider>>>().Object)
                               .AddSingleton<Mock<ICacheEntry>>()
                               .AddSingleton(p =>
                               {
                                   Mock<IMemoryCache> cacheMock = new();

                                   cacheMock.Setup(cache => cache.CreateEntry(It.IsAny<object>()))
                                            .Returns(() => p.GetRequiredService<Mock<ICacheEntry>>().Object);

                                   return cacheMock;
                               })
                               .AddSingleton(p => p.GetRequiredService<Mock<IMemoryCache>>().Object)
                               .AddSingleton<OAuth2Provider>()
                               .BuildServiceProvider();
        }
    }
}
