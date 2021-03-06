/*global define*/

define(function (require) {
	"use strict";
   
    var ClientPlayerView = require('./views/clientplayer_view'),
        OpponentsView = require('./views/opponents_view'),
        OptionsView = require('./views/options_view'),
        SimulateView = require('./views/simulate_view'),
        PlayerInTurnView = require('./views/playerinturn_view'),
        CharacterView = require('./views/character_view'),
        BoardView = require('./views/board'),
        ScoreboardView = require('./views/scoreboard_view'),
        utils = require('utils'),
        $ = require('jquery'),
        template = utils.getTemplate('app'),
        views,
        opponentsView,
        playerView,
        boardView,
        optionsView,
        playerInTurnView,
        simulateView,
        characterView,
        scoreboardView;
    
    /* CLASS */
    return function AppView(model, state, playercontroller, simulatecontroller) {
        var that = this;
        
        function attach(Constructor, idd) {
            var container = that.elm.find('#'+idd);
            var view = new Constructor(container, model, state, playercontroller);
            views.push(view);
        }
        
        /* CONSTRUCTOR */
        function initialize() {
            that.elm  = $(template);
            
            // DEFINE AND INJECT VIEWS
            views = [];
            
            attach(ScoreboardView, 'scoreboard');            
            attach(OpponentsView, 'opponents');
            attach(ClientPlayerView, 'board_player');       
//            playerView = new ClientPlayerView(model, playercontroller, state, playercontroller);
//            that.elm.find('#player').html(playerView.elm);
//            views.push(playerView);
//            
//            boardView = new BoardView(state, model, playercontroller);
//            that.elm.find('#board').html(boardView.elm);
//            views.push(boardView);
//                
//            optionsView = new OptionsView(model, playercontroller);
//            that.elm.find('#optionsView').append(optionsView.elm); // or replacewith
//            views.push(optionsView);
//        
//            playerInTurnView = new PlayerInTurnView(model);
//            that.elm.find('#playerinturnView').append(playerInTurnView.elm);
//            views.push(playerInTurnView);
//        
//            simulateView = new SimulateView(simulatecontroller);
//            that.elm.find('#simulateView').append(simulateView.elm);
//            views.push(simulateView);
//            
//            characterView = new CharacterView(model, playercontroller);
//            that.elm.find('#characterView').append(characterView.elm);
//            views.push(characterView);

            that.render(model);
        }
        
        /* METHOD */
        that.notify = function (update) {
            views.forEach(function (view) {
                if (view.notify) { view.notify(update); }
            });
        };
        
        /* METHOD */
        that.render = function () {
            views.forEach(function (view) {
                view.render();
            });
        };
        
        initialize();
    };
});