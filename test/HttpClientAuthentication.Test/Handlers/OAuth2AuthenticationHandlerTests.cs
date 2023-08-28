// Copyright © 2023 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using FluentAssertions;
using KISS.HttpClientAuthentication.Configuration;
using KISS.HttpClientAuthentication.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;

namespace KISS.HttpClientAuthentication.Test.Handlers
{
    public class OAuth2AuthenticationHandlerTests : HandlerTestBase
    {
        [Fact]
        public async Task TestSendAsyncThrowsExceptionWhenGrantTypeIsMissing()
        {
            IServiceProvider services = BuildServices("Test", new Dictionary<string, string?>
                                                              {
                                                                  { "Test:AuthenticationProvider", "OAuth2" },
                                                                  { "Test:OAuth2:AuthorizationScheme", "NoOp" }
                                                              });

            HttpClient httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("Test");

            Func<Task> act = () => httpClient.GetAsync("https://somehost");

            await act.Should().ThrowAsync<InvalidOperationException>()
                              .WithMessage("GrantType must be specified.")
                              .ConfigureAwait(false);
        }

        [Fact]
        public async Task TestSendAsyncThrowsExceptionWhenGrantTypeIsNone()
        {
            IServiceProvider services = BuildServices("Test", new Dictionary<string, string?>
                                                              {
                                                                  { "Test:AuthenticationProvider", "OAuth2" },
                                                                  { "Test:OAuth2:GrantType", "None" }
                                                              });

            HttpClient httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("Test");

            Func<Task> act = () => httpClient.GetAsync("https://somehost");

            await act.Should().ThrowAsync<InvalidOperationException>()
                              .WithMessage("GrantType must be specified.")
                              .ConfigureAwait(false);
        }

        [Fact]
        public async Task TestSendAsyncThrowsExceptionWhenGrantTypeIsInvalid()
        {
            IServiceProvider services = BuildServices("Test", new Dictionary<string, string?>
                                                              {
                                                                  { "Test:AuthenticationProvider", "OAuth2" },
                                                                  { "Test:OAuth2:GrantType", "99" }
                                                              });

            HttpClient httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("Test");

            Func<Task> act = () => httpClient.GetAsync("https://somehost");

            await act.Should().ThrowAsync<InvalidOperationException>()
                              .WithMessage("The GrantType 99 is not supported.")
                              .ConfigureAwait(false);
        }

        [Fact]
        public async Task TestSendAsyncThrowsExceptionWhenNoTokenIsReturnedFromOAuthProvider()
        {
            IServiceProvider services = BuildServices("Test", new Dictionary<string, string?>
                                                              {
                                                                  { "Test:AuthenticationProvider", "OAuth2" },
                                                                  { "Test:OAuth2:GrantType", "ClientCredentials" }
                                                              });

            HttpClient httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("Test");

            Func<Task> act = () => httpClient.GetAsync("https://somehost");

            await act.Should().ThrowAsync<InvalidOperationException>()
                              .WithMessage("HTTP client configured to use OAuth2 authentication, but no valid access token could be retrieved.")
                              .ConfigureAwait(false);
        }

        [Fact]
        public async Task TestSendAsyncSetsAuthorizationHeaderCorrectly()
        {
            IServiceProvider services = BuildServices("Test", new Dictionary<string, string?>
                                                              {
                                                                  { "Test:AuthenticationProvider", "OAuth2" },
                                                                  { "Test:OAuth2:GrantType", "ClientCredentials" }
                                                              });

            Mock<IOAuth2Provider> providerMock = services.GetRequiredService<Mock<IOAuth2Provider>>();

            providerMock.Setup(provider => provider.GetClientCredentialsAccessTokenAsync(It.IsAny<OAuth2Configuration>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new AccessTokenResponse
                        {
                            AccessToken = "ACCESS_TOKEN",
                            TokenType = "TOKEN_TYPE"
                        });

            HttpClient httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("Test");

            await httpClient.GetAsync("https://somehost").ConfigureAwait(false);

            Mock<HttpMessageHandler> hmhMock = services.GetRequiredService<Mock<HttpMessageHandler>>();

            hmhMock.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(),
                                                                  ItExpr.Is<HttpRequestMessage>((hrm) => VerifyOAuth2Request(hrm)),
                                                                  ItExpr.IsAny<CancellationToken>());
        }

        private static bool VerifyOAuth2Request(object obj)
        {
            return obj is HttpRequestMessage hrm &&
                   hrm.Headers.Authorization is not null &&
                   hrm.Headers.Authorization.Scheme == "TOKEN_TYPE" &&
                   hrm.Headers.Authorization.Parameter == "ACCESS_TOKEN";
        }
    }
}
