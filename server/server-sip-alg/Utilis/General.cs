using System;

namespace server_sip_alg.Utilis
{
    public static class General
    {
        public static string RandomString(int numCaracteres = 6, string chars = "abcdefghjkmnpqrstuvwxyz0123456789")
        {
            string result = "";
            Random rand = new Random();

            for (int count = 0; numCaracteres > count; count++)
            {
                result += chars.Substring(rand.Next(1, chars.Length - 1), 1);
            }

            return result;
        }

        public static string GenerateBranch()
        {
            return "z9hG4bK" + General.RandomString(8);
        }

        public static string GenerateTag()
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

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
