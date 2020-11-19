namespace OwnID.Services
{
    public interface IEncodingService
    {
        byte[] Base64UrlDecode(string arg);
        string Base64UrlEncode(byte[] arg);
        byte[] ASCIIDecode(string arg);
    }
}