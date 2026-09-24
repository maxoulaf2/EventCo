using EventCo.Domain.Common;
using EventCo.Domain.Users.DomainEvents;
using EventCo.Domain.Users.Exceptions;
using EventCo.Domain.ValueObjects;

namespace EventCo.Domain.Users;

public class User : Entity
{
    public Email Email { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    // Clé de l'objet dans le stockage de fichiers (cf. IFileStorage côté Application), pas une URL :
    // l'URL publique dépend de l'infrastructure (bucket, endpoint) et est résolue à la lecture.
    public string? AvatarStorageKey { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Attribué uniquement en base (aucun use case ne le modifie) : donne un accès en lecture seule à tous
    // les événements, sans faire de l'administrateur un participant (cf. Event.EnsureCanBeViewedBy).
    public bool IsAdmin { get; private set; }

    private User(Guid id, Email email, string displayName, DateTime createdAt) : base(id)
    {
        Email = email;
        DisplayName = displayName;
        CreatedAt = createdAt;
    }

    public static User Create(Email email, string displayName, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new UserDisplayNameEmptyException();

        var user = new User(Guid.NewGuid(), email, displayName.Trim(), now);
        user.AddDomainEvent(new UserCreatedDomainEvent(user.Id));
        return user;
    }

    internal static User Reconstitute(Guid id, Email email, string displayName, string? avatarStorageKey, DateTime createdAt, bool isAdmin) =>
        new(id, email, displayName, createdAt) { AvatarStorageKey = avatarStorageKey, IsAdmin = isAdmin };

    public void UpdateProfile(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new UserDisplayNameEmptyException(Id);

        DisplayName = displayName.Trim();
        AddDomainEvent(new UserProfileUpdatedDomainEvent(Id));
    }

    // Clé générée par le use case (jamais saisie par l'utilisateur) : une clé vide est une erreur de
    // programmation, pas une règle métier violée.
    public void ChangeAvatar(string avatarStorageKey)
    {
        if (string.IsNullOrWhiteSpace(avatarStorageKey))
            throw new ArgumentException("La clé de stockage de l'avatar est obligatoire.", nameof(avatarStorageKey));

        AvatarStorageKey = avatarStorageKey;
        AddDomainEvent(new UserProfileUpdatedDomainEvent(Id));
    }

    public void RemoveAvatar()
    {
        if (AvatarStorageKey is null)
            return;

        AvatarStorageKey = null;
        AddDomainEvent(new UserProfileUpdatedDomainEvent(Id));
    }
}
