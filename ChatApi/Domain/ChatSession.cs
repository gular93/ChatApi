using ChatApi.Domain;

public class ChatSession
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public bool IsActive { get; set; }
    public Agent AssignedAgent { get; set; }
    public int AssignedAgentId { get; set; } 
    private int MissedPolls { get; set; }
    public DateTime LastPollTime { get; set; }
    public DateTime? QueuedTime { get; set; }

    public void ReceivedPoll()
    {
        MissedPolls = 0;
        LastPollTime = DateTime.Now; 
    }

    public void MissedPoll()
    {
        MissedPolls++;

        if (MissedPolls >= 3)
        {
            IsActive = false;
        }
    }
}
