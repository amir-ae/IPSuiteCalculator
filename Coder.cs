using System.Text;

namespace IPSuiteCalculator
{
    public class Coder
    {
        static Coder()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static string Decode(byte[] utfBytes)
        {
            byte[] koi8rBytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("KOI8-R"), utfBytes);
            string koi8rString = Encoding.GetEncoding("KOI8-R").GetString(koi8rBytes);
            return koi8rString;
        }

        public static byte[] Encode(string str)
        {
            byte[] koi8rBytes = Encoding.GetEncoding("KOI8-R").GetBytes(str);
            byte[] utfBytes = Encoding.Convert(Encoding.GetEncoding("KOI8-R"), Encoding.UTF8, koi8rBytes);
            return utfBytes;
        }
    }
}
