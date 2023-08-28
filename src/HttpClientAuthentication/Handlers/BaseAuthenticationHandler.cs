// Copyright © 2023 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

namespace KISS.HttpClientAuthentication.Handlers
{
    internal abstract class BaseAuthenticationHandler<TConfiguration> : DelegatingHandler
    {
        internal TConfiguration Configuration { get; set; } = default!;
    }
}
