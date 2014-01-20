using System.Configuration;
using System.Linq;
using Nancy;
using Nancy.TinyIoc;
using hipchat_filterer.Model.Pipeline;

namespace hipchat_filterer
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            var buildStepNames = ConfigurationManager.AppSettings["PipelineBuildSteps"].Split(',');

            container.Register(buildStepNames.Select(s => (IBuildStep) new BuildStep(s)).ToArray());
        }
    }
}