// Copyright © 2023 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

namespace KISS.HttpClientAuthentication.Configuration
{
    /// <summary>
    ///     Configuration settings for API key authentication.
    /// </summary>
    public sealed class ApiKeyConfiguration
    {
        /// <summary>
        ///     The header value used for the API key, defaults to x-api-key.
        /// </summary>
        public string Header { get; set; } = default!;

        /// <summary>
        ///     The api key to use.
        /// </summary>
        public string Value { get; set; } = default!;
    }
}
