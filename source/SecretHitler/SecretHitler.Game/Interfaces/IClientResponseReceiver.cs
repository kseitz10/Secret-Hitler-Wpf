using System.Collections.Generic;
using SecretHitler.Game.Enums;

namespace SecretHitler.Game.Interfaces
{
    /// <summary>
    /// Interface for an object the handles responses coming from clients.
    /// </summary>
    public interface IClientResponseReceiver
    {
        /// <summary>
        /// Indicates that a player has been selected by the client that was last issued a request.
        /// </summary>
        /// <param name="player">The selected player.</param>
        void PlayerSelected(IPlayerInfo player);

        /// <summary>
        /// Indicates that votes have been collected from all active players.
        /// </summary>
        /// <param name="votes">The collected votes.</param>
        void VotesCollected(IEnumerable<bool> votes);

        /// <summary>
        /// Indicates that one or more policies were selected by the client asked to select policies.
        /// </summary>
        /// <param name="policies">The selected policies.</param>
        void PoliciesSelected(IEnumerable<PolicyType> policies);

        /// <summary>
        /// Indicates a simple acknowledgement from a client.
        /// </summary>
        /// <param name="acknowledge">Favorable or unfavorable response, or null if not applicable.</param>
        void Acknowledge(bool? acknowledge);
    }
}
