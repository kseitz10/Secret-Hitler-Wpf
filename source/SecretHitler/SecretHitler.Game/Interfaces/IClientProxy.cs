using System.Collections.Generic;
using SecretHitler.Game.Enums;

namespace SecretHitler.Game.Interfaces
{
    public interface IClientProxy
    {
        void Broadcast(string message);

        void SelectPlayer(IPlayerInfo chooser, GameState gameState, IEnumerable<IPlayerInfo> candidates);

        void GetVotes(IEnumerable<IPlayerInfo> voters);

        void GetPresidentialPolicies(IPlayerInfo president, IEnumerable<PolicyType> drawnPolicies);

        void GetEnactedPolicy(IPlayerInfo chancellor, IEnumerable<PolicyType> drawnPolicies);

        void PolicyPeek(IPlayerInfo president, IList<PolicyType> deckTopThree);

        void Reveal(IPlayerInfo president, PlayerRole role);

        void ApproveVeto(IPlayerInfo president);
    }
}
