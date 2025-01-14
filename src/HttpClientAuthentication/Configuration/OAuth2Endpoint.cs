// Copyright © 2025 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

namespace KISS.HttpClientAuthentication.Configuration
{
    /// <summary>
    ///     Endpoint configuration for OAuth2 endpoints
    /// </summary>
    public sealed partial class OAuth2Endpoint
    {
        /// <summary>
        ///     Gets or sets the Url to the OAuth2 endpoint.
        /// </summary>
        public Uri Url { get; set; } = default!;

        /// <summary>
        ///     Gets a dictionary that can contain additional headers that will be
        ///     supplied when requesting <see cref="Url"/>.
        /// </summary>
        public Dictionary<string, string> AdditionalHeaderParameters { get; } = [];


        /// <summary>
        ///     Gets a collection of additional form body parameters that will be
        ///     supplied when requesting <see cref="Url"/>.
        /// </summary>
        public Dictionary<string, string> AdditionalBodyParameters { get; } = [];

        /// <summary>
        ///     Gets a collection of additional query string parameters that will
        ///     be supplied when requesting <see cref="Url"/>.
        /// </summary>
        public Dictionary<string, string> AdditionalQueryParameters { get; } = [];

        public override string ToString()
        {
            return Url.ToString();
        }
    }
}
