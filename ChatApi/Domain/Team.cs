namespace ChatApi.Domain
{
    public class Team
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<Agent>? Agents { get; set; }
        public bool IsOverflow { get; set; }
    }
}