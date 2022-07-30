namespace BrokerIQ.Online.Services.Interface
{
    using System.IO;
    using System.Threading.Tasks;

    public interface ILogoService
    {
        Task<bool> UploadLogo(string fileName, MemoryStream logoStream, int brokerId);
    }
}
