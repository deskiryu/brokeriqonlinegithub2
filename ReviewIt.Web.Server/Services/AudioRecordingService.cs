namespace ReviewIt.Web.Services
{
    using System.Threading.Tasks;
    using ReviewIt.Web.Server.Models;
    using ReviewIt.Web.Services.Interface;

    public class AudioRecordingService : IAudioRecordingService
    {
        private ILocalStorageService _localStorageService;

        private string _audioRecordingKey = "audiorecording";

        public AudioRecordingService(
            ILocalStorageService localStorageService
        ) {
            _localStorageService = localStorageService;
        }

        public async Task<string> GetAudioRecordingAsBase64()
        {
            var returned = (await _localStorageService.GetItem<Wav>(_audioRecordingKey));
            return returned.Data;
        } 

        public async Task Delete()
        {
            await _localStorageService.DeleteItem(_audioRecordingKey);
        }
    }
}