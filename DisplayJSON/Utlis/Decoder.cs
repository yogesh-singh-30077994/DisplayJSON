using System.Text;

namespace DisplayJSON.Utlis
{
    public class Decoder
    {
        public static string Base64UrlDecode(string value)
        {
            string urlDecodedValue = value.Replace('_', '/').Replace('-', '+');
            switch (value.Length % 4)
            {
                case 2:
                    urlDecodedValue += "==";
                    break;
                case 3:
                    urlDecodedValue += "=";
                    break;
            }
            //return Encoding.ASCII.GetString(Convert.FromBase64String(urlDecodedValue));
            return Encoding.UTF8.GetString(Convert.FromBase64String(urlDecodedValue));
        }
    }
}
