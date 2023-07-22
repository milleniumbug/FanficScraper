using System;
using System.Collections.Generic;

namespace BrowserCookieGrabberService
{
    public partial class MozCookie
    {
        public long Id { get; set; }
        public string OriginAttributes { get; set; } = null!;
        public string? Name { get; set; }
        public string? Value { get; set; }
        public string? Host { get; set; }
        public string? Path { get; set; }
        public long? Expiry { get; set; }
        public long? LastAccessed { get; set; }
        public long? CreationTime { get; set; }
        public long? IsSecure { get; set; }
        public long? IsHttpOnly { get; set; }
        public long? InBrowserElement { get; set; }
        public long? SameSite { get; set; }
        public long? RawSameSite { get; set; }
        public long? SchemeMap { get; set; }
    }
}
