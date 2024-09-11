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

        Task<Audio> GetAudio(int id, int brokerId);

        Task<(int, string)> UploadAndAnalyseAudio(string fileName, MemoryStream audioStream, int brokerId);

        Task<bool> DeleteAudio(int id, int brokerId);
    }
}
