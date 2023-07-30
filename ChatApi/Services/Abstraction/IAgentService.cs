using ChatApi.Domain;
using ChatApi.Domain.Enum;

namespace ChatApi.Services.Abstraction
{
    public interface IAgentService
    {
        Task<int> GetTotalAgentsCapacity();
        bool IsOfficeHours();
        bool UseOverflowTeam(int queueLength, int teamCapacity);
        Shift DetermineCurrentShift();
        Task<List<Agent>> GetAvailableAgentsForShift(Shift shift);
        Task<List<Agent>> GetAvailableAgentAsync();
    }
}
