using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using SecretHitler.Game.Entities;
using SecretHitler.Game.Engine;

namespace SecretHitler.Server
{
    public class ServerHub : Hub
    {
        #region Persistent Data

        public static GameStateMachine StateMachine { get; private set; } = new GameStateMachine(Director.Instance);

        public static List<PlayerData> Players => StateMachine.GameData.Players;

        #endregion

        public void BroadcastMessage(string message)
        {
            message = $"{Context.QueryString["nickname"]} says: {message}";
            BroadcastMessageImpl(message);
        }

        public override Task OnConnected()
        {
            var nickname = Context.QueryString["nickname"];
            if (!Guid.TryParse(Context.QueryString["guid"], out Guid guid))
                throw new ArgumentException("Guid required");

            var signalrConnectionId = Context.ConnectionId;
            var existingPlayer = Players.SingleOrDefault(_ => _.Identifier == guid);
            if (existingPlayer != null)
            {
                Groups.Add(signalrConnectionId, guid.ToString());
                BroadcastMessageImpl($"Client {nickname} has rejoined.");
            }
            else
            {
                // TODO Creating the player is not the responsibility of this object.
                var player = new PlayerData() { Name = nickname, Identifier = guid };
                StateMachine.GameData.Players.Add(player);
                Groups.Add(signalrConnectionId, guid.ToString());
                BroadcastMessageImpl($"Client {nickname} connected.");
            }

            Clients.All.UpdatePlayerStates(Players);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // TODO Handle all the problems caused by disconnecting players.
            ////var player = Players.Single(_ => _.HasSameIdentifierAs(;
            ////player.IsAlive = false;
            ////BroadcastMessageImpl($"Client {player.Name} disconnected.");

            ////Clients.All.UpdatePlayerStates(Players);
            return base.OnDisconnected(stopCalled);
        }

        private void BroadcastMessageImpl(string message)
        {
            // TODO Broadcast predefined server messages with enum instead of as string to support localization, if we care.
            Console.WriteLine(message);
            Clients.All.MessageReceived(message);
        }
    }
}
