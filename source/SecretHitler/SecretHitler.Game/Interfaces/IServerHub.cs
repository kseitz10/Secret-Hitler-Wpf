using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SecretHitler.Game.Enums;

namespace SecretHitler.Game.Interfaces
{
    public interface IServerHub
    {
        Task BroadcastMessage(string message);
        Task PlayerSelected(Guid playerGuid);
        Task VoteSelected(bool vote);
        Task PoliciesSelected(IEnumerable<PolicyType> policies);
        Task Acknowledge(bool? result);
    }
}
