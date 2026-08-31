using EventCo.Domain.Common;
using EventCo.Domain.Events;

namespace EventCo.Domain.Tests.Events;

public class EventTests
{
    private static Event CreateEvent(out Guid creatorId)
    {
        creatorId = Guid.NewGuid();
        return Event.Create("Repas de Noël", "Chez Alice", DateTime.UtcNow.AddDays(30), "Paris", creatorId);
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
    public void Create_EmptyTitle_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Event.Create(" ", null, DateTime.UtcNow, null, Guid.NewGuid()));
    }

    [Fact]
    public void InviteParticipant_UserAlreadyInvited_ThrowsDomainException()
    {
        var @event = CreateEvent(out var creatorId);

        Assert.Throws<DomainException>(() => @event.InviteParticipant(creatorId));
    }

    [Fact]
    public void InviteParticipant_NewUser_AddsParticipantNotYetJoined()
    {
        var @event = CreateEvent(out _);
        var invitedUserId = Guid.NewGuid();

        var participant = @event.InviteParticipant(invitedUserId);

        Assert.Equal(ParticipantRole.Participant, participant.Role);
        Assert.False(participant.HasJoined);
    }

    [Fact]
    public void PromoteToOrganizer_ActingUserNotCreator_ThrowsDomainException()
    {
        var @event = CreateEvent(out _);
        var invitedUserId = Guid.NewGuid();
        @event.InviteParticipant(invitedUserId);

        Assert.Throws<DomainException>(() => @event.PromoteToOrganizer(invitedUserId, invitedUserId));
    }

    [Fact]
    public void PromoteToOrganizer_ActingUserIsCreator_ChangesRole()
    {
        var @event = CreateEvent(out var creatorId);
        var invitedUserId = Guid.NewGuid();
        var participant = @event.InviteParticipant(invitedUserId);

        @event.PromoteToOrganizer(creatorId, invitedUserId);

        Assert.Equal(ParticipantRole.Organizer, participant.Role);
    }

    [Fact]
    public void RemoveParticipant_TargetIsCreator_ThrowsDomainException()
    {
        var @event = CreateEvent(out var creatorId);

        Assert.Throws<DomainException>(() => @event.RemoveParticipant(creatorId, creatorId));
    }

    [Fact]
    public void RemoveParticipant_TargetIsRegularParticipant_RemovesFromCollection()
    {
        var @event = CreateEvent(out var creatorId);
        var invitedUserId = Guid.NewGuid();
        @event.InviteParticipant(invitedUserId);

        @event.RemoveParticipant(creatorId, invitedUserId);

        Assert.DoesNotContain(@event.Participants, p => p.UserId == invitedUserId);
    }

    [Fact]
    public void AssignTask_UserNotParticipant_ThrowsDomainException()
    {
        var @event = CreateEvent(out _);
        var task = @event.AddTask("Bûche au chocolat", TaskCategory.Courses, "1");

        Assert.Throws<DomainException>(() => @event.AssignTask(task.Id, Guid.NewGuid()));
    }

    [Fact]
    public void AssignTask_UserIsParticipant_AssignsTask()
    {
        var @event = CreateEvent(out var creatorId);
        var task = @event.AddTask("Bûche au chocolat", TaskCategory.Courses, "1");

        @event.AssignTask(task.Id, creatorId);

        Assert.Equal(creatorId, task.AssignedToUserId);
    }

    [Fact]
    public void CompleteTask_ExistingTask_SetsIsDoneTrue()
    {
        var @event = CreateEvent(out _);
        var task = @event.AddTask("Bûche au chocolat", TaskCategory.Courses, "1");

        @event.CompleteTask(task.Id);

        Assert.True(task.IsDone);
    }

    [Fact]
    public void RemoveTask_UnknownTaskId_ThrowsDomainException()
    {
        var @event = CreateEvent(out _);

        Assert.Throws<DomainException>(() => @event.RemoveTask(Guid.NewGuid()));
    }
}
