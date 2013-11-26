using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.Elmah;
using Nancy.TinyIoc;
using hipchat_filterer.Model.Pipeline;

namespace hipchat_filterer
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            container.Register(new IBuildStep[] {
                new BuildStep("1s Job"), 
                new BuildStep("30s test job"), 
            });
        }
    }
}