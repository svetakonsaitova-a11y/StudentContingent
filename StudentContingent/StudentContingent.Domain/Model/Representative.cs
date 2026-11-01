namespace StudentContingent.Domain.Model;

/// <summary>«Законный представитель» студента.</summary>
public class Representative : IEntity
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
}
