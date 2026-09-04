using EventCo.Domain.Events;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Domain.Tests.Events;

public class EventTests
{
    private static Event CreateEvent(out Guid creatorId)
    {
        creatorId = Guid.NewGuid();
        return Event.Create("Repas de Noël", "Chez Alice", DateTime.UtcNow.AddDays(30), "Paris", creatorId, DateTime.UtcNow);
    }

    [Fact]
    public void Create_ValidData_AddsCreatorAsJoinedOrganizer()
    {
        var @event = CreateEvent(out var creatorId);

        var creatorParticipant = Assert.Single(@event.Participants);
        Assert.Equal(creatorId, creatorParticipant.UserId);
        Assert.Equal(ParticipantRole.Organizer, creatorParticipant.Role);
        Assert.True(creatorParticipant.HasJoined);
        Assert.Equal(EventStatus.Planned, @event.Status);
    }

    [Fact]
    public void Create_EmptyTitle_ThrowsEventTitleEmptyException()
    {
        Assert.Throws<EventTitleEmptyException>(() => Event.Create(" ", null, DateTime.UtcNow, null, Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void InviteParticipant_UserAlreadyInvited_ThrowsParticipantAlreadyInvitedException()
    {
        var @event = CreateEvent(out var creatorId);

        Assert.Throws<ParticipantAlreadyInvitedException>(() => @event.InviteParticipant(creatorId, DateTime.UtcNow));
    }

    [Fact]
    public void InviteParticipant_NewUser_AddsParticipantNotYetJoined()
    {
        var @event = CreateEvent(out _);
        var invitedUserId = Guid.NewGuid();

        var participant = @event.InviteParticipant(invitedUserId, DateTime.UtcNow);

        Assert.Equal(ParticipantRole.Participant, participant.Role);
        Assert.False(participant.HasJoined);
    }

    [Fact]
    public void PromoteToOrganizer_ActingUserNotCreator_ThrowsUserNotEventCreatorException()
    {
        var @event = CreateEvent(out _);
        var invitedUserId = Guid.NewGuid();
        @event.InviteParticipant(invitedUserId, DateTime.UtcNow);

        Assert.Throws<UserNotEventCreatorException>(() => @event.PromoteToOrganizer(invitedUserId, invitedUserId));
    }

    [Fact]
    public void PromoteToOrganizer_ActingUserIsCreator_ChangesRole()
    {
        var @event = CreateEvent(out var creatorId);
        var invitedUserId = Guid.NewGuid();
        var participant = @event.InviteParticipant(invitedUserId, DateTime.UtcNow);

        @event.PromoteToOrganizer(creatorId, invitedUserId);

        Assert.Equal(ParticipantRole.Organizer, participant.Role);
    }

    [Fact]
    public void RemoveParticipant_TargetIsCreator_ThrowsEventCreatorCannotBeRemovedException()
    {
        var @event = CreateEvent(out var creatorId);

        Assert.Throws<EventCreatorCannotBeRemovedException>(() => @event.RemoveParticipant(creatorId, creatorId));
    }

    [Fact]
    public void RemoveParticipant_TargetIsRegularParticipant_RemovesFromCollection()
    {
        var @event = CreateEvent(out var creatorId);
        var invitedUserId = Guid.NewGuid();
        @event.InviteParticipant(invitedUserId, DateTime.UtcNow);

        @event.RemoveParticipant(creatorId, invitedUserId);

        Assert.DoesNotContain(@event.Participants, p => p.UserId == invitedUserId);
    }

    [Fact]
    public void EnsureCanBeDeletedBy_ActingUserNotCreator_ThrowsUserNotEventCreatorException()
    {
        var @event = CreateEvent(out _);
        var invitedUserId = Guid.NewGuid();
        @event.InviteParticipant(invitedUserId, DateTime.UtcNow);

        Assert.Throws<UserNotEventCreatorException>(() => @event.EnsureCanBeDeletedBy(invitedUserId));
    }

    [Fact]
    public void EnsureCanBeDeletedBy_ActingUserIsCreator_DoesNotThrow()
    {
        var @event = CreateEvent(out var creatorId);

        var exception = Record.Exception(() => @event.EnsureCanBeDeletedBy(creatorId));

        Assert.Null(exception);
    }

    [Fact]
    public void AssignTask_UserNotParticipant_ThrowsTaskAssigneeNotParticipantException()
    {
        var @event = CreateEvent(out _);
        var task = @event.AddTask("Bûche au chocolat", TaskCategory.Courses, "1", DateTime.UtcNow);

        Assert.Throws<TaskAssigneeNotParticipantException>(() => @event.AssignTask(task.Id, Guid.NewGuid()));
    }

    [Fact]
    public void AssignTask_UserIsParticipant_AssignsTask()
    {
        var @event = CreateEvent(out var creatorId);
        var task = @event.AddTask("Bûche au chocolat", TaskCategory.Courses, "1", DateTime.UtcNow);

        @event.AssignTask(task.Id, creatorId);

        Assert.Equal(creatorId, task.AssignedToUserId);
    }

    [Fact]
    public void CompleteTask_ExistingTask_SetsIsDoneTrue()
    {
        var @event = CreateEvent(out _);
        var task = @event.AddTask("Bûche au chocolat", TaskCategory.Courses, "1", DateTime.UtcNow);

        @event.CompleteTask(task.Id);

        Assert.True(task.IsDone);
    }

    [Fact]
    public void RemoveTask_UnknownTaskId_ThrowsEventTaskNotFoundException()
    {
        var @event = CreateEvent(out _);

        Assert.Throws<EventTaskNotFoundException>(() => @event.RemoveTask(Guid.NewGuid()));
    }
}
