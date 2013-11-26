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

    public class NancyPipelineRoutes : NancyModule
    {
        public NancyPipelineRoutes(IPipeline pipeline, INotificationTarget notifier) {
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