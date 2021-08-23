using SecretHitler.Application.TodoLists.Queries.ExportTodos;
using System.Collections.Generic;

namespace SecretHitler.Application.Common.Interfaces
{
    public interface ICsvFileBuilder
    {
        byte[] BuildTodoItemsFile(IEnumerable<TodoItemRecord> records);
    }
}
