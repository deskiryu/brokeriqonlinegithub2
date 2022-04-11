using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewIt.Web.Pages
{
    using Dto.Models;
    using Microsoft.AspNetCore.Components;
    using Models;
    using Services.Interface;

    public class BrokerListBase : ComponentBase
    {
        [Inject]
        public IBrokerService BrokerService { get; set; }

        public List<Broker> Brokers { get; set; }

    }
}
