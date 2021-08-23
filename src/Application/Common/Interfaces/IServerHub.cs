using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SecretHitler.Domain.Enums;

namespace SecretHitler.Application.Common.Interfaces
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
