using System.Collections.Generic;

namespace GuestFlow.Api.Configuration
{
    /// <summary>
    /// Security headers configuration (CSP, etc.)
    /// </summary>
    public class SecurityHeadersSettings
    {
        /// <summary>
        /// Additional connect-src entries to append to CSP (e.g. extra API hosts, CDN hosts).
        /// </summary>
        public List<string> ConnectSrc { get; set; } = new();
    }
}

