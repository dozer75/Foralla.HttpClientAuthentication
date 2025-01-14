// Copyright © 2025 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

namespace KISS.HttpClientAuthentication.Constants
{
    /// <summary>
    ///     The various grant types available.
    /// </summary>
    public enum OAuth2GrantType
    {
        /// <summary>
        ///     No grant types selected.
        /// </summary>
        /// <remarks>
        ///     This is not a valid option.
        /// </remarks>
        None,
        /// <summary>
        ///     Grant type to be used for authentication is <see cref="OAuth2Keyword.ClientCredentials"/>.
        /// </summary>
        ClientCredentials
    }
}
