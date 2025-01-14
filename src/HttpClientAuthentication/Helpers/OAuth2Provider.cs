// Copyright © 2025 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using KISS.HttpClientAuthentication.Configuration;
using KISS.HttpClientAuthentication.Constants;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace KISS.HttpClientAuthentication.Helpers
{
    /// <summary>
    ///     Implementation of <see cref="IOAuth2Provider"/>.
    /// </summary>
    internal sealed class OAuth2Provider(IHttpClientFactory clientFactory, ILogger<OAuth2Provider> logger, IMemoryCache memoryCache)
        : IOAuth2Provider
    {
        private readonly HttpClient _client = clientFactory.CreateClient(nameof(HttpClientAuthentication));

        /// <inheritdoc />
        public async ValueTask<AccessTokenResponse?> GetClientCredentialsAccessTokenAsync(OAuth2Configuration configuration, CancellationToken cancellationToken = default)
        {
            ValidateClientCredentialParameters(configuration);

            string cacheKey = $"{configuration.GrantType}#{configuration.TokenEndpoint}#{configuration.ClientCredentials!.ClientId}";

            AccessTokenResponse? token;

            if (!configuration.DisableTokenCache)
            {
                if (memoryCache.TryGetValue(cacheKey, out token))
                {
                    logger.LogDebug("Token for {TokenEndpoint} with client id {ClientId} found in cache, using this.",
                                          configuration.TokenEndpoint, configuration.ClientCredentials.ClientId);
                    return token;
                }

                logger.LogDebug("Could not find existing token in cache, requesting token from endpoint {TokenEndpoint} with client id {ClientId}.",
                            configuration.TokenEndpoint, configuration.ClientCredentials.ClientId);
            }

            using HttpRequestMessage request = GetTokenRequest(configuration);

            using HttpResponseMessage response = await _client.SendAsync(request, cancellationToken);

            token = await ParseResponseAsync(configuration, response, cancellationToken).ConfigureAwait(false);

            if (token is null)
            {
                return null;
            }

            if (configuration.DisableTokenCache)
            {
                logger.LogDebug("Token retrieved from {TokenEndpoint} with client id {ClientId}, but the token cache is disabled.",
                                      configuration.TokenEndpoint, configuration.ClientCredentials!.ClientId);
            }
            else if (token.ExpiresIn > 0)
            {
                double cacheExpiresIn = (int)token.ExpiresIn * 0.95;
                memoryCache.Set(cacheKey, token, TimeSpan.FromSeconds(cacheExpiresIn));

                logger.LogDebug("Token retrieved from {TokenEndpoint} with client id {ClientId} and cached for {CacheExpiresIn} seconds.",
                                      configuration.TokenEndpoint, configuration.ClientCredentials!.ClientId, cacheExpiresIn);
            }
            else
            {
                logger.LogDebug("Token retrieved from {TokenEndpoint} with client id {ClientId}, but not cached since it is missing expires_in information.",
                                      configuration.TokenEndpoint, configuration.ClientCredentials!.ClientId);
            }

            return token;
        }

        private static void AddTokenRequestHeaders(HttpRequestMessage request, OAuth2Configuration configuration)
        {
            if (configuration.ClientCredentials!.UseBasicAuthorizationHeader)
            {
                string encodedAuthorization = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{configuration.ClientCredentials!.ClientId}:{configuration.ClientCredentials.ClientSecret}"));

                request.Headers.Authorization = new("Basic", encodedAuthorization);
            }

            foreach (KeyValuePair<string, string> parameter in configuration.TokenEndpoint.AdditionalHeaderParameters)
            {
                request.Headers.Add(parameter.Key, parameter.Value);
            }
        }

        private static FormUrlEncodedContent GetClientCredentialsContent(OAuth2Configuration configuration)
        {
            Dictionary<string, string> requestBody = new()
            {
                { OAuth2Keyword.GrantType, OAuth2Keyword.ClientCredentials }
            };

            if (!configuration.ClientCredentials!.UseBasicAuthorizationHeader)
            {
                requestBody.Add(OAuth2Keyword.ClientId, configuration.ClientCredentials.ClientId);
                requestBody.Add(OAuth2Keyword.ClientSecret, configuration.ClientCredentials.ClientSecret);
            }

            if (!string.IsNullOrWhiteSpace(configuration.Scope))
            {
                requestBody.Add(OAuth2Keyword.Scope, configuration.Scope.Trim());
            }

            foreach (KeyValuePair<string, string> parameter in configuration.TokenEndpoint.AdditionalBodyParameters)
            {
                requestBody.Add(parameter.Key, parameter.Value);
            }

            return new FormUrlEncodedContent(requestBody);
        }

        private static Uri GetCompleteTokenUrl(OAuth2Endpoint tokenEndpoint)
        {
            if (tokenEndpoint.AdditionalQueryParameters.Count == 0)
            {
                return tokenEndpoint.Url;
            }

            UriBuilder uriBuilder = new(tokenEndpoint.Url);

            StringBuilder stringBuilder = new();

            foreach (KeyValuePair<string, string> parameter in tokenEndpoint.AdditionalQueryParameters)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"{parameter.Key}={WebUtility.UrlEncode(parameter.Value)}");
            }

            stringBuilder.Replace("&", "?", 0, 1);

            uriBuilder.Query = stringBuilder.ToString();

            return uriBuilder.Uri;
        }

        private static HttpRequestMessage GetTokenRequest(OAuth2Configuration configuration)
        {
            HttpRequestMessage request = new(HttpMethod.Get, GetCompleteTokenUrl(configuration.TokenEndpoint))
            {
                Method = HttpMethod.Post,
                Content = GetClientCredentialsContent(configuration)
            };

            AddTokenRequestHeaders(request, configuration);
            return request;
        }

        private async Task<AccessTokenResponse?> ParseResponseAsync(OAuth2Configuration configuration, HttpResponseMessage response,
                                                                    CancellationToken cancellationToken)
        {
            string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode != HttpStatusCode.BadRequest ||
                    !TryParseAndLogOAuth2Error(body, configuration.TokenEndpoint.Url, configuration.ClientCredentials!.ClientId))
                {
                    logger.LogError("Could not authenticate against {TokenEndpoint}, the returned status code was {StatusCode}. Response body: {Body}.",
                                    configuration.TokenEndpoint, response.StatusCode, body);
                }

                return null;
            }

            AccessTokenResponse? token = JsonSerializer.Deserialize<AccessTokenResponse>(body);

            if (token?.AccessToken is null)
            {
                logger.LogError("The result from {TokenEndpoint} is not a valid OAuth2 result.", configuration.TokenEndpoint);

                return null;
            }

            token.TokenType ??= "Bearer";

            if (!string.IsNullOrWhiteSpace(configuration.AuthorizationScheme))
            {
                token.TokenType = configuration.AuthorizationScheme!;
            }

            return token;
        }

        private bool TryParseAndLogOAuth2Error(string errorContent, Uri tokenEndpoint, string? clientId)
        {
            ErrorResponse? response = null;

            try
            {
                response = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
            }
            catch (JsonException)
            {
            }

            if (response == null || string.IsNullOrWhiteSpace(response.Error))
            {
                return false;
            }

            StringBuilder logMessage = new($"Could not authenticate against ");

            logMessage.Append(tokenEndpoint);

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                logMessage.Append(" with client id ");
                logMessage.Append(clientId);
            }
            logMessage.Append(". Error code: ");
            logMessage.Append(response.Error);

            if (!string.IsNullOrWhiteSpace(response.Description))
            {
                logMessage.Append(", description: ");
                logMessage.Append(response.Description);
            }

            if (response.Uri != null)
            {
                logMessage.Append(" (");
                logMessage.Append(response.Uri);
                logMessage.Append(')');
            }

            logMessage.Append('.');

