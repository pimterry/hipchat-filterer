using System;
using System.Configuration;
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