using Microsoft.Extensions.Logging;

namespace Cubido.Template.Application.Common.Behaviours;

public partial class LoggingBehaviour<TRequest, TResponse>(ILogger<TRequest> logger, IUser user, IIdentityService identityService) : MessagePreProcessor<TRequest, TResponse> where TRequest : IMessage
{
    private readonly ILogger logger = logger;

    protected override async ValueTask Handle(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = user.Id ?? string.Empty;
        string? userName = string.Empty;

        if (!string.IsNullOrEmpty(userId))
        {
            userName = await identityService.GetUserNameAsync(userId);
        }

        LogRequest(requestName, userId, userName, request);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Cubido.Template Request: {Name} {@UserId} {@UserName} {@Request}")]
    private partial void LogRequest(string? name, string? userId, string? userName, object? request);
}
