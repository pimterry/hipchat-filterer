using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using CsQuery.Engine.PseudoClassSelectors;
using hipchat_filterer;
using hipchat_filterer.Model.Pipeline;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace hipchat_filterer_test
{
    [TestClass]
    public class PipelineTests
    {
        public Mock<INotificationTarget> NotifierMock;

        [TestInitialize]
        public void Setup()
        {
            NotifierMock = new Mock<INotificationTarget>();
        }

        [TestMethod]
        public void PipelineShouldPassCommitsIntoFirstBuildStep()
        {
            var stepMock = new Mock<IBuildStep>();
            var pipeline = new Pipeline(NotifierMock.Object, stepMock.Object);

            var commit = NewCommit();
            pipeline.AddToPipeline(commit);

            stepMock.Verify(s => s.AddWaitingCommit(commit));
        }

        [TestMethod]
        public void PipelineShouldNotNotifyOnNonFinalBuildSuccess()
        {
            var firstStepMock = new Mock<IBuildStep>();
            firstStepMock.SetupProperty(s => s.SuccessCallback);

            var pipeline = new Pipeline(NotifierMock.Object, firstStepMock.Object, new Mock<IBuildStep>().Object);
            firstStepMock.Object.SuccessCallback(SingletonCommit());

            VerifyThereWereNoNotifications();
        }

        [TestMethod]
        public void PipelineShouldNotifySuccessOnFinalBuildSuccess()
        {
            var lastStepMock = new Mock<IBuildStep>();
            lastStepMock.SetupProperty(s => s.SuccessCallback);

            var pipeline = new Pipeline(NotifierMock.Object, new Mock<IBuildStep>().Object, lastStepMock.Object);
            lastStepMock.Object.SuccessCallback(SingletonCommit());

            VerifyNotification(s => s.Contains("passed"));
        }

        [TestMethod]
        public void PipelineShouldNotifyFailureOnEarlyBuildFailure()
        {
            var firstStepMock = new Mock<IBuildStep>();
            firstStepMock.SetupProperty(s => s.FailureCallback);

            var pipeline = new Pipeline(NotifierMock.Object, firstStepMock.Object, new Mock<IBuildStep>().Object);
            firstStepMock.Object.FailureCallback(SingletonCommit());

            VerifyNotification(s => s.Contains("failed"));
        }

        [TestMethod]
        public void PipelineShouldNotifyFailureOnFinalBuildFailure()
        {
            var lastStepMock = new Mock<IBuildStep>();
            lastStepMock.SetupProperty(s => s.FailureCallback);

            var pipeline = new Pipeline(NotifierMock.Object, new Mock<IBuildStep>().Object, lastStepMock.Object);
            lastStepMock.Object.FailureCallback(SingletonCommit());

            VerifyNotification(s => s.Contains("failed"));
        }

        [TestMethod]
        public void PipelineShouldNotNotifyOnPipelinesWithoutCommits()
        {
            var firstStepMock = new Mock<IBuildStep>();
            var lastStepMock = new Mock<IBuildStep>();
            firstStepMock.SetupAllProperties();
            lastStepMock.SetupAllProperties();

            var pipeline = new Pipeline(NotifierMock.Object, firstStepMock.Object, lastStepMock.Object);
            firstStepMock.Object.SuccessCallback(new List<ICommit>());
            lastStepMock.Object.SuccessCallback(new List<ICommit>());

            VerifyThereWereNoNotifications();
        }

        private ICommit NewCommit() {
            return new Commit("", "", "");
        }

        private IEnumerable<ICommit> SingletonCommit()
        {
            return new List<ICommit>() { NewCommit() };
        }

        private void VerifyThereWereNoNotifications()
        {
            NotifierMock.Verify(n => n.SendNotification(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        private void VerifyNotification(Expression<Func<string, bool>> expectedNotification)
        {
            NotifierMock.Verify(x => x.SendNotification(It.IsAny<string>(), It.Is(expectedNotification)));
        }
    }
}
