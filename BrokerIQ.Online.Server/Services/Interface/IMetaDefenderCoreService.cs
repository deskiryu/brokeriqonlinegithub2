using System.IO;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
	public interface IMetaDefenderCoreService
	{
        Task<string> AnalyseFile(string fileName, MemoryStream data);
        Task<object> FetchAnalysisResult(string dataId);
    }
}
