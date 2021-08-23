using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using SecretHitler.Game.Entities;
using SecretHitler.Game.Engine;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;

namespace SecretHitler.Server
{
    public class ServerHub : Hub<IHubClient>, IServerHub
    {
        private readonly GameStateMachine _stateMachine;
        private readonly ILogger<ServerHub> _logger;

        public ServerHub(GameStateMachine stateMachine, GameDataAccessor gameRepo, ILogger<ServerHub> logger)
        {
            _stateMachine = stateMachine;
            _logger = logger;
            _stateMachine.LoadGameState(gameRepo.GameData);
        }

        public IList<PlayerData> Players => _stateMachine.GameData.Players;

        public async Task BroadcastMessage(string message)
        {
            message = $"{Context.GetHttpContext().Request.Query["nickname"]} says: {message}";
            await Clients.All.MessageReceived(message);
        }

        public async Task PlayerSelected(Guid playerGuid)
        {
            _logger.LogDebug($"Received {nameof(PlayerSelected)} with {playerGuid}.");
            await _stateMachine.PlayerSelected(playerGuid);
        }

        public async Task VoteSelected(bool vote)
        {
            _logger.LogDebug($"Received {nameof(VoteSelected)} with {(vote ? "Ja" : "Nein")}.");
            await _stateMachine.VoteCollected(vote);
        }

        public async Task PoliciesSelected(IEnumerable<PolicyType> policies)
        {
            var policyTypes = policies as PolicyType[] ?? policies.ToArray();
            _logger.LogDebug($"Received {nameof(PoliciesSelected)} with {string.Join(",", policyTypes.Select(_ => _.ToString()))}.");
            await _stateMachine.PoliciesSelected(policyTypes);
        }

        public async Task Acknowledge(bool? result)
        {
            _logger.LogDebug($"Received {nameof(Acknowledge)} with {result}.");
            await _stateMachine.Acknowledge(result);
        }

        public override async Task OnConnectedAsync()
        {
            var request = Context.GetHttpContext().Request;
            var nickname = request.Query["nickname"];
            if (!Guid.TryParse(request.Query["guid"], out Guid guid))
                throw new ArgumentException("Guid required");

            var signalrConnectionId = Context.ConnectionId;
            var existingPlayer = Players.SingleOrDefault(_ => _.Identifier == guid);
            if (existingPlayer != null)
            {
                await Groups.AddToGroupAsync(signalrConnectionId, guid.ToString());
                await Clients.All.MessageReceived($"Client {nickname} has rejoined.");
            }
            else
            {
                // TODO Creating the player is not the responsibility of this object.
                var player = new PlayerData { Name = nickname, Identifier = guid };
                _stateMachine.GameData.Players.Add(player);
                await Groups.AddToGroupAsync(signalrConnectionId, guid.ToString());

                await Task.Delay(TimeSpan.FromSeconds(1));
                await Clients.All.MessageReceived($"Client {nickname} has joined.");

                if (_stateMachine.GameData.MachineState != StateMachineState.None)
                {
                    await _stateMachine.DisseminateGameData();
                }
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            // TODO Handle all the problems caused by disconnecting players.
            ////var player = Players.Single(_ => _.HasSameIdentifierAs(;
            ////player.IsAlive = false;
            ////BroadcastMessageImpl($"Client {player.Name} disconnected.");

            ////Clients.All.UpdatePlayerStates(Players);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
