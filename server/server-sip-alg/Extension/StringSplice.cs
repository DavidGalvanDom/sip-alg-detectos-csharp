using System.Text.RegularExpressions;

namespace server_sip_alg.Extension
{
    public static class StringSplice
    {
        public static string SpliceText (this string pText, int size)
        {
            return Regex.Replace(pText, "(.{" + size +"})", "$1" + "\n");
        }

    }
}
