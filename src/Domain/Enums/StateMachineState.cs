using System;
using System.Collections.Generic;
using System.Linq;

namespace SecretHitler.Domain.Enums
{
    public enum StateMachineState
    {
        None,
        AwaitingNomination,
        AwaitingVotes,
        AwaitingPresidentialPolicies,
        AwaitingEnactedPolicy,
        AwaitingSpecialPowerAcknowledgment,
        AwaitingVetoResponse,
        AwaitingExecutionPick,
        AwaitingSpecialElectionPick,
        AwaitingInvestigateLoyaltyPick,
    }
}
