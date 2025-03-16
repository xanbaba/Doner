namespace Doner.Features.SprintFeature;

public class Sprint
{
    public int Id { get; set; }
    
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    public DateTime StartDateUtc { get; set; }
    public DateTime ExpireDateUtc { get; set; }
    public DateTime DeadlineDateUtc { get; set; }
    
    public Status Status { get; set; }
    
    
}