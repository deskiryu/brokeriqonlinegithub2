using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Server.Helper
{
    public static class RunningWasm
    {
        public async static Task<bool> IsWebAssembly(IJSRuntime js)
        {
            return  js is IJSInProcessRuntime;
        }
    }
}
