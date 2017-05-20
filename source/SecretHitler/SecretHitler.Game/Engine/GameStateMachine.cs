using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Utility;
using Stateless;

namespace SecretHitler.Game.Engine
{
    /// <summary>
    /// State machine for the Secret Hitler game.
    /// </summary>
    public class GameStateMachine
    {
        private StateMachine<GameState, Trigger> _stateMachine;

        /// <summary>
        /// Create a new instance of the state machine.
        /// </summary>
        public GameStateMachine()
        {
            _stateMachine = new StateMachine<GameState, Trigger>(() => State, val => State = val);

            _stateMachine.Configure(GameState.None)
                .Permit(Trigger.StartGame, GameState.ChancellorNomination)
                .OnExit(PrepareGame);
            ////_stateMachine.Configure(GameState.ChancellorNomination)
            ////    .Permit(Trigger.PlayerSelected, GameState.Voting);
            ////_stateMachine.Configure(GameState.Voting)
            ////    .Permit(Trigger.VotesCollected, GameState.
        }

        /// <summary>
        /// The current state of the machine.
        /// </summary>
        public GameState State { get; private set; } = GameState.None;

        /// <summary>
        /// The data backing the state machine.
        /// </summary>
        public Entities.Game GameData { get; private set; } = new Entities.Game();

        /// <summary>
        /// The number of fascist roles (not including Hitler) to be assigned given the
        /// current number of players.
        /// </summary>
        public int FascistCount => GameData.Players.Count - 4 + ((GameData.Players.Count - 5) / 2);

        #region Methods

        private void PrepareGame()
        {
            // Mark all players as alive and assign them some roles
            foreach (var player in GameData.Players)
            {
                player.Role = PlayerRole.Liberal;
                player.IsAlive = true;
            }

            var liberals = GameData.Players.ToArray();
            liberals.Shuffle();

            foreach (var fascist in liberals.Take(FascistCount))
                fascist.Role = PlayerRole.Fascist;

            liberals.Skip(FascistCount).First().Role = PlayerRole.Hitler;

            // Set up the presidential queue
            var shuffled = GameData.Players.Select(_ => _.Identifier).ToList();
            shuffled.Shuffle();
            GameData.PresidentialQueue = new Queue<Guid>(shuffled);

            GameData.IneligibleChancellors = new List<Guid>();
            GameData.EnactedFascistPolicyCount = 0;
            GameData.EnactedLiberalPolicyCount = 0;
            GameData.ElectionTracker = 0;
            GameData.PolicyDeck.Reset();
        }

        private void PresidentialSuccession(bool successfulElection = true)
        {
            ////var nextPresident

            ////if (_nextPresident == null || !GameData.Players.Contains(_nextPresident))
            ////    _nextPresident = GameData.Players[_rng.Next(0, GameData.Players.Count)];

            ////if (!_wasSpecialElection && successfulElection)
            ////{
            ////    if (_president != null)
            ////        _previousPresident = _president;

            ////    if (_chancellor != null)
            ////        _previousChancellor = _chancellor;
            ////}

            ////_wasSpecialElection = false;
            ////_chancellor = null;
            ////_president = null;

            ////while (_president == null || !_president.IsAlive)
            ////{
            ////    _president = _nextPresident;
            ////    _nextPresident = GameData.Players.Skip(GameData.Players.IndexOf(Coerce(_nextPresident)) + 1)
            ////        .FirstOrDefault(_ => _.IsAlive) ?? GameData.Players.First(_ => _.IsAlive);
            ////}
        }

        #endregion
    }
}
