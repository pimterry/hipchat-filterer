using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hipchat_filterer.Model
{
    public class BitbucketPostReceiveNotification
    {

        public List<Commit> Commits { get; set; }
        public string User { get; set; }

        public class Commit
        {
            public string Author { get; set; }
            public string Branch { get; set; }
            public string Message { get; set; }
        }
    }
}