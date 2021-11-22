using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace client_sip_alg
{
    public static class General
    {        
        public static string RandomString(int numCaracteres = 6, string chars = "abcdefghjkmnpqrstuvwxyz0123456789")
        {
            string result = "";
            Random rand = new Random();
            
            for (int count = 0; numCaracteres > count; count++)
            {
                result +=  chars.Substring(rand.Next(1, chars.Length - 1), 1);
            }
            
            return result;
        }

        public static string GenerateBranch ()
        {
            return "z9hG4bK" + General.RandomString(8);
        }

        public static string GenerateTag ()
        {
            return General.RandomString(8);
        }

        public static string GenerateCallid()
        {
            return General.RandomString(10);
        }

        public static string GenerateCseq()
        {
            var rand = new Random();
            return rand.Next(0, 999).ToString();
        }

        public static string Base64Decode(string base64EncodedData)
        {                       
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string GetLocalIp ()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            var ipResult = localIPs.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToArray();
            if (ipResult.Length > 0)
                return ipResult[0].ToString();
            else
                return "170.0.0.1";
        }

        public static int GetContentLength(string contentLength) 
        {
            var arrContent = contentLength.Split(':');

            if (arrContent.Length == 2)
                return Convert.ToInt32(arrContent[1]);

            return -1;
        }
    }
}
