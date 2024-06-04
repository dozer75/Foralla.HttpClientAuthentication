// Copyright © 2024 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using System.Net.Http.Headers;
using System.Text;
using KISS.HttpClientAuthentication.Configuration;

namespace KISS.HttpClientAuthentication.Handlers
{
    internal sealed class BasicAuthenticationHandler : BaseAuthenticationHandler<BasicConfiguration>
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(Configuration.Username))
            {
                throw new InvalidOperationException($@"HTTP client configured to use basic authentication but {nameof(Configuration.Username)} is missing in configuration.");
            }

            if (string.IsNullOrWhiteSpace(Configuration.Password))
            {
                throw new InvalidOperationException($@"HTTP client configured to use basic authentication but {nameof(Configuration.Password)} is missing in configuration.");
            }

            string encodedBasicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Configuration.Username}:{Configuration.Password}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedBasicAuth);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
