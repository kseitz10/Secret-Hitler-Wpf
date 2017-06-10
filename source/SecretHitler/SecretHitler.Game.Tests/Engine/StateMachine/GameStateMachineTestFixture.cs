using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SecretHitler.Game.Engine;
using SecretHitler.Game.Entities;
using SecretHitler.Game.Interfaces;

namespace SecretHitler.Game.Tests.Engine.StateMachine
{
    public abstract class GameStateMachineTestFixture
    {
        public const int MinPlayerCount = 5;
        public const int MaxPlayerCount = 10;

        public GameStateMachine StateMachine { get; private set; }

        public Mock<IClientProxy> ClientProxy { get; private set; }

        public Random Random { get; } = new Random();

        public Game.Entities.Game GameData => StateMachine.GameData; 

        public IList<Player> Players => GameData.Players;

        public StateMachineState State => StateMachine.MachineState;

        [TestInitialize]
        public void TestInitialize()
        {
            ClientProxy = new Mock<IClientProxy>();
            StateMachine = new GameStateMachine(ClientProxy.Object);
            ResetPlayers();
        }

        protected void ResetPlayers(int playerCt = MinPlayerCount)
        {
            Players.Clear();
            AddPlayers(playerCt);
        }

        protected void AddPlayers(int playerCt = MinPlayerCount)
        {
            for(var i = 0; i < playerCt; i++)
            {
                var player = new Player()
                {
                    Identifier = Guid.NewGuid(),
                };
                player.Name = player.Identifier.ToString();
                Players.Add(player); // TODO Expose methods for player management.
            }
        }
    }
}
