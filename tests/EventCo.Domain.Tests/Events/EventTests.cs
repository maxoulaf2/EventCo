using EventCo.Domain.Events;
using EventCo.Domain.Events.DomainEvents;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Domain.Tests.Events;

public class EventTests
{
    private static Event CreateEvent(out Guid creatorId)
    {
        creatorId = Guid.NewGuid();
        return Event.Create("Repas de Noël", "Chez Alice", DateTime.UtcNow.AddDays(30), "Paris", null, "invite-token", creatorId, DateTime.UtcNow);
    }

    [Fact]
    public void Create_ValidData_AddsCreatorAsOrganizer()
    {
        var @event = CreateEvent(out var creatorId);

        var creatorParticipant = Assert.Single(@event.Participants);
        Assert.Equal(creatorId, creatorParticipant.UserId);
        Assert.Equal(ParticipantRole.Organizer, creatorParticipant.Role);
        Assert.Equal(ParticipationStatus.Attending, creatorParticipant.ParticipationStatus);
        Assert.Equal(EventStatus.Planned, @event.Status);
    }

    [Fact]
    public void Create_EmptyTitle_ThrowsEventTitleEmptyException()
    {
        Assert.Throws<EventTitleEmptyException>(() => Event.Create(" ", null, DateTime.UtcNow, null, null, "invite-token", Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void Create_NoImageUrl_ImageUrlIsNull()
    {
        var @event = CreateEvent(out _);

        Assert.Null(@event.ImageUrl);
    }

    [Fact]
    public void Create_WithImageUrl_ImageUrlIsSet()
    {
        var @event = Event.Create("Repas de Noël", "Chez Alice", DateTime.UtcNow.AddDays(30), "Paris", "https://example.com/photo.jpg", "invite-token", Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal("https://example.com/photo.jpg", @event.ImageUrl);
    }

    [Fact]
    public void Cancel_SetsStatusAndRaisesDomainEvent()
    {
        var @event = CreateEvent(out _);
        @event.ClearDomainEvents();

        @event.Cancel();

        Assert.Equal(EventStatus.Cancelled, @event.Status);
        Assert.Contains(@event.DomainEvents, e => e is EventStatusChangedDomainEvent);
    }

    [Fact]
    public void Complete_SetsStatusAndRaisesDomainEvent()
    {
        var @event = CreateEvent(out _);
        @event.ClearDomainEvents();

        @event.Complete();

        Assert.Equal(EventStatus.Completed, @event.Status);
        Assert.Contains(@event.DomainEvents, e => e is EventStatusChangedDomainEvent);
    }

    [Fact]
    public void InviteParticipant_UserAlreadyInvited_ThrowsParticipantAlreadyInvitedException()
    {
        var @event = CreateEvent(out var creatorId);

        Assert.Throws<ParticipantAlreadyInvitedException>(() => @event.InviteParticipant(creatorId, creatorId, DateTime.UtcNow));
    }

    [Fact]
    public void InviteParticipant_NewUser_AddsParticipantWithUnknownParticipationStatus()
    {
        var @event = CreateEvent(out var creatorId);
        var invitedUserId = Guid.NewGuid();

        var participant = @event.InviteParticipant(creatorId, invitedUserId, DateTime.UtcNow);

        Assert.Equal(ParticipantRole.Participant, participant.Role);
        Assert.Equal(ParticipationStatus.Unknown, participant.ParticipationStatus);
    }

    [Fact]
    public void InviteParticipant_ActingUserNotCreatorNorOrganizer_ThrowsUserNotEventOrganizerException()
    {
        var @event = CreateEvent(out var creatorId);
        var regularParticipantId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, regularParticipantId, DateTime.UtcNow);

        Assert.Throws<UserNotEventOrganizerException>(
            () => @event.InviteParticipant(regularParticipantId, Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void InviteParticipant_ActingUserIsOrganizer_AddsParticipant()
    {
        var @event = CreateEvent(out var creatorId);
        var organizerId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, organizerId, DateTime.UtcNow);
        @event.PromoteToOrganizer(creatorId, organizerId);

        var participant = @event.InviteParticipant(organizerId, Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal(ParticipantRole.Participant, participant.Role);
    }

    [Fact]
    public void PromoteToOrganizer_ActingUserNotCreator_ThrowsUserNotEventCreatorException()
    {
        var @event = CreateEvent(out var creatorId);
        var invitedUserId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, invitedUserId, DateTime.UtcNow);

        Assert.Throws<UserNotEventCreatorException>(() => @event.PromoteToOrganizer(invitedUserId, invitedUserId));
    }

    [Fact]
    public void PromoteToOrganizer_ActingUserIsCreator_ChangesRole()
    {
        var @event = CreateEvent(out var creatorId);
        var invitedUserId = Guid.NewGuid();
        var participant = @event.InviteParticipant(creatorId, invitedUserId, DateTime.UtcNow);

        @event.PromoteToOrganizer(creatorId, invitedUserId);

        Assert.Equal(ParticipantRole.Organizer, participant.Role);
    }

    [Fact]
    public void PromoteToOrganizer_TargetNotParticipant_ThrowsParticipantNotFoundException()
    {
        var @event = CreateEvent(out var creatorId);

        Assert.Throws<ParticipantNotFoundException>(() => @event.PromoteToOrganizer(creatorId, Guid.NewGuid()));
    }

    [Fact]
    public void DemoteToParticipant_ActingUserNotCreator_ThrowsUserNotEventCreatorException()
    {
        var @event = CreateEvent(out var creatorId);
        var invitedUserId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, invitedUserId, DateTime.UtcNow);

        Assert.Throws<UserNotEventCreatorException>(() => @event.DemoteToParticipant(invitedUserId, invitedUserId));
    }

    [Fact]
    public void DemoteToParticipant_TargetIsCreator_ThrowsEventCreatorCannotBeDemotedException()
    {
        var @event = CreateEvent(out var creatorId);

        Assert.Throws<EventCreatorCannotBeDemotedException>(() => @event.DemoteToParticipant(creatorId, creatorId));
    }

    [Fact]
    public void DemoteToParticipant_TargetNotParticipant_ThrowsParticipantNotFoundException()
    {
        var @event = CreateEvent(out var creatorId);

        Assert.Throws<ParticipantNotFoundException>(() => @event.DemoteToParticipant(creatorId, Guid.NewGuid()));
    }

    [Fact]
    public void DemoteToParticipant_ActingUserIsCreator_ChangesRole()
    {
        var @event = CreateEvent(out var creatorId);
        var organizerId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, organizerId, DateTime.UtcNow);
        @event.PromoteToOrganizer(creatorId, organizerId);
        var participant = @event.Participants.Single(p => p.UserId == organizerId);

        @event.DemoteToParticipant(creatorId, organizerId);

        Assert.Equal(ParticipantRole.Participant, participant.Role);
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
        @event.InviteParticipant(creatorId, invitedUserId, DateTime.UtcNow);

        @event.RemoveParticipant(creatorId, invitedUserId);

        Assert.DoesNotContain(@event.Participants, p => p.UserId == invitedUserId);
    }

    [Fact]
    public void SetParticipationStatus_UserIsCreator_ThrowsEventCreatorCannotChangeParticipationStatusException()
    {
        var @event = CreateEvent(out var creatorId);

        Assert.Throws<EventCreatorCannotChangeParticipationStatusException>(
            () => @event.SetParticipationStatus(creatorId, ParticipationStatus.NotAttending));
    }

    [Fact]
    public void SetParticipationStatus_UserNotParticipant_ThrowsParticipantNotFoundException()
    {
        var @event = CreateEvent(out _);

        Assert.Throws<ParticipantNotFoundException>(() => @event.SetParticipationStatus(Guid.NewGuid(), ParticipationStatus.Attending));
    }

    [Fact]
    public void SetParticipationStatus_UserIsParticipant_ChangesStatus()
    {
        var @event = CreateEvent(out var creatorId);
        var invitedUserId = Guid.NewGuid();
        var participant = @event.InviteParticipant(creatorId, invitedUserId, DateTime.UtcNow);

        @event.SetParticipationStatus(invitedUserId, ParticipationStatus.Attending);

        Assert.Equal(ParticipationStatus.Attending, participant.ParticipationStatus);
    }

    [Fact]
    public void EnsureCanBeDeletedBy_ActingUserNotCreator_ThrowsUserNotEventCreatorException()
    {
        var @event = CreateEvent(out var creatorId);
        var invitedUserId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, invitedUserId, DateTime.UtcNow);

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
    public void EnsureCanBeViewedBy_ActingUserNotParticipant_ThrowsUserNotEventParticipantException()
    {
        var @event = CreateEvent(out _);

        Assert.Throws<UserNotEventParticipantException>(() => @event.EnsureCanBeViewedBy(Guid.NewGuid(), actingUserIsAdmin: false));
    }

    [Fact]
    public void EnsureCanBeViewedBy_ActingUserIsParticipant_DoesNotThrow()
    {
        var @event = CreateEvent(out var creatorId);
        var invitedUserId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, invitedUserId, DateTime.UtcNow);

        var exception = Record.Exception(() => @event.EnsureCanBeViewedBy(invitedUserId, actingUserIsAdmin: false));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanBeViewedBy_ActingUserIsAdminNotParticipant_DoesNotThrow()
    {
        var @event = CreateEvent(out _);

        var exception = Record.Exception(() => @event.EnsureCanBeViewedBy(Guid.NewGuid(), actingUserIsAdmin: true));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanBeViewedBy_ActingUserIsAdmin_DoesNotAddAdminAsParticipant()
    {
        var @event = CreateEvent(out _);
        var adminUserId = Guid.NewGuid();

        @event.EnsureCanBeViewedBy(adminUserId, actingUserIsAdmin: true);

        Assert.DoesNotContain(@event.Participants, p => p.UserId == adminUserId);
    }

    [Fact]
    public void AddItem_ActingUserIsAdminNotParticipant_ThrowsUserNotEventParticipantException()
    {
        // La consultation administrateur est en lecture seule : aucune action d'écriture n'est ouverte.
        var @event = CreateEvent(out _);

        Assert.Throws<UserNotEventParticipantException>(() =>
            @event.AddItem(Guid.NewGuid(), "Bûche", null, DateTime.UtcNow));
    }

    [Fact]
    public void UpdateDetails_ActingUserNotCreatorNorOrganizer_ThrowsUserNotEventOrganizerException()
    {
        var @event = CreateEvent(out var creatorId);
        var invitedUserId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, invitedUserId, DateTime.UtcNow);

        Assert.Throws<UserNotEventOrganizerException>(
            () => @event.UpdateDetails(invitedUserId, "Réveillon de Noël", null, DateTime.UtcNow.AddDays(31), "Lyon"));
    }

    [Fact]
    public void UpdateDetails_EmptyTitle_ThrowsEventTitleEmptyException()
    {
        var @event = CreateEvent(out var creatorId);

        Assert.Throws<EventTitleEmptyException>(
            () => @event.UpdateDetails(creatorId, " ", null, DateTime.UtcNow.AddDays(31), "Lyon"));
    }

    [Fact]
    public void UpdateDetails_ActingUserIsOrganizer_UpdatesDetails()
    {
        var @event = CreateEvent(out var creatorId);
        var organizerId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, organizerId, DateTime.UtcNow);
        @event.PromoteToOrganizer(creatorId, organizerId);

        @event.UpdateDetails(organizerId, "Réveillon de Noël", null, DateTime.UtcNow.AddDays(31), "Lyon");

        Assert.Equal("Réveillon de Noël", @event.Title);
    }

    [Fact]
    public void AddItem_ActingUserNotParticipant_ThrowsUserNotEventParticipantException()
    {
        var @event = CreateEvent(out _);

        Assert.Throws<UserNotEventParticipantException>(
            () => @event.AddItem(Guid.NewGuid(), "Bûche au chocolat", "1", DateTime.UtcNow));
    }

    [Fact]
    public void AddItem_ActingUserIsParticipantNotOrganizer_AddsItem()
    {
        var @event = CreateEvent(out var creatorId);
        var regularParticipantId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, regularParticipantId, DateTime.UtcNow);

        var item = @event.AddItem(regularParticipantId, "Bûche au chocolat", "1", DateTime.UtcNow);

        Assert.Contains(item, @event.Items);
    }

    [Fact]
    public void AddItem_EmptyTitle_ThrowsEventItemTitleEmptyException()
    {
        var @event = CreateEvent(out var creatorId);

        Assert.Throws<EventItemTitleEmptyException>(
            () => @event.AddItem(creatorId, " ", "1", DateTime.UtcNow));
    }

    [Fact]
    public void AssignItem_ActingUserNotParticipant_ThrowsUserNotEventParticipantException()
    {
        var @event = CreateEvent(out var creatorId);
        var item = @event.AddItem(creatorId, "Bûche au chocolat", "1", DateTime.UtcNow);

        Assert.Throws<UserNotEventParticipantException>(() => @event.AssignItem(Guid.NewGuid(), item.Id, creatorId));
    }

    [Fact]
    public void AssignItem_TargetUserNotParticipant_ThrowsItemAssigneeNotParticipantException()
    {
        var @event = CreateEvent(out var creatorId);
        var item = @event.AddItem(creatorId, "Bûche au chocolat", "1", DateTime.UtcNow);

        Assert.Throws<ItemAssigneeNotParticipantException>(() => @event.AssignItem(creatorId, item.Id, Guid.NewGuid()));
    }

    [Fact]
    public void AssignItem_CreatorAssignsToAnotherParticipant_AssignsItem()
    {
        var @event = CreateEvent(out var creatorId);
        var regularParticipantId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, regularParticipantId, DateTime.UtcNow);
        var item = @event.AddItem(creatorId, "Bûche au chocolat", "1", DateTime.UtcNow);

        @event.AssignItem(creatorId, item.Id, regularParticipantId);

        Assert.Equal(regularParticipantId, item.AssignedToUserId);
    }

