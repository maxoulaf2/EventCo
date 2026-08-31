using EventCo.Domain.Common;

namespace EventCo.Domain.Events;

public class Event : Entity
{
    private readonly List<EventParticipant> _participants = [];
    private readonly List<EventTask> _tasks = [];

    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime EventDate { get; private set; }
    public string? Location { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public EventStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<EventParticipant> Participants => _participants.AsReadOnly();
    public IReadOnlyCollection<EventTask> Tasks => _tasks.AsReadOnly();

    private Event()
    {
    }

    private Event(Guid id, string title, string? description, DateTime eventDate, string? location, Guid createdByUserId, DateTime createdAt)
        : base(id)
    {
        Title = title;
        Description = description;
        EventDate = eventDate;
        Location = location;
        CreatedByUserId = createdByUserId;
        Status = EventStatus.Planned;
        CreatedAt = createdAt;
    }

    public static Event Create(string title, string? description, DateTime eventDate, string? location, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Le titre de l'événement ne peut pas être vide.");

        var now = DateTime.UtcNow;
        var @event = new Event(Guid.NewGuid(), title.Trim(), description, eventDate, location, createdByUserId, now);

        var creatorParticipant = new EventParticipant(@event.Id, createdByUserId, ParticipantRole.Organizer, now);
        creatorParticipant.Join(now);
        @event._participants.Add(creatorParticipant);

        return @event;
    }

    public void UpdateDetails(string title, string? description, DateTime eventDate, string? location)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Le titre de l'événement ne peut pas être vide.");

        Title = title.Trim();
        Description = description;
        EventDate = eventDate;
        Location = location;
    }

    public void Cancel() => Status = EventStatus.Cancelled;

    public void Complete() => Status = EventStatus.Completed;

    public EventParticipant InviteParticipant(Guid userId)
    {
        if (_participants.Any(p => p.UserId == userId))
            throw new DomainException("Cet utilisateur est déjà invité à l'événement.");

        var participant = new EventParticipant(Id, userId, ParticipantRole.Participant, DateTime.UtcNow);
        _participants.Add(participant);
        return participant;
    }

    public void ConfirmParticipant(Guid userId) => GetParticipant(userId).Join(DateTime.UtcNow);

    public void PromoteToOrganizer(Guid actingUserId, Guid targetUserId)
    {
        EnsureActingUserIsCreator(actingUserId);
        GetParticipant(targetUserId).ChangeRole(ParticipantRole.Organizer);
    }

    public void DemoteToParticipant(Guid actingUserId, Guid targetUserId)
    {
        EnsureActingUserIsCreator(actingUserId);

        if (targetUserId == CreatedByUserId)
            throw new DomainException("Le créateur de l'événement ne peut pas être rétrogradé.");

        GetParticipant(targetUserId).ChangeRole(ParticipantRole.Participant);
    }

    public void RemoveParticipant(Guid actingUserId, Guid targetUserId)
    {
        EnsureActingUserIsCreator(actingUserId);

        if (targetUserId == CreatedByUserId)
            throw new DomainException("Le créateur de l'événement ne peut pas être retiré.");

        _participants.Remove(GetParticipant(targetUserId));
    }

    public EventTask AddTask(string title, TaskCategory category, string? quantity)
    {
        var task = new EventTask(Id, title, category, quantity, DateTime.UtcNow);
        _tasks.Add(task);
        return task;
    }

    public void AssignTask(Guid taskId, Guid userId)
    {
        if (_participants.All(p => p.UserId != userId))
            throw new DomainException("Impossible d'assigner la tâche à un utilisateur qui n'est pas participant.");

        GetTask(taskId).AssignTo(userId);
    }

    public void UnassignTask(Guid taskId) => GetTask(taskId).Unassign();

    public void CompleteTask(Guid taskId) => GetTask(taskId).MarkDone();

    public void ReopenTask(Guid taskId) => GetTask(taskId).MarkNotDone();

    public void RemoveTask(Guid taskId) => _tasks.Remove(GetTask(taskId));

    private void EnsureActingUserIsCreator(Guid actingUserId)
    {
        if (actingUserId != CreatedByUserId)
            throw new DomainException("Seul le créateur de l'événement peut effectuer cette action.");
    }

    private EventParticipant GetParticipant(Guid userId) =>
        _participants.FirstOrDefault(p => p.UserId == userId)
        ?? throw new DomainException("Cet utilisateur ne participe pas à l'événement.");

    private EventTask GetTask(Guid taskId) =>
        _tasks.FirstOrDefault(t => t.Id == taskId)
        ?? throw new DomainException("Tâche introuvable pour cet événement.");
}
