using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Services.Interface;

namespace BrokerIQ.Online.Server.Services
{
    public class AddOnBenefitSampleService : IAddOnBenefitSampleService
    {
        private const int DefaultInsuranceTypeKey = 0;

        private readonly SemaphoreSlim loadLock = new(1, 1);
        private Dictionary<int, List<AddOnBenefit>> samplesByType;
        private bool isLoaded;

        public async Task<IReadOnlyList<AddOnBenefit>> GetSamplesAsync(int insuranceType, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<AddOnBenefit>();
            }

            await EnsureSamplesLoadedAsync();

            if (samplesByType == null || samplesByType.Count == 0)
            {
                return Array.Empty<AddOnBenefit>();
            }

            IEnumerable<AddOnBenefit> source = Enumerable.Empty<AddOnBenefit>();

            if (samplesByType.TryGetValue(insuranceType, out var typeMatches) && typeMatches.Count > 0)
            {
                source = typeMatches;
            }
            else if (samplesByType.TryGetValue(DefaultInsuranceTypeKey, out var defaults) && defaults.Count > 0)
            {
                source = defaults;
            }
            else
            {
                source = samplesByType.Values.FirstOrDefault(v => v != null && v.Count > 0) ?? Enumerable.Empty<AddOnBenefit>();
            }

            return source
                .Where(s => !string.IsNullOrWhiteSpace(s.Title) || !string.IsNullOrWhiteSpace(s.Description))
                .Take(count)
                .Select(CloneBenefit)
                .ToList();
        }

        private async Task EnsureSamplesLoadedAsync()
        {
            if (isLoaded)
            {
                return;
            }

            await loadLock.WaitAsync();
            try
            {
                if (isLoaded)
                {
                    return;
                }

                samplesByType = await LoadSamplesAsync();
                isLoaded = true;
            }
            finally
            {
                loadLock.Release();
            }
        }

        private static async Task<Dictionary<int, List<AddOnBenefit>>> LoadSamplesAsync()
        {
            var result = new Dictionary<int, List<AddOnBenefit>>();

            var assembly = typeof(InsuranceEnum).Assembly;
            var resourceName = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("AddOnBenefitsSample.json", StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
            {
                return result;
            }

            await using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                return result;
            }

            using var document = await JsonDocument.ParseAsync(stream, new JsonDocumentOptions
            {
                AllowTrailingCommas = true
            });

            var root = document.RootElement;
            switch (root.ValueKind)
            {
                case JsonValueKind.Array:
                    foreach (var element in root.EnumerateArray())
                    {
                        ProcessEntry(element, result, null);
                    }
                    break;
                case JsonValueKind.Object:
                    foreach (var property in root.EnumerateObject())
                    {
                        if (property.NameEquals("insuranceBenefits") ||
                            property.Name.Equals("insuranceBenefits", StringComparison.OrdinalIgnoreCase))
                        {
                            if (property.Value.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var group in property.Value.EnumerateArray())
                                {
                                    ProcessBenefitGroup(group, result);
                                }
                            }
                            continue;
                        }

                        var typeOverride = ParseInsuranceType(property.Name);
                        if (typeOverride == null && !property.Name.Equals("default", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        int? resolvedType = typeOverride ?? DefaultInsuranceTypeKey;

                        if (property.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var element in property.Value.EnumerateArray())
                            {
                                ProcessEntry(element, result, resolvedType);
                            }
                        }
                        else
                        {
                            ProcessEntry(property.Value, result, resolvedType);
                        }
                    }
                    break;
            }

            return result;
        }

        private static void ProcessBenefitGroup(JsonElement element, IDictionary<int, List<AddOnBenefit>> accumulator)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            int? insuranceType = ParseInsuranceType(element);
            if (insuranceType == null && TryReadNumber(element, "insuranceId", out var idFromProperty))
            {
                insuranceType = idFromProperty;
            }

            if (insuranceType == null)
            {
                return;
            }

            if (!TryGetPropertyCaseInsensitive(element, "benefits", out var benefitsElement) ||
                benefitsElement.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var benefit in benefitsElement.EnumerateArray())
            {
                ProcessEntry(benefit, accumulator, insuranceType);
            }
        }

        private static void ProcessEntry(JsonElement element, IDictionary<int, List<AddOnBenefit>> accumulator, int? typeOverride)
        {
            int? insuranceType = typeOverride ?? ParseInsuranceType(element);
            insuranceType ??= DefaultInsuranceTypeKey;

            var title = ReadStringProperty(element, "Title") ?? ReadStringProperty(element, "Name");
            var description = ReadStringProperty(element, "Description") ?? ReadStringProperty(element, "Details");

            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(description))
            {
                return;
            }

            var key = insuranceType.Value;

            if (!accumulator.TryGetValue(key, out var list))
            {
                list = new List<AddOnBenefit>();
                accumulator[key] = list;
            }

            list.Add(new AddOnBenefit
            {
                Title = title?.Trim(),
                Description = description?.Trim()
            });
        }

        private static int? ParseInsuranceType(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (TryReadNumber(element, "InsuranceType", out var numeric))
            {
                return numeric;
            }

            if (TryReadNumber(element, "insuranceId", out var byId))
            {
                return byId;
            }

            var typeName = ReadStringProperty(element, "InsuranceType") ??
                           ReadStringProperty(element, "Type") ??
                           ReadStringProperty(element, "Insurance") ??
                           ReadStringProperty(element, "insuranceName") ??
                           ReadStringProperty(element, "name");

            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            return ParseInsuranceType(typeName);
        }

        private static int? ParseInsuranceType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric))
            {
                return numeric;
            }

            var normalized = value.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase)
                                  .Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase)
                                  .Replace("_", string.Empty, StringComparison.OrdinalIgnoreCase);

            foreach (var name in Enum.GetNames(typeof(InsuranceEnum)))
            {
                var enumNormalized = name.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase)
                                         .Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase)
                                         .Replace("_", string.Empty, StringComparison.OrdinalIgnoreCase);

                if (string.Equals(enumNormalized, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    var enumValue = (InsuranceEnum)Enum.Parse(typeof(InsuranceEnum), name);
                    return (int)enumValue;
                }
            }

            return null;
        }

        private static bool TryReadNumber(JsonElement element, string propertyName, out int value)
        {
            value = default;
            if (!TryGetPropertyCaseInsensitive(element, propertyName, out var property))
            {
                return false;
            }

            switch (property.ValueKind)
            {
                case JsonValueKind.Number:
                    if (property.TryGetInt32(out value))
                    {
                        return true;
                    }
                    break;
                case JsonValueKind.String:
                    if (int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }

        private static string ReadStringProperty(JsonElement element, string propertyName)
        {
            if (!TryGetPropertyCaseInsensitive(element, propertyName, out var property))
            {
                return null;
            }

            return property.ValueKind switch
            {
                JsonValueKind.String => property.GetString(),
                JsonValueKind.Number => property.ToString(),
                JsonValueKind.True => bool.TrueString,
                JsonValueKind.False => bool.FalseString,
                _ => null
            };
        }

        private static bool TryGetPropertyCaseInsensitive(JsonElement element, string propertyName, out JsonElement value)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                value = default;
                return false;
            }

            if (element.TryGetProperty(propertyName, out value))
            {
                return true;
            }

            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals(propertyName) ||
                    property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static AddOnBenefit CloneBenefit(AddOnBenefit benefit)
        {
            if (benefit == null)
            {
                return null;
            }

            return new AddOnBenefit
            {
                Title = benefit.Title,
                Description = benefit.Description
            };
        }
    }
}
