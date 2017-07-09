using SecretHitler.Game.Interfaces;

namespace SecretHitler.Game.Engine
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
