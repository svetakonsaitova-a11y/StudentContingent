namespace StudentContingent.Domain.Model;

/// <summary>«Предыдущее образование» студента.</summary>
public class PreviousEducation : IEntity
{
    public Guid Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
}
