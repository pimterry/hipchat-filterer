using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hipchat_filterer.Model.Incoming;
using hipchat_filterer.Model.Pipeline;
using Newtonsoft.Json;
using hipchat_filterer.Model;
using Nancy.ModelBinding;

namespace hipchat_filterer
{
    using Nancy;

    public class NancyRoutes : NancyModule
    {
        public NancyRoutes(IPipeline pipeline, INotificationTarget notifier) {
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
                notifier.SendNotification("Debug: Body", !String.IsNullOrEmpty(body) ? body : "No body");
                
                string form = String.Join(",", Request.Form.Keys);
                notifier.SendNotification("Debug: Form", !String.IsNullOrEmpty(form) ? form : "No form");

                string query = String.Join(",", Request.Query.Keys);
                notifier.SendNotification("Debug: Query", !String.IsNullOrEmpty(query) ? query : "No query");

                string parametersString = String.Join(",", parameters.Keys);
                notifier.SendNotification("Debug: Parameters", !String.IsNullOrEmpty(parametersString) ? parametersString : "No params");

                return "DEBUGGED";
            };

            Post["/bitbucket"] = parameters => {
                string notificationString = Request.Form.Payload;

                var commitNotification = JsonConvert.DeserializeObject<BitbucketPostReceiveNotification>(notificationString);
                commitNotification.Commits.ForEach(c => pipeline.AddToPipeline(new Commit(c.Author, c.Message, c.Branch)));

                return "THANKS BITBUCKET";
            };

            Post["/jenkins"] = parameters => {
                var buildNotification = this.Bind<JenkinsBuildNotification>();

                var buildStep = pipeline[buildNotification.Name];

                if (buildNotification.Build.Phase == "STARTED") {
                    buildStep.Start();
                } else if (buildNotification.Build.Phase == "FINISHED") {
                    if (buildNotification.Build.Status == "SUCCESS")
                    {
                        buildStep.Pass();
                    }
                    else
                    {
                        buildStep.Fail();
                    }
                }

                return "THANKS FOR THE BUILD DEETS";
            };
        }
    }
}