    [Fact]
    public void AssignItem_SimpleParticipantAssignsToSelf_AssignsItem()
    {
        var @event = CreateEvent(out var creatorId);
        var regularParticipantId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, regularParticipantId, DateTime.UtcNow);
        var item = @event.AddItem(creatorId, "Bûche au chocolat", "1", DateTime.UtcNow);

        @event.AssignItem(regularParticipantId, item.Id, regularParticipantId);

        Assert.Equal(regularParticipantId, item.AssignedToUserId);
    }

    [Fact]
    public void AssignItem_UnknownItemId_ThrowsEventItemNotFoundException()
    {
        var @event = CreateEvent(out var creatorId);

        Assert.Throws<EventItemNotFoundException>(() => @event.AssignItem(creatorId, Guid.NewGuid(), creatorId));
    }

    [Fact]
    public void AssignItem_SimpleParticipantAssignsToAnotherParticipant_ThrowsParticipantCannotAssignItemToOthersException()
    {
        var @event = CreateEvent(out var creatorId);
        var regularParticipantId = Guid.NewGuid();
        var otherParticipantId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, regularParticipantId, DateTime.UtcNow);
        @event.InviteParticipant(creatorId, otherParticipantId, DateTime.UtcNow);
        var item = @event.AddItem(creatorId, "Bûche au chocolat", "1", DateTime.UtcNow);

        Assert.Throws<ParticipantCannotAssignItemToOthersException>(
            () => @event.AssignItem(regularParticipantId, item.Id, otherParticipantId));
    }

    [Fact]
    public void UnassignItem_UnknownItemId_ThrowsEventItemNotFoundException()
    {
        var @event = CreateEvent(out var creatorId);

        Assert.Throws<EventItemNotFoundException>(() => @event.UnassignItem(creatorId, Guid.NewGuid()));
    }

    [Fact]
    public void UnassignItem_ActingUserNotParticipant_ThrowsUserNotEventParticipantException()
    {
        var @event = CreateEvent(out var creatorId);
        var item = @event.AddItem(creatorId, "Bûche au chocolat", "1", DateTime.UtcNow);
        @event.AssignItem(creatorId, item.Id, creatorId);

        Assert.Throws<UserNotEventParticipantException>(() => @event.UnassignItem(Guid.NewGuid(), item.Id));
    }

    [Fact]
    public void UnassignItem_ActingUserIsAssignedParticipant_ClearsAssignedToUserId()
    {
        var @event = CreateEvent(out var creatorId);
        var regularParticipantId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, regularParticipantId, DateTime.UtcNow);
        var item = @event.AddItem(creatorId, "Bûche au chocolat", "1", DateTime.UtcNow);
        @event.AssignItem(creatorId, item.Id, regularParticipantId);

        @event.UnassignItem(regularParticipantId, item.Id);

        Assert.Null(item.AssignedToUserId);
    }

    [Fact]
    public void UnassignItem_ActingUserIsCreatorOrOrganizer_ClearsAssignedToUserIdForAnotherParticipant()
    {
        var @event = CreateEvent(out var creatorId);
        var regularParticipantId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, regularParticipantId, DateTime.UtcNow);
        var item = @event.AddItem(creatorId, "Bûche au chocolat", "1", DateTime.UtcNow);
        @event.AssignItem(creatorId, item.Id, regularParticipantId);

        @event.UnassignItem(creatorId, item.Id);

        Assert.Null(item.AssignedToUserId);
    }

    [Fact]
    public void UnassignItem_ActingUserIsParticipantNotAssigned_ThrowsParticipantCannotUnassignOthersItemException()
    {
        var @event = CreateEvent(out var creatorId);
        var regularParticipantId = Guid.NewGuid();
        var otherParticipantId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, regularParticipantId, DateTime.UtcNow);
        @event.InviteParticipant(creatorId, otherParticipantId, DateTime.UtcNow);
        var item = @event.AddItem(creatorId, "Bûche au chocolat", "1", DateTime.UtcNow);
        @event.AssignItem(creatorId, item.Id, otherParticipantId);

        Assert.Throws<ParticipantCannotUnassignOthersItemException>(() => @event.UnassignItem(regularParticipantId, item.Id));
    }

    [Fact]
    public void RemoveItem_UnknownItemId_ThrowsEventItemNotFoundException()
    {
        var @event = CreateEvent(out var creatorId);

        Assert.Throws<EventItemNotFoundException>(() => @event.RemoveItem(creatorId, Guid.NewGuid()));
    }

    [Fact]
    public void RemoveItem_ActingUserNotParticipant_ThrowsUserNotEventParticipantException()
    {
        var @event = CreateEvent(out var creatorId);
        var item = @event.AddItem(creatorId, "Bûche au chocolat", "1", DateTime.UtcNow);

        Assert.Throws<UserNotEventParticipantException>(() => @event.RemoveItem(Guid.NewGuid(), item.Id));
    }

    [Fact]
    public void RemoveItem_ActingUserIsItemCreator_RemovesFromCollection()
    {
        var @event = CreateEvent(out var creatorId);
        var regularParticipantId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, regularParticipantId, DateTime.UtcNow);
        var item = @event.AddItem(regularParticipantId, "Bûche au chocolat", "1", DateTime.UtcNow);

        @event.RemoveItem(regularParticipantId, item.Id);

        Assert.DoesNotContain(@event.Items, t => t.Id == item.Id);
    }

    [Fact]
    public void RemoveItem_ActingUserIsCreatorOrOrganizer_RemovesItemCreatedByAnotherParticipant()
    {
        var @event = CreateEvent(out var creatorId);
        var regularParticipantId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, regularParticipantId, DateTime.UtcNow);
        var item = @event.AddItem(regularParticipantId, "Bûche au chocolat", "1", DateTime.UtcNow);

        @event.RemoveItem(creatorId, item.Id);

        Assert.DoesNotContain(@event.Items, t => t.Id == item.Id);
    }

    [Fact]
    public void RemoveItem_ActingUserIsParticipantNotItemCreatorNorOrganizer_ThrowsParticipantCannotDeleteOthersItemException()
    {
        var @event = CreateEvent(out var creatorId);
        var itemCreatorId = Guid.NewGuid();
        var otherParticipantId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, itemCreatorId, DateTime.UtcNow);
        @event.InviteParticipant(creatorId, otherParticipantId, DateTime.UtcNow);
        var item = @event.AddItem(itemCreatorId, "Bûche au chocolat", "1", DateTime.UtcNow);

        Assert.Throws<ParticipantCannotDeleteOthersItemException>(() => @event.RemoveItem(otherParticipantId, item.Id));
    }

    [Fact]
    public void JoinViaInviteLink_NewUser_AddsParticipantAndRaisesDomainEvent()
    {
        var @event = CreateEvent(out _);
        var joiningUserId = Guid.NewGuid();

        var participant = @event.JoinViaInviteLink(joiningUserId, DateTime.UtcNow);

        Assert.Contains(@event.Participants, p => p.UserId == joiningUserId);
        Assert.Equal(ParticipantRole.Participant, participant.Role);
        Assert.Contains(@event.DomainEvents, e => e is ParticipantInvitedDomainEvent);
    }

    [Fact]
    public void JoinViaInviteLink_AlreadyParticipant_ReturnsExistingParticipantWithoutNewDomainEvent()
    {
        var @event = CreateEvent(out var creatorId);
        var joiningUserId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, joiningUserId, DateTime.UtcNow);
        @event.ClearDomainEvents();

        var participant = @event.JoinViaInviteLink(joiningUserId, DateTime.UtcNow);

        Assert.Equal(joiningUserId, participant.UserId);
        Assert.Single(@event.Participants, p => p.UserId == joiningUserId);
        Assert.Empty(@event.DomainEvents);
    }

    [Fact]
    public void RegenerateInviteLink_ActingUserNotCreatorNorOrganizer_ThrowsUserNotEventOrganizerException()
    {
        var @event = CreateEvent(out var creatorId);
        var regularParticipantId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, regularParticipantId, DateTime.UtcNow);

        Assert.Throws<UserNotEventOrganizerException>(() => @event.RegenerateInviteLink(regularParticipantId, "new-token"));
    }

    [Fact]
    public void RegenerateInviteLink_ActingUserIsCreator_ChangesTokenAndRaisesDomainEvent()
    {
        var @event = CreateEvent(out var creatorId);
        var previousToken = @event.InviteLinkToken;

        @event.RegenerateInviteLink(creatorId, "new-token");

        Assert.Equal("new-token", @event.InviteLinkToken);
        Assert.NotEqual(previousToken, @event.InviteLinkToken);
        Assert.Contains(@event.DomainEvents, e => e is EventInviteLinkRegeneratedDomainEvent);
    }
}
