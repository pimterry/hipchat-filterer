using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using HipChat;

namespace hipchat_filterer
{
    public class HipchatNotificationTarget : INotificationTarget
    {
        private readonly int _roomId;
        private readonly string _authToken;

        public HipchatNotificationTarget()
        {
            _roomId = Convert.ToInt32(Environment.GetEnvironmentVariable("RoomId") ?? ConfigurationManager.AppSettings["RoomId"]);
            _authToken = Environment.GetEnvironmentVariable("HipchatAuthToken") ?? ConfigurationManager.AppSettings["HipchatAuthToken"];
        }

        public void SendNotification(string sourceName, string notification)
        {
            var client = new HipChatClient(_authToken, _roomId, sourceName)
            {
                Notify = false,
                Color = HipChatClient.BackgroundColor.random
            };

            client.SendMessage(notification);
        }
    }
}