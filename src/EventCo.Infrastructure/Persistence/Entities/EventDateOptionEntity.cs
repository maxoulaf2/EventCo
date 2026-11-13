namespace EventCo.Infrastructure.Persistence.Entities;

public class EventDateOptionEntity
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public DateTime Date { get; set; }

    public List<EventDateVoteEntity> Votes { get; set; } = [];
}
