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
    public class TryParseAndLogOAuth2ErrorTests : TestBase
    {
        [Fact]
        public async Task TestErrorIsFullyParsedAndLogged()
        {
            IServiceProvider services = BuildServices();

            Mock<HttpClient> httpClientMock = services.GetRequiredService<Mock<HttpClient>>();

            ErrorResponse error = new()
            {
                Error = "invalid_client",
                Description = "error-description",
                Uri = new Uri("https://somehost/error/invalid_client")
            };

            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = JsonContent.Create(error) });

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

            loggerMock.VerifyExt(l => l.LogError("Could not authenticate against https://somehost/ with client id client_id. Error code: invalid_client, description: error-description (https://somehost/error/invalid_client)."),
                                                 Times.Once);
        }

        [Fact]
        public async Task TestDescriptionIsSkippedWhenNotSpecified()
        {
            IServiceProvider services = BuildServices();

            Mock<HttpClient> httpClientMock = services.GetRequiredService<Mock<HttpClient>>();

            ErrorResponse error = new()
            {
                Error = "invalid_client",
                Uri = new Uri("https://somehost/error/invalid_client")
            };

            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = JsonContent.Create(error) });

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

            loggerMock.VerifyExt(l => l.LogError("Could not authenticate against https://somehost/ with client id client_id. Error code: invalid_client (https://somehost/error/invalid_client)."),
                                                 Times.Once);
        }

        [Fact]
        public async Task TestUriIsSkippedWhenNotSpecified()
        {
            IServiceProvider services = BuildServices();

            Mock<HttpClient> httpClientMock = services.GetRequiredService<Mock<HttpClient>>();

            ErrorResponse error = new()
            {
                Error = "invalid_client",
                Description = "error-description"
            };

            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = JsonContent.Create(error) });

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

            loggerMock.VerifyExt(l => l.LogError("Could not authenticate against https://somehost/ with client id client_id. Error code: invalid_client, description: error-description."),
                                                 Times.Once);
        }


        [Fact]
        public async Task TestDescriptionAndUriIsSkippedWhenNotSpecified()
        {
            IServiceProvider services = BuildServices();

            Mock<HttpClient> httpClientMock = services.GetRequiredService<Mock<HttpClient>>();

            ErrorResponse error = new()
            {
                Error = "invalid_client"
            };

            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = JsonContent.Create(error) });

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

            loggerMock.VerifyExt(l => l.LogError("Could not authenticate against https://somehost/ with client id client_id. Error code: invalid_client."),
                                                 Times.Once);
        }

        [Theory]
        [InlineData("null")]
        [InlineData("moo")]
        [InlineData("{}")]
        [InlineData("{ \"moo\": \"boo\"}")]
        public async Task TestParsingIsSkippedOnInvalidErrorContent(string content)
        {
            IServiceProvider services = BuildServices();

            Mock<HttpClient> httpClientMock = services.GetRequiredService<Mock<HttpClient>>();

            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(content) });

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
                                                 "https://somehost/", HttpStatusCode.BadRequest, content), Times.Once);
        }
    }
}
