using System;
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

        private void VerifyNotification(Expression<Func<string, bool>> expectedNotification)
        {
            _notifier.Verify(x => x.SendNotification(It.IsAny<string>(), It.Is(expectedNotification)));
        }
    }
}