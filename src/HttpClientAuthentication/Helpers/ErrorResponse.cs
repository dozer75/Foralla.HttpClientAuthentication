// Copyright © 2025 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using System.Text.Json.Serialization;

namespace KISS.HttpClientAuthentication.Helpers
{
    /// <summary>
    ///     OAuth2 error response.
    /// </summary>
    internal sealed class ErrorResponse
    {
        /// <summary>
        ///     The OAuth2 error code.
        /// </summary>
        [JsonPropertyName("error")]
        public string Error { get; set; } = default!;

        /// <summary>
        ///     An optional description of the error in detail.
        /// </summary>
        [JsonPropertyName("error_description")]
        public string? Description { get; set; }

        /// <summary>
        ///     An optional Uri with more details of the error.
        /// </summary>
        [JsonPropertyName("error_uri")]
        public Uri? Uri { get; set; }
    }
}
