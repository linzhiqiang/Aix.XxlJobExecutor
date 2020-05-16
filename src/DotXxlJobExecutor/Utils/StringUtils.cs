using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class StringUtils
    {
        public static string AppendUrlPath(this string url, string path)
        {
            if (url != null) url = url.TrimEnd('/');
            if (path != null) path = path.TrimStart('/');
            return url + "/" + path;
        }
    }
}
