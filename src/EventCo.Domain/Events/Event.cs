using EventCo.Domain.Common;
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
    public Guid CreatedByUserId { get; private set; }
    public EventStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<EventParticipant> Participants => _participants.AsReadOnly();
    public IReadOnlyCollection<EventTask> Tasks => _tasks.AsReadOnly();

    private Event(Guid id, string title, string? description, DateTime eventDate, string? location, Guid createdByUserId, EventStatus status, DateTime createdAt)
        : base(id)
    {
        Title = title;
        Description = description;
        EventDate = eventDate;
        Location = location;
        CreatedByUserId = createdByUserId;
        Status = status;
        CreatedAt = createdAt;
    }

    public static Event Create(string title, string? description, DateTime eventDate, string? location, Guid createdByUserId, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new EventTitleEmptyException();

        var @event = new Event(Guid.NewGuid(), title.Trim(), description, eventDate, location, createdByUserId, EventStatus.Planned, now);

        var creatorParticipant = new EventParticipant(@event.Id, createdByUserId, ParticipantRole.Organizer, now);
        creatorParticipant.Join(now);
        @event._participants.Add(creatorParticipant);

        return @event;
    }

    internal static Event Reconstitute(
        Guid id, string title, string? description, DateTime eventDate, string? location,
        Guid createdByUserId, EventStatus status, DateTime createdAt,
        IEnumerable<EventParticipant> participants, IEnumerable<EventTask> tasks)
    {
        var @event = new Event(id, title, description, eventDate, location, createdByUserId, status, createdAt);
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
    }

    public void Cancel() => Status = EventStatus.Cancelled;

    public void Complete() => Status = EventStatus.Completed;

    public EventParticipant InviteParticipant(Guid actingUserId, Guid userId, DateTime now)
    {
        EnsureActingUserIsCreatorOrOrganizer(actingUserId);

        if (_participants.Any(p => p.UserId == userId))
            throw new ParticipantAlreadyInvitedException(Id, userId);

        var participant = new EventParticipant(Id, userId, ParticipantRole.Participant, now);
        _participants.Add(participant);
        return participant;
    }

    public void ConfirmParticipant(Guid userId, DateTime now) => GetParticipant(userId).Join(now);

    public void PromoteToOrganizer(Guid actingUserId, Guid targetUserId)
    {
        EnsureActingUserIsCreator(actingUserId);
        GetParticipant(targetUserId).ChangeRole(ParticipantRole.Organizer);
    }

    public void DemoteToParticipant(Guid actingUserId, Guid targetUserId)
    {
        EnsureActingUserIsCreator(actingUserId);

        if (targetUserId == CreatedByUserId)
            throw new EventCreatorCannotBeDemotedException(Id, targetUserId);

        GetParticipant(targetUserId).ChangeRole(ParticipantRole.Participant);
    }

    public void RemoveParticipant(Guid actingUserId, Guid targetUserId)
    {
        EnsureActingUserIsCreator(actingUserId);

        if (targetUserId == CreatedByUserId)
            throw new EventCreatorCannotBeRemovedException(Id, targetUserId);

        _participants.Remove(GetParticipant(targetUserId));
    }

    public void EnsureCanBeDeletedBy(Guid actingUserId) => EnsureActingUserIsCreator(actingUserId);

    public void EnsureCanBeViewedBy(Guid actingUserId) => EnsureActingUserIsParticipant(actingUserId);

    public EventTask AddTask(Guid actingUserId, string title, TaskCategory category, string? quantity, DateTime now)
    {
        EnsureActingUserIsParticipant(actingUserId);

        var task = new EventTask(Id, title, category, quantity, actingUserId, now);
        _tasks.Add(task);
        return task;
    }

    public void AssignTask(Guid actingUserId, Guid taskId, Guid userId)
    {
        EnsureActingUserIsParticipant(actingUserId);

        if (_participants.All(p => p.UserId != userId))
            throw new TaskAssigneeNotParticipantException(Id, taskId, userId);

        if (userId != actingUserId && !IsCreatorOrOrganizer(actingUserId))
            throw new ParticipantCannotAssignTaskToOthersException(Id, taskId, actingUserId, userId);

        GetTask(taskId).AssignTo(userId);
    }

    public void UnassignTask(Guid taskId) => GetTask(taskId).Unassign();

    public void CompleteTask(Guid actingUserId, Guid taskId)
    {
        var task = GetTask(taskId);
        EnsureActingUserCanToggleTaskDone(actingUserId, task);
        task.MarkDone();
    }

    public void ReopenTask(Guid actingUserId, Guid taskId)
    {
        var task = GetTask(taskId);
        EnsureActingUserCanToggleTaskDone(actingUserId, task);
        task.MarkNotDone();
    }

    public void RemoveTask(Guid actingUserId, Guid taskId)
    {
        var task = GetTask(taskId);
        EnsureActingUserCanDeleteTask(actingUserId, task);
        _tasks.Remove(task);
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

    private void EnsureActingUserCanToggleTaskDone(Guid actingUserId, EventTask task)
    {
        EnsureActingUserIsParticipant(actingUserId);

        if (task.AssignedToUserId != actingUserId && !IsCreatorOrOrganizer(actingUserId))
            throw new ParticipantCannotToggleOthersTaskException(Id, task.Id, actingUserId);
    }

    private void EnsureActingUserCanDeleteTask(Guid actingUserId, EventTask task)
    {
        EnsureActingUserIsParticipant(actingUserId);

        if (task.CreatedByUserId != actingUserId && !IsCreatorOrOrganizer(actingUserId))
            throw new ParticipantCannotDeleteOthersTaskException(Id, task.Id, actingUserId);
    }

    private EventParticipant GetParticipant(Guid userId) =>
        _participants.FirstOrDefault(p => p.UserId == userId)
        ?? throw new ParticipantNotFoundException(Id, userId);

    private EventTask GetTask(Guid taskId) =>
        _tasks.FirstOrDefault(t => t.Id == taskId)
        ?? throw new EventTaskNotFoundException(Id, taskId);
}
