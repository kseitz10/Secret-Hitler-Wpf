using System.Threading.Tasks;
using static SecretHitler.App.Utility.Client;

namespace SecretHitler.App.Interfaces
{
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
        /// Send a message to other clients.
        /// </summary>
        /// <param name="message">Message text.</param>
        void SendMessage(string message);

        /// <summary>
        /// Event raised when a broadcast message was received.
        /// </summary>
        event MessageReceivedDelegate MessageReceived;
    }
}
