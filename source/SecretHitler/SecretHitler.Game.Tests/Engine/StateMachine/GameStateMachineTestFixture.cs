using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SecretHitler.Game.Engine;
using SecretHitler.Game.Entities;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;

namespace SecretHitler.Game.Tests.Engine.StateMachine
{
    public abstract class GameStateMachineTestFixture
    {
        public GameStateMachine StateMachine { get; private set; }

        public Mock<IPlayerDirector> Director { get; private set; }

        public Mock<GameDataManipulator> Manipulator { get; private set; }

        public Random Random { get; } = new Random();

        public GameData GameData => StateMachine.GameData;

        public IList<PlayerData> Players => GameData.Players;

        [TestInitialize]
        public void TestInitialize()
        {
            Director = new Mock<IPlayerDirector>();
            StateMachine = new GameStateMachine(Director.Object, Mock.Of<ILogger<GameStateMachine>>());
            Manipulator = new Mock<GameDataManipulator>(GameData) { CallBase = true };
            StateMachine.GameDataManipulator = Manipulator.Object;
            ResetPlayers();
        }

        protected void ResetPlayers(int playerCt = Constants.MinPlayerCount)
        {
            GameData.Players = new List<PlayerData>();
            AddPlayers(playerCt);
        }

        protected void AddPlayers(int playerCt = Constants.MinPlayerCount)
        {
            for (var i = 0; i < playerCt; i++)
            {
                var player = new PlayerData()
                {
                    Identifier = Guid.NewGuid(),
                    IsAlive = true
                };
                player.Name = player.Identifier.ToString();
                Players.Add(player); // TODO Expose methods for player management.
            }
        }

        protected async Task VotesCollected(IEnumerable<bool> votes)
        {
            foreach (var v in votes)
                await StateMachine.VoteCollected(v);
        }
    }
}
