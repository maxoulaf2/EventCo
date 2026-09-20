using EventCo.Domain.Common;
using EventCo.Domain.Events.DomainEvents;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Domain.Events;

public class Event : Entity
{
    private readonly List<EventParticipant> _participants = [];
    private readonly List<EventTask> _tasks = [];

    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime EventDate { get; private set; }
    public string? Location { get; private set; }
    public string? ImageUrl { get; private set; }
    public string InviteLinkToken { get; private set; } = null!;
    public Guid CreatedByUserId { get; private set; }
    public EventStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<EventParticipant> Participants => _participants.AsReadOnly();
    public IReadOnlyCollection<EventTask> Tasks => _tasks.AsReadOnly();

    private Event(Guid id, string title, string? description, DateTime eventDate, string? location, string? imageUrl, string inviteLinkToken, Guid createdByUserId, EventStatus status, DateTime createdAt)
        : base(id)
    {
        Title = title;
        Description = description;
        EventDate = eventDate;
        Location = location;
        ImageUrl = imageUrl;
        InviteLinkToken = inviteLinkToken;
        CreatedByUserId = createdByUserId;
        Status = status;
        CreatedAt = createdAt;
    }

    public static Event Create(string title, string? description, DateTime eventDate, string? location, string? imageUrl, string inviteLinkToken, Guid createdByUserId, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new EventTitleEmptyException();

        var @event = new Event(Guid.NewGuid(), title.Trim(), description, eventDate, location, imageUrl, inviteLinkToken, createdByUserId, EventStatus.Planned, now);

        var creatorParticipant = new EventParticipant(@event.Id, createdByUserId, ParticipantRole.Organizer, now);
        @event._participants.Add(creatorParticipant);

        @event.AddDomainEvent(new EventCreatedDomainEvent(@event.Id));

        return @event;
    }

    internal static Event Reconstitute(
        Guid id, string title, string? description, DateTime eventDate, string? location, string? imageUrl, string inviteLinkToken,
        Guid createdByUserId, EventStatus status, DateTime createdAt,
        IEnumerable<EventParticipant> participants, IEnumerable<EventTask> tasks)
    {
        var @event = new Event(id, title, description, eventDate, location, imageUrl, inviteLinkToken, createdByUserId, status, createdAt);
        @event._participants.AddRange(participants);
        @event._tasks.AddRange(tasks);
        return @event;
    }

    public void UpdateDetails(Guid actingUserId, string title, string? description, DateTime eventDate, string? location)
    {
        EnsureActingUserIsCreatorOrOrganizer(actingUserId);

        if (string.IsNullOrWhiteSpace(title))
            throw new EventTitleEmptyException(Id);

        Title = title.Trim();
        Description = description;
        EventDate = eventDate;
        Location = location;

        AddDomainEvent(new EventDetailsUpdatedDomainEvent(Id));
    }

    public void Cancel()
    {
        Status = EventStatus.Cancelled;
        AddDomainEvent(new EventStatusChangedDomainEvent(Id));
    }

    public void Complete()
    {
        Status = EventStatus.Completed;
        AddDomainEvent(new EventStatusChangedDomainEvent(Id));
    }

    public EventParticipant InviteParticipant(Guid actingUserId, Guid userId, DateTime now)
    {
        EnsureActingUserIsCreatorOrOrganizer(actingUserId);

        if (_participants.Any(p => p.UserId == userId))
            throw new ParticipantAlreadyInvitedException(Id, userId);

        var participant = new EventParticipant(Id, userId, ParticipantRole.Participant, now);
        _participants.Add(participant);
        AddDomainEvent(new ParticipantInvitedDomainEvent(Id, participant.Id));
        return participant;
    }

    public EventParticipant JoinViaInviteLink(Guid userId, DateTime now)
    {
        var existing = _participants.FirstOrDefault(p => p.UserId == userId);
        if (existing is not null)
            return existing; // idempotent : recliquer un lien déjà utilisé ramène juste sur l'événement

        var participant = new EventParticipant(Id, userId, ParticipantRole.Participant, now);
        _participants.Add(participant);
        // Réutilise ParticipantInvitedDomainEvent : la persistance (insertion) est identique à une
        // invitation classique, et aucun consommateur ne distingue aujourd'hui les deux origines.
        AddDomainEvent(new ParticipantInvitedDomainEvent(Id, participant.Id));
        return participant;
    }

    public void RegenerateInviteLink(Guid actingUserId, string newToken)
    {
        EnsureActingUserIsCreatorOrOrganizer(actingUserId);
        InviteLinkToken = newToken;
        AddDomainEvent(new EventInviteLinkRegeneratedDomainEvent(Id));
    }

    public void SetParticipationStatus(Guid userId, ParticipationStatus status)
    {
        if (userId == CreatedByUserId)
            throw new EventCreatorCannotChangeParticipationStatusException(Id, userId);

        var participant = GetParticipant(userId);
        participant.ChangeParticipationStatus(status);
        AddDomainEvent(new ParticipantParticipationStatusChangedDomainEvent(Id, participant.Id));
    }

