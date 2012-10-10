﻿using System.Collections.Generic;

namespace HotCit
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
            } catch(KeyNotFoundException)
            {
                return null;
            }
            
        }

        public bool AddGameSetup(string id, GameSetup setup)
        {
            if (_gameSetups.ContainsKey(id))
                return false;
            _gameSetups[id] = setup;
            return true;
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
            return true;
        }

        public Game GetGame(string id)
        {
            try
            {
                return _games[id];
            }
            catch(KeyNotFoundException)
            {
                return null;
            }
        }


        public static GameRepository GetInstance()
        {
            return _instance ?? (_instance = new GameRepository());
        }
        private GameRepository() {
            _games["test"] = new Game(new SimpleGameFactory());
        }

        private static GameRepository _instance;
        private readonly IDictionary<string, Game> _games = new Dictionary<string, Game>();



    }
}