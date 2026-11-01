namespace StudentContingent.Domain.Model;

/// <summary>
/// «Событие движения контингента». История только дополняется:
/// события не изменяются и не удаляются задним числом.
/// </summary>
public class MovementEvent : IEntity
{
    public Guid Id { get; set; }
    public MovementType EventType { get; set; }
    public DateTime EffectiveDate { get; set; }
    public StudentStatus PreviousStatus { get; set; }
    public StudentStatus NewStatus { get; set; }

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    /// <summary>Приказ-основание.</summary>
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    /// <summary>Группа до события.</summary>
    public Guid? FromGroupId { get; set; }
    public StudyGroup? FromGroup { get; set; }

    /// <summary>Группа после события.</summary>
    public Guid? ToGroupId { get; set; }
    public StudyGroup? ToGroup { get; set; }
}
