using System.Collections.Generic;
using System.Threading.Tasks;
using SecretHitler.Game.Enums;

namespace SecretHitler.Game.Interfaces
{
    public interface IClientProxy
    {
        void Broadcast(string message);

        Task<IPlayerInfo> SelectPlayer(IPlayerInfo chooser, GameState gameState, IEnumerable<IPlayerInfo> candidates);

        Task<IEnumerable<bool>> GetVotes(IEnumerable<IPlayerInfo> voters);

        Task<IEnumerable<PolicyType>> GetPresidentialPolicies(IPlayerInfo president, IEnumerable<PolicyType> drawnPolicies);

        Task<PolicyType> GetEnactedPolicy(IPlayerInfo chancellor, IEnumerable<PolicyType> drawnPolicies);

        Task PolicyPeek(IPlayerInfo president, IList<PolicyType> deckTopThree);

        Task<bool> ApproveVeto(IPlayerInfo president);
    }
}
