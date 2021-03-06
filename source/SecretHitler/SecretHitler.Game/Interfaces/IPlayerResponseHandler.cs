using System.Collections.Generic;
using SecretHitler.Game.Enums;
using System;
using System.Threading.Tasks;

namespace SecretHitler.Game.Interfaces
{
    /// <summary>
    /// Interface for an object the handles responses coming from clients.
    /// </summary>
    public interface IPlayerResponseHandler
    {
        /// <summary>
        /// Indicates that a player has been selected by the client that was last issued a request.
        /// </summary>
        /// <param name="player">The selected player.</param>
        Task PlayerSelected(Guid player);

        /// <summary>
        /// Indicates that a vote has been collected from a player.
        /// </summary>
        /// <param name="vote">The collected vote.</param>
        Task VoteCollected(bool vote);

        /// <summary>
        /// Indicates that one or more policies were selected by the client asked to select policies.
        /// </summary>
        /// <param name="policies">The selected policies.</param>
        Task PoliciesSelected(IEnumerable<PolicyType> policies);

        /// <summary>
        /// Indicates a simple acknowledgement from a client.
        /// </summary>
        /// <param name="acknowledge">Favorable or unfavorable response, or null if not applicable.</param>
        Task Acknowledge(bool? acknowledge);
    }
}
