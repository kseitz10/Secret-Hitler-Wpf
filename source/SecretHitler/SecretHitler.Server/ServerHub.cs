using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using SecretHitler.Game.Entities;
using SecretHitler.Game.Engine;
using SecretHitler.Game.Enums;

namespace SecretHitler.Server
{
    public class ServerHub : Hub
    {
        #region Persistent Data

        public static GameStateMachine StateMachine { get; private set; } = new GameStateMachine(Director.Instance);

        public static List<PlayerData> Players => StateMachine.GameData.Players;

        private static object _lock = new object();

        #endregion

        public void BroadcastMessage(string message)
        {
            message = $"{Context.QueryString["nickname"]} says: {message}";
            BroadcastMessageImpl(message);
        }

        public void PlayerSelected(Guid playerGuid)
        {
            StateMachine.PlayerSelected(playerGuid);
        }

        public void VoteSelected(bool vote)
        {
            StateMachine.VoteCollected(vote);
        }

        public void PoliciesSelected(IEnumerable<PolicyType> policies)
        {
            StateMachine.PoliciesSelected(policies);
        }

        public void Acknowledge(bool? result)
        {
            StateMachine.Acknowledge(result);
        }

        public override Task OnConnected()
        {
            lock (_lock)
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

                    // Add a short delay. Uggh.
                    Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        BroadcastMessageImpl($"Client {nickname} connected.");

                        if (StateMachine.MachineState != StateMachineState.None)
                            StateMachine.DisseminateGameData();
                    });
                }
            }

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
