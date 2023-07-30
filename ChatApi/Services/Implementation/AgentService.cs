using ChatApi.Domain;
using ChatApi.Domain.Enum;
using ChatApi.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

public class AgentService : IAgentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AgentService> _logger;

    public AgentService(ApplicationDbContext dbContext, ILogger<AgentService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
 
    public async Task<List<Agent>> GetAvailableAgentsForShift(Shift shift)
    {
        try
        {
            var agents = await _dbContext.Agents.ToListAsync();
            return agents.Where(a => a.Shift == shift && a.CurrentChatCount < a.MaximumCapacity).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting available agents for the shift.");
            return new List<Agent>();
        }
    }

    public async Task<int> GetTotalAgentsCapacity()
    {
        return await _dbContext.Agents.SumAsync(a => a.MaximumCapacity);
    }

    public async Task<List<Agent>> GetAvailableAgentAsync()
    {
        try
        {
            return await _dbContext.Agents
                .Where(a => a.CurrentChatCount < a.MaximumCapacity)
                .OrderBy(a => a.Seniority)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting available agents.");
            return new List<Agent>();
        }
    }

    public bool IsOfficeHours()
    {
        var currentTime = DateTime.UtcNow.TimeOfDay;
        return currentTime >= new TimeSpan(8, 0, 0) && currentTime < new TimeSpan(18, 0, 0);
    }

    public bool UseOverflowTeam(int queueLength, int teamCapacity)
    {
        return IsOfficeHours() && queueLength > teamCapacity * 1.5;
    }

    public Shift DetermineCurrentShift()
    {
        var currentTime = DateTime.UtcNow.TimeOfDay;

        if (currentTime >= new TimeSpan(0, 0, 0) && currentTime < new TimeSpan(8, 0, 0))
            return Shift.Night;
        else if (currentTime >= new TimeSpan(8, 0, 0) && currentTime < new TimeSpan(16, 0, 0))
            return Shift.Morning;
        else
            return Shift.Evening;
    }
}
