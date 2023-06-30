using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Online.Server.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface ITrainingVideoService
    {
        Task<List<TrainingVideo>> GetVideos();
    }
}
