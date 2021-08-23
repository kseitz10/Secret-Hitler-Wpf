using System;
using System.Collections.Generic;
using System.Linq;

namespace SecretHitler.Domain.Enums
{
    /// <summary>
    /// Triggers for the state machine.
    /// </summary>
    public enum Trigger
    {
        StartGame,
        VetoApproved,
        VetoDenied,
        PoliciesSelected,
        VotesCollected,
        PlayerSelected
    }
}
