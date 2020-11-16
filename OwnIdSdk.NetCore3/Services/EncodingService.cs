using System.Text;
using Fido2NetLib;

namespace OwnIdSdk.NetCore3.Services
{
    public class EncodingService : IEncodingService
    {
        public byte[] Base64UrlDecode(string arg)
        {
            return Base64Url.Decode(arg);
        }

        public string Base64UrlEncode(byte[] arg)
        {
            return Base64Url.Encode(arg);
        }

        public byte[] ASCIIDecode(string arg)
        {
            return Encoding.ASCII.GetBytes(arg);
        }
    }
}