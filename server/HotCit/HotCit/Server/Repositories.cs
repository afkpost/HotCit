using System.Collections.Generic;
using HotCit.Data;
using HotCit.Test;
using HotCit.Util;

namespace HotCit.Server
{
    public class GameSetupRepository
    {
        public IDictionary<string, GameSetup> GameSetups
        {
            get
            {
                return new Dictionary<string, GameSetup>(_gameSetups); //data integrety
            }
        }

        public GameSetup GetSetup(string id)
        {
            try
            {
                return _gameSetups[id];
            }
            catch (KeyNotFoundException)
            {
                throw new HotCitException(ExceptionType.NotFound, "GameSetup " + id + " not found.");
            }

        }

        public void AddGameSetup(string id, GameSetup setup)
        {
            if (_gameSetups.ContainsKey(id))
                throw new HotCitException(ExceptionType.Impossible);
            _gameSetups[id] = setup;
        }

        public bool RemoveGameFactory(string id)
        {
            return _gameSetups.Remove(id);
        }

        public static GameSetupRepository GetInstance()
        {
            return _instance ?? (_instance = new GameSetupRepository());
        }


        private GameSetupRepository() { }
        private readonly IDictionary<string, GameSetup> _gameSetups = new Dictionary<string, GameSetup>();
        private static GameSetupRepository _instance;
    }

    public class GameRepository
    {
        public IEnumerable<string> Games
        {
            get
            {
                return _games.Keys; //data integrety
            }
        }

        public bool AddGame(string id, Game game)
        {
            if (_games.ContainsKey(id))
            {
                return false;
            }
            _games[id] = game;
            _listeners[id] = new BlockingGameListener(game);
            return true;
        }

        public void RemoveAllGames()
        {
            _games.Clear();
        }

        public KeyValuePair<int, GameResponse> GetPartialGame(string id, int lastSeenGame)
        {
            try
            {
                return _listeners[id].GetGame(lastSeenGame);
            } catch(KeyNotFoundException)
            {
                throw new HotCitException(ExceptionType.NotFound, "Game " + id + " not found");
            }
        }

        public Game GetGame(string id)
        {
            try
            {
                return _games[id];
            } catch(KeyNotFoundException)
            {
                throw new HotCitException(ExceptionType.NotFound, "Game " + id + " not found");
            }
        }


        public static GameRepository GetInstance()
        {
            return _instance ?? (_instance = new GameRepository());
        }
        private GameRepository()
        {
            AddGame("test", new Game(new SimpleGameFactory()));
        }


        private static GameRepository _instance;
        private readonly IDictionary<string, Game> _games = new Dictionary<string, Game>();
        private readonly IDictionary<string, BlockingGameListener> _listeners = new Dictionary<string, BlockingGameListener>();

        public void RemoveGame(string id)
        {
            _games.Remove(id);
            _listeners.Remove(id);
        }
    }
}