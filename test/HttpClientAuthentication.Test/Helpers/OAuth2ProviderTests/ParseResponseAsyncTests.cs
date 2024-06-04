// Copyright © 2024 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using KISS.HttpClientAuthentication.Configuration;
using KISS.HttpClientAuthentication.Constants;
using KISS.HttpClientAuthentication.Helpers;
using KISS.Moq.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KISS.HttpClientAuthentication.Test.Helpers.OAuth2ProviderTests
{
    public class ParseResponseAsyncTests : TestBase
    {
        [Fact]
        public async Task TestSetsTokenTypeToBearerWhenNotSpecified()
        {
            IServiceProvider services = BuildServices();

            AccessTokenResponse expected = new()
            {
                AccessToken = "ACCESS_TOKEN",
                ExpiresIn = null
            };

            Mock<HttpClient> httpClientMock = services.GetRequiredService<Mock<HttpClient>>();

            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                          {
                              Content = JsonContent.Create(expected)
                          });

            OAuth2Provider provider = services.GetRequiredService<OAuth2Provider>();

            OAuth2Configuration configuration = new()
            {
                GrantType = OAuth2GrantType.ClientCredentials,
                AuthorizationEndpoint = new("https://somehost/"),
                ClientCredentials = new()
                {
                    ClientId = "client_id",
                    ClientSecret = "client_secret"
                }
            };

            AccessTokenResponse? result = await provider.GetClientCredentialsAccessTokenAsync(configuration, default);

            result.Should().BeEquivalentTo(expected, options =>
                options.Using<AccessTokenResponse>(ctx => ctx.Subject.Should().Match<AccessTokenResponse>(f => f.TokenType == "Bearer"))
                       .WhenTypeIs<AccessTokenResponse>());
        }

        [Fact]
        public async Task TestUsesConfiguredAuthorizationSchemeAsTokenType()
        {
            IServiceProvider services = BuildServices();

            AccessTokenResponse expected = new()
            {
                AccessToken = "ACCESS_TOKEN",
                TokenType = "TOKEN_TYPE",
                ExpiresIn = null
            };

            Mock<HttpClient> httpClientMock = services.GetRequiredService<Mock<HttpClient>>();

            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                          {
                              Content = JsonContent.Create(expected)
                          });

            OAuth2Provider provider = services.GetRequiredService<OAuth2Provider>();

            OAuth2Configuration configuration = new()
            {
                GrantType = OAuth2GrantType.ClientCredentials,
                AuthorizationEndpoint = new("https://somehost/"),
                AuthorizationScheme = "Authorization_Scheme",
                ClientCredentials = new()
                {
                    ClientId = "client_id",
                    ClientSecret = "client_secret"
                }
            };

            AccessTokenResponse? result = await provider.GetClientCredentialsAccessTokenAsync(configuration, default);

            result.Should().BeEquivalentTo(expected, options =>
                options.Using<AccessTokenResponse>(ctx => ctx.Subject.Should().Match<AccessTokenResponse>(f => f.TokenType == "Authorization_Scheme"))
                       .WhenTypeIs<AccessTokenResponse>());
        }

        [Fact]
        public async Task TestFailedRequestReturnsNullAndLogsError()
        {
            IServiceProvider services = BuildServices();

            Mock<HttpClient> httpClientMock = services.GetRequiredService<Mock<HttpClient>>();

            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("ERROR_BODY") });

            OAuth2Configuration configuration = new()
            {
                GrantType = OAuth2GrantType.ClientCredentials,
                AuthorizationEndpoint = new("https://somehost/"),
                ClientCredentials = new()
                {
                    ClientId = "client_id",
                    ClientSecret = "client_secret"
                }
            };

            OAuth2Provider provider = services.GetRequiredService<OAuth2Provider>();

            AccessTokenResponse? result = await provider.GetClientCredentialsAccessTokenAsync(configuration, default);

            result.Should().BeNull();

            Mock<ILogger<OAuth2Provider>> loggerMock = services.GetRequiredService<Mock<ILogger<OAuth2Provider>>>();

            loggerMock.VerifyExt(l => l.LogError("Could not authenticate against {AuthorizationEndpoint}, the returned status code was {StatusCode}. Response body: {Body}.",
                                                 "https://somehost/", HttpStatusCode.NotFound, "ERROR_BODY"), Times.Once);
        }

        [Theory]
        [InlineData("null")]
        [InlineData("{ }")]
        [InlineData("{ \"Moo\": \"Boo\" }")]
        public async Task TestInvalidResponseReturnsNullAndLogsError(string response)
        {
            IServiceProvider services = BuildServices();

            Mock<HttpClient> httpClientMock = services.GetRequiredService<Mock<HttpClient>>();

            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response) });

            OAuth2Configuration configuration = new()
            {
                GrantType = OAuth2GrantType.ClientCredentials,
                AuthorizationEndpoint = new("https://somehost/"),
                ClientCredentials = new()
                {
                    ClientId = "client_id",
                    ClientSecret = "client_secret"
                }
            };

            OAuth2Provider provider = services.GetRequiredService<OAuth2Provider>();

            AccessTokenResponse? result = await provider.GetClientCredentialsAccessTokenAsync(configuration, default);

            result.Should().BeNull();

            Mock<ILogger<OAuth2Provider>> loggerMock = services.GetRequiredService<Mock<ILogger<OAuth2Provider>>>();

            loggerMock.VerifyExt(l => l.LogError("The result from {AuthorizationEndpoint} is not a valid OAuth2 result.", "https://somehost/"),
                                                 Times.Once);
        }
    }
}
