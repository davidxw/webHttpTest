using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace webHttpTest.Models
{
    public class TraceRtViewModel
    {
        [Display(Name = "Host Name")]
        [Required]
        public string DestinationHost { get; set; }

        public List<string> Results { get; set; }

        public string ErrorText { get; set; }
    }
}
