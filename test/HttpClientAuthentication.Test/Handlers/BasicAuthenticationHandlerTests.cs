// Copyright © 2025 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;

namespace KISS.HttpClientAuthentication.Test.Handlers
{
    public class BasicAuthenticationHandlerTests : HandlerTestBase
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" \t\r\n ")]
        public async Task TestSendAsyncThrowsExceptionWhenUsernameIsNotSpecified(string? username)
        {
            IServiceProvider services = BuildServices("Test", new Dictionary<string, string?>
            {
                                                                  { "Test:AuthenticationProvider", "Basic" },
                                                                  { "Test:Basic:Username", username }
                                                              });

            HttpClient httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("Test");

            Func<Task> act = () => httpClient.GetAsync("https://somehost");

            await act.Should().ThrowAsync<InvalidOperationException>()
                              .WithMessage("HTTP client configured to use basic authentication but Username is missing in configuration.");

        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" \t\r\n ")]
        public async Task TestSendAsyncThrowsExceptionWhenPasswordIsNotSpecified(string? password)
        {
            IServiceProvider services = BuildServices("Test", new Dictionary<string, string?>
            {
                                                                  { "Test:AuthenticationProvider", "Basic" },
                                                                  { "Test:Basic:Username", "USERNAME" },
                                                                  { "Test:Basic:Password", password }
                                                              });

            HttpClient httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("Test");

            Func<Task> act = () => httpClient.GetAsync("https://somehost");

            await act.Should().ThrowAsync<InvalidOperationException>()
                              .WithMessage("HTTP client configured to use basic authentication but Password is missing in configuration.");

        }

        [Fact]
        public async Task TestSendAsyncSetsBasicAuthorizationCorrectly()
        {

            IServiceProvider services = BuildServices("Test", new Dictionary<string, string?>
                                                              {
                                                                  { "Test:AuthenticationProvider", "Basic" },
                                                                  { "Test:Basic:Username", "USERNAME" },
                                                                  { "Test:Basic:Password", "PASSWORD" }
                                                              });

            HttpClient httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("Test");

            await httpClient.GetAsync("https://somehost");

            Mock<HttpMessageHandler> hmhMock = services.GetRequiredService<Mock<HttpMessageHandler>>();

            hmhMock.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(),
                                                                  ItExpr.Is<HttpRequestMessage>((hrm) => VerifyBasicRequest(hrm)),
                                                                  ItExpr.IsAny<CancellationToken>());
        }

        private static bool VerifyBasicRequest(object obj)
        {
            if (obj is not HttpRequestMessage hrm ||
                hrm.Headers.Authorization is null ||
                hrm.Headers.Authorization.Scheme != "Basic" ||
                hrm.Headers.Authorization.Parameter is null)
            {
                return false;
            }

            string usernameAndPassword = Encoding.UTF8.GetString(Convert.FromBase64String(hrm.Headers.Authorization.Parameter));

            string[] splitUsernameAndPassword = usernameAndPassword.Split(":", StringSplitOptions.RemoveEmptyEntries);

            return splitUsernameAndPassword.Length == 2 &&
                   splitUsernameAndPassword[0] == "USERNAME" &&
                   splitUsernameAndPassword[1] == "PASSWORD";
        }
    }
}
