using System;
using System.Collections.Generic;

namespace webHttpTest.Models
{
    public class HostingEnvironment
    {
        public string MachineName { get;set;}

        public string HostName { get; set; }

        public List<string> IpAddresses { get; set; }

        public int ProcessorCount { get; set; }

        public long VirtualMemory {get; set; }

        public Dictionary<string, string> EnvironmentVariables { get; set; }
    }
}
