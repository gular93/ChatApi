using ChatApi.Domain.Enum;

namespace ChatApi.Domain
{
    public class Agent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SeniorityLevel Seniority { get; set; }
        public int CurrentChatCount { get; set; }
        public Team Team { get; set; }
        public int TeamId { get; set; } 
        public Shift Shift { get; set; }
        public bool CanTakeChat => CurrentChatCount < MaximumCapacity;

        public int MaximumCapacity => GetCapacityBasedOnSeniority();

        private int GetCapacityBasedOnSeniority()
        {
            return Seniority switch
            {
                SeniorityLevel.Junior => 4,
                SeniorityLevel.MidLevel => 6,
                SeniorityLevel.Senior => 8,
                SeniorityLevel.TeamLead => 5,
                _ => 0
            };
        }

    }



}
