// Copyright © 2025 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

namespace KISS.HttpClientAuthentication.Constants
{
    /// <summary>
    ///     Different authentication provider systems that this library supports
    /// </summary>
    public enum AuthenticationProvider
    {
        /// <summary>
        ///     No authentication provider, this will ignore configuring authentication.
        /// </summary>
        None,

        /// <summary>
        ///     Enables OAuth2 authorization.
        /// </summary>
        /// <remarks>
        ///     Initializes an OAuth2 authorization process based on
        ///     the configuration specified in <see cref="Configuration.OAuth2Configuration"/>.
        /// </remarks>
        OAuth2,

        /// <summary>
        ///     Enables ApiKey authentication.
        /// </summary>
        /// <remarks>
        ///     Initializes an API key based authentication process based
        ///     on the configuration specified in <see cref="Configuration.ApiKeyConfiguration"/>.
        /// </remarks>
        ApiKey,

        /// <summary>
        ///     Enables bearer authentication.
        /// </summary>
        /// <remarks>
        ///     Initializes a basic authentication process based on the configuration
        ///     specified in <see cref="Configuration.BasicConfiguration"/>.
        /// </remarks>
        Basic
    }
}
