﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using HotCit.Data;
using HotCit.Strategies;
using HotCit.Util;
using ServiceStack.Common.Web;
using Action = HotCit.Data.Action;
using HotCit.Test;

namespace HotCit.Server
{
    public class LobbyServer : AbstractServer<LobbyRequest>
    {
        public object Get(LobbyRequest request)
        {
            var id = request.GameId;
            if (id == null)
                return SetupRepository.GameSetups;
            return GetGameSetup(id);
        }

        public object Post(LobbyRequest request)
        {
            var id = request.GameId;
            var minPlayers = request.MinPlayers;
            var maxPlayers = request.MaxPlayers;
            var password = request.Password;
            var user = User;

            ICharacterDiscardStrategy discardStrategy;
            switch (request.Discard)
            {
                case "fixed":
                    discardStrategy = new FixedDiscardStrategy
                    {
                        CharacterNumber = 7 //discard architect each round
                    };
                    break;

                //case "random":
                default:
                    discardStrategy = new RandomDiscardStrategy();
                    break;
            }

            var setup = new GameSetup(minPlayers, maxPlayers, password, discardStrategy);
            SetupRepository.AddGameSetup(id, setup);
            setup.Join(user);
            return new HttpResult
                {
                    StatusCode = HttpStatusCode.Created,
                    Response = setup
                };
        }
    }

    public class JoinServer : AbstractServer<JoinRequest>
    {
        public object Get(JoinRequest request)
        {
            var id = request.GameId;
            if (id == null) throw new HotCitException(ExceptionType.IllegalInput);
            return GetGameSetup(id).GetUsers();
        }

        public object Put(JoinRequest request)
        {
            var id = request.GameId;
            if (id == null) throw new HotCitException(ExceptionType.IllegalInput);
            var fac = GetGameSetup(id);

            fac.Join(User);
            return Succeeded;
        }

        public object Delete(JoinRequest request)
        {
            var id = request.GameId;
            if (id == null) throw new HotCitException(ExceptionType.IllegalInput);

            var game = GetGameSetup(id);
            game.Leave(User);

            if (game.GetUsers().Count == 0)
                SetupRepository.RemoveGameFactory(id);
            return Succeeded;
        }
    }

    public class ReadyServer : AbstractServer<ReadyRequest>
    {
        public object Put(ReadyRequest request)
        {
            var id = request.GameId;
            if (id == null) throw new HotCitException(ExceptionType.IllegalInput);

            var fac = GetGameSetup(id);

            if (fac.SetReady(User, true)) //everyone is ready
            {
                var game = new Game(fac);
                GameRepository.AddGame(id, game);
                SetupRepository.RemoveGameFactory(id);
            }
            return Succeeded;
        }

        public object Delete(ReadyRequest request)
        {
            var id = request.GameId;
            if (id == null) throw new HotCitException(ExceptionType.IllegalInput);

            var fac = GetGameSetup(id);

            fac.SetReady(User, false);
            return Succeeded;
        }

    }

    public class GameServer : AbstractServer<GameRequest>
    {
        public object Get(GameRequest request)
        {
            var id = request.GameId;
            if (id == null) return GameRepository.Games;
            var statusCode = IfRange == -1 ? HttpStatusCode.OK : HttpStatusCode.PartialContent;
            return new HttpResult(GetPartialGame(request.GameId, IfRange), statusCode);
        }

        public object Put(GameRequest request)
        {
            var game = GetGame(request.GameId);

            if (request.Select != null)
            {
                game.Select(User, request.Select);
                return new HttpResult(HttpStatusCode.NoContent, "");
            }

            if (request.Action != null)
            {
                switch (request.Action)
                {
                    case Action.EndTurn:
                        game.EndTurn(User);
                        return Succeeded;
                    case Action.TakeGold:
                        game.TakeGold(User);
                        return Succeeded;
                    case Action.DrawDistricts:
                        game.DrawDistricts(User);
                        return Succeeded;
                }
            }

            if (request.Build != null)
            {
                game.BuildDistrict(User, request.Build);
                return Succeeded;
            }

            if (request.Ability != null)
            {
                game.UseAbility(User, request.Ability);
                return Succeeded;
            }

            throw new HotCitException(ExceptionType.IllegalInput);
        }

        public object Delete(GameRequest request)
        {
            var id = request.GameId;
            if (id == null)
            {
                //Restart server
                GameRepository.RemoveAllGames();
                GameRepository.AddGame("test", new Game(new SimpleGameFactory()));
                return Succeeded;
            }
            if (GetGame(id).Players.Any(p => p.Username == User))
            {
                GameRepository.RemoveGame(id);
                return Succeeded;
            }
            throw new HotCitException(ExceptionType.BadAction, "You are not a part of the game, and therefore cannot delete it");
        }
    }

    public class ResourceServer : AbstractServer<ResourceRequest>
    {
        public object Get(ResourceRequest request)
        {
            var type = request.ResourceType;
            var id = request.ResourceId;
            var all = id == null;
            switch (type)
            {
                case ResourceType.All:
                    var map = new Dictionary<ResourceType, object>();
                    map[ResourceType.Characters] = Resources.Characters;
                    map[ResourceType.Districts] = Resources.Districts;
                    return map;
                case ResourceType.Characters:
                    if (all)
                        return Resources.Characters;
                    var ch = Resources.GetCharacter(id);
                    if (ch == null) return HttpError.NotFound("Character " + id + " not found");
                    return ch;
                case ResourceType.Districts:
                    if (all)
                        return Resources.Districts;
                    var di = Resources.GetDistrict(id);
                    if (di == null) return HttpError.NotFound("District " + id + " not found");
                    return di;
                case ResourceType.Images:
                    if (all) return new HttpError(HttpStatusCode.BadRequest, "");
                    var img = Resources.GetImage(id, request.Dpi);
                    if (img == null) return HttpError.NotFound("Image " + id + " not found");
                    return new HttpResult(img, "image/png");
            }
            throw new HotCitException(ExceptionType.IllegalInput);
        }
    }

    public class SecretServer : AbstractServer<SecretRequest>
    {
        public object Get(SecretRequest request)
        {
            var game = GetGame(request.GameId);
            switch (request.What)
            {
                case What.Options: return game.GetOptions(User);
                case What.Hand: return game.GetHand(User);
                default: throw new IllegalInputException("Should not happen!!");
            }
            
        }
    }
}