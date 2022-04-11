using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewIt.Web.Services.Interface
{
    public interface IHealthService
    {
        Task<bool> CanPingApi();
    }
}
