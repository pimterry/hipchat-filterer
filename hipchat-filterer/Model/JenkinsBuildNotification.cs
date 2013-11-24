using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using Nancy;

namespace hipchat_filterer.Model
{
    public class JenkinsBuildNotification
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public BuildStatus Build { get; set; }

        public class BuildStatus
        {
            public int Number  { get; set; }
            public string Phase { get; set; }
            public string Status { get; set; }
            public string Url { get; set; }
            public string FullUrl { get; set; }
            public Dictionary<string, string> Parameters { get; set; } 
        }
    }
}