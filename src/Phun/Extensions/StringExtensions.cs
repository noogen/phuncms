namespace Phun.Extensions
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// String extension methods
    /// </summary>
    internal static class StringExtensions
    {
        private static Regex seoFriendlyRegEx = new Regex("[\\w]+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Gets the HTML body.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The content between the HTML body tag.</returns>
        internal static string GetHtmlBody(this string source)
        {
            var dataString = source;
            var startIndex = dataString.IndexOf("<body>", StringComparison.OrdinalIgnoreCase);
            var endIndex = dataString.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
            if (startIndex > 0 && endIndex > 0)
            {
                startIndex = startIndex + 6;
                dataString = dataString.Substring(startIndex, endIndex - startIndex);
            }

            return dataString;
        }

        /// <summary>
        /// To the name of the SEO friendly.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="maxLength">Length of the max.</param>
        /// <returns>
        /// Convert to SEO friendly name.
        /// </returns>
        internal static string ToSeoName(this string title, int maxLength = 250)
        {
            if (title == null)
            {
                return string.Empty;
            }

            int len = title.Length;
            bool prevdash = false;
            var sb = new StringBuilder(len);

            for (var i = 0; i < len; i++)
            {
                var c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase
                    sb.Append((char)(c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' ||
                    c == '\\' || c == '-' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if (c == '/' || c == '_')
                {
                    sb.Append(c);
                }
                else if ((int)c >= 128)
                {
                    int prevlen = sb.Length;
                    sb.Append(RemapInternationalCharToAscii(c));
                    if (prevlen != sb.Length)
                    {
                        prevdash = false;
                    }
                }

                if (i == maxLength)
                {
                    break;
                }
            }

            return prevdash ? sb.ToString().Substring(0, sb.Length - 1) : sb.ToString();
        }

        /// <summary>
        /// Remaps the international char to ASCII.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        private static string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s))
            {
                return "a";
            }
            else if ("èéêëę".Contains(s))
            {
                return "e";
            }
            else if ("ìíîïı".Contains(s))
            {
                return "i";
            }
            else if ("òóôõöøőð".Contains(s))
            {
                return "o";
            }
            else if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }
            else if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            else if ("żźž".Contains(s))
            {
                return "z";
            }
            else if ("śşšŝ".Contains(s))
            {
                return "s";
            }
            else if ("ñń".Contains(s))
            {
                return "n";
            }
            else if ("ýÿ".Contains(s))
            {
                return "y";
            }
            else if ("ğĝ".Contains(s))
            {
                return "g";
            }
            else if (c == 'ř')
            {
                return "r";
            }
            else if (c == 'ł')
            {
                return "l";
            }
            else if (c == 'đ')
            {
                return "d";
            }
            else if (c == 'ß')
            {
                return "ss";
            }
            else if (c == 'Þ')
            {
                return "th";
            }
            else if (c == 'ĥ')
            {
                return "h";
            }
            else if (c == 'ĵ')
            {
                return "j";
            }
            else
            {
                return "";
            }
        }
    }
}
