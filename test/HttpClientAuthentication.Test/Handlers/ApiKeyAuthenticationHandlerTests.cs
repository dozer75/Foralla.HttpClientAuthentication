// Copyright © 2023 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;

namespace KISS.HttpClientAuthentication.Test.Handlers
{
    public class ApiKeyAuthenticationHandlerTests : HandlerTestBase
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" \t\r\n ")]
        public async Task TestSendAsyncThrowsExceptionWhenApiKeyHeaderIsNotSpecified(string header)
        {
            IServiceProvider services = BuildServices("Test", new Dictionary<string, string?>
                                                              {
                                                                  { "Test:AuthenticationProvider", "ApiKey" },
                                                                  { "Test:ApiKey:Header", header }
                                                              });

            HttpClient httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("Test");

            Func<Task> act = () => httpClient.GetAsync("https://somehost");

            await act.Should().ThrowAsync<InvalidOperationException>()
                              .WithMessage("HTTP client configured to use ApiKey, but Header is not set in the ApiKey configuration.")
                              .ConfigureAwait(false);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" \t\r\n ")]
        public async Task TestSendAsyncThrowsExceptionWhenApiKeyValueIsNotSpecified(string value)
        {
            IServiceProvider services = BuildServices("Test", new Dictionary<string, string?>
                                                              {
                                                                  { "Test:AuthenticationProvider", "ApiKey" },
                                                                  { "Test:ApiKey:Header", "API_KEY" },
                                                                  { "Test:ApiKey:Value", value }
                                                              });

            HttpClient httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("Test");

            Func<Task> act = () => httpClient.GetAsync("https://somehost");

            await act.Should().ThrowAsync<InvalidOperationException>()
                              .WithMessage("HTTP client configured to use ApiKey, but Value is not set in the ApiKey configuration.")
                              .ConfigureAwait(false);
        }

        [Fact]
        public async Task TestSendAsyncSetsApiKeyCorrectly()
        {

            IServiceProvider services = BuildServices("Test", new Dictionary<string, string?>
                                                              {
                                                                  { "Test:AuthenticationProvider", "ApiKey" },
                                                                  { "Test:ApiKey:Header", "API_HEADER" },
                                                                  { "Test:ApiKey:Value", "API_KEY" }
                                                              });

            HttpClient httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("Test");

            await httpClient.GetAsync("https://somehost").ConfigureAwait(false);

            Mock<HttpMessageHandler> hmhMock = services.GetRequiredService<Mock<HttpMessageHandler>>();

            hmhMock.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(),
                                                                  ItExpr.Is<HttpRequestMessage>((hrm) => VerifyApiKeyRequest(hrm)),
                                                                  ItExpr.IsAny<CancellationToken>());
        }

        private static bool VerifyApiKeyRequest(object obj)
        {
            return obj is HttpRequestMessage hrm &&
                   hrm.Headers.TryGetValues("API_HEADER", out IEnumerable<string>? values) &&
                   values.All(val => val == "API_KEY");
        }
    }
}
