using EventCo.Domain.Common;

namespace EventCo.Domain.Events;

public class EventDateOption : Entity
{
    private readonly HashSet<Guid> _voterUserIds = [];

    public Guid EventId { get; private set; }
    public DateTime Date { get; private set; }

    public IReadOnlyCollection<Guid> VoterUserIds => _voterUserIds;

    internal EventDateOption(Guid eventId, DateTime date)
        : this(Guid.NewGuid(), eventId, date, [])
    {
    }

    private EventDateOption(Guid id, Guid eventId, DateTime date, IEnumerable<Guid> voterUserIds)
        : base(id)
    {
        EventId = eventId;
        Date = date;
        _voterUserIds.UnionWith(voterUserIds);
    }

    internal static EventDateOption Reconstitute(Guid id, Guid eventId, DateTime date, IEnumerable<Guid> voterUserIds) =>
        new(id, eventId, date, voterUserIds);

    // Renvoient false si rien n'a changé (vote déjà présent / déjà absent) : voter est idempotent.
    internal bool AddVote(Guid userId) => _voterUserIds.Add(userId);

    internal bool RemoveVote(Guid userId) => _voterUserIds.Remove(userId);
}
