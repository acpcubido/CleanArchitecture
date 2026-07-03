using Cubido.Template.Application.Common.Models;
using Cubido.Template.Application.TodoItems.Queries.GetTodoItemsWithPagination;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Cubido.Template.Web.Tools;

[McpServerToolType]
public class TodoItemTools(ISender sender)
{
    [McpServerTool(UseStructuredContent = true)]
    [Description("Reads rows from TodoItem database table filtered for a given list. Sorted by title.")]
    public async Task<PaginatedList<TodoItemBriefDto>> GetTodoItemsWithPagination(
        [Description("The ID of the list to filter TodoItems.")] int listId,
        [Description("Pagination: The page number to retrieve.")] int pageNumber,
        [Description("Pagination: The number of items per page.")] int pageSize)
    {
        return await sender.Send(new GetTodoItemsWithPaginationQuery()
        {
            ListId = listId,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }
}
