using SecretHitler.Domain.Common;
using SecretHitler.Domain.Entities;

namespace SecretHitler.Domain.Events
{
    public class TodoItemCreatedEvent : DomainEvent
    {
        public TodoItemCreatedEvent(TodoItem item)
        {
            Item = item;
        }

        public TodoItem Item { get; }
    }
}
