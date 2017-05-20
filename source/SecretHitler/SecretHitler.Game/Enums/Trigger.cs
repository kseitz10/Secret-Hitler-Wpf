using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Game.Enums
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
