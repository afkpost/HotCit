﻿using System;
using Funq;
using HotCit.Data;
using HotCit.Server;
using ServiceStack.WebHost.Endpoints;

namespace HotCit
{
    public class Global : System.Web.HttpApplication
    {
        public class ServerHost : AppHostBase
        {
            public ServerHost()
                : base("HotCit Web Services", typeof(LobbyServer).Assembly)
            {

            }

            public override void Configure(Container container)
            {
                
                SetConfig(new EndpointHostConfig
                    {
                        GlobalResponseHeaders =
                            {
                                {"Access-Control-Allow-Origin", "*"},
                                {"Access-Control-Allow-Methods", "*"},
                                {"Access-Control-Allow-Headers", "*"}
                            },
                        DebugMode = true
                    });


                Routes.
                    Add<LobbyRequest>("/lobby/").
                    Add<LobbyRequest>("/lobby/{GameId}/").

                    Add<JoinRequest>("/lobby/{GameId}/users/").

                    Add<ReadyRequest>("/lobby/{GameId}/ready/").

                    Add<GameRequest>("/games/").
                    Add<GameRequest>("/games/{GameId}/").

                    Add<SecretRequest>("/games/{GameId}/secrets/").

                    Add<ResourceRequest>("/resources/").
                    Add<ResourceRequest>("/resources/{ResourceType}/").
                    Add<ResourceRequest>("/resources/{ResourceType}/{ResourceId}/");
            }
        }


        void Application_Start(object sender, EventArgs e)
        {
            new ServerHost().Init();
        }

        

    }
}
