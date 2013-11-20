﻿using System;
using System.Configuration;
using HipChat;

namespace hipchat_filterer
{
    using Nancy;

    public class IndexModule : NancyModule
    {
        public IndexModule() {
            Get["/"] = parameters => {
                int room_id = Convert.ToInt32(Environment.GetEnvironmentVariable("RoomId") ?? ConfigurationManager.AppSettings["RoomId"]);
                string auth_token = Environment.GetEnvironmentVariable("HipchatAuthToken") ?? ConfigurationManager.AppSettings["HipchatAuthToken"];
                string name_of_sender = Environment.GetEnvironmentVariable("NameOfSender") ?? ConfigurationManager.AppSettings["NameOfSender"];

                var client = new HipChatClient(auth_token, room_id, name_of_sender);
                client.Notify = false;
                client.Color = HipChatClient.BackgroundColor.random;
                client.SendMessage("Test message");

                return "HELLO";
            };
        }
    }
}