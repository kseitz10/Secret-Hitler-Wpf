using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecretHitler.App.Interfaces
{
    /// <summary>
    /// Represents a SignalR class used to initiate coordination with the server.
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Creates and connects the hub connection and hub proxy.
        /// </summary>
        Task<bool> ConnectAsync();

        /// <summary>
        /// Disconnects from the hub.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// The object that is delivered information and requests by the server.
        /// </summary>
        IPlayerLogic ClientUI { get; set; }

        /// <summary>
        /// Send a message to other clients.
        /// </summary>
        /// <param name="message">Message text.</param>
        void SendMessage(string message);

        /// <summary>
        /// Notify the server a player has been selected.
        /// </summary>
        /// <param name="playerGuid">Selected player</param>
        void PlayerSelected(Guid playerGuid);

        /// <summary>
        /// Notify the server that a vote has been selected.
        /// </summary>
        /// <param name="vote">True or false vote for ja or nein respectively.</param>
        void VoteSelected(bool vote);

        /// <summary>
        /// Notify the server that one or more policies were selected.
        /// </summary>
        /// <param name="policies">Selected policies. Null/empty indicates a veto attempt.</param>
        void PoliciesSelected(IEnumerable<PolicyType> policies);

        /// <summary>
        /// Indicates a simple acknowledgement from a client.
        /// </summary>
        /// <param name="acknowledge">Favorable or unfavorable response, or null if not applicable.</param>
        void Acknowledge(bool? acknowledge);
    }
}
