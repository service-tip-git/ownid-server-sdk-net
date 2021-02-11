using System.Threading.Tasks;

namespace OwnID.Extensibility.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toAddress, string subject, string body, bool isHtml = false, string toName = "");
    }
}