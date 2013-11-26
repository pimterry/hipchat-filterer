using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hipchat_filterer.Model.Pipeline
{
    public interface IBuildStep
    {
        string Name { get; }
        Action<IEnumerable<ICommit>> SuccessCallback { get; set; }
        Action<IEnumerable<ICommit>> FailureCallback { get; set; }
        void AddWaitingCommit(ICommit commit);
        void AddWaitingCommits(IEnumerable<ICommit> commits);

        void Start();
        void Pass();
        void Fail();
    }

    public class BuildStep : IBuildStep
    {
        public BuildStep(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
        public Action<IEnumerable<ICommit>> SuccessCallback { get; set; }
        public Action<IEnumerable<ICommit>> FailureCallback { get; set; }

        private readonly List<ICommit> _waitingCommits = new List<ICommit>();
        private readonly List<ICommit> _runningCommits = new List<ICommit>(); 

        public void AddWaitingCommit(ICommit commit)
        {
            _waitingCommits.Add(commit);
        }

        public void AddWaitingCommits(IEnumerable<ICommit> commits)
        {
            _waitingCommits.AddRange(commits);
        }

        public void Start()
        {
            // TODO: Think about race conditions
            _runningCommits.AddRange(_waitingCommits);
            _waitingCommits.Clear();
        }

        public void Pass()
        {
            SuccessCallback(_runningCommits);
            _runningCommits.Clear();
        }

        public void Fail()
        {
            FailureCallback(_runningCommits);
            _runningCommits.Clear();
        }
    }
}