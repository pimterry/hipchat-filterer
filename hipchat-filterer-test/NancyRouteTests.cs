using System;
using System.Linq;
using System.Linq.Expressions;
using hipchat_filterer;
using hipchat_filterer.Model.Pipeline;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nancy.Testing;
using Nancy.Validation;

namespace hipchat_filterer_test
{
    [TestClass]
    public class NancyRouteTests
    {
        private Browser browser;
        private Mock<IPipeline> pipelineMock;

        [TestInitialize]
        public void Setup()
        {
            pipelineMock = new Mock<IPipeline>();
            var notifier = new Mock<INotificationTarget>();
            browser = new Browser(bootstrapper => bootstrapper.Module<NancyRoutes>().Dependencies(pipelineMock.Object, notifier.Object));
        }

        [TestMethod]
        public void JenkinsRouteShouldFailPipelineForFailedBuilds()
        {
            var mockBuildStep = new Mock<IBuildStep>();
            pipelineMock.SetupGet(p => p[It.IsAny<string>()]).Returns(mockBuildStep.Object);

            browser.Post("/jenkins", with =>
            {
                with.HttpRequest();
                with.Header("Content-Type", "application/json");
                with.Body(@"{
                    ""name"": ""JobName"",
                    ""build"": {
	                    ""phase"": ""FINISHED"",
	                    ""status"": ""FAILED""
                    }
                }");
            });

            mockBuildStep.Verify(step => step.Fail());
        }

        [TestMethod]
        public void JenkinsRouteShouldUpdatePipelineForSuccessfulBuilds()
        {
            var mockBuildStep = new Mock<IBuildStep>();
            pipelineMock.SetupGet(p => p[It.IsAny<string>()]).Returns(mockBuildStep.Object);

            browser.Post("/jenkins", with =>
            {
                with.HttpRequest();
                with.Header("Content-Type", "application/json");
                with.Body(@"{
                    ""name"": ""JobName"",
                    ""build"": {
	                    ""phase"": ""FINISHED"",
	                    ""status"": ""FAILED""
                    }
                }");
            });

            mockBuildStep.Verify(step => step.Fail());
        }

        [TestMethod]
        public void BitbucketRouteShouldSendAddCommitsToPipeline()
        {
            browser.Post("/bitbucket", with =>
            {
                with.HttpRequest();
                with.Header("Content-Type", "application/x-www-form-urlencoded");
                with.FormValue("payload", BitbucketNotificationJson("Bob",
                                          BitbucketCommit("Bob", "Fixed bug 4", "master"),
                                          BitbucketCommit("Bob", "Added patch for #24", "master")));
            });

            pipelineMock.Verify(p => p.AddToPipeline(It.IsAny<ICommit>()));
        }

        private string BitbucketCommit(string user, string message, string branch)
        {
            return @"{
                ""author"": """ + user + @""", 
                ""branch"": """ + branch + @""", 
                ""files"": [
                    {
                        ""file"": ""somefile.py"", 
                        ""type"": ""modified""
                    }
                ], 
                ""message"": """ + message + @""",
                ""node"": ""620ade18607a"", 
                ""parents"": [
                    ""702c70160afc""
                ], 
                ""raw_author"": """ + user + @" <user@example.com>"",
                ""raw_node"": ""620ade18607ac42d872b568bb92acaa9a28620e9"", 
                ""revision"": null, 
                ""size"": -1, 
                ""timestamp"": """ + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ssK") + @""", 
                ""utctimestamp"": """ + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ssK") + @"""
            }";
        }

        private string BitbucketNotificationJson(string user, params string[] commits)
        {
            return @"{
                ""canon_url"": ""https://bitbucket.org"", 
                ""commits"": [
                    " + String.Join(",", commits) + @"
                ], 
                ""repository"": {
                    ""absolute_url"": ""/project/path/"", 
                    ""fork"": false, 
                    ""is_private"": false, 
                    ""name"": ""Project Name"", 
                    ""owner"": ""Mr Project Owner"", 
                    ""scm"": ""git"", 
                    ""slug"": ""project-name"", 
                    ""website"": ""https://project-website.com/""
                }, 
                ""user"": """ + user + @"""
            }";
        }
    }
}