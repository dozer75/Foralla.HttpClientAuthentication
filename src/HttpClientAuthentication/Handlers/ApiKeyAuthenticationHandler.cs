// Copyright © 2023 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using KISS.HttpClientAuthentication.Configuration;
using KISS.HttpClientAuthentication.Constants;

namespace KISS.HttpClientAuthentication.Handlers
{
    /// <summary>
    ///     A <see cref="DelegatingHandler"/> that adds a API key header to 
    ///     the request based on the configuration.
    /// </summary>
    internal sealed class ApiKeyAuthenticationHandler : BaseAuthenticationHandler<ApiKeyConfiguration>
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(Configuration.Header))
            {
                throw new InvalidOperationException($"HTTP client configured to use {AuthenticationProvider.ApiKey}, but {nameof(ApiKeyConfiguration.Header)} is not set in the {nameof(HttpClientAuthenticationConfiguration.ApiKey)} configuration.");
            }

            if (string.IsNullOrWhiteSpace(Configuration.Value))
            {
                throw new InvalidOperationException($"HTTP client configured to use {AuthenticationProvider.ApiKey}, but {nameof(ApiKeyConfiguration.Value)} is not set in the {nameof(HttpClientAuthenticationConfiguration.ApiKey)} configuration.");
            }

            request.Headers.Add(Configuration.Header, Configuration.Value);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
