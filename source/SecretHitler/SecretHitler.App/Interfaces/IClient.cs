using SecretHitler.Game.Interfaces;
using System;
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
    }
}
