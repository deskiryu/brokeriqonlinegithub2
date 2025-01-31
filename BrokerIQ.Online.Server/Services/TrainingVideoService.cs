using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;

using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server.Services
{
    public class TrainingVideoService : ITrainingVideoService
    {
        private readonly string videoUrl = "TrainingVideo";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;

        public TrainingVideoService(IRequestProviderService requestProviderService, IMapper mapper)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
        }

        public async Task<List<TrainingVideo>> GetVideos()
        {
            var answer = new List<AzureTrainingVideoDto>();
            try
            {
                answer = await this.requestProviderService.Get<List<AzureTrainingVideoDto>>(this.videoUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetVideos: exception {ex.Message}");
            }
            return this.mapper.Map<List<TrainingVideo>>(answer);
        }
    }
}
