using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace webHttpTest.Models
{
    public class TraceRtViewModel
    {
        [Display(Name = "Host Name")]
        [Required]
        public string DestinationHost { get; set; }

        public List<PingResult> Results { get; set; }

        public string ErrorText { get; set; }
    }

    public class PingResult
    {
        public int hop { get; set; }
        public int et1 { get; set; }
        public int et2 { get; set; }
        public int et3 { get; set; }
        public IPAddress iPAddress { get; set; }
        public string hostName { get; set; }

        public PingResult()
        {
            iPAddress = null;
        }

        public string FormattedHostName
        {
            get
            {
                if (iPAddress == null)
                    return "Request Timed Out";

                if (string.IsNullOrEmpty(hostName))
                    return $"{iPAddress}";
                else
                    return $"{hostName} [{iPAddress}]";
            }
        }

        public string FormattedEt1
        {
            get
            {
                {
                    if (iPAddress == null)
                        return "*";
                    else
                        return $"{et1} ms";
                }
            }
        }
    }
}
