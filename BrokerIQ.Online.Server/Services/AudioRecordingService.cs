namespace BrokerIQ.Online.Services
{
    using System.Threading.Tasks;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Services.Interface;

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