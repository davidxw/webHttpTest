using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace webHttpTest.Models
{
    public class HttpRequestViewModel
    {
        [Required]
        public string Method { get; set; }

        [Required]
        [Url]
        public string Url { get; set; }

        public string RequestContentType { get; set; }

        public string RequestHeaders { get; set; }

        public string RequestBody { get; set; }

        public int ResponseCode { get; set; }

        public string ResponseCodeText { get; set; }

        public long ResponseTimeMilliseconds { get; set; }

        public string ResponseBody { get; set; }

        public long ResponseBodyLength { get; set; }

        public string ResponseContentType { get; set; }

        public List<Header> ResponseHeaders { get; set; }

        public string ErrorText { get; set; }

        public List<SelectListItem> Methods { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "GET", Text="GET" },
            new SelectListItem { Value = "POST", Text ="POST" },
            new SelectListItem { Value = "PUT", Text ="PUT" },
            new SelectListItem { Value = "PATCH", Text ="PATCH" },
            new SelectListItem { Value = "DELETE", Text ="DELETE" }
        };

        public List<SelectListItem> ContentTypes { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "application/json", Text="JSON (application/json)" },
            new SelectListItem { Value = "text/html", Text="HTML (text/html)" },
            new SelectListItem { Value = "application/xml", Text="XML (application/xml)" },
            new SelectListItem { Value = "text/plain", Text="TEXT (text/plain)" },
            new SelectListItem { Value = "application/x-www.form-urlencoded", Text="FORM URL Encoded (application/x-www.form-urlencoded)" }
        };
    }

    public class Header
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
