using Microsoft.AspNet.SignalR.Client;
using SecretHitler.App.Interfaces;
using SecretHitler.Game.Entities;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SecretHitler.App.Utility
{
    /// <summary>
    /// The SignalR client.
    /// </summary>
    public class Client : IClient
    {
        private string _serverUrl;
        private string _nickname;
        private Guid _clientGuid;
        private HubConnection _connection;
        private IHubProxy _hubProxy;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serverUrl">The URL of the server to connect to.</param>
        /// <param name="nickname">Nickname of the client to display to other clients.</param>
        /// <param name="clientGuid">Guid for the client. Used for reconnections under the same identity, regardless of name.</param>
        public Client(string serverUrl, string nickname, Guid? clientGuid = null)
        {
            _serverUrl = serverUrl;
            _nickname = nickname;
            _clientGuid = clientGuid ?? Guid.NewGuid();
        }

        /// <summary>
        /// The object that is delivered information and requests by the server.
        /// </summary>
        public IPlayerLogic ClientUI { get; set; }

        /// <summary>
        /// Creates and connects the hub connection and hub proxy.
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            _connection = new HubConnection(_serverUrl, new Dictionary<string, string>
            {
                { "nickname", _nickname },
                { "guid", _clientGuid.ToString() }
            });

            _connection.TraceLevel = TraceLevels.All;
            _connection.TraceWriter = Console.Out;

            // TODO
            //// _connection.Closed += Connection_Closed;

            _hubProxy = _connection.CreateHubProxy("ServerHub");
            _hubProxy.On<string>("MessageReceived", _ => ClientUI?.MessageReceived(_));
            _hubProxy.On<GameData>("UpdateGameData", _ => ClientUI?.UpdateGameData(_));
            _hubProxy.On<GameState, IEnumerable<Guid>>("PlayerSelectionRequested", async (state, players) =>
                PlayerSelected(await ClientUI?.SelectPlayer(state, players)));
            _hubProxy.On("PlayerVoteRequested", async () => VoteSelected(await ClientUI?.GetVote()));
            _hubProxy.On<IEnumerable<PolicyType>, int, bool>("PolicySelectionRequested", async (policies, ct, veto) =>
                PoliciesSelected(await ClientUI?.SelectPolicies(policies, ct, veto)));
            _hubProxy.On<IEnumerable<PolicyType>>("PolicyPeek", async (policies) =>
            {
                await ClientUI?.ShowPolicies(policies);
                Acknowledge(null);
            });
            _hubProxy.On<Guid, PlayerRole>("LoyaltyPeek", async (guid, loyalty) =>
            {
                await ClientUI?.RevealLoyalty(guid, loyalty);
                Acknowledge(null);
            });
            _hubProxy.On("ApproveVetoRequested", async () =>
                Acknowledge(await ClientUI?.PromptForVetoApproval()));

            try
            {
                await _connection.Start();
                return true;
            }
            catch (HttpRequestException ex)
            {
                // TODO
                throw;
                return false;
            }
        }

        /// <summary>
        /// Disconnect
        /// </summary>
        public void Disconnect()
        {
            _hubProxy = null;
            _connection?.Dispose();
            _connection = null;
        }

        /// <summary>
        /// Notify the server that a player has been selected.
        /// </summary>
        /// <param name="playerGuid">Selected player</param>
        public void PlayerSelected(Guid playerGuid)
        {
            _hubProxy.Invoke("PlayerSelected", playerGuid);
        }

        /// <summary>
        /// Notify the server that a vote has been selected.
        /// </summary>
        /// <param name="vote">True or false vote for ja or nein respectively.</param>
        public void VoteSelected(bool vote)
        {
            _hubProxy.Invoke("VoteSelected", vote);
        }

        /// <summary>
        /// Notify the server that one or more policies were selected.
        /// </summary>
        /// <param name="policies">Selected policies. Null/empty indicates a veto attempt.</param>
        public void PoliciesSelected(IEnumerable<PolicyType> policies)
        {
            _hubProxy.Invoke("PoliciesSelected", policies);
        }

        /// <summary>
        /// Indicates a simple acknowledgement from a client.
        /// </summary>
        /// <param name="acknowledge">Favorable or unfavorable response, or null if not applicable.</param>
        public void Acknowledge(bool? acknowledge)
        {
            _hubProxy.Invoke("Acknowledge", acknowledge);
        }

        /// <summary>
        /// Send a message to other players.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendMessage(string message)
        {
            _hubProxy.Invoke("broadcastMessage", message);
        }
    }
}
