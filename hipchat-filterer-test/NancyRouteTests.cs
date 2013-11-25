using System;
using System.Linq;
using System.Linq.Expressions;
using hipchat_filterer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nancy.Testing;
using Nancy.Validation;

namespace hipchat_filterer_test
{
    [TestClass]
    public class NancyRouteTests
    {
        private Browser _browser;
        private Mock<INotificationTarget> _notifier;

        [TestInitialize]
        public void Setup()
        {
            _notifier = new Mock<INotificationTarget>();
            _browser = new Browser(bootstrapper => bootstrapper.Module<NancyRoutes>().Dependency(_notifier.Object));
        }

        [TestMethod]
        public void JenkinsRouteShouldSendNotificationsForFailedBuilds()
        {
            _browser.Post("/jenkins", with =>
            {
                with.HttpRequest();
                with.Header("Content-Type", "application/json");
                with.Body(@"{
                    ""name"": ""JobName"",
                    ""build"": {
	                    ""phase"": ""STARTED"",
	                    ""status"": ""FAILED""
                    }
                }");
            });

            VerifyNotification(s => s.Contains("JobName") && s.Contains("FAILED"));
        }

        [TestMethod]
        public void BitbucketRouteShouldSendFullNotificationForSingleCommit()
        {
            _browser.Post("/bitbucket", with =>
            {
                with.HttpRequest();
                with.Header("Content-Type", "application/x-www-form-urlencoded");
                with.FormValue("payload", BitbucketNotificationJson("Bob", BitbucketCommit("Bob", "Fixed bug 4", "master")));
            });

            VerifyNotification(s => s.Contains("Bob") && s.Contains("Fixed bug 4") && s.Contains("master"));
        }

        [TestMethod]
        public void BitbucketRouteShouldSendSimpleNotificationForMultipleCommits()
        {
            _browser.Post("/bitbucket", with =>
            {
                with.HttpRequest();
                with.Header("Content-Type", "application/x-www-form-urlencoded");
                with.FormValue("payload", BitbucketNotificationJson("Bob", 
                                          BitbucketCommit("Bob", "Fixed bug 4", "master"),
                                          BitbucketCommit("Bob", "Added patch for #24", "master")));
            });

            VerifyNotification(s => s.Contains("Bob") && s.Contains("2 commits") && !s.Contains("bug 4"));
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

        private void VerifyNotification(Expression<Func<string, bool>> expectedNotification)
        {
            _notifier.Verify(x => x.SendNotification(It.IsAny<string>(), It.Is(expectedNotification)));
        }
    }
}