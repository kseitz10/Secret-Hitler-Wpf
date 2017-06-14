using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using SecretHitler.Game.Entities;
using SecretHitler.Game.Interfaces;

namespace SecretHitler.Server
{
    public class ServerHub : Hub
    {
        #region Persistent Data

        public static Dictionary<string, IPlayerInfo> Players = new Dictionary<string, IPlayerInfo>();

        #endregion

        #region API

        public void BroadcastMessage(string message)
        {
            var player = Players[Context.ConnectionId];
            message = $"{player.Name} says: {message}";
            BroadcastMessageImpl(message);
        }

        private void BroadcastMessageImpl(string message)
        {
            // TODO Broadcast predefined server messages with enum instead of as string to support localization, if we care.
            Console.WriteLine(message);
            Clients.All.broadcastMessage(message);
        }

        #endregion

        #region Overrides

        public override Task OnConnected()
        {
            var nickname = Context.QueryString["nickname"];
            if (!Guid.TryParse(Context.QueryString["guid"], out Guid guid))
                throw new ArgumentException("Guid required");

            var identifier = Context.ConnectionId;
            var existingPlayer = Players.SingleOrDefault(_ => _.Value.Identifier == guid);
            if (existingPlayer.Value != null)
            {
                Players.Remove(existingPlayer.Key);
                Players[identifier] = existingPlayer.Value;

                BroadcastMessageImpl($"Client {nickname} has rejoined.");
            }
            else
            {
                Players[identifier] = new PlayerData() { Name = nickname };
                BroadcastMessageImpl($"Client {nickname} connected.");
            }

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var player = Players[Context.ConnectionId];
            BroadcastMessageImpl($"Client {player.Name} disconnected.");
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        #endregion
    }

}
