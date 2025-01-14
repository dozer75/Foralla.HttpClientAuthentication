// Copyright © 2025 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

namespace KISS.HttpClientAuthentication.Configuration
{
    /// <summary>
    ///     Configuration settings for basic authentication.
    /// </summary>
    public sealed class BasicConfiguration
    {
        /// <summary>
        ///     The user name for the authentication.
        /// </summary>
        public string Username { get; set; } = default!;

        /// <summary>
        ///     The password for the authentication.
        /// </summary>
        public string Password { get; set; } = default!;
    }
}
