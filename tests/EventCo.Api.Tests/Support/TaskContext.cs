namespace EventCo.Api.Tests.Support;

// Injectée par Reqnroll (context injection) dans les classes [Binding] d'un même scénario,
// pour partager l'identifiant de la tâche posée par le step Given commun (ExistingTaskSteps).
public sealed class TaskContext
{
    public Guid? TaskId { get; set; }
}
