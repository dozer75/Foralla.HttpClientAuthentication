// Copyright © 2024 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using System.Net.Http.Headers;
using KISS.HttpClientAuthentication.Configuration;
using KISS.HttpClientAuthentication.Constants;
using KISS.HttpClientAuthentication.Helpers;

namespace KISS.HttpClientAuthentication.Handlers
{
    internal sealed class OAuth2AuthenticationHandler(IOAuth2Provider provider) : BaseAuthenticationHandler<OAuth2Configuration>
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AccessTokenResponse? token = Configuration.GrantType switch
            {
                OAuth2GrantType.ClientCredentials => await provider.GetClientCredentialsAccessTokenAsync(Configuration, cancellationToken).ConfigureAwait(false),
                OAuth2GrantType.None => throw new InvalidOperationException($"{nameof(Configuration.GrantType)} must be specified."),
                _ => throw new InvalidOperationException($"The {nameof(Configuration.GrantType)} {Configuration.GrantType} is not supported."),
            };

            request.Headers.Authorization = token is not null
                ? new AuthenticationHeaderValue(token.TokenType, token.AccessToken)
                : throw new InvalidOperationException("HTTP client configured to use OAuth2 authentication, but no valid access token could be retrieved.");

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
