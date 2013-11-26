using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace hipchat_filterer
{
    public interface INotificationTarget
    {
        void SendNotification(string sourceName, string notification);
    }
}