    public void PromoteToOrganizer(Guid actingUserId, Guid targetUserId)
    {
        EnsureActingUserIsCreator(actingUserId);
        var participant = GetParticipant(targetUserId);
        participant.ChangeRole(ParticipantRole.Organizer);
        AddDomainEvent(new ParticipantRoleChangedDomainEvent(Id, participant.Id));
    }

    public void DemoteToParticipant(Guid actingUserId, Guid targetUserId)
    {
        EnsureActingUserIsCreator(actingUserId);

        if (targetUserId == CreatedByUserId)
            throw new EventCreatorCannotBeDemotedException(Id, targetUserId);

        var participant = GetParticipant(targetUserId);
        participant.ChangeRole(ParticipantRole.Participant);
        AddDomainEvent(new ParticipantRoleChangedDomainEvent(Id, participant.Id));
    }

    public void RemoveParticipant(Guid actingUserId, Guid targetUserId)
    {
        EnsureActingUserIsCreator(actingUserId);

        if (targetUserId == CreatedByUserId)
            throw new EventCreatorCannotBeRemovedException(Id, targetUserId);

        var participant = GetParticipant(targetUserId);
        _participants.Remove(participant);
        AddDomainEvent(new ParticipantRemovedDomainEvent(Id, participant.Id));
    }

    public void EnsureCanBeDeletedBy(Guid actingUserId) => EnsureActingUserIsCreator(actingUserId);

    public void EnsureCanBeViewedBy(Guid actingUserId) => EnsureActingUserIsParticipant(actingUserId);

    public EventTask AddTask(Guid actingUserId, string title, TaskCategory category, string? quantity, DateTime now)
    {
        EnsureActingUserIsParticipant(actingUserId);

        var task = new EventTask(Id, title, category, quantity, actingUserId, now);
        _tasks.Add(task);
        AddDomainEvent(new TaskCreatedDomainEvent(task));
        return task;
    }

    public void AssignTask(Guid actingUserId, Guid taskId, Guid userId)
    {
        EnsureActingUserIsParticipant(actingUserId);

        if (_participants.All(p => p.UserId != userId))
            throw new TaskAssigneeNotParticipantException(Id, taskId, userId);

        if (userId != actingUserId && !IsCreatorOrOrganizer(actingUserId))
            throw new ParticipantCannotAssignTaskToOthersException(Id, taskId, actingUserId, userId);

        var task = GetTask(taskId);
        task.AssignTo(userId);
        AddDomainEvent(new TaskAssignedDomainEvent(task));
    }

    public void UnassignTask(Guid actingUserId, Guid taskId)
    {
        var task = GetTask(taskId);
        EnsureActingUserCanUnassignTask(actingUserId, task);
        task.Unassign();
        AddDomainEvent(new TaskUnassignedDomainEvent(task));
    }

    public void RemoveTask(Guid actingUserId, Guid taskId)
    {
        var task = GetTask(taskId);
        EnsureActingUserCanDeleteTask(actingUserId, task);
        _tasks.Remove(task);
        AddDomainEvent(new TaskDeletedDomainEvent(Id, taskId));
    }

    private void EnsureActingUserIsCreator(Guid actingUserId)
    {
        if (actingUserId != CreatedByUserId)
            throw new UserNotEventCreatorException(Id, actingUserId);
    }

    // Le créateur possède toujours une entrée EventParticipant avec Role = Organizer (cf. Create),
    // donc cette seule vérification de rôle couvre à la fois le créateur et les co-organisateurs.
    private void EnsureActingUserIsCreatorOrOrganizer(Guid actingUserId)
    {
        if (!IsCreatorOrOrganizer(actingUserId))
            throw new UserNotEventOrganizerException(Id, actingUserId);
    }

    private bool IsCreatorOrOrganizer(Guid userId) =>
        _participants.Any(p => p.UserId == userId && p.Role == ParticipantRole.Organizer);

    private void EnsureActingUserIsParticipant(Guid actingUserId)
    {
        if (_participants.All(p => p.UserId != actingUserId))
            throw new UserNotEventParticipantException(Id, actingUserId);
    }

    private void EnsureActingUserCanDeleteTask(Guid actingUserId, EventTask task)
    {
        EnsureActingUserIsParticipant(actingUserId);

        if (task.CreatedByUserId != actingUserId && !IsCreatorOrOrganizer(actingUserId))
            throw new ParticipantCannotDeleteOthersTaskException(Id, task.Id, actingUserId);
    }

    private void EnsureActingUserCanUnassignTask(Guid actingUserId, EventTask task)
    {
        EnsureActingUserIsParticipant(actingUserId);

        if (task.AssignedToUserId != actingUserId && !IsCreatorOrOrganizer(actingUserId))
            throw new ParticipantCannotUnassignOthersTaskException(Id, task.Id, actingUserId);
    }

    private EventParticipant GetParticipant(Guid userId) =>
        _participants.FirstOrDefault(p => p.UserId == userId)
        ?? throw new ParticipantNotFoundException(Id, userId);

    private EventTask GetTask(Guid taskId) =>
        _tasks.FirstOrDefault(t => t.Id == taskId)
        ?? throw new EventTaskNotFoundException(Id, taskId);
}
