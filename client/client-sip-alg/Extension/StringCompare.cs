using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client_sip_alg.Extension
{
    public static class StringCompare
    {
        public static List<string> StringCompareRequest(this string requestInit,string request)
        {
            List<string> differences = new List<string>();
            string[] newLine = new string[] { "\r\n" };
            var arrRequest = request.Split(newLine, StringSplitOptions.None);
            var arrIniRequest = requestInit.Split(newLine, StringSplitOptions.None);

            for (int count = 0; count < arrRequest.Length; count++)
            {
                if (arrRequest[count] != arrIniRequest[count])
                    differences.Add(arrRequest[count] + Environment.NewLine + "                  " + arrIniRequest[count]);
            }

            return differences;
        }

    }
}
