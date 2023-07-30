using ChatApi.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

public class ChatSessionService : IChatSessionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IAgentService _agentService;
    private readonly ILogger<ChatSessionService> _logger;

    public ChatSessionService(ApplicationDbContext dbContext, IAgentService agentService, ILogger<ChatSessionService> logger)
    {
       _dbContext = dbContext;
        _agentService = agentService;
        _logger = logger;
    }
    public async Task<ChatSession?> CreateSessionAsync()
    {
        var chatSession = new ChatSession
        {
            StartTime = DateTime.UtcNow,
            IsActive = _agentService.IsOfficeHours(),
            QueuedTime = DateTime.Now
        };
        try
        {
            _dbContext.ChatSessions.Add(chatSession);

            if (chatSession.IsActive)
            {
                if (!(await CanQueueChatSession()))
                {
                    chatSession.IsActive = false;
                    chatSession.QueuedTime = null; // Not in queue
                }
            }
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a chat session.");
            return null;
        }

        return chatSession;
    }
    private async Task<bool> CanQueueChatSession()
    {
        var currentShift = _agentService.DetermineCurrentShift();
        var availableAgents = await _agentService.GetAvailableAgentsForShift(currentShift);
        var teamCapacity = availableAgents.Sum(a => a.MaximumCapacity - a.CurrentChatCount);
        var activeChatSessions = await GetActiveSessionsAsync();

        if (activeChatSessions.Count >= teamCapacity)
        {
            if (!_agentService.IsOfficeHours())
            {
                return false;
            }
        }

        return true;
    }

   

    public async Task<ChatSession> GetSessionAsync(int sessionId)
    {
        return await _dbContext.ChatSessions.FindAsync(sessionId);
    }
    public async Task AssignAgentToChatSessionAsync()
    {
        var currentShift = _agentService.DetermineCurrentShift();
        var availableAgents = await _agentService.GetAvailableAgentsForShift(currentShift);

        foreach (var agent in availableAgents)
        {
            // Find the oldest (based on QueuedTime) unassigned session
            var chatSession = await _dbContext.ChatSessions
                .Where(s => s.IsActive && s.AssignedAgent == null)
                .OrderBy(s => s.QueuedTime)
                .FirstOrDefaultAsync();

            if (chatSession == null)
            {
                break;
            }

            chatSession.AssignedAgent = agent;
            chatSession.QueuedTime = null;

            agent.CurrentChatCount++;

            await _dbContext.SaveChangesAsync();
        }

    }

    public async Task UpdateSessionAsync(ChatSession chatSession)
    {
        try
        {
            _dbContext.Entry(chatSession).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while updating chat session {chatSession.Id}.");
            throw;
        }
    }
    public async Task<List<ChatSession>> GetQueuedSessionsAsync()
    {
        return await _dbContext.ChatSessions
            .Where(session => session.IsActive && session.AssignedAgent == null)
            .ToListAsync();
    }

    public async Task<List<ChatSession>> GetActiveSessionsAsync()
    {
        return await _dbContext.ChatSessions
            .Where(session => session.IsActive)
            .ToListAsync();
    }
}
