using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using hipchat_filterer.Model;
using Nancy.ModelBinding;

namespace hipchat_filterer
{
    using Nancy;

    public class NancyRoutes : NancyModule
    {
        private Pipeline pipeline;
        private INotificationTarget notifier;

        public NancyRoutes(INotificationTarget notifier) {
            this.notifier = notifier;

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
                commitNotification.Commits.ForEach(c => pipeline.AddCommit("'" + c.Message + "' by " + c.Author + " on " + c.Branch));

                notifier.SendNotification("Bitbucket", "Commit from " + commitNotification.User);

                return "THANKS BITBUCKET";
            };

            Post["/jenkins"] = parameters => {
                var buildNotification = this.Bind<JenkinsBuildNotification>();

                if (buildNotification.Build.Phase == "STARTED") {
                    pipeline.GetStep(buildNotification.Name).Start();
                } else if (buildNotification.Build.Phase == "FINISHED") {
                    pipeline.GetStep(buildNotification.Name).Finish(buildNotification.Build.Status == "SUCCESS");
                }

                string message = "Build " + buildNotification.Name + " has " + buildNotification.Build.Status +
                                 " in phase " + buildNotification.Build.Phase;
                notifier.SendNotification("Jenkins", message);

                return "THANKS FOR THE BUILD DEETS";
            };

            pipeline = new Pipeline() {
                steps = new List<BuildStep>() {
                    new BuildStep() { Name = "1s Job" },
                    new BuildStep() { Name = "30s test job" },
                },
                notifier = notifier
            };
        }

        // TODO: Refactor this out properly: just a PoC for now

        public class BuildStep
        {
            public BuildStep NextStep;
            public List<string> WaitingCommits = new List<string>();
            public List<string> RunningCommits = new List<string>();
            public string Name;
            public Pipeline Pipeline;

            public void Start() {
                // TODO: Worry even slightly about race conditions
                RunningCommits.AddRange(WaitingCommits);
                WaitingCommits.Clear();
            }

            public void Finish(bool success) {
                if (!success) {
                    Pipeline.FailCommits(RunningCommits, this);
                }
                else if (NextStep != null) {
                    NextStep.WaitingCommits.AddRange(RunningCommits);
                }
                else {
                    Pipeline.PassCommits(RunningCommits);
                }
                RunningCommits.Clear();
            }
        }

        public class Pipeline
        {
            public List<BuildStep> steps { get; set; }
            public INotificationTarget notifier { get; set; }

            public void AddCommit(string commit) {
                steps.ElementAt(0).WaitingCommits.Add(commit);
                steps.ForEach(s => s.Pipeline = this);
            }

            public BuildStep GetStep(string name) {
                return steps.Single(s => s.Name == name);
            }

            public void PassCommits(IEnumerable<string> commits) {string message;
                if (commits.Count() == 1) {
                    var commit = commits.Single();
                    message = "Commit " + commit + " passed all steps";
                }
                else {
                    message = "Commits " + String.Join(", ", commits) + " passed all steps";
                }

                notifier.SendNotification("Build Pipeline", message);
            }

            public void FailCommits(IEnumerable<string> commits, BuildStep failingStep) {
                string message;
                if (commits.Count() == 1) {
                    var commit = commits.Single();
                    message = "Commit " + commit + " failed at step " + failingStep.Name;
                }
                else {
                    message = "Commits " + String.Join(", ", commits) + " failed at " + failingStep.Name;
                }

                notifier.SendNotification("Build Pipeline", message);
            }
        }
    }
}