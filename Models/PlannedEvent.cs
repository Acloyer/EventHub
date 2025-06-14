namespace EventHub.Models;

public class PlannedEvent
{
    public int UserId { get; set; }
    public int EventId { get; set; }
    public virtual User? User { get; set; }
    public virtual Event? Event { get; set; }
}
