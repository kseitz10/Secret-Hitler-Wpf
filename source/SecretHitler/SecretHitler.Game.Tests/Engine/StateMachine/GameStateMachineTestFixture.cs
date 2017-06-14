﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SecretHitler.Game.Engine;
using SecretHitler.Game.Entities;
using SecretHitler.Game.Interfaces;

namespace SecretHitler.Game.Tests.Engine.StateMachine
{
    public abstract class GameStateMachineTestFixture
    {
        public GameStateMachine StateMachine { get; private set; }

        public Mock<IClientProxy> ClientProxy { get; private set; }

        public Mock<GameDataManipulator> Manipulator { get; private set; }

        public Random Random { get; } = new Random();

        public GameData Game => StateMachine.GameData;

        public IList<PlayerData> Players => Game.Players;

        [TestInitialize]
        public void TestInitialize()
        {
            ClientProxy = new Mock<IClientProxy>();
            StateMachine = new GameStateMachine(ClientProxy.Object);
            Manipulator = new Mock<GameDataManipulator>(Game) { CallBase = true };
            StateMachine.GameDataManipulator = Manipulator.Object;
            ResetPlayers();
        }

        protected void ResetPlayers(int playerCt = Constants.MinPlayerCount)
        {
            Players.Clear();
            AddPlayers(playerCt);
        }

        protected void AddPlayers(int playerCt = Constants.MinPlayerCount)
        {
            for (var i = 0; i < playerCt; i++)
            {
                var player = new PlayerData()
                {
                    Identifier = Guid.NewGuid(),
                };
                player.Name = player.Identifier.ToString();
                Players.Add(player); // TODO Expose methods for player management.
            }
        }
    }
}
