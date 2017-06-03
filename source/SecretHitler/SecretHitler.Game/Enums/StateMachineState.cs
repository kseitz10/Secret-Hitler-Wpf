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
        AwaitingPolicyPeekConfirmation,
        AwaitingVetoResponse,
        AwaitingExecution,
        AwaitingPolicyPeek,
        AwaitingSpecialElectionPick,
    }
}
