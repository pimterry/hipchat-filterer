using System;
using System.Configuration;
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

            Post["/bitbucket"] = parameters =>
            {
                var commitNotification = this.Bind<BitbucketPostReceiveNotification>();

                string message;

                if (commitNotification.Commits.Count > 1)
                {
                    message = commitNotification.User + " pushed " + commitNotification.Commits.Count +
                                     " commits to " +
                                     String.Join(", ", commitNotification.Commits.Select(c => c.Branch).Distinct());
                }
                else
                {
                    var commit = commitNotification.Commits.Single();
                    message = commitNotification.User + " pushed '" + commit.Message + "' to " + commit.Branch;
                }

                notifier.SendNotification("Bitbucket", message);

                return "THANKS BITBUCKET";
            };

            Post["/jenkins"] = parameters =>
            {
                // Use Nancy model binding to unwrap Hudson's build notification structure
                // Associate this with whatever pipeline progress we're tracking
                // Ping hipchat if the pipeline's now complete (successfully or otherwise)

                var buildNotification = this.Bind<JenkinsBuildNotification>();

                string message = "Build " + buildNotification.Name + " has " + buildNotification.Build.Status + " in phase " + buildNotification.Build.Phase;
                notifier.SendNotification("Jenkins", message);

                return "THANKS FOR THE BUILD DEETS";
            };
        }
    }
}