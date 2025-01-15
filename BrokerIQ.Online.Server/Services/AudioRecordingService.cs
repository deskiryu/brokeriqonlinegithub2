using System.Threading.Tasks;
using Blazored.SessionStorage;
using BrokerIQ.Online.Server.Data;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class AudioRecordingService : IAudioRecordingService
    {
        private ISessionStorageService _sessionStorageService;

        public AudioRecordingService(ISessionStorageService sessionStorageService)
        {
            _sessionStorageService = sessionStorageService;
        }

        public async Task<string> GetAudioRecordingAsBase64()
        {
            var returned = (await _sessionStorageService.GetItemAsync<Wav>(ApplicationKeys.RECORDING_KEY));
            return returned.Data;
        }

        public async Task Delete()
        {
            await _sessionStorageService.RemoveItemAsync(ApplicationKeys.RECORDING_KEY);
        }
    }
}