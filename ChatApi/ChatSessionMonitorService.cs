using ChatApi.Services.Abstraction;

public class ChatSessionMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ChatSessionMonitorService> _logger;

    public ChatSessionMonitorService(IServiceScopeFactory serviceScopeFactory, ILogger<ChatSessionMonitorService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var chatSessionService = scope.ServiceProvider.GetRequiredService<IChatSessionService>();
                var agentService = scope.ServiceProvider.GetRequiredService<IAgentService>();

                try
                {
                    await MarkInactiveSessions(chatSessionService);
                    await AssignChatsToAgents(chatSessionService, agentService);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating chat sessions and assigning chats to agents.");
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
    private static async Task MarkInactiveSessions(IChatSessionService chatSessionService)
    {
        var activeSessions = await chatSessionService.GetActiveSessionsAsync();
        foreach (var session in activeSessions)
        {
            if ((DateTime.UtcNow - session.LastPollTime).TotalMinutes >= 1)
            {
                session.IsActive = false;
                await chatSessionService.UpdateSessionAsync(session);
            }
        }
    }
    private static async Task AssignChatsToAgents(IChatSessionService chatSessionService, IAgentService agentService)
    {
        var chatSessions = await chatSessionService.GetQueuedSessionsAsync();
        var availableAgents = await agentService.GetAvailableAgentAsync();

        foreach (var session in chatSessions)
        {
            if (!availableAgents.Any(a => a.CanTakeChat))
                break;

            var nextAvailableAgent = availableAgents.First(a => a.CanTakeChat);
            session.AssignedAgent = nextAvailableAgent;
            session.StartTime = DateTime.UtcNow;
            session.IsActive = true;

            await chatSessionService.UpdateSessionAsync(session);
        }
    }
}


