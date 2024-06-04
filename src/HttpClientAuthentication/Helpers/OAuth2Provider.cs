// Copyright © 2024 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using System.Net;
using System.Net.Http.Headers;
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
            if (configuration.GrantType is not OAuth2GrantType.ClientCredentials)
            {
                throw new ArgumentException($"{nameof(configuration.GrantType)} must be {OAuth2GrantType.ClientCredentials}.", nameof(configuration));
            }

            if (configuration.ClientCredentials is null)
            {
                throw new ArgumentException($"No valid {nameof(configuration.ClientCredentials)} found.", nameof(configuration));
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


            string cacheKey = $"{configuration.GrantType}#{configuration.AuthorizationEndpoint}#{configuration.ClientCredentials!.ClientId}";

            if (memoryCache.TryGetValue(cacheKey, out AccessTokenResponse? token))
            {
                logger.LogInformation("Token for {AuthorizationEndpoint} with client id {ClientId} found in cache, using this.",
                                      configuration.AuthorizationEndpoint, configuration.ClientCredentials.ClientId);
                return token;
            }

            logger.LogDebug("Could not find existing token in cache, requesting token from endpoint {AuthorizationEndpoint} with client id {ClientId}.",
                            configuration.AuthorizationEndpoint, configuration.ClientCredentials.ClientId);

            using FormUrlEncodedContent requestContent = GetClientCredentialsContent(configuration.ClientCredentials!, configuration.Scope);

            using HttpResponseMessage result = configuration.ClientCredentials!.UseBasicAuthorizationHeader
                ? await PostWithBasicAuthenticationAsync(configuration, requestContent, cancellationToken).ConfigureAwait(false)
                : await _client.PostAsync(configuration.AuthorizationEndpoint, requestContent, cancellationToken).ConfigureAwait(false);

            token = await ParseResponseAsync(configuration, result, cancellationToken).ConfigureAwait(false);

            if (token is null)
            {
                return null;
            }

            if (configuration.DisableTokenCache)
            {
                logger.LogInformation("Token retrieved from {AuthorizationEndpoint} with client id {ClientId}, but the token cache is disabled.",
                                      configuration.AuthorizationEndpoint, configuration.ClientCredentials!.ClientId);
            }
            else if (token.ExpiresIn > 0)
            {
                double cacheExpiresIn = (int)token.ExpiresIn * 0.95;
                memoryCache.Set(cacheKey, token, TimeSpan.FromSeconds(cacheExpiresIn));

                logger.LogInformation("Token retrieved from {AuthorizationEndpoint} with client id {ClientId} and cached for {CacheExpiresIn} seconds.",
                                      configuration.AuthorizationEndpoint, configuration.ClientCredentials!.ClientId, cacheExpiresIn);
            }
            else
            {
                logger.LogInformation("Token retrieved from {AuthorizationEndpoint} with client id {ClientId}, but not cached since it is missing expires_in information.",
                                      configuration.AuthorizationEndpoint, configuration.ClientCredentials!.ClientId);
            }

            return token;
        }

        private static FormUrlEncodedContent GetClientCredentialsContent(ClientCredentialsConfiguration configuration, string? scope)
        {
            Dictionary<string, string> requestBody = new()
            {
                { OAuth2Keyword.GrantType, OAuth2Keyword.ClientCredentials }
            };

            if (!configuration.UseBasicAuthorizationHeader)
            {
                requestBody.Add(OAuth2Keyword.ClientId, configuration.ClientId);
                requestBody.Add(OAuth2Keyword.ClientSecret, configuration.ClientSecret);
            }

            if (!string.IsNullOrWhiteSpace(scope))
            {
                requestBody.Add(OAuth2Keyword.Scope, scope!.Trim());
            }

            return new FormUrlEncodedContent(requestBody!);
        }

        private async Task<AccessTokenResponse?> ParseResponseAsync(OAuth2Configuration configuration, HttpResponseMessage result,
                                                                    CancellationToken cancellationToken)
        {
#if NET6_0_OR_GREATER
            string body = await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            string body = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif

            if (!result.IsSuccessStatusCode)
            {
                if (result.StatusCode != HttpStatusCode.BadRequest ||
                    !TryParseAndLogOAuth2Error(body, configuration.AuthorizationEndpoint, configuration.ClientCredentials!.ClientId))
                {
                    logger.LogError("Could not authenticate against {AuthorizationEndpoint}, the returned status code was {StatusCode}. Response body: {Body}.",
                                    configuration.AuthorizationEndpoint, result.StatusCode, body);
                }

                return null;
            }

            AccessTokenResponse? token = JsonSerializer.Deserialize<AccessTokenResponse>(body);

            if (token?.AccessToken is null)
            {
                logger.LogError("The result from {AuthorizationEndpoint} is not a valid OAuth2 result.", configuration.AuthorizationEndpoint);

                return null;
            }

            token.TokenType ??= "Bearer";

            if (!string.IsNullOrWhiteSpace(configuration.AuthorizationScheme))
            {
                token.TokenType = configuration.AuthorizationScheme!;
            }

            return token;
        }

        private async Task<HttpResponseMessage> PostWithBasicAuthenticationAsync(OAuth2Configuration configuration, FormUrlEncodedContent requestContent,
                                                                           CancellationToken cancellationToken)
        {
            string encodedAuthorization = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{configuration.ClientCredentials!.ClientId}:{configuration.ClientCredentials.ClientSecret}"));

            using HttpRequestMessage request = new()
            {
                Content = requestContent,
                Method = HttpMethod.Post,
                RequestUri = configuration.AuthorizationEndpoint,
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Basic", encodedAuthorization)
                }
            };

            return await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private bool TryParseAndLogOAuth2Error(string errorContent, Uri authorizationEndpoint, string? clientId)
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

            logMessage.Append(authorizationEndpoint);

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
    }
}
