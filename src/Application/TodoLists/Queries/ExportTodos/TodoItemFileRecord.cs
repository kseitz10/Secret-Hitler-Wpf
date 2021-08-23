using SecretHitler.Application.Common.Mappings;
using SecretHitler.Domain.Entities;

namespace SecretHitler.Application.TodoLists.Queries.ExportTodos
{
    public class TodoItemRecord : IMapFrom<TodoItem>
    {
        public string Title { get; set; }

        public bool Done { get; set; }
    }
}
