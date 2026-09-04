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

    public static Event Create(string title, string? description, DateTime eventDate, string? location, Guid createdByUserId, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new EventTitleEmptyException();

        var @event = new Event(Guid.NewGuid(), title.Trim(), description, eventDate, location, createdByUserId, now);

        var creatorParticipant = new EventParticipant(@event.Id, createdByUserId, ParticipantRole.Organizer, now);
        creatorParticipant.Join(now);
        @event._participants.Add(creatorParticipant);

        return @event;
    }

    public void UpdateDetails(string title, string? description, DateTime eventDate, string? location)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new EventTitleEmptyException(Id);

        Title = title.Trim();
        Description = description;
        EventDate = eventDate;
        Location = location;
    }

    public void Cancel() => Status = EventStatus.Cancelled;

    public void Complete() => Status = EventStatus.Completed;

    public EventParticipant InviteParticipant(Guid userId, DateTime now)
    {
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

    public EventTask AddTask(string title, TaskCategory category, string? quantity, DateTime now)
    {
        var task = new EventTask(Id, title, category, quantity, now);
        _tasks.Add(task);
        return task;
    }

    public void AssignTask(Guid taskId, Guid userId)
    {
        if (_participants.All(p => p.UserId != userId))
            throw new TaskAssigneeNotParticipantException(Id, taskId, userId);

        GetTask(taskId).AssignTo(userId);
    }

    public void UnassignTask(Guid taskId) => GetTask(taskId).Unassign();

    public void CompleteTask(Guid taskId) => GetTask(taskId).MarkDone();

    public void ReopenTask(Guid taskId) => GetTask(taskId).MarkNotDone();

    public void RemoveTask(Guid taskId) => _tasks.Remove(GetTask(taskId));

    private void EnsureActingUserIsCreator(Guid actingUserId)
    {
        if (actingUserId != CreatedByUserId)
            throw new UserNotEventCreatorException(Id, actingUserId);
    }

    private EventParticipant GetParticipant(Guid userId) =>
        _participants.FirstOrDefault(p => p.UserId == userId)
        ?? throw new ParticipantNotFoundException(Id, userId);

    private EventTask GetTask(Guid taskId) =>
        _tasks.FirstOrDefault(t => t.Id == taskId)
        ?? throw new EventTaskNotFoundException(Id, taskId);
}