#pragma warning disable CA2254 // Template should be a static expression
            logger.LogError(logMessage.ToString());
#pragma warning restore CA2254 // Template should be a static expression

            return true;
        }

        private static void ValidateClientCredentialParameters(OAuth2Configuration configuration)
        {
            if (configuration.GrantType is not OAuth2GrantType.ClientCredentials)
            {
                throw new ArgumentException($"{nameof(configuration.GrantType)} must be {OAuth2GrantType.ClientCredentials}.", nameof(configuration));
            }

            if (configuration.ClientCredentials is null)
            {
                throw new ArgumentException($"{nameof(configuration.ClientCredentials)} is null.", nameof(configuration));
            }

            if (string.IsNullOrWhiteSpace(configuration.ClientCredentials.ClientId))
            {
                throw new ArgumentException($"{nameof(configuration.ClientCredentials)}.{nameof(configuration.ClientCredentials.ClientId)} must be specified.",
                                            nameof(configuration));
            }

            if (string.IsNullOrWhiteSpace(configuration.ClientCredentials.ClientSecret))
            {
                throw new ArgumentException($"{nameof(configuration.ClientCredentials)}.{nameof(configuration.ClientCredentials.ClientSecret)} must be specified.",
                                            nameof(configuration));
            }

            if (configuration.TokenEndpoint is null)
            {
                throw new ArgumentException($"{nameof(configuration.TokenEndpoint)} is null.", nameof(configuration));
            }

            if (configuration.TokenEndpoint.Url is null)
            {
                throw new ArgumentException($"{nameof(configuration.TokenEndpoint)}.{nameof(configuration.TokenEndpoint.Url)} must be specified.", nameof(configuration));
            }
        }
    }
}
