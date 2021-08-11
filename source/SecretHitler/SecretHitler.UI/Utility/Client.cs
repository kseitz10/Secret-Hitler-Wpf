using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;

using SecretHitler.Game.Entities;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;

namespace SecretHitler.UI.Utility
{
    /// <summary>
    /// The SignalR client.
    /// </summary>
    public class Client
    {
        private readonly string _serverUrl;
        private readonly string _nickname;
        private Guid _clientGuid;
        private HubConnection _connection;

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
        public IPlayerInterface ClientUI { get; set; }

        /// <summary>
        /// Creates and connects the hub connection and hub proxy.
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            var uriBuilder = new UriBuilder(_serverUrl);
            uriBuilder.Query = $"nickname={WebUtility.UrlEncode(_nickname)}&guid={WebUtility.UrlEncode(_clientGuid.ToString())}";
            _connection = new HubConnectionBuilder()
                          .WithUrl(uriBuilder.Uri)
                          .WithAutomaticReconnect()
                          .Build();

            // TODO
            //// _connection.Closed += Connection_Closed;

            _connection.On<string>(nameof(IHubClient.MessageReceived), _ => ClientUI?.MessageReceived(_));
            _connection.On<GameData>(nameof(IHubClient.UpdateGameData), _ => ClientUI?.UpdateGameData(_));
            _connection.On<GameState, IEnumerable<Guid>>(nameof(IHubClient.PlayerSelectionRequested), async (state, players) => PlayerSelected(await ClientUI?.SelectPlayer(state, players)));
            _connection.On(nameof(IHubClient.PlayerVoteRequested), async () => VoteSelected(await ClientUI?.GetVote()));
            _connection.On<IEnumerable<PolicyType>, int, bool>(nameof(IHubClient.PolicySelectionRequested), async (policies, ct, veto) => PoliciesSelected(await ClientUI?.SelectPolicies(policies, ct, veto)));
            _connection.On<IEnumerable<PolicyType>>(nameof(IHubClient.PolicyPeek), async (policies) =>
            {
                await ClientUI?.ShowPolicies(policies);
                Acknowledge(null);
            });
            _connection.On<Guid, PlayerRole>(nameof(IHubClient.LoyaltyPeek), async (guid, loyalty) =>
            {
                await ClientUI?.RevealLoyalty(guid, loyalty);
                Acknowledge(null);
            });
            _connection.On(nameof(IHubClient.ApproveVetoRequested), async () => Acknowledge(await ClientUI?.PromptForVetoApproval()));

            try
            {
                await _connection.StartAsync();
                return true;
            }
            catch (HttpRequestException)
            {
                // TODO
                throw;
            }
        }

        /// <summary>
        /// Disconnect
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (_connection != null)
            {
                await _connection?.DisposeAsync().AsTask();
            }

            _connection = null;
        }

        /// <summary>
        /// Notify the server that a player has been selected.
        /// </summary>
        /// <param name="playerGuid">Selected player</param>
        internal void PlayerSelected(Guid playerGuid)
        {
            _connection.InvokeAsync(nameof(IServerHub.PlayerSelected), playerGuid);
        }

        /// <summary>
        /// Notify the server that a vote has been selected.
        /// </summary>
        /// <param name="vote">True or false vote for ja or nein respectively.</param>
        internal void VoteSelected(bool vote)
        {
            _connection.InvokeAsync(nameof(IServerHub.VoteSelected), vote);
        }

        /// <summary>
        /// Notify the server that one or more policies were selected.
        /// </summary>
        /// <param name="policies">Selected policies. Null/empty indicates a veto attempt.</param>
        internal void PoliciesSelected(IEnumerable<PolicyType> policies)
        {
            _connection.InvokeAsync(nameof(IServerHub.PoliciesSelected), policies);
        }

        /// <summary>
        /// Indicates a simple acknowledgement from a client.
        /// </summary>
        /// <param name="acknowledge">Favorable or unfavorable response, or null if not applicable.</param>
        internal void Acknowledge(bool? acknowledge)
        {
            _connection.InvokeAsync(nameof(IServerHub.Acknowledge), acknowledge);
        }

        /// <summary>
        /// Send a message to other players.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendMessage(string message)
        {
            _connection.InvokeAsync(nameof(IServerHub.BroadcastMessage), message);
        }
    }
}
