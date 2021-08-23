using SecretHitler.Domain.Common;
using System.Threading.Tasks;

namespace SecretHitler.Application.Common.Interfaces
{
    public interface IDomainEventService
    {
        Task Publish(DomainEvent domainEvent);
    }
}
