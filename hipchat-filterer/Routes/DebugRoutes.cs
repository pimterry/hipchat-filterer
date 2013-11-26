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

    public class NancyDebugRoutes : NancyModule
    {
        public NancyDebugRoutes(INotificationTarget notifier)
        {
            Get["/"] = parameters => {
                notifier.SendNotification("Tim", "Test Message");
                return "HELLO";
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

            this.OnError += (ctx, ex) =>
            {
                while (ex is AggregateException)
                {
                    ex = ex.InnerException;
                }

                var message = ex.Message + " at " + ex.StackTrace;
                notifier.SendNotification("OnError", message);

                return "Error!";
            };
        }
    }
}