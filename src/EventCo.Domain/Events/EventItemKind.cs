namespace EventCo.Domain.Events;

public enum EventItemKind
{
    // Demande d'un (co-)organisateur : créée sans attribution, n'importe quel participant peut la prendre.
    ToBring,
    // Déclaration d'un participant de ce qu'il apporte : attribuée à son créateur dès la création, définitivement.
    Contribution
}
