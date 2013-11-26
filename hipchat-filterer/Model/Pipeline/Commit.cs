using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace hipchat_filterer.Model.Pipeline
{
    public interface ICommit
    {
    }

    public class Commit : ICommit
    {
        private readonly string branch;
        private readonly string message;
        private readonly string author;

        public Commit(string author, string message, string branch) {
            this.author = author;
            this.message = message;
            this.branch = branch;
        }

        public override string ToString() {
            return "'" + message + "' by " + author + " on " + branch;
        }
    }
}