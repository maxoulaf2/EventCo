using EventCo.Domain.Events;
using EventCo.Domain.Events.DomainEvents;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Domain.Tests.Events;

public class EventDatePollTests
{
    private static readonly DateTime Now = new(2026, 11, 13, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FirstDate = Now.AddDays(10);
    private static readonly DateTime SecondDate = Now.AddDays(11);

    private static Event CreatePoll(out Guid creatorId, params DateTime[] dates)
    {
        creatorId = Guid.NewGuid();
        var options = dates.Length == 0 ? [FirstDate, SecondDate] : dates;
        var @event = Event.CreateWithDatePoll("Week-end", null, options, "Lyon", "invite-token", creatorId, Now);
        @event.ClearDomainEvents();
        return @event;
    }

    private static Guid OptionId(Event @event, DateTime date) => @event.DateOptions.Single(o => o.Date == date).Id;

    private static Guid Invite(Event @event, Guid creatorId)
    {
        var userId = Guid.NewGuid();
        @event.InviteParticipant(creatorId, userId, Now);
        return userId;
    }

    [Fact]
    public void CreateWithDatePoll_TwoOptions_HasNoDateAndOpenPoll()
    {
        var @event = CreatePoll(out _);

        Assert.Null(@event.EventDate);
        Assert.True(@event.IsDatePollOpen);
        Assert.Equal(2, @event.DateOptions.Count);
    }

    [Fact]
    public void Create_FixedDate_HasNoOpenPoll()
    {
        var @event = Event.Create("Repas", null, FirstDate, null, "invite-token", Guid.NewGuid(), Now);

        Assert.False(@event.IsDatePollOpen);
        Assert.Empty(@event.DateOptions);
    }

    [Fact]
    public void CreateWithDatePoll_SingleOption_ThrowsDateOptionCountOutOfRangeException()
    {
        Assert.Throws<DateOptionCountOutOfRangeException>(() =>
            Event.CreateWithDatePoll("Week-end", null, [FirstDate], null, "invite-token", Guid.NewGuid(), Now));
    }

    [Fact]
    public void CreateWithDatePoll_ElevenOptions_ThrowsDateOptionCountOutOfRangeException()
    {
        var dates = Enumerable.Range(1, 11).Select(i => Now.AddDays(i));

        Assert.Throws<DateOptionCountOutOfRangeException>(() =>
            Event.CreateWithDatePoll("Week-end", null, dates, null, "invite-token", Guid.NewGuid(), Now));
    }

    [Fact]
    public void CreateWithDatePoll_PastOption_ThrowsDateOptionInThePastException()
    {
        Assert.Throws<DateOptionInThePastException>(() =>
            Event.CreateWithDatePoll("Week-end", null, [FirstDate, Now.AddDays(-1)], null, "invite-token", Guid.NewGuid(), Now));
    }

    [Fact]
    public void CreateWithDatePoll_DuplicateOption_ThrowsDateOptionDuplicateException()
    {
        Assert.Throws<DateOptionDuplicateException>(() =>
            Event.CreateWithDatePoll("Week-end", null, [FirstDate, FirstDate], null, "invite-token", Guid.NewGuid(), Now));
    }

    [Fact]
    public void AddDateOption_ActingUserIsOrganizer_AddsOptionAndRaisesDomainEvent()
    {
        var @event = CreatePoll(out var creatorId);

        var option = @event.AddDateOption(creatorId, Now.AddDays(12), Now);

        Assert.Equal(3, @event.DateOptions.Count);
        Assert.Contains(@event.DomainEvents, e => e is DateOptionAddedDomainEvent added && added.DateOptionId == option.Id);
    }

    [Fact]
    public void AddDateOption_ActingUserIsSimpleParticipant_ThrowsUserNotEventOrganizerException()
    {
        var @event = CreatePoll(out var creatorId);
        var participantId = Invite(@event, creatorId);

        Assert.Throws<UserNotEventOrganizerException>(() => @event.AddDateOption(participantId, Now.AddDays(12), Now));
    }

    [Fact]
    public void AddDateOption_EventWithFixedDate_ThrowsDatePollNotOpenException()
    {
        var creatorId = Guid.NewGuid();
        var @event = Event.Create("Repas", null, FirstDate, null, "invite-token", creatorId, Now);

        Assert.Throws<DatePollNotOpenException>(() => @event.AddDateOption(creatorId, SecondDate, Now));
    }

    [Fact]
    public void RemoveDateOption_OnlyTwoOptionsLeft_ThrowsDateOptionCountOutOfRangeException()
    {
        var @event = CreatePoll(out var creatorId);

        Assert.Throws<DateOptionCountOutOfRangeException>(() => @event.RemoveDateOption(creatorId, OptionId(@event, FirstDate)));
    }

    [Fact]
    public void RemoveDateOption_ThreeOptions_RemovesOptionAndRaisesDomainEvent()
    {
        var @event = CreatePoll(out var creatorId, FirstDate, SecondDate, Now.AddDays(12));
        var optionId = OptionId(@event, FirstDate);

        @event.RemoveDateOption(creatorId, optionId);

        Assert.DoesNotContain(@event.DateOptions, o => o.Id == optionId);
        Assert.Contains(@event.DomainEvents, e => e is DateOptionRemovedDomainEvent removed && removed.DateOptionId == optionId);
    }

    [Fact]
    public void VoteForDateOption_Participant_CanVoteForSeveralOptions()
    {
        var @event = CreatePoll(out var creatorId);
        var participantId = Invite(@event, creatorId);

        @event.VoteForDateOption(participantId, OptionId(@event, FirstDate));
        @event.VoteForDateOption(participantId, OptionId(@event, SecondDate));

        Assert.All(@event.DateOptions, o => Assert.Contains(participantId, o.VoterUserIds));
        Assert.Equal(2, @event.DomainEvents.OfType<DateVoteCastDomainEvent>().Count());
    }

    [Fact]
    public void VoteForDateOption_AlreadyVoted_IsIdempotent()
    {
        var @event = CreatePoll(out var creatorId);
        var optionId = OptionId(@event, FirstDate);

        @event.VoteForDateOption(creatorId, optionId);
        @event.VoteForDateOption(creatorId, optionId);

        Assert.Single(@event.DomainEvents.OfType<DateVoteCastDomainEvent>());
    }

    [Fact]
    public void VoteForDateOption_UserNotParticipant_ThrowsUserNotEventParticipantException()
    {
        var @event = CreatePoll(out _);

        Assert.Throws<UserNotEventParticipantException>(() => @event.VoteForDateOption(Guid.NewGuid(), OptionId(@event, FirstDate)));
    }

    [Fact]
    public void VoteForDateOption_UnknownOption_ThrowsDateOptionNotFoundException()
    {
        var @event = CreatePoll(out var creatorId);

        Assert.Throws<DateOptionNotFoundException>(() => @event.VoteForDateOption(creatorId, Guid.NewGuid()));
    }

    [Fact]
    public void WithdrawDateVote_ExistingVote_RemovesVoteAndRaisesDomainEvent()
    {
        var @event = CreatePoll(out var creatorId);
        var optionId = OptionId(@event, FirstDate);
        @event.VoteForDateOption(creatorId, optionId);

        @event.WithdrawDateVote(creatorId, optionId);

        Assert.Empty(@event.DateOptions.Single(o => o.Id == optionId).VoterUserIds);
        Assert.Single(@event.DomainEvents.OfType<DateVoteWithdrawnDomainEvent>());
    }

    [Fact]
    public void ChooseDate_Organizer_SetsEventDateAndMarksOnlyItsVotersAttending()
    {
        var @event = CreatePoll(out var creatorId);
        var voterId = Invite(@event, creatorId);
        var otherVoterId = Invite(@event, creatorId);
        @event.VoteForDateOption(voterId, OptionId(@event, FirstDate));
        @event.VoteForDateOption(otherVoterId, OptionId(@event, SecondDate));
        @event.ClearDomainEvents();

        @event.ChooseDate(creatorId, OptionId(@event, FirstDate), Now);

        Assert.Equal(FirstDate, @event.EventDate);
        Assert.False(@event.IsDatePollOpen);
        Assert.Equal(ParticipationStatus.Attending, @event.Participants.Single(p => p.UserId == voterId).ParticipationStatus);
        Assert.Equal(ParticipationStatus.Unknown, @event.Participants.Single(p => p.UserId == otherVoterId).ParticipationStatus);
        Assert.Contains(@event.DomainEvents, e => e is EventDateChosenDomainEvent);
        Assert.Single(@event.DomainEvents.OfType<ParticipantParticipationStatusChangedDomainEvent>());
    }

    [Fact]
    public void ChooseDate_NoVoteAtAll_ClosesThePoll()
    {
        var @event = CreatePoll(out var creatorId);

        @event.ChooseDate(creatorId, OptionId(@event, SecondDate), Now);

        Assert.Equal(SecondDate, @event.EventDate);
    }

    [Fact]
    public void ChooseDate_OptionNowInThePast_ThrowsDateOptionInThePastException()
    {
        var @event = CreatePoll(out var creatorId);

        Assert.Throws<DateOptionInThePastException>(() => @event.ChooseDate(creatorId, OptionId(@event, FirstDate), FirstDate.AddHours(1)));
    }

    [Fact]
    public void ChooseDate_SimpleParticipant_ThrowsUserNotEventOrganizerException()
    {
        var @event = CreatePoll(out var creatorId);
        var participantId = Invite(@event, creatorId);

        Assert.Throws<UserNotEventOrganizerException>(() => @event.ChooseDate(participantId, OptionId(@event, FirstDate), Now));
    }

    [Fact]
    public void VoteForDateOption_PollClosed_ThrowsDatePollNotOpenException()
    {
        var @event = CreatePoll(out var creatorId);
        @event.ChooseDate(creatorId, OptionId(@event, FirstDate), Now);

        Assert.Throws<DatePollNotOpenException>(() => @event.VoteForDateOption(creatorId, OptionId(@event, SecondDate)));
    }

    [Fact]
    public void RemoveParticipant_WithVotes_WithdrawsTheirVotes()
    {
        var @event = CreatePoll(out var creatorId);
        var participantId = Invite(@event, creatorId);
        @event.VoteForDateOption(participantId, OptionId(@event, FirstDate));
        @event.ClearDomainEvents();

        @event.RemoveParticipant(creatorId, participantId);

        Assert.All(@event.DateOptions, o => Assert.DoesNotContain(participantId, o.VoterUserIds));
        Assert.Single(@event.DomainEvents.OfType<DateVoteWithdrawnDomainEvent>());
    }

    [Fact]
    public void UpdateDetails_PollOpenWithDate_ThrowsEventDateUnderPollException()
    {
        var @event = CreatePoll(out var creatorId);

        Assert.Throws<EventDateUnderPollException>(() => @event.UpdateDetails(creatorId, "Week-end", null, FirstDate, "Lyon"));
    }

    [Fact]
    public void UpdateDetails_PollOpenWithoutDate_UpdatesOtherDetails()
    {
        var @event = CreatePoll(out var creatorId);

        @event.UpdateDetails(creatorId, "Week-end ski", null, null, "Chamonix");

        Assert.Equal("Week-end ski", @event.Title);
        Assert.Null(@event.EventDate);
    }

    [Fact]
    public void UpdateDetails_FixedDateEventWithoutDate_ThrowsEventDateRequiredException()
    {
        var creatorId = Guid.NewGuid();
        var @event = Event.Create("Repas", null, FirstDate, null, "invite-token", creatorId, Now);

        Assert.Throws<EventDateRequiredException>(() => @event.UpdateDetails(creatorId, "Repas", null, null, null));
    }
}
