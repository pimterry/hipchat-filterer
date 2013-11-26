using System;
using System.Collections.Generic;
using System.Linq;

namespace hipchat_filterer.Model.Pipeline
{
    public interface IPipeline
    {
        void AddToPipeline(ICommit commit);
        IBuildStep this[string stepName] { get; }
    }

    public class Pipeline : IPipeline
    {
        private readonly IEnumerable<IBuildStep> _steps;
        private readonly INotificationTarget _notifier;

        public Pipeline(INotificationTarget notifier, params IBuildStep[] steps)
        {
            this._notifier = notifier;
            this._steps = steps.Zip(Enumerable.Range(0, steps.Length), (step, i) =>
            {
                var nextStep = steps.ElementAtOrDefault(i + 1);
                if (nextStep == null)
                {
                    step.SuccessCallback = RecordSuccessfulCommits;
                }
                else
                {
                    step.SuccessCallback = nextStep.AddWaitingCommits;
                }

                step.FailureCallback = (commits => PipelineFailed(step, commits));

                return step;
            }).ToList();
        }

        public void AddToPipeline(ICommit commit)
        {
            _steps.First().AddWaitingCommit(commit);
        }

        public IBuildStep this[string stepName]
        {
            get
            {
                return _steps.Single(s => s.Name == stepName);
            }
        }

        private void RecordSuccessfulCommits(IEnumerable<ICommit> commits)
        {
            if (commits.Any())
            {
                string message;

                if (commits.Count() == 1)
                {
                    var commit = commits.Single();
                    message = "Commit " + commit + " passed all steps";
                }
                else
                {
                    message = "Commits " + String.Join(", ", commits) + " passed all steps";
                }

                this._notifier.SendNotification("Build pipeline", message);
            }
        }

        private void PipelineFailed(IBuildStep failingStep, IEnumerable<ICommit> commits)
        {
            if (commits.Any())
            {
                string message;
                if (commits.Count() == 1)
                {
                    var commit = commits.Single();
                    message = "Commit " + commit + " failed at step " + failingStep.Name;
                }
                else
                {
                    message = "Commits " + String.Join(", ", commits) + " failed at " + failingStep.Name;
                }

                _notifier.SendNotification("Build pipeline", message);
            }
        }

    }
}