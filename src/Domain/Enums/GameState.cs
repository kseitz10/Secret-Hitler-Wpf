using System;
using System.Collections.Generic;
using System.Linq;

namespace SecretHitler.Domain.Enums
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
