using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using HipChat;
using hipchat_filterer.Model;
using Nancy.ModelBinding;

namespace hipchat_filterer
{
    using Nancy;

    public class NancyRoutes : NancyModule
    {
        public NancyRoutes(INotificationTarget notifier) {
            Get["/"] = parameters => {
                notifier.SendNotification("Tim", "Test Message");
                return "HELLO";
            };

            this.OnError += (ctx, ex) => {
                while (ex is AggregateException) {
                    ex = ex.InnerException;
                }

                var message = ex.Message + " at " + ex.StackTrace;
                notifier.SendNotification("OnError", message);

                return "Error!";
            };

            Post["/debug"] = parameters => {
                var body = new StreamReader(Request.Body).ReadToEnd();

                notifier.SendNotification("Debug: Body", body);
                
                string form = String.Join(",", Request.Form.Keys);
                notifier.SendNotification("Debug: Form", String.IsNullOrEmpty(form) ? form : "No form");

                string query = String.Join(",", Request.Query.Keys);
                notifier.SendNotification("Debug: Query", String.IsNullOrEmpty(query) ? query : "No query");

                string parametersString = String.Join(",", parameters.Keys);
                notifier.SendNotification("Debug: Parameters", String.IsNullOrEmpty(parametersString) ? parametersString : "No params");

                return "DEBUGGED";
            };

            Post["/bitbucket"] = parameters => {
                var commitNotification = this.Bind<BitbucketPostReceiveNotification>();

                string message;

                if (commitNotification.Commits.Count > 1) {
                    message = commitNotification.User + " pushed " + commitNotification.Commits.Count +
                              " commits to " +
                              String.Join(", ", commitNotification.Commits.Select(c => c.Branch).Distinct());
                }
                else {
                    var commit = commitNotification.Commits.Single();
                    message = commitNotification.User + " pushed '" + commit.Message + "' to " + commit.Branch;
                }

                notifier.SendNotification("Bitbucket", message);

                return "THANKS BITBUCKET";
            };

            Post["/jenkins"] = parameters => {
                // Use Nancy model binding to unwrap Hudson's build notification structure
                // Associate this with whatever pipeline progress we're tracking
                // Ping hipchat if the pipeline's now complete (successfully or otherwise)

                var buildNotification = this.Bind<JenkinsBuildNotification>();

                string message = "Build " + buildNotification.Name + " has " + buildNotification.Build.Status +
                                 " in phase " + buildNotification.Build.Phase;
                notifier.SendNotification("Jenkins", message);

                return "THANKS FOR THE BUILD DEETS";
            };
        }
    }

    // TODO: Refactor this out properly: just a PoC for now

    public class BuildStep
    {
        public BuildStep NextStep;
        public List<string> WaitingCommits = new List<string>();
        public string Name;
    }

    public class Pipeline
    {
        public List<BuildStep> steps { get; set; }

        public void AddCommit(string commit) {
            steps.ElementAt(0).WaitingCommits.Add(commit);
        }
    }
}