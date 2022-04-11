using System;
using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Server.AppSettings
{
    public class AzureStorageDetails
    {
        public string Key { get; set; }
        public string ConnectionString { get; set; }
        public string VideoContainerName { get; set; }
        public string AudioContainerName { get; set; }
        public string LogoContainerName { get; set; }
    }
}
