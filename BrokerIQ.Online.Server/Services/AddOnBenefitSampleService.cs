using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.SampleData;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Services.Interface;

namespace BrokerIQ.Online.Server.Services
{
    public class AddOnBenefitSampleService : IAddOnBenefitSampleService
    {
        private readonly object cacheLock = new();

        private IReadOnlyDictionary<int, IReadOnlyList<AddOnBenefit>> samples;

        public Task<IReadOnlyList<AddOnBenefit>> GetSamplesAsync(int insuranceType, int maxCount = 5)
        {
            if (maxCount <= 0)
            {
                return Task.FromResult<IReadOnlyList<AddOnBenefit>>(System.Array.Empty<AddOnBenefit>());
            }

            var cache = EnsureSamplesLoaded();

            if (!cache.TryGetValue(insuranceType, out var benefits) || benefits == null || benefits.Count == 0)
            {
                return Task.FromResult<IReadOnlyList<AddOnBenefit>>(System.Array.Empty<AddOnBenefit>());
            }

            var result = benefits
                .Take(maxCount)
                .Select(CloneBenefit)
                .ToList();

            return Task.FromResult<IReadOnlyList<AddOnBenefit>>(result);
        }

        private IReadOnlyDictionary<int, IReadOnlyList<AddOnBenefit>> EnsureSamplesLoaded()
        {
            if (samples != null)
            {
                return samples;
            }

            lock (cacheLock)
            {
                if (samples != null)
                {
                    return samples;
                }

                samples = LoadSamples();
                return samples;
            }
        }

        private static IReadOnlyDictionary<int, IReadOnlyList<AddOnBenefit>> LoadSamples()
        {
            var source = AddOnBenefitsSample.Data?.InsuranceBenefits;

            if (source == null)
            {
                return new Dictionary<int, IReadOnlyList<AddOnBenefit>>();
            }

            return source
                .Where(item => item?.Benefits != null)
                .GroupBy(item => item.InsuranceId)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<AddOnBenefit>)group
                        .SelectMany(item => item.Benefits
                            .Where(benefit => benefit != null && !string.IsNullOrWhiteSpace(benefit.Title))
                            .Select(benefit => new AddOnBenefit
                            {
                                InsuranceId = group.Key,
                                Title = benefit.Title,
                                Description = benefit.Description
                            }))
                        .ToList());
        }

        private static AddOnBenefit CloneBenefit(AddOnBenefit source)
        {
            return new AddOnBenefit
            {
                InsuranceId = source.InsuranceId,
                Title = source.Title,
                Description = source.Description
            };
        }
    }
}
