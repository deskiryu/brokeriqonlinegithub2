using System;
using System.Collections.Generic;

namespace BrokerIQ.Online.Server.Services
{
    using System.Threading.Tasks;

    using AutoMapper;

    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Services.Abstract;
    using BrokerIQ.Online.Services.Interface;

    public class TrainingVideoService : ITrainingVideoService
    {
        private readonly string videoUrl = "TrainingVideo";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public TrainingVideoService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }
        public async Task<List<TrainingVideo>> GetVideos()
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

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
