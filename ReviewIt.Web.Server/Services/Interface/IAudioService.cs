using System;
using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Services.Interface
{
    using System.IO;
    using System.Threading.Tasks;
    using BrokerIQ.Online.Server.Models;

    public interface IAudioService
    {
        Task<List<Audio>> GetAudios(int brokerId);

        Task<bool> UploadAudio(string fileName, string audioAsBase64, int brokerId);

        Task<bool> DeleteAudio(string fileName);

        Task<bool> NameAvailable(string name);
    }
}
