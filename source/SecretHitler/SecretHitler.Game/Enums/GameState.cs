using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Game.Enums
{
    public enum GameState
    {
        None,
        ChancellorNomination,
        Voting,
        PresidentialPolicy,
        PolicyEnactment,
        InvestigateLoyalty,
        SpecialElection,
        Execution,
        FascistVictory,
        LiberalVictory
    }
}
