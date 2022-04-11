namespace ReviewIt.Web.Services.Interface
{
    using System.Threading.Tasks;

    public interface IAudioRecordingService
    {
        Task<string> GetAudioRecordingAsBase64();

        Task Delete();
    }
}